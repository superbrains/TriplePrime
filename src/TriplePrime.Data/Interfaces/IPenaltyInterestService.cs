using System.Threading.Tasks;

namespace TriplePrime.Data.Interfaces
{
    public interface IPenaltyInterestService
    {
        Task<decimal> GetDailyInterestRateAsync();
        Task SetDailyInterestRateAsync(decimal rate, string userId);
        Task<bool> IsPenaltyInterestEnabledAsync();
        Task SetPenaltyInterestEnabledAsync(bool enabled, string userId);

        /// <summary>
        /// Manually triggers interest accrual calculation for all overdue schedules.
        /// Returns a summary of what was calculated.
        /// </summary>
        Task<InterestCalculationResult> RunCalculationAsync();
    }

    public class InterestCalculationResult
    {
        public int SchedulesProcessed { get; set; }
        public int SchedulesUpdated { get; set; }
        public decimal TotalInterestAccrued { get; set; }
        public string Message { get; set; }
    }
}
