using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;
using TriplePrime.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Security.Claims;

namespace TriplePrime.Data.Services
{
    public class MarketerService
    {
        private readonly IGenericRepository<Marketer> _marketerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AuthenticationService _authService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MarketerService(
            IGenericRepository<Marketer> marketerRepository, 
            IUnitOfWork unitOfWork,
            AuthenticationService authService,
            UserManager<ApplicationUser> userManager)
        {
            _marketerRepository = marketerRepository;
            _unitOfWork = unitOfWork;
            _authService = authService;
            _userManager = userManager;
        }

        public async Task<MarketerDetails> CreateMarketerAsync(CreateMarketerRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Create the user account first
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    IsActive = true
                };

                // Generate a random password if none is provided
                string password = request.Password;
                if (string.IsNullOrEmpty(password))
                {
                    password = GenerateRandomPassword();
                }

                // Create user directly
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(", ", result.Errors));
                }

                // Add Marketer role and claims
                await _userManager.AddToRoleAsync(user, "Marketer");
                await _userManager.AddClaimAsync(user, new Claim("user_type", "marketer"));

                // Create the marketer profile
                var marketer = new Marketer
                {
                    UserId = user.Id,
                    CompanyName = request.CompanyName,
                    PhoneNumber = request.PhoneNumber,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    Website = request.Website,
                    SocialMediaHandle = request.SocialMediaHandle,
                    CommissionRate = request.CommissionRate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };

                await _marketerRepository.AddAsync(marketer);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new MarketerDetails
                {
                    Id = marketer.Id,
                    UserId = marketer.UserId,
                    Name = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    PhoneNumber = marketer.PhoneNumber,
                    CompanyName = marketer.CompanyName,
                    CommissionRate = marketer.CommissionRate,
                    Status = marketer.IsActive ? "active" : "inactive",
                    TotalCustomers = marketer.TotalCustomers,
                    TotalSales = marketer.TotalSales,
                    TotalCommission = marketer.TotalCommissionEarned,
                    RegistrationDate = marketer.CreatedAt,
                    GeneratedPassword = string.IsNullOrEmpty(request.Password) ? password : null
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<IReadOnlyList<MarketerDetails>> GetAllMarketersAsync()
        {
            var marketers = await _marketerRepository.ListAsync(new MarketerSpecification());
            var marketerDetails = new List<MarketerDetails>();

            foreach (var marketer in marketers)
            {
                var user = await _userManager.FindByIdAsync(marketer.UserId);
                if (user != null)
                {
                    marketerDetails.Add(new MarketerDetails
                    {
                        Id = marketer.Id,
                        UserId = marketer.UserId,
                        Name = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        PhoneNumber = marketer.PhoneNumber,
                        CompanyName = marketer.CompanyName,
                        CommissionRate = marketer.CommissionRate,
                        Status = marketer.IsActive ? "active" : "inactive",
                        TotalCustomers = marketer.TotalCustomers,
                        TotalSales = marketer.TotalSales,
                        TotalCommission = marketer.TotalCommissionEarned,
                        RegistrationDate = marketer.CreatedAt
                    });
                }
            }

            return marketerDetails;
        }

        public async Task<MarketerDetails> GetMarketerByIdAsync(int id)
        {
            var marketer = await _marketerRepository.GetEntityWithSpec(new MarketerSpecification(id));
            if (marketer == null) return null;

            var user = await _userManager.FindByIdAsync(marketer.UserId);
            if (user == null) return null;

            return new MarketerDetails
            {
                Id = marketer.Id,
                UserId = marketer.UserId,
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = marketer.PhoneNumber,
                CompanyName = marketer.CompanyName,
                CommissionRate = marketer.CommissionRate,
                Status = marketer.IsActive ? "active" : "inactive",
                TotalCustomers = marketer.TotalCustomers,
                TotalSales = marketer.TotalSales,
                TotalCommission = marketer.TotalCommissionEarned,
                RegistrationDate = marketer.CreatedAt
            };
        }

        public async Task<MarketerDetails> UpdateMarketerAsync(int id, UpdateMarketerRequest request)
        {
            var marketer = await _marketerRepository.GetEntityWithSpec(new MarketerSpecification(id));
            if (marketer == null)
                throw new ArgumentException("Marketer not found");

            var user = await _userManager.FindByIdAsync(marketer.UserId);
            if (user == null)
                throw new ArgumentException("Associated user not found");

            // Update user information
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            await _userManager.UpdateAsync(user);

            // Update marketer information
            marketer.CompanyName = request.CompanyName;
            marketer.PhoneNumber = request.PhoneNumber;
            marketer.Address = request.Address;
            marketer.City = request.City;
            marketer.State = request.State;
            marketer.PostalCode = request.PostalCode;
            marketer.Country = request.Country;
            marketer.Website = request.Website;
            marketer.SocialMediaHandle = request.SocialMediaHandle;
            marketer.UpdatedAt = DateTime.UtcNow;
            marketer.UpdatedBy = request.UpdatedBy;

            _marketerRepository.Update(marketer);
            await _unitOfWork.SaveChangesAsync();

            return new MarketerDetails
            {
                Id = marketer.Id,
                UserId = marketer.UserId,
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = marketer.PhoneNumber,
                CompanyName = marketer.CompanyName,
                CommissionRate = marketer.CommissionRate,
                Status = marketer.IsActive ? "active" : "inactive",
                TotalCustomers = marketer.TotalCustomers,
                TotalSales = marketer.TotalSales,
                TotalCommission = marketer.TotalCommissionEarned,
                RegistrationDate = marketer.CreatedAt
            };
        }

        public async Task<MarketerDetails> UpdateCommissionRateAsync(int id, decimal newRate)
        {
            if (newRate < 0 || newRate > 0.25m)
                throw new ArgumentException("Commission rate must be between 0 and 25%");

            var marketer = await _marketerRepository.GetEntityWithSpec(new MarketerSpecification(id));
            if (marketer == null)
                throw new ArgumentException("Marketer not found");

            marketer.CommissionRate = newRate;
            marketer.UpdatedAt = DateTime.UtcNow;
            _marketerRepository.Update(marketer);
            await _unitOfWork.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(marketer.UserId);
            return new MarketerDetails
            {
                Id = marketer.Id,
                UserId = marketer.UserId,
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = marketer.PhoneNumber,
                CompanyName = marketer.CompanyName,
                CommissionRate = marketer.CommissionRate,
                Status = marketer.IsActive ? "active" : "inactive",
                TotalCustomers = marketer.TotalCustomers,
                TotalSales = marketer.TotalSales,
                TotalCommission = marketer.TotalCommissionEarned,
                RegistrationDate = marketer.CreatedAt
            };
        }

        public async Task<MarketerDetails> ChangeStatusAsync(int id, bool isActive)
        {
            var marketer = await _marketerRepository.GetEntityWithSpec(new MarketerSpecification(id));
            if (marketer == null)
                throw new ArgumentException("Marketer not found");

            marketer.IsActive = isActive;
            marketer.UpdatedAt = DateTime.UtcNow;
            _marketerRepository.Update(marketer);
            await _unitOfWork.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(marketer.UserId);
            return new MarketerDetails
            {
                Id = marketer.Id,
                UserId = marketer.UserId,
                Name = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                PhoneNumber = marketer.PhoneNumber,
                CompanyName = marketer.CompanyName,
                CommissionRate = marketer.CommissionRate,
                Status = marketer.IsActive ? "active" : "inactive",
                TotalCustomers = marketer.TotalCustomers,
                TotalSales = marketer.TotalSales,
                TotalCommission = marketer.TotalCommissionEarned,
                RegistrationDate = marketer.CreatedAt
            };
        }

        public async Task<MarketerPerformance> GetMarketerPerformanceAsync(int id, DateTime startDate, DateTime endDate)
        {
            var marketer = await _marketerRepository.GetEntityWithSpec(new MarketerSpecification(id));
            if (marketer == null)
                throw new ArgumentException("Marketer not found");

            // Get performance data from referrals and commissions
            var referrals = marketer.Referrals
                .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
                .ToList();

            var commissions = marketer.Commissions
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .ToList();

            var monthlyStats = referrals
                .GroupBy(r => new { r.CreatedAt.Year, r.CreatedAt.Month })
                .Select(g => new MonthlyStats
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Customers = g.Count(),
                    Sales = g.Sum(r => r.CommissionAmount),
                    Commission = commissions
                        .Where(c => c.CreatedAt.Year == g.Key.Year && c.CreatedAt.Month == g.Key.Month)
                        .Sum(c => c.Amount)
                })
                .ToList();

            return new MarketerPerformance
            {
                Marketer = new MarketerBasicInfo
                {
                    Id = marketer.Id,
                    Name = $"{marketer.User.FirstName} {marketer.User.LastName}"
                },
                Customers = referrals.Count,
                Sales = referrals.Sum(r => r.CommissionAmount),
                Commission = commissions.Sum(c => c.Amount),
                ConversionRate = referrals.Any() ? (decimal)referrals.Count(r => r.Status == ReferralStatus.Completed) / referrals.Count : 0,
                MonthlyStats = monthlyStats
            };
        }

        private string GenerateRandomPassword()
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=[]{}|;:,.<>?";
            const int length = 12;

            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var bytes = new byte[length];
                rng.GetBytes(bytes);

                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    chars[i] = validChars[bytes[i] % validChars.Length];
                }

                return new string(chars);
            }
        }
    }

    public class CreateMarketerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; } = "";
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Website { get; set; }
        public string SocialMediaHandle { get; set; }
        public decimal CommissionRate { get; set; }
        public string CreatedBy { get; set; } = "Admin";
    }

    public class UpdateMarketerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Website { get; set; }
        public string SocialMediaHandle { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class MarketerDetails
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public decimal CommissionRate { get; set; }
        public string Status { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalCommission { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string GeneratedPassword { get; set; }
    }

    public class MarketerPerformance
    {
        public MarketerBasicInfo Marketer { get; set; }
        public int Customers { get; set; }
        public decimal Sales { get; set; }
        public decimal Commission { get; set; }
        public decimal ConversionRate { get; set; }
        public List<MonthlyStats> MonthlyStats { get; set; }
    }

    public class MarketerBasicInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class MonthlyStats
    {
        public string Month { get; set; }
        public int Customers { get; set; }
        public decimal Sales { get; set; }
        public decimal Commission { get; set; }
    }
} 