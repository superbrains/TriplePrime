using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface IDeliveryService
    {
        Task<Delivery> GetDeliveryByIdAsync(int id);
        Task<IReadOnlyList<Delivery>> GetDeliveriesByOrderIdAsync(int orderId);
        Task<IReadOnlyList<Delivery>> GetDeliveriesByUserIdAsync(string userId);
        Task<Delivery> CreateDeliveryAsync(Delivery delivery);
        Task<Delivery> UpdateDeliveryAsync(Delivery delivery);
        Task<bool> DeleteDeliveryAsync(int id);
        Task<bool> UpdateDeliveryStatusAsync(int id, string status);
        Task<IReadOnlyList<Delivery>> GetDeliveriesByStatusAsync(string status);
        Task<IReadOnlyList<Delivery>> GetDeliveriesByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
} 