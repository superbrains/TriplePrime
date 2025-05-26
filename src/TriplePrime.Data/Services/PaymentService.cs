using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Services
{
    public class PaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                payment.CreatedAt = DateTime.UtcNow;
                payment.Status = PaymentStatus.Pending;

                await _unitOfWork.Repository<Payment>().AddAsync(payment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return payment;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            var spec = new PaymentSpecification(id);
            return await _unitOfWork.Repository<Payment>().GetEntityWithSpec(spec);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByUserIdAsync(string userId)
        {
            var spec = new PaymentSpecification();
            spec.ApplyUserFilter(userId);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            var spec = new PaymentSpecification();
            spec.ApplyStatusFilter(status);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var spec = new PaymentSpecification();
            spec.ApplyDateRangeFilter(startDate, endDate);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task UpdatePaymentStatusAsync(int id, PaymentStatus status, string transactionId = null)
        {
            var payment = await GetPaymentByIdAsync(id);
            if (payment == null)
            {
                throw new ArgumentException($"Payment with ID {id} not found");
            }

            payment.Status = status;
            payment.TransactionId = transactionId;
            payment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var spec = new PaymentSpecification();
            spec.ApplyDateRangeFilter(startDate, endDate);
            spec.ApplyStatusFilter(PaymentStatus.Completed);

            var payments = await _unitOfWork.Repository<Payment>().ListAsync(spec);
            return payments.Sum(p => p.Amount);
        }

        public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync()
        {
            var spec = new PaymentSpecification();
            spec.ApplyStatusFilter(PaymentStatus.Failed);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            var spec = new PaymentSpecification();
            spec.ApplyStatusFilter(PaymentStatus.Pending);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<Payment> ProcessPaymentAsync(Payment payment)
        {
            // Here you would integrate with your payment gateway
            // This is a placeholder for the actual payment processing logic
            try
            {
                payment.Status = PaymentStatus.Completed;
                payment.TransactionId = Guid.NewGuid().ToString();
                payment.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync();

                return payment;
            }
            catch
            {
                payment.Status = PaymentStatus.Failed;
                payment.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Repository<Payment>().Update(payment);
                await _unitOfWork.SaveChangesAsync();

                throw;
            }
        }

        public async Task<Payment> RefundPaymentAsync(int paymentId, decimal amount)
        {
            var payment = await GetPaymentByIdAsync(paymentId);
            if (payment == null)
            {
                throw new ArgumentException($"Payment with ID {paymentId} not found");
            }

            if (payment.Status != PaymentStatus.Completed)
            {
                throw new InvalidOperationException("Only completed payments can be refunded");
            }

            if (amount > payment.Amount)
            {
                throw new ArgumentException("Refund amount cannot exceed the original payment amount");
            }

            var refund = new Payment
            {
                UserId = payment.UserId,
                Amount = -amount, // Negative amount for refund
                PaymentMethodId = payment.PaymentMethodId,
                Status = PaymentStatus.Completed,
                TransactionId = $"REFUND_{payment.TransactionId}",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Payment>().AddAsync(refund);
            await _unitOfWork.SaveChangesAsync();

            return refund;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByPaymentMethodAsync(int paymentMethodId)
        {
            var spec = new PaymentSpecification();
            spec.ApplyPaymentMethodFilter(paymentMethodId);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByFoodPackAsync(int foodPackId)
        {
            var spec = new PaymentSpecification();
            spec.ApplyFoodPackFilter(foodPackId);
            return await _unitOfWork.Repository<Payment>().ListAsync(spec);
        }
    }
} 