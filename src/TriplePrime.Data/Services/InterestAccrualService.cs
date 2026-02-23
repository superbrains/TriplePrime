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
    public class InterestAccrualService : BackgroundService
    {
        private readonly ILogger<InterestAccrualService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Run once per day

        public InterestAccrualService(
            ILogger<InterestAccrualService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Interest Accrual Service is starting.");

            // Wait a bit before starting the first run (to allow app initialization)
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessInterestAccrualAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing interest accrual");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessInterestAccrualAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var penaltyInterestService = scope.ServiceProvider.GetRequiredService<IPenaltyInterestService>();

            _logger.LogInformation("Starting interest accrual process at {Time}", DateTime.UtcNow);

            // Check if penalty interest is enabled
            var isEnabled = await penaltyInterestService.IsPenaltyInterestEnabledAsync();
            if (!isEnabled)
            {
                _logger.LogInformation("Penalty interest accrual is disabled. Skipping.");
                return;
            }

            // Get the daily interest rate
            var dailyRate = await penaltyInterestService.GetDailyInterestRateAsync();
            _logger.LogInformation("Using daily interest rate: {Rate}% ({RateDecimal})",
                dailyRate * 100, dailyRate);

            // Query all overdue payment schedules (Pending and past due date)
            var spec = new PaymentScheduleSpecification();
            spec.ApplyStatusFilter("Pending");
            spec.ApplyDueDateFilter(DateTime.UtcNow.Date);

            var overdueSchedules = await unitOfWork.Repository<PaymentSchedule>().ListAsync(spec);

            if (!overdueSchedules.Any())
            {
                _logger.LogInformation("No overdue schedules found. Interest accrual process completed.");
                return;
            }

            _logger.LogInformation("Processing {Count} overdue payment schedules", overdueSchedules.Count());

            decimal totalInterestAccrued = 0m;
            int schedulesUpdated = 0;
            var today = DateTime.UtcNow.Date;

            foreach (var schedule in overdueSchedules)
            {
                try
                {
                    // Initialize interest accrual dates if not already set
                    if (!schedule.InterestAccrualStartDate.HasValue)
                    {
                        schedule.InterestAccrualStartDate = schedule.DueDate.Date.AddDays(1);
                        schedule.LastInterestCalculationDate = schedule.InterestAccrualStartDate;
                        schedule.UpdatedAt = DateTime.UtcNow;
                        schedule.UpdatedBy = "InterestAccrualService";

                        _logger.LogInformation(
                            "Initialized interest accrual for schedule {ScheduleId}. Start date: {StartDate}",
                            schedule.Id, schedule.InterestAccrualStartDate.Value);
                    }

                    // Calculate days since last calculation
                    var daysSinceLastCalculation = (today - schedule.LastInterestCalculationDate.Value.Date).Days;

                    if (daysSinceLastCalculation > 0)
                    {
                        // Calculate new interest: Amount × DailyRate × Days
                        var newInterest = schedule.Amount * dailyRate * daysSinceLastCalculation;

                        // Update accrued interest
                        schedule.AccruedInterest += newInterest;
                        schedule.LastInterestCalculationDate = today;
                        schedule.UpdatedAt = DateTime.UtcNow;
                        schedule.UpdatedBy = "InterestAccrualService";

                        totalInterestAccrued += newInterest;
                        schedulesUpdated++;

                        var daysOverdue = (today - schedule.DueDate.Date).Days;

                        _logger.LogInformation(
                            "Interest accrued for schedule {ScheduleId}: ₦{NewInterest:F2} " +
                            "({Days} days × {Rate}% × ₦{Amount:F2}). " +
                            "Total accrued: ₦{Total:F2}. Days overdue: {DaysOverdue}",
                            schedule.Id,
                            newInterest,
                            daysSinceLastCalculation,
                            dailyRate * 100,
                            schedule.Amount,
                            schedule.AccruedInterest,
                            daysOverdue);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing interest accrual for schedule {ScheduleId}",
                        schedule.Id);
                    // Continue processing other schedules
                }
            }

            // Save all changes in a single transaction
            if (schedulesUpdated > 0)
            {
                try
                {
                    await unitOfWork.SaveChangesAsync();

                    _logger.LogInformation(
                        "Interest accrual completed successfully. " +
                        "Updated {SchedulesCount} schedules. " +
                        "Total interest accrued: ₦{TotalInterest:F2}",
                        schedulesUpdated,
                        totalInterestAccrued);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save interest accrual changes to database");
                    throw;
                }
            }
            else
            {
                _logger.LogInformation(
                    "No interest to accrue. All {Count} overdue schedules are already up to date.",
                    overdueSchedules.Count());
            }
        }
    }
}
