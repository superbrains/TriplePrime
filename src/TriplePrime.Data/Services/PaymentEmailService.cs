using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

using TriplePrime.Data.Models;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Services
{
    public class PaymentEmailService : BackgroundService
    {
        private readonly Channel<PaymentConfirmationMessage> _channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentEmailService> _logger;

        public PaymentEmailService(
            IServiceProvider serviceProvider,
            ILogger<PaymentEmailService> logger)
        {
            _channel = Channel.CreateUnbounded<PaymentConfirmationMessage>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task QueuePaymentConfirmationEmail(PaymentConfirmationMessage message)
        {
            try
            {
                await _channel.Writer.WriteAsync(message);
                _logger.LogInformation("Payment confirmation email queued for plan {PlanId}", message.PlanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue payment confirmation email for plan {PlanId}", message.PlanId);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = await _channel.Reader.ReadAsync(stoppingToken);
                    await ProcessPaymentConfirmationEmail(message);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment confirmation email");
                }
            }
        }

        private async Task ProcessPaymentConfirmationEmail(PaymentConfirmationMessage message)
        {
            using var scope = _serviceProvider.CreateScope();
            try
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var pushNotificationService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                var user = await userManager.FindByIdAsync(message.UserId);
                var foodPack = await unitOfWork.Repository<FoodPack>().GetByIdAsync(message.FoodPackId);

                if (user == null || foodPack == null)
                {
                    _logger.LogWarning("User or FoodPack not found for payment confirmation. PlanId: {PlanId}", message.PlanId);
                    return;
                }

                var baseUrl = configuration["AppSettings:ApiBaseUrl"];
                var nextPayment = message.PaymentSchedules
                    .Where(s => s.Status == "Pending")
                    .OrderBy(s => s.DueDate)
                    .FirstOrDefault();

                var emailModel = new
                {
                    CustomerName = $"{user.FirstName} {user.LastName}".Trim(),
                    AmountPaid = message.AmountPaid.ToString("N2"),
                    PaymentDate = message.PaymentDate.ToString("MMMM dd, yyyy"),
                    PaymentReference = message.PaymentReference,
                    FoodPackName = foodPack.Name,
                    TotalAmount = message.TotalAmount.ToString("N2"),
                    Duration = message.Duration,
                    PaymentFrequency = message.PaymentFrequency,
                    AmountPerPayment = message.AmountPaid.ToString("N2"),
                    PaymentSchedules = message.PaymentSchedules
                        .Take(5)
                        .Select(s => new
                        {
                            DueDate = s.DueDate.ToString("MMMM dd, yyyy"),
                            Amount = s.Amount.ToString("N2"),
                            Status = s.Status
                        }),
                    NextPaymentAmount = nextPayment?.Amount.ToString("N2") ?? "0.00",
                    NextPaymentDate = nextPayment?.DueDate.ToString("MMMM dd, yyyy") ?? "N/A",
                    IsAutomaticPayment = message.PaymentPreference == "automatic",
                    DashboardUrl = $"{baseUrl}/profile",
                    LogoUrl = $"{baseUrl}/images/logo.png",
                    CurrentYear = DateTime.UtcNow.Year
                };

                await emailService.SendTemplatedEmailAsync(
                    user.Email,
                    "Payment Confirmation - TriplePrime Food Savings",
                    "PaymentConfirmationTemplate.html",
                    emailModel
                );

                // Send push notification
                await SendPaymentConfirmationPushNotification(
                    pushNotificationService,
                    user,
                    message.AmountPaid,
                    message.PaymentReference,
                    foodPack.Name
                );

                _logger.LogInformation("Payment confirmation sent (email + push) to {Email} for plan {PlanId}", user.Email, message.PlanId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process payment confirmation for plan {PlanId}", message.PlanId);
            }
        }

        private async Task SendPaymentConfirmationPushNotification(
            IPushNotificationService pushNotificationService,
            ApplicationUser user,
            decimal amount,
            string paymentReference,
            string foodPackName)
        {
            try
            {
                if (user?.DeviceToken == null)
                {
                    _logger.LogInformation("No device token found for user {UserId}. Skipping push notification.", user?.Id);
                    return;
                }

                var title = "Payment Confirmed - TriplePrime";
                var body = $"Payment successful! ₦{amount:N0} received for your {foodPackName} savings plan. Ref: {paymentReference}";
                
                var data = new Dictionary<string, string>
                {
                    ["type"] = "payment_confirmation",
                    ["userId"] = user.Id,
                    ["amount"] = amount.ToString(),
                    ["paymentReference"] = paymentReference,
                    ["foodPackName"] = foodPackName
                };

                var tokens = new List<string> { user.DeviceToken };
                await pushNotificationService.SendNotificationAsync(tokens, title, body, data);

                _logger.LogInformation("Payment confirmation push notification sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send payment confirmation push notification to user {UserId}", user?.Id);
            }
        }
    }
} 