using System.Threading.Tasks;

namespace TriplePrime.Data.Interfaces
{
    public interface IPenaltyInterestService
    {
        /// <summary>
        /// Gets the daily penalty interest rate for overdue payments.
        /// </summary>
        /// <returns>Daily interest rate as decimal (e.g., 0.005 for 0.5% per day)</returns>
        Task<decimal> GetDailyInterestRateAsync();

        /// <summary>
        /// Sets the daily penalty interest rate for overdue payments.
        /// </summary>
        /// <param name="rate">Daily interest rate as decimal (e.g., 0.005 for 0.5%)</param>
        /// <param name="userId">User making the change</param>
        Task SetDailyInterestRateAsync(decimal rate, string userId);

        /// <summary>
        /// Checks if penalty interest accrual is enabled.
        /// </summary>
        /// <returns>True if penalty interest is enabled, false otherwise</returns>
        Task<bool> IsPenaltyInterestEnabledAsync();

        /// <summary>
        /// Enables or disables penalty interest accrual.
        /// </summary>
        /// <param name="enabled">True to enable, false to disable</param>
        /// <param name="userId">User making the change</param>
        Task SetPenaltyInterestEnabledAsync(bool enabled, string userId);
    }
}
