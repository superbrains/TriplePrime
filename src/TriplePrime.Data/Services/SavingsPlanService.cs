using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;
using TriplePrime.Data.Specifications;
using System.Text.Json;
using TriplePrime.Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace TriplePrime.Data.Services
{
    public class SavingsPlanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaymentService _paymentService;
        private readonly PaymentEmailService _paymentEmailService;
        private readonly ILogger<SavingsPlanService> _logger;

        public SavingsPlanService(
            IUnitOfWork unitOfWork,
            PaymentService paymentService,
            PaymentEmailService paymentEmailService,
            ILogger<SavingsPlanService> logger)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _paymentEmailService = paymentEmailService;
            _logger = logger;
        }

        private async Task QueuePaymentConfirmationEmail(SavingsPlan plan, PaymentSchedule paidSchedule, string paymentReference)
        {
            try
            {
                var message = new PaymentConfirmationMessage
                {
                    PlanId = plan.Id,
                    UserId = plan.UserId,
                    FoodPackId = plan.FoodPackId,
                    AmountPaid = paidSchedule.Amount,
                    PaymentDate = paidSchedule.PaidAt ?? DateTime.UtcNow,
                    PaymentReference = paymentReference,
                    TotalAmount = plan.TotalAmount,
                    Duration = plan.Duration,
                    PaymentFrequency = plan.PaymentFrequency,
                    PaymentPreference = plan.PaymentPreference,
                    PaymentSchedules = plan.PaymentSchedules
                        .OrderBy(s => s.DueDate)
                        .Select(s => new PaymentScheduleInfo
                        {
                            DueDate = s.DueDate,
                            Amount = s.Amount,
                            Status = s.Status
                        })
                };

                await _paymentEmailService.QueuePaymentConfirmationEmail(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue payment confirmation email for plan {PlanId}", plan.Id);
            }
        }

        public async Task<SavingsPlan> CreateSavingsPlanAsync(SavingsPlan plan, string paymentReference)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Set initial values
                plan.CreatedAt = DateTime.UtcNow;
                plan.Status = "Active";
                plan.AmountPaid = 0;
                plan.RemindersEnabled = true;
                plan.CreatedBy = "Admin";
                plan.UpdatedBy = "Admin";

                // Generate payment schedule with the first one marked as paid
                var schedules = GeneratePaymentSchedule(plan, paymentReference);
                plan.PaymentSchedules = schedules;

                // Update plan with the first payment
                var firstSchedule = schedules.First();
                plan.AmountPaid = firstSchedule.Amount;
                plan.LastPaymentDate = DateTime.UtcNow;
                plan.UpdatedAt = DateTime.UtcNow;

                // Save the plan with all schedules
                await _unitOfWork.Repository<SavingsPlan>().AddAsync(plan);
                await _unitOfWork.SaveChangesAsync();

                // Check for referral and create commission if applicable
                var referral = await _unitOfWork.Repository<Referral>()
                    .GetEntityWithSpec(new ReferralSpecification(plan.UserId, true, true));

                if (referral != null)
                {
                    // Update referral status to Active if it's not already
                    if (referral.Status != ReferralStatus.Active)
                    {
                        referral.Status = ReferralStatus.Active;
                        referral.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Repository<Referral>().Update(referral);
                    }

                    var marketer = await _unitOfWork.Repository<Marketer>()
                        .GetEntityWithSpec(new MarketerSpecification(referral.MarketerId));

                    if (marketer != null && marketer.IsActive)
                    {
                        var commissionAmount = plan.AmountPaid * marketer.CommissionRate;
                        
                        var commission = new Commission
                        {
                            MarketerId = marketer.Id,
                            ReferralId = referral.Id,
                            Amount = commissionAmount,
                            Rate = marketer.CommissionRate,
                            Status = CommissionStatus.Pending,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "System",
                            UpdatedBy = "System"
                        };

                        await _unitOfWork.Repository<Commission>().AddAsync(commission);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                // Queue payment confirmation email
                await QueuePaymentConfirmationEmail(plan, firstSchedule, paymentReference);

                await _unitOfWork.CommitTransactionAsync();
                return plan;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private List<PaymentSchedule> GeneratePaymentSchedule(SavingsPlan plan, string firstPaymentReference = null)
        {
            var schedules = new List<PaymentSchedule>();
            var currentDate = plan.StartDate;
            int numberOfPayments;
            decimal amountPerPayment;

            // Calculate number of payments and amount per payment based on frequency
            switch (plan.PaymentFrequency?.ToLower())
            {
                case "daily":
                    numberOfPayments = plan.Duration * 30; // Approximate days per month
                    amountPerPayment = Math.Round(plan.TotalAmount / numberOfPayments, 2);
                    break;
                case "weekly":
                    numberOfPayments = plan.Duration * 4; // Approximate weeks per month
                    amountPerPayment = Math.Round(plan.TotalAmount / numberOfPayments, 2);
                    break;
                default: // monthly
                    numberOfPayments = plan.Duration;
                    amountPerPayment = plan.MonthlyAmount;
                    break;
            }

            // Generate schedules based on frequency
            for (int i = 0; i < numberOfPayments; i++)
            {
                var isPaid = i == 0 && !string.IsNullOrEmpty(firstPaymentReference);
                var dueDate = plan.PaymentFrequency?.ToLower() switch
                {
                    "daily" => currentDate.AddDays(i),
                    "weekly" => currentDate.AddDays(i * 7),
                    _ => currentDate.AddMonths(i) // monthly
                };

                schedules.Add(new PaymentSchedule
                {
                    SavingsPlanId = plan.Id,
                    DueDate = dueDate,
                    Amount = amountPerPayment,
                    Status = isPaid ? "Paid" : "Pending",
                    PaymentReference = isPaid ? firstPaymentReference : "",
                    PaidAt = isPaid ? DateTime.UtcNow : (DateTime?)null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "Admin",
                    UpdatedBy = "Admin",
                    UpdatedAt = DateTime.UtcNow
                });
            }

            // Handle any remaining amount due to rounding
            var totalScheduledAmount = schedules.Sum(s => s.Amount);
            if (totalScheduledAmount != plan.TotalAmount)
            {
                var difference = plan.TotalAmount - totalScheduledAmount;
                schedules.Last().Amount += difference;
            }

            return schedules;
        }

        public async Task<SavingsPlan> GetSavingsPlanByIdAsync(int id)
        {
            var spec = new SavingsPlanSpecification(id);
            return await _unitOfWork.Repository<SavingsPlan>().GetEntityWithSpec(spec);
        }

        public async Task<IEnumerable<SavingsPlan>> GetUserSavingsPlansAsync(string userId)
        {
            var spec = new SavingsPlanSpecification();
            spec.ApplyUserFilter(userId);
            var plans = await _unitOfWork.Repository<SavingsPlan>().ListAsync(spec);

            // Order payment schedules by due date
            foreach (var plan in plans)
            {
                plan.PaymentSchedules = plan.PaymentSchedules
                    .OrderBy(s => s.DueDate)
                    .ToList();
            }

            return plans;
        }

        public async Task UpdateSavingsPlanStatusAsync(int id, string status)
        {
            var plan = await GetSavingsPlanByIdAsync(id);
            if (plan == null)
            {
                throw new ArgumentException($"Savings plan with ID {id} not found");
            }

            plan.Status = status;
            plan.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<SavingsPlan>().Update(plan);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdatePaymentPreferenceAsync(int id, string preference, string paymentMethodJson)
        {
            var plan = await GetSavingsPlanByIdAsync(id);
            if (plan == null)
            {
                throw new ArgumentException($"Savings plan with ID {id} not found");
            }

            plan.PaymentPreference = preference;
            
            // Parse payment method JSON and create/update payment method
            if (!string.IsNullOrEmpty(paymentMethodJson))
            {
                var paymentMethodData = JsonSerializer.Deserialize<Dictionary<string, string>>(paymentMethodJson);
                var paymentMethod = new PaymentMethod
                {
                    UserId = plan.UserId,
                    Provider = "Paystack",
                    Type = paymentMethodData["type"],
                    LastFourDigits = paymentMethodData["lastFourDigits"],
                    AuthorizationCode = paymentMethodData["authorizationCode"],
                    CardType = paymentMethodData["cardType"],
                    Bank = paymentMethodData["bank"],
                    IsDefault = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = plan.UserId,
                    UpdatedBy = plan.UserId
                };

                var paymentMethodId = await _paymentService.CreatePaymentMethodAsync(paymentMethod);
                plan.PaymentMethodId = paymentMethodId;
            }

            plan.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<SavingsPlan>().Update(plan);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ToggleRemindersAsync(int id, bool enabled)
        {
            var plan = await GetSavingsPlanByIdAsync(id);
            if (plan == null)
            {
                throw new ArgumentException($"Savings plan with ID {id} not found");
            }

            plan.RemindersEnabled = enabled;
            plan.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<SavingsPlan>().Update(plan);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ProcessPaymentAsync(int planId, decimal amount, string paymentReference)
        {
            var plan = await GetSavingsPlanByIdAsync(planId);
            if (plan == null)
            {
                throw new ArgumentException($"Savings plan with ID {planId} not found");
            }

            // Update plan
            plan.AmountPaid += amount;
            plan.LastPaymentDate = DateTime.UtcNow;
            plan.UpdatedAt = DateTime.UtcNow;

            // Find the next pending payment schedule
            var schedule = plan.PaymentSchedules
                .Where(s => s.Status == "Pending")
                .OrderBy(s => s.DueDate)
                .FirstOrDefault();
            
            if (schedule != null)
            {
                schedule.Status = "Paid";
                schedule.PaymentReference = paymentReference;
                schedule.PaidAt = DateTime.UtcNow;
                schedule.UpdatedAt = DateTime.UtcNow;
                schedule.UpdatedBy = "System";
            }

            // Check if plan is completed
            if (plan.AmountPaid >= plan.TotalAmount)
            {
                plan.Status = "Completed";
            }

            _unitOfWork.Repository<SavingsPlan>().Update(plan);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<SavingsPlan> GetSavingsPlanBySubscriptionCodeAsync(string subscriptionCode)
        {
            var spec = new SavingsPlanSpecification();
            spec.ApplySubscriptionCodeFilter(subscriptionCode);
            return await _unitOfWork.Repository<SavingsPlan>().GetEntityWithSpec(spec);
        }

        public async Task UpdateSubscriptionDetailsAsync(string customerEmail, string subscriptionCode)
        {
            var spec = new SavingsPlanSpecification();
            spec.ApplyUserEmailFilter(customerEmail);
            spec.ApplyStatusFilter("Active");
            
            var plans = await _unitOfWork.Repository<SavingsPlan>().ListAsync(spec);
            var plan = plans.FirstOrDefault(p => string.IsNullOrEmpty(p.SubscriptionCode));
            
            if (plan != null)
            {
                plan.SubscriptionCode = subscriptionCode;
                plan.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<SavingsPlan>().Update(plan);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PaymentSchedule>> GetUpcomingPaymentsAsync(string userId)
        {
            var spec = new PaymentScheduleSpecification();
            spec.ApplyUserFilter(userId);
            spec.ApplyStatusFilter("Pending");
            spec.ApplyDateRangeFilter(DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
            return await _unitOfWork.Repository<PaymentSchedule>().ListAsync(spec);
        }

        public async Task<IEnumerable<SavingsPlanWithUserDetails>> GetAllSavingsPlansForAdminAsync(DateTime? startDate, DateTime? endDate, string status)
        {
            var spec = new SavingsPlanSpecification();
            
            if (startDate.HasValue && endDate.HasValue)
            {
                spec.ApplyDateRangeFilter(startDate.Value, endDate.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                spec.ApplyStatusFilter(status);
            }

            var plans = await _unitOfWork.Repository<SavingsPlan>().ListAsync(spec);
            
            // Include user and food pack details
            var plansWithDetails = plans.Select(plan => new SavingsPlanWithUserDetails
            {
                Id = plan.Id,
                UserFullName = $"{plan.User.FirstName} {plan.User.LastName}",
                UserPhoneNumber = plan.User.PhoneNumber,
                EmailAddress =plan.User.Email,
                UserAddress = plan.User.Address,
                FoodPackName = plan.FoodPack.Name,
                TotalAmount = plan.TotalAmount,
                AmountPaid = plan.AmountPaid,
                Status = plan.Status,
                StartDate = plan.StartDate,
                LastPaymentDate = plan.LastPaymentDate,
                PaymentFrequency = plan.PaymentFrequency,
                PaymentSchedules = plan.PaymentSchedules
                    .OrderBy(s => s.DueDate)
                    .Select(s => new PaymentScheduleDto
                    {
                        Id = s.Id,
                        DueDate = s.DueDate,
                        Amount = s.Amount,
                        Status = s.Status,
                        PaidAt = s.PaidAt
                    }).ToList()
            });

            return plansWithDetails;
        }

        public async Task<SavingsPlanWithUserDetails> GetSavingsPlanScheduleAsync(int id)
        {
            var plan = await GetSavingsPlanByIdAsync(id);
            if (plan == null)
                return null;

            return new SavingsPlanWithUserDetails
            {
                Id = plan.Id,
                UserFullName = $"{plan.User.FirstName} {plan.User.LastName}",
                UserPhoneNumber = plan.User.PhoneNumber,
                FoodPackName = plan.FoodPack.Name,
                TotalAmount = plan.TotalAmount,
                AmountPaid = plan.AmountPaid,
                Status = plan.Status,
                StartDate = plan.StartDate,
                LastPaymentDate = plan.LastPaymentDate,
                PaymentFrequency = plan.PaymentFrequency,
                PaymentSchedules = plan.PaymentSchedules
                    .OrderBy(s => s.DueDate)
                    .Select(s => new PaymentScheduleDto
                    {
                        Id = s.Id,
                        DueDate = s.DueDate,
                        Amount = s.Amount,
                        Status = s.Status,
                        PaidAt = s.PaidAt
                    }).ToList()
            };
        }

        public async Task ProcessAutomaticPayments()
        {
            // This method can be called by a background job to process due payments
            var spec = new PaymentScheduleSpecification();
            spec.ApplyStatusFilter("Pending");
            spec.ApplyDueDateFilter(DateTime.UtcNow.Date);
            
            var dueSchedules = await _unitOfWork.Repository<PaymentSchedule>().ListAsync(spec);
            
            foreach (var schedule in dueSchedules)
            {
                var plan = await GetSavingsPlanByIdAsync(schedule.SavingsPlanId);
                if (plan != null && plan.PaymentPreference == "automatic" && !string.IsNullOrEmpty(plan.SubscriptionCode))
                {
                    // Paystack will handle the automatic payment through the subscription
                    // We just need to wait for the webhook to update the payment status
                    continue;
                }
                else if (plan != null && plan.PaymentPreference == "manual")
                {
                    // Send reminder notification for manual payment
                    // This would integrate with your notification service
                }
            }
        }

        public async Task<SavingsPlan> UpdatePaymentAndPreferenceAsync(
            int planId,
            string userId,
            string paymentPreference,
            Authorization authorization,
            string paymentReference,
            decimal amount,
            int scheduleId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var plan = await GetSavingsPlanByIdAsync(planId);
                if (plan == null || plan.UserId != userId)
                {
                    throw new ArgumentException("Invalid savings plan");
                }

                // Find and validate the payment schedule
                var schedule = plan.PaymentSchedules.FirstOrDefault(s => s.Id == scheduleId);
                if (schedule == null)
                {
                    throw new ArgumentException("Invalid payment schedule");
                }

                if (schedule.Status == "Paid")
                {
                    throw new InvalidOperationException("This schedule has already been paid");
                }

                if (amount != schedule.Amount)
                {
                    throw new ArgumentException("Payment amount does not match schedule amount");
                }

                // Update payment preference and method if automatic is selected
                plan.PaymentPreference = paymentPreference;

                // Update the payment schedule
                schedule.Status = "Paid";
                schedule.PaymentReference = paymentReference;
                schedule.PaidAt = DateTime.UtcNow;
                schedule.UpdatedAt = DateTime.UtcNow;
                schedule.UpdatedBy = userId;

                // Update plan payment details
                plan.AmountPaid += amount;
                plan.LastPaymentDate = DateTime.UtcNow;
                plan.UpdatedAt = DateTime.UtcNow;
                plan.UpdatedBy = userId;

                // Queue payment confirmation email
                await QueuePaymentConfirmationEmail(plan, schedule, paymentReference);

                _unitOfWork.Repository<SavingsPlan>().Update(plan);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return plan;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
} 