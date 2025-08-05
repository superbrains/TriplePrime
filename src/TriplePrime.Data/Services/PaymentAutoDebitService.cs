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
using TriplePrime.Data.Specifications;

namespace TriplePrime.Data.Services
{
    public class PaymentAutoDebitService : BackgroundService
    {
        private readonly ILogger<PaymentAutoDebitService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);

        public PaymentAutoDebitService(ILogger<PaymentAutoDebitService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Auto-Debit Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAutoDebitsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing auto-debits");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessAutoDebitsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var paymentService = scope.ServiceProvider.GetRequiredService<PaymentService>();
            var savingsPlanService = scope.ServiceProvider.GetRequiredService<SavingsPlanService>();
            var pushNotificationService = scope.ServiceProvider.GetRequiredService<IPushNotificationService>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Find all pending schedules due now or earlier
            var scheduleSpec = new PaymentScheduleSpecification();
            scheduleSpec.ApplyStatusFilter("Pending");
            scheduleSpec.ApplyDueDateFilter(DateTime.UtcNow.Date);
            var dueSchedules = await unitOfWork.Repository<PaymentSchedule>().ListAsync(scheduleSpec);

            foreach (var schedule in dueSchedules)
            {
                try
                {
                    var plan = schedule.SavingsPlan;
                    if (plan == null || plan.PaymentPreference?.ToLower() != "automatic")
                        continue;

                    // Retrieve payment method for the plan/user
                    PaymentMethod paymentMethod = null;

                    if (plan.PaymentMethodId.HasValue)
                    {
                        paymentMethod = await unitOfWork.Repository<PaymentMethod>().GetByIdAsync(plan.PaymentMethodId.Value);
                    }

                    if (paymentMethod == null)
                    {
                        // Fallback: get user's default payment method
                        var pmSpec = new PaymentMethodSpecification();
                        pmSpec.ApplyUserFilter(plan.UserId);
                        var methods = await unitOfWork.Repository<PaymentMethod>().ListAsync(pmSpec);
                        paymentMethod = methods.FirstOrDefault(m => m.IsDefault) ?? methods.FirstOrDefault();
                    }

                    if (paymentMethod == null)
                    {
                        _logger.LogWarning("No payment method found for automatic debit. Plan {PlanId}, Schedule {ScheduleId}", plan.Id, schedule.Id);
                        
                        // Send notification about failed auto-debit due to missing payment method
                        await SendAutoDebitFailureNotification(
                            pushNotificationService,
                            userManager,
                            plan.UserId,
                            $"{plan.User?.FirstName} {plan.User?.LastName}".Trim() ?? "Customer",
                            schedule.Amount,
                            plan.FoodPack?.Name ?? "Food Pack"
                        );
                        continue;
                    }

                    var user = plan.User;
                    if (user == null)
                        continue;

                    var reference = $"AUTO-{schedule.Id}-{DateTime.UtcNow.Ticks}";

                    var paystackData = await paymentService.ChargeAuthorizationAsync(user.Email, paymentMethod.AuthorizationCode, schedule.Amount, reference);

                    if (paystackData.Status?.ToLower() == "success")
                    {
                        // Mark as paid via SavingsPlanService helper to reuse email + completion logic
                        await savingsPlanService.ProcessPaymentAsync(plan.Id, schedule.Amount, paystackData.Reference);

                        _logger.LogInformation("Auto-debit successful for schedule {ScheduleId}", schedule.Id);
                    }
                    else
                    {
                        _logger.LogWarning("Auto-debit failed for schedule {ScheduleId}: {Status}", schedule.Id, paystackData.Status);
                        
                        // Send notification about failed auto-debit
                        await SendAutoDebitFailureNotification(
                            pushNotificationService,
                            userManager,
                            plan.UserId,
                            $"{plan.User?.FirstName} {plan.User?.LastName}".Trim() ?? "Customer",
                            schedule.Amount,
                            plan.FoodPack?.Name ?? "Food Pack"
                        );
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing auto-debit for schedule {ScheduleId}", schedule.Id);
                    
                    // Send notification about failed auto-debit due to error
                    var schedule_safe = schedule;
                    if (schedule_safe?.SavingsPlan != null)
                    {
                        await SendAutoDebitFailureNotification(
                            pushNotificationService,
                            userManager,
                            schedule_safe.SavingsPlan.UserId,
                            $"{schedule_safe.SavingsPlan.User?.FirstName} {schedule_safe.SavingsPlan.User?.LastName}".Trim() ?? "Customer",
                            schedule_safe.Amount,
                            schedule_safe.SavingsPlan.FoodPack?.Name ?? "Food Pack"
                        );
                    }
                }
            }
        }

        private async Task SendAutoDebitFailureNotification(
            IPushNotificationService pushNotificationService,
            UserManager<ApplicationUser> userManager,
            string userId,
            string userName,
            decimal amount,
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

                var title = "Auto-Payment Failed - TriplePrime";
                var body = $"Unable to auto-debit ₦{amount:N0} for {foodPackName}. Please update your payment method or pay manually.";
                
                var data = new Dictionary<string, string>
                {
                    ["type"] = "auto_debit_failed",
                    ["userId"] = userId,
                    ["amount"] = amount.ToString(),
                    ["foodPackName"] = foodPackName,
                    ["success"] = "false"
                };

                var tokens = new List<string> { user.DeviceToken };
                await pushNotificationService.SendNotificationAsync(tokens, title, body, data);

                _logger.LogInformation("Auto-debit failure push notification sent to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send auto-debit failure push notification to user {UserId}", userId);
            }
        }
    }
} 