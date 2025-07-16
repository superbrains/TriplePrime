using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing auto-debit for schedule {ScheduleId}", schedule.Id);
                }
            }
        }
    }
} 