using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using TriplePrime.Data.Entities;

namespace TriplePrime.Data.Repositories
{
    public class UserSpecification : BaseSpecification<ApplicationUser>
    {
        public UserSpecification(string userId)
            : base(u => u.Id == userId)
        {
            AddInclude(u => u.DeliveryAddresses);
            AddInclude(u => u.PaymentMethods);
            AddInclude(u => u.UserRoles);
        }

        public UserSpecification()
            : base()
        {
            AddInclude(u => u.DeliveryAddresses);
            AddInclude(u => u.PaymentMethods);
            AddInclude(u => u.UserRoles);
        }

        public void ApplyEmailFilter(string email)
        {
            AddCriteria(u => u.Email == email);
        }

        public void ApplyRoleFilter(string roleId)
        {
            AddCriteria(u => u.UserRoles.Any(ur => ur.RoleId == roleId));
        }

        public void ApplySearchFilter(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return;
                
            AddCriteria(u => 
                (u.FirstName != null && u.FirstName.Contains(searchTerm)) || 
                (u.LastName != null && u.LastName.Contains(searchTerm)) || 
                (u.Email != null && u.Email.Contains(searchTerm)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
        }

        public void ApplyActiveFilter(bool isActive)
        {
            AddCriteria(u => u.IsActive == isActive);
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            AddCriteria(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);
        }

        public void ApplyOrderByCreatedDate(bool descending = false)
        {
            if (descending)
            {
                ApplyOrderByDescending(u => u.CreatedAt);
            }
            else
            {
                ApplyOrderBy(u => u.CreatedAt);
            }
        }

        public void ApplyOrderByName(bool descending = false)
        {
            if (descending)
            {
                ApplyOrderByDescending(u => u.LastName);
                ApplyOrderByDescending(u => u.FirstName);
            }
            else
            {
                ApplyOrderBy(u => u.LastName);
                ApplyOrderBy(u => u.FirstName);
            }
        }
    }
} 