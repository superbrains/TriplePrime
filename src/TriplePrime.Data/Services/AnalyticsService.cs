using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Models;
using TriplePrime.Data.Repositories;
using Microsoft.AspNetCore.Identity;

namespace TriplePrime.Data.Services
{
    public class AnalyticsService
    {
        private readonly IGenericRepository<FoodPack> _foodPackRepository;
        private readonly IGenericRepository<Payment> _paymentRepository;
        private readonly IGenericRepository<Delivery> _deliveryRepository;
        private readonly IGenericRepository<Marketer> _marketerRepository;
        private readonly IGenericRepository<Referral> _referralRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<ApplicationUser> _userRepository;

        public AnalyticsService(
            IGenericRepository<FoodPack> foodPackRepository,
            IGenericRepository<Payment> paymentRepository,
            IGenericRepository<Delivery> deliveryRepository,
            IGenericRepository<Marketer> marketerRepository,
            IGenericRepository<Referral> referralRepository,
            IUnitOfWork unitOfWork,
            IGenericRepository<ApplicationUser> userRepository)
        {
            _foodPackRepository = foodPackRepository;
            _paymentRepository = paymentRepository;
            _deliveryRepository = deliveryRepository;
            _marketerRepository = marketerRepository;
            _referralRepository = referralRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<SalesAnalytics> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var foodPackSpec = new FoodPackSpecification();
            foodPackSpec.ApplyDateRangeFilter(startDate, endDate);
            
            var paymentSpec = new PaymentSpecification();
            paymentSpec.ApplyDateRangeFilter(startDate, endDate);

            var foodPacks = await _foodPackRepository.ListAsync(foodPackSpec);
            var payments = await _paymentRepository.ListAsync(paymentSpec);

            return new SalesAnalytics
            {
                TotalOrders = foodPacks.Count(),
                TotalRevenue = payments.Sum(p => p.Amount),
                AverageOrderValue = payments.Any() ? payments.Average(p => p.Amount) : 0,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<MarketingAnalytics> GetMarketingAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var marketerSpec = new MarketerSpecification(startDate, endDate);
            
            var referralSpec = new ReferralSpecification(startDate, endDate);

            var marketers = await _marketerRepository.ListAsync(marketerSpec);
            var referrals = await _referralRepository.ListAsync(referralSpec);

            return new MarketingAnalytics
            {
                TotalMarketers = marketers.Count(),
                ActiveMarketers = marketers.Count(m => m.IsActive),
                TotalReferrals = referrals.Count(),
                SuccessfulReferrals = referrals.Count(r => r.Status == ReferralStatus.Completed),
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<DeliveryAnalytics> GetDeliveryAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var deliverySpec = new DeliverySpecification();
            deliverySpec.ApplyDateRangeFilter(startDate, endDate);
            
            var deliveries = await _deliveryRepository.ListAsync(deliverySpec);

            return new DeliveryAnalytics
            {
                TotalDeliveries = deliveries.Count(),
                CompletedDeliveries = deliveries.Count(d => d.Status == DeliveryStatus.Delivered),
                AverageDeliveryTime = deliveries.Any() ? 
                    TimeSpan.FromTicks((long)deliveries.Where(d => d.ActualDeliveryDate.HasValue && d.CreatedAt != default)
                        .Average(d => (d.ActualDeliveryDate.Value - d.CreatedAt).Ticks)) : 
                    TimeSpan.Zero,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<UserAnalytics> GetUserAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var foodPackSpec = new FoodPackSpecification();
            foodPackSpec.ApplyDateRangeFilter(startDate, endDate);
            
            var paymentSpec = new PaymentSpecification();
            paymentSpec.ApplyDateRangeFilter(startDate, endDate);

            var foodPacks = await _foodPackRepository.ListAsync(foodPackSpec);
            var payments = await _paymentRepository.ListAsync(paymentSpec);

            var activeUsers = foodPacks.Select(fp => fp.UserId).Distinct().Count();
            var repeatCustomers = foodPacks.GroupBy(fp => fp.UserId)
                                         .Count(g => g.Count() > 1);

            return new UserAnalytics
            {
                ActiveUsers = activeUsers,
                RepeatCustomers = repeatCustomers,
                AverageOrdersPerUser = activeUsers > 0 ? (double)foodPacks.Count() / activeUsers : 0,
                AverageSpendPerUser = activeUsers > 0 ? payments.Sum(p => p.Amount) / activeUsers : 0,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<FinancialAnalytics> GetFinancialAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var paymentSpec = new PaymentSpecification();
            paymentSpec.ApplyDateRangeFilter(startDate, endDate);
            
            var payments = await _paymentRepository.ListAsync(paymentSpec);

            return new FinancialAnalytics
            {
                TotalRevenue = payments.Sum(p => p.Amount),
                AverageTransactionValue = payments.Any() ? payments.Average(p => p.Amount) : 0,
                PaymentMethodBreakdown = payments.GroupBy(p => p.PaymentMethod.Type)
                                               .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<DashboardMetrics> GetDashboardMetricsAsync()
        {
            try
            {
                var marketers = await _marketerRepository.ListAsync(new MarketerSpecification());
                var foodPacks = await _foodPackRepository.ListAsync(new FoodPackSpecification());
                var payments = await _paymentRepository.ListAsync(new PaymentSpecification());

                // Get users with their roles
                var users = await _userRepository.ListAsync(new UserSpecification());

                // Count users who don't have the Marketer or Admin role
                var totalCustomers = users.Count(u => 
                    !u.UserRoles.Any(ur => ur.RoleId == "Marketer" || ur.RoleId == "Admin"));
                var activeMarketers = marketers.Count(m => m.IsActive);
                var totalFoodPacks = foodPacks.Count();
                var totalRevenue = payments
                    .Where(p => p.Status == PaymentStatus.Completed)
                    .Sum(p => p.Amount);

                return new DashboardMetrics
                {
                    TotalCustomers = totalCustomers,
                    ActiveMarketers = activeMarketers,
                    TotalFoodPacks = totalFoodPacks,
                    TotalRevenue = totalRevenue
                };
            }
            catch (Exception ex)
            {
                // Log the error here if you have a logging service
                throw new Exception("Failed to retrieve dashboard metrics", ex);
            }
        }
    }

    public class SalesAnalytics
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class MarketingAnalytics
    {
        public int TotalMarketers { get; set; }
        public int ActiveMarketers { get; set; }
        public int TotalReferrals { get; set; }
        public int SuccessfulReferrals { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class DeliveryAnalytics
    {
        public int TotalDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public TimeSpan AverageDeliveryTime { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class UserAnalytics
    {
        public int ActiveUsers { get; set; }
        public int RepeatCustomers { get; set; }
        public double AverageOrdersPerUser { get; set; }
        public decimal AverageSpendPerUser { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class FinancialAnalytics
    {
        public decimal TotalRevenue { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public Dictionary<PaymentMethodType, decimal> PaymentMethodBreakdown { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
} 