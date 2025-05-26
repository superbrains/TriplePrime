using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> ProcessPaymentAsync(string userId, decimal amount, string paymentMethodId);
        Task<Payment> GetPaymentByIdAsync(int id);
        Task<IReadOnlyList<Payment>> GetPaymentsByUserAsync(string userId);
        Task<bool> RefundPaymentAsync(int paymentId);
        Task<PaymentMethod> AddPaymentMethodAsync(string userId, string provider, string lastFourDigits);
        Task<bool> RemovePaymentMethodAsync(string userId, int paymentMethodId);
        Task<IReadOnlyList<PaymentMethod>> GetUserPaymentMethodsAsync(string userId);
        Task<bool> ValidatePaymentMethodAsync(string userId, int paymentMethodId);
    }
} 