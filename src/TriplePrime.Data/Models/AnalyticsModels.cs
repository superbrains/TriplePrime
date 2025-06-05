using System;
using System.Collections.Generic;

namespace TriplePrime.Data.Models
{
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
        public Dictionary<string, decimal> PaymentMethodBreakdown { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class DashboardMetrics
    {
        public int TotalCustomers { get; set; }
        public int ActiveMarketers { get; set; }
        public int TotalFoodPacks { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class SalesTrendAnalytics
    {
        public List<PeriodSales> SalesByPeriod { get; set; }
        public List<TopSellingPack> TopSellingPacks { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PeriodSales
    {
        public string Period { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class TopSellingPack
    {
        public int PackId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class UserGrowthAnalytics
    {
        public List<UserGrowthPoint> UserGrowth { get; set; }
        public UserDistribution UserDistribution { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class UserGrowthPoint
    {
        public string Date { get; set; }
        public int NewUsers { get; set; }
        public int ActiveUsers { get; set; }
    }

    public class UserDistribution
    {
        public List<RoleDistribution> ByRole { get; set; }
        public List<StatusDistribution> ByStatus { get; set; }
    }

    public class RoleDistribution
    {
        public string Role { get; set; }
        public int Count { get; set; }
    }

    public class StatusDistribution
    {
        public string Status { get; set; }
        public int Count { get; set; }
    }

    public class FoodPackAnalytics
    {
        public int TotalPacks { get; set; }
        public int ActivePacks { get; set; }
        public int TotalPurchases { get; set; }
        public List<PackRevenue> RevenueByPack { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PackRevenue
    {
        public int PackId { get; set; }
        public string Name { get; set; }
        public int Purchases { get; set; }
        public decimal Revenue { get; set; }
    }
} 