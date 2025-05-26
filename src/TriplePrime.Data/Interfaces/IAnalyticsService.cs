using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface IAnalyticsService
    {
        Task<Dictionary<string, decimal>> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate, string period);
        Task<Dictionary<string, int>> GetUserRegistrationsByPeriodAsync(DateTime startDate, DateTime endDate, string period);
        Task<Dictionary<string, int>> GetActiveUsersByPeriodAsync(DateTime startDate, DateTime endDate, string period);
        Task<Dictionary<string, decimal>> GetAverageOrderValueByPeriodAsync(DateTime startDate, DateTime endDate, string period);
        Task<Dictionary<string, int>> GetOrdersByStatusAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, decimal>> GetCommissionsByMarketerAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetReferralsByMarketerAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, decimal>> GetPaymentMethodBreakdownAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetDeliveryStatusBreakdownAsync(DateTime startDate, DateTime endDate);
    }
} 