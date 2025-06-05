using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Models;
using TriplePrime.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using TriplePrime.Data.Specifications;
using Microsoft.Extensions.Logging;

namespace TriplePrime.Data.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IGenericRepository<FoodPack> _foodPackRepository;
        private readonly IGenericRepository<Payment> _paymentRepository;
        private readonly IGenericRepository<Delivery> _deliveryRepository;
        private readonly IGenericRepository<Marketer> _marketerRepository;
        private readonly IGenericRepository<Referral> _referralRepository;
        private readonly IGenericRepository<FoodPackPurchase> _foodPackPurchaseRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<ApplicationUser> _userRepository;

        public AnalyticsService(
            IGenericRepository<FoodPack> foodPackRepository,
            IGenericRepository<Payment> paymentRepository,
            IGenericRepository<Delivery> deliveryRepository,
            IGenericRepository<Marketer> marketerRepository,
            IGenericRepository<Referral> referralRepository,
            IGenericRepository<FoodPackPurchase> foodPackPurchaseRepository,
            IUnitOfWork unitOfWork,
            IGenericRepository<ApplicationUser> userRepository)
        {
            _foodPackRepository = foodPackRepository;
            _paymentRepository = paymentRepository;
            _deliveryRepository = deliveryRepository;
            _marketerRepository = marketerRepository;
            _referralRepository = referralRepository;
            _foodPackPurchaseRepository = foodPackPurchaseRepository;
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
        }

        public async Task<SalesAnalytics> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var foodPackPurchases = await _foodPackPurchaseRepository.ListAsync(new FoodPackPurchaseSpecification());
            var payments = await _paymentRepository.ListAsync(new PaymentSpecification());

            // Filter by date range in memory
            foodPackPurchases = foodPackPurchases.Where(fp => fp.PurchaseDate >= startDate && fp.PurchaseDate <= endDate);
            payments = payments.Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

            return new SalesAnalytics
            {
                TotalOrders = foodPackPurchases.Count(),
                TotalRevenue = payments.Sum(p => p.Amount),
                AverageOrderValue = payments.Any() ? payments.Average(p => p.Amount) : 0,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<MarketingAnalytics> GetMarketingAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var marketers = await _marketerRepository.ListAsync(new MarketerSpecification());
            
            // Filter by date range in memory
            marketers = marketers.Where(m => m.CreatedAt >= startDate && m.CreatedAt <= endDate);

            return new MarketingAnalytics
            {
                TotalMarketers = marketers.Count(),
                ActiveMarketers = marketers.Count(m => m.IsActive),
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
            var foodPackPurchases = await _foodPackPurchaseRepository.ListAsync(new FoodPackPurchaseSpecification());
            var payments = await _paymentRepository.ListAsync(new PaymentSpecification());

            // Filter by date range in memory
            foodPackPurchases = foodPackPurchases.Where(fp => fp.PurchaseDate >= startDate && fp.PurchaseDate <= endDate);
            payments = payments.Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

            var activeUsers = foodPackPurchases.Select(fp => fp.UserId).Distinct().Count();
            var repeatCustomers = foodPackPurchases.GroupBy(fp => fp.UserId)
                                                 .Count(g => g.Count() > 1);

            return new UserAnalytics
            {
                ActiveUsers = activeUsers,
                RepeatCustomers = repeatCustomers,
                AverageOrdersPerUser = activeUsers > 0 ? (double)foodPackPurchases.Count() / activeUsers : 0,
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

        public async Task<SalesTrendAnalytics> GetSalesTrendsAsync(DateTime startDate, DateTime endDate, string groupBy)
        {
            var foodPackPurchases = await _foodPackPurchaseRepository.ListAsync(new FoodPackPurchaseSpecification());
            var payments = await _paymentRepository.ListAsync(new PaymentSpecification());

            // Filter by date range
            foodPackPurchases = foodPackPurchases.Where(fp => fp.PurchaseDate >= startDate && fp.PurchaseDate <= endDate);
            payments = payments.Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate);

            var salesByPeriod = new List<PeriodSales>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                DateTime periodEnd;
                switch (groupBy.ToLower())
                {
                    case "day":
                        periodEnd = currentDate.AddDays(1);
                        break;
                    case "week":
                        periodEnd = currentDate.AddDays(7);
                        break;
                    case "month":
                        periodEnd = currentDate.AddMonths(1);
                        break;
                    default:
                        periodEnd = currentDate.AddDays(1);
                        break;
                }

                var periodPurchases = foodPackPurchases.Where(fp => fp.PurchaseDate >= currentDate && fp.PurchaseDate < periodEnd);
                var periodPayments = payments.Where(p => p.CreatedAt >= currentDate && p.CreatedAt < periodEnd);

                salesByPeriod.Add(new PeriodSales
                {
                    Period = currentDate.ToString("yyyy-MM-dd"),
                    Amount = periodPayments.Sum(p => p.Amount),
                    Count = periodPurchases.Count()
                });

                currentDate = periodEnd;
            }

            return new SalesTrendAnalytics
            {
                SalesByPeriod = salesByPeriod,
                TopSellingPacks = await GetTopSellingPacksAsync(startDate, endDate),
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<UserGrowthAnalytics> GetUserGrowthAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var users = await _userRepository.ListAsync(new UserSpecification());
            var foodPackPurchases = await _foodPackPurchaseRepository.ListAsync(new FoodPackPurchaseSpecification());

            // Filter by date range
            users = users.Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);
            foodPackPurchases = foodPackPurchases.Where(fp => fp.PurchaseDate >= startDate && fp.PurchaseDate <= endDate);

            var userGrowth = new List<UserGrowthPoint>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var nextDate = currentDate.AddDays(1);
                var newUsers = users.Count(u => u.CreatedAt >= currentDate && u.CreatedAt < nextDate);
                var activeUsers = foodPackPurchases
                    .Where(fp => fp.PurchaseDate >= currentDate && fp.PurchaseDate < nextDate)
                    .Select(fp => fp.UserId)
                    .Distinct()
                    .Count();

                userGrowth.Add(new UserGrowthPoint
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    NewUsers = newUsers,
                    ActiveUsers = activeUsers
                });

                currentDate = nextDate;
            }

            return new UserGrowthAnalytics
            {
                UserGrowth = userGrowth,
                UserDistribution = new UserDistribution
                {
                    ByRole = users.GroupBy(u => u.UserRoles.FirstOrDefault()?.RoleId ?? "Customer")
                                .Select(g => new RoleDistribution { Role = g.Key, Count = g.Count() })
                                .ToList(),
                    ByStatus = users.GroupBy(u => u.IsActive ? "Active" : "Inactive")
                                  .Select(g => new StatusDistribution { Status = g.Key, Count = g.Count() })
                                  .ToList()
                },
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<FoodPackAnalytics> GetFoodPackAnalyticsAsync(DateTime startDate, DateTime endDate)
        {
            var foodPacks = await _foodPackRepository.ListAsync(new FoodPackSpecification());
            var foodPackPurchases = await _foodPackPurchaseRepository.ListAsync(new FoodPackPurchaseSpecification());

            // Filter by date range
            foodPackPurchases = foodPackPurchases.Where(fp => fp.PurchaseDate >= startDate && fp.PurchaseDate <= endDate);

            var revenueByPack = foodPackPurchases
                .GroupBy(fp => fp.FoodPackId)
                .Select(g => new PackRevenue
                {
                    PackId = g.Key,
                    Name = foodPacks.FirstOrDefault(p => p.Id == g.Key)?.Name ?? "Unknown",
                    Purchases = g.Count(),
                    Revenue = g.Sum(fp => fp.PurchasePrice)
                })
                .ToList();

            return new FoodPackAnalytics
            {
                TotalPacks = foodPacks.Count(),
                ActivePacks = foodPacks.Count(p => p.Available),
                TotalPurchases = foodPackPurchases.Count(),
                RevenueByPack = revenueByPack,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        private async Task<List<TopSellingPack>> GetTopSellingPacksAsync(DateTime startDate, DateTime endDate)
        {
            var foodPacks = await _foodPackRepository.ListAsync(new FoodPackSpecification());
            var foodPackPurchases = await _foodPackPurchaseRepository.ListAsync(new FoodPackPurchaseSpecification());

            // Filter by date range
            foodPackPurchases = foodPackPurchases.Where(fp => fp.PurchaseDate >= startDate && fp.PurchaseDate <= endDate);

            return foodPackPurchases
                .GroupBy(fp => fp.FoodPackId)
                .Select(g => new TopSellingPack
                {
                    PackId = g.Key,
                    Name = foodPacks.FirstOrDefault(p => p.Id == g.Key)?.Name ?? "Unknown",
                    Quantity = g.Count(),
                    Revenue = g.Sum(fp => fp.PurchasePrice)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(10)
                .ToList();
        }

        public async Task<DashboardMetrics> GetDashboardMetricsAsync()
        {
            try
            {
                var marketers = await _marketerRepository.ListAsync(new MarketerSpecification());
                var foodPacks = await _foodPackRepository.ListAsync(new FoodPackSpecification());
                var savingsPlans = await _unitOfWork.Repository<SavingsPlan>().ListAsync(new SavingsPlanSpecification());
                var foodPackPurchases = await _foodPackPurchaseRepository.ListAsync(new FoodPackPurchaseSpecification());

                // Get users with their roles
                var users = await _userRepository.ListAsync(new UserSpecification());

                // Count users who don't have the Marketer or Admin role
                var totalCustomers = users.Count(u => 
                    !u.UserRoles.Any(ur => ur.RoleId == "Marketer" || ur.RoleId == "Admin"));
                var activeMarketers = marketers.Count(m => m.IsActive);
                var totalFoodPacks = foodPacks.Count();
                var totalRevenue = savingsPlans.Sum(sp => sp.AmountPaid);

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

        // Stub implementations for missing interface methods
        public Task<Dictionary<string, decimal>> GetRevenueByPeriodAsync(DateTime startDate, DateTime endDate, string period)
        {
            return Task.FromResult(new Dictionary<string, decimal>());
        }

        public Task<Dictionary<string, int>> GetUserRegistrationsByPeriodAsync(DateTime startDate, DateTime endDate, string period)
        {
            return Task.FromResult(new Dictionary<string, int>());
        }

        public Task<Dictionary<string, int>> GetActiveUsersByPeriodAsync(DateTime startDate, DateTime endDate, string period)
        {
            return Task.FromResult(new Dictionary<string, int>());
        }

        public Task<Dictionary<string, decimal>> GetAverageOrderValueByPeriodAsync(DateTime startDate, DateTime endDate, string period)
        {
            return Task.FromResult(new Dictionary<string, decimal>());
        }

        public Task<Dictionary<string, int>> GetOrdersByStatusAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(new Dictionary<string, int>());
        }

        public Task<Dictionary<string, decimal>> GetCommissionsByMarketerAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(new Dictionary<string, decimal>());
        }

        public Task<Dictionary<string, int>> GetReferralsByMarketerAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(new Dictionary<string, int>());
        }

        public Task<Dictionary<string, decimal>> GetPaymentMethodBreakdownAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(new Dictionary<string, decimal>());
        }

        public Task<Dictionary<string, int>> GetDeliveryStatusBreakdownAsync(DateTime startDate, DateTime endDate)
        {
            return Task.FromResult(new Dictionary<string, int>());
        }
    }
} 