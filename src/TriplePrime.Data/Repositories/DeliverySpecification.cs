using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;

namespace TriplePrime.Data.Repositories
{
    public class DeliverySpecification : BaseSpecification<Delivery>, ISpecification<Delivery>
    {
        public DeliverySpecification(int id)
            : base(d => d.Id == id)
        {
            AddInclude(d => d.User);
            AddInclude(d => d.Driver);
            AddInclude(d => d.DeliveryAddress);
            AddInclude(d => d.FoodPack);
        }

        public DeliverySpecification()
            : base()
        {
            AddInclude(d => d.User);
            AddInclude(d => d.Driver);
            AddInclude(d => d.DeliveryAddress);
            AddInclude(d => d.FoodPack);
        }

        public void ApplyUserFilter(string userId)
        {
            AddCriteria(d => d.UserId == userId);
        }

        public void ApplyStatusFilter(DeliveryStatus status)
        {
            AddCriteria(d => d.Status == status);
        }

        public void ApplyDateRangeFilter(DateTime startDate, DateTime endDate)
        {
            AddCriteria(d => d.ScheduledDate >= startDate && d.ScheduledDate <= endDate);
        }

        public void ApplyAddressFilter(int addressId)
        {
            AddCriteria(d => d.DeliveryAddressId == addressId);
        }

        public void ApplyDriverFilter(string driverId)
        {
            AddCriteria(d => d.DriverId == driverId);
        }

        public void ApplyFoodPackFilter(int foodPackId)
        {
            AddCriteria(d => d.FoodPackId == foodPackId);
        }

        public void ApplyOrderByDeliveryDate(bool descending = false)
        {
            if (descending)
            {
                ApplyOrderByDescending(d => d.ScheduledDate);
            }
            else
            {
                ApplyOrderBy(d => d.ScheduledDate);
            }
        }

        public void ApplyOrderByStatus()
        {
            ApplyOrderBy(d => d.Status);
        }

        public void ApplyOrderByAddress()
        {
            ApplyOrderBy(d => d.DeliveryAddress.Address);
        }
    }
} 