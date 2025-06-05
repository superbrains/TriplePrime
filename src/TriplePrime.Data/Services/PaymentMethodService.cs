using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Models;
using System;

namespace TriplePrime.Data.Services
{
    public interface IPaymentMethodService
    {
        Task<PaymentMethod> GetUserPaymentMethodAsync(string userId);
        Task<PaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod);
        Task<PaymentMethod> SavePaystackPaymentMethodAsync(
            string userId,
            PaystackAuthorization authorization,
            PaystackCustomer customer,
            string paymentType);
    }

    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentMethod> GetUserPaymentMethodAsync(string userId)
        {
            return await _context.PaymentMethods
                .FirstOrDefaultAsync(p => p.UserId == userId && p.IsDefault);
        }

        public async Task<PaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            var existing = await _context.PaymentMethods
                .FirstOrDefaultAsync(p => p.Id == paymentMethod.Id);

            if (existing == null)
            {
                _context.PaymentMethods.Add(paymentMethod);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(paymentMethod);
            }

            await _context.SaveChangesAsync();
            return paymentMethod;
        }

        public async Task<PaymentMethod> SavePaystackPaymentMethodAsync(
            string userId,
            PaystackAuthorization authorization,
            PaystackCustomer customer,
            string paymentType)
        {
            // Determine the payment method type
            var type = paymentType.ToLower() switch
            {
                "card" => PaymentMethodType.CreditCard.ToString(),
                "bank" => PaymentMethodType.BankTransfer.ToString(),
                "mobile_money" => PaymentMethodType.MobileMoney.ToString(),
                _ => PaymentMethodType.CreditCard.ToString()
            };

            // Create new payment method
            var paymentMethod = new PaymentMethod
            {
                UserId = userId,
                Provider = "Paystack",
                Type = type,
                LastFourDigits = authorization.Last4,
                AuthorizationCode = authorization.AuthorizationCode,
                CardType = authorization.CardType,
                Bank = authorization.Bank,
                IsDefault = true, // Make this the default payment method
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            // Set all other payment methods as non-default
            var existingMethods = await _context.PaymentMethods
                .Where(p => p.UserId == userId)
                .ToListAsync();

            foreach (var existing in existingMethods)
            {
                existing.IsDefault = false;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = userId;
            }

            // Add the new payment method
            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();

            return paymentMethod;
        }
    }
} 