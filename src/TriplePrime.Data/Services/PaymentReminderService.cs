using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Models;

namespace TriplePrime.Data.Services
{
    public class PaymentReminderService : BackgroundService
    {
        private readonly ILogger<PaymentReminderService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Check once per day

        public PaymentReminderService(
            ILogger<PaymentReminderService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Reminder Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPaymentRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing payment reminders");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessPaymentRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var savingsPlanService = scope.ServiceProvider.GetRequiredService<SavingsPlanService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var pushNotificationService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Get all active savings plans
            var activePlans = await savingsPlanService.GetAllSavingsPlansForAdminAsync(null, null, "Active");

            foreach (var plan in activePlans)
            {
                try
                {
                    // Get upcoming payments for the next 7 days
                    var upcomingPayments = plan.PaymentSchedules
                        .Where(s => s.Status == "Pending" && 
                                  s.DueDate.Date <= DateTime.UtcNow.AddDays(3).Date &&
                                  s.DueDate.Date >= DateTime.UtcNow.Date)
                        .OrderBy(s => s.DueDate)
                        .ToList();

                    if (!upcomingPayments.Any()) continue;

                    var nextPayment = upcomingPayments.First();
                    var progressPercentage = (int)((plan.AmountPaid / plan.TotalAmount) * 100);

                    // Prepare email model
                    var emailModel = new
                    {
                        Name = plan.UserFullName,
                        PlanId = plan.Id,
                        FoodPackName = plan.FoodPackName,
                        PaymentFrequency = plan.PaymentFrequency,
                        TotalAmount = plan.TotalAmount,
                        AmountPaid = plan.AmountPaid,
                        ProgressPercentage = progressPercentage,
                        PaymentReference = nextPayment.Id,
                        Amount = nextPayment.Amount,
                        DueDate = nextPayment.DueDate.ToString("dd MMMM yyyy"),
                        Currency = "₦", // Nigerian Naira
                        PaymentUrl = $"https://tripleprime.com.ng" // Replace with actual payment URL
                    };

                    // Send reminder email
                    await emailService.SendTemplatedEmailAsync(
                        plan.EmailAddress, // Assuming phone number is used as email
                        "Savings Plan Payment Reminder",
                        "PaymentReminderTemplate.html",
                        emailModel
                    );

                    // Send push notification
                    await SendPaymentReminderPushNotification(
                        pushNotificationService, 
                        userManager, 
                        plan.UserId,
                        plan.UserFullName,
                        nextPayment.Amount,
                        nextPayment.DueDate.ToString("dd MMMM yyyy"),
                        plan.FoodPackName
                    );

                    _logger.LogInformation(
                        "Payment reminder sent (email + push) for plan {PlanId}, payment {PaymentId}, due date {DueDate}",
                        plan.Id, nextPayment.Id, nextPayment.DueDate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Error sending payment reminder for plan {PlanId}", 
                        plan.Id);
                }
            }
        }

        private async Task SendPaymentReminderPushNotification(
            IPushNotificationService pushNotificationService,
            UserManager<ApplicationUser> userManager,
            string userId,
            string userName,
            decimal amount,
            string dueDate,
            string foodPackName)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user?.DeviceToken == null)
                {
                    _logger.LogInformation("No device token found for user {UserId}. Skipping push notification.", userId);
                    return;
                }

                var title = "Payment Reminder - TriplePrime";
                var body = $"Hi {userName}, your payment of ₦{amount:N0} for {foodPackName} is due on {dueDate}. Don't miss out!";
                
                var data = new Dictionary<string, string>
                {
                    ["type"] = "payment_reminder",
                    ["userId"] = userId,
                    ["amount"] = amount.ToString(),
                    ["dueDate"] = dueDate,
                    ["foodPackName"] = foodPackName
                };

                var tokens = new List<string> { user.DeviceToken };
                await pushNotificationService.SendNotificationAsync(tokens, title, body, data);

                _logger.LogInformation("Payment reminder push notification sent to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment reminder push notification to user {UserId}", userId);
            }
        }
    }
} 