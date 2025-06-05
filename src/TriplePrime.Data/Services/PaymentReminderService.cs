using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

                    _logger.LogInformation(
                        "Payment reminder sent for plan {PlanId}, payment {PaymentId}, due date {DueDate}",
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
    }
} 