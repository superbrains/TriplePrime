using System.Collections.Generic;
using System.Threading.Tasks;

namespace TriplePrime.Data.Interfaces
{
    public interface IGlobalPricingService
    {
        /// <summary>
        /// Gets the global interest rate for a specific duration.
        /// </summary>
        /// <param name="durationMonths">Duration in months (1-4)</param>
        /// <returns>Interest rate as decimal (e.g., 0.02 for 2%)</returns>
        Task<decimal> GetGlobalRateAsync(int durationMonths);

        /// <summary>
        /// Gets all global interest rates (1-4 months).
        /// </summary>
        /// <returns>Dictionary with duration as key and rate as value</returns>
        Task<Dictionary<int, decimal>> GetAllGlobalRatesAsync();

        /// <summary>
        /// Sets the global interest rate for a specific duration.
        /// </summary>
        /// <param name="durationMonths">Duration in months (1-4)</param>
        /// <param name="interestRate">Interest rate as decimal (e.g., 0.02 for 2%)</param>
        /// <param name="userId">User making the change</param>
        Task SetGlobalRateAsync(int durationMonths, decimal interestRate, string userId);

        /// <summary>
        /// Sets all global interest rates at once.
        /// </summary>
        /// <param name="rates">Dictionary with duration (1-4) as key and rate as value</param>
        /// <param name="userId">User making the change</param>
        Task SetAllGlobalRatesAsync(Dictionary<int, decimal> rates, string userId);
    }
}
