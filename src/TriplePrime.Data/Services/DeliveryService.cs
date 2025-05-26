using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Services
{
    public class DeliveryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Delivery> _deliveryRepository;
        private readonly IGenericRepository<DeliveryAddress> _addressRepository;
        private readonly IGenericRepository<FoodPack> _foodPackRepository;

        public DeliveryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _deliveryRepository = unitOfWork.Repository<Delivery>();
            _addressRepository = unitOfWork.Repository<DeliveryAddress>();
            _foodPackRepository = unitOfWork.Repository<FoodPack>();
        }

        public async Task<Delivery> CreateDeliveryAsync(Delivery delivery)
        {
            if (delivery == null)
                throw new ArgumentNullException(nameof(delivery));

            // Validate delivery address
            var address = await _addressRepository.GetByIdAsync(delivery.DeliveryAddressId);
            if (address == null)
                throw new ArgumentException("Invalid delivery address");

            // Validate food pack
            var foodPack = await _foodPackRepository.GetByIdAsync(delivery.FoodPackId);
            if (foodPack == null)
                throw new ArgumentException("Invalid food pack");

            delivery.Status = DeliveryStatus.Pending;
            delivery.CreatedAt = DateTime.UtcNow;

            await _deliveryRepository.AddAsync(delivery);
            await _unitOfWork.SaveChangesAsync();

            return delivery;
        }

        public async Task<Delivery> GetDeliveryByIdAsync(int id)
        {
            var specification = new DeliverySpecification(id);
            return await _deliveryRepository.GetEntityWithSpec(specification);
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByUserAsync(string userId)
        {
            var specification = new DeliverySpecification();
            specification.ApplyUserFilter(userId);
            specification.ApplyOrderByDeliveryDate(true);
            return await _deliveryRepository.ListAsync(specification);
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var specification = new DeliverySpecification();
            specification.ApplyDateRangeFilter(startDate, endDate);
            specification.ApplyOrderByDeliveryDate();
            return await _deliveryRepository.ListAsync(specification);
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByStatusAsync(DeliveryStatus status)
        {
            var specification = new DeliverySpecification();
            specification.ApplyStatusFilter(status);
            specification.ApplyOrderByDeliveryDate();
            return await _deliveryRepository.ListAsync(specification);
        }

        public async Task<Delivery> UpdateDeliveryStatusAsync(int id, DeliveryStatus status, string updatedBy)
        {
            var delivery = await GetDeliveryByIdAsync(id);
            if (delivery == null)
                throw new ArgumentException("Delivery not found");

            delivery.Status = status;
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.UpdatedBy = updatedBy;

            if (status == DeliveryStatus.Delivered)
                delivery.ActualDeliveryDate = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return delivery;
        }

        public async Task<Delivery> AssignDriverAsync(int id, string driverId)
        {
            var delivery = await GetDeliveryByIdAsync(id);
            if (delivery == null)
                throw new ArgumentException("Delivery not found");

            delivery.DriverId = driverId;
            delivery.Status = DeliveryStatus.Scheduled;
            delivery.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return delivery;
        }

        public async Task<Delivery> UpdateDeliveryAddressAsync(int id, int addressId)
        {
            var delivery = await GetDeliveryByIdAsync(id);
            if (delivery == null)
                throw new ArgumentException("Delivery not found");

            var address = await _addressRepository.GetByIdAsync(addressId);
            if (address == null)
                throw new ArgumentException("Invalid delivery address");

            delivery.DeliveryAddressId = addressId;
            delivery.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return delivery;
        }

        public async Task<Delivery> CancelDeliveryAsync(int id, string reason, string updatedBy)
        {
            var delivery = await GetDeliveryByIdAsync(id);
            if (delivery == null)
                throw new ArgumentException("Delivery not found");

            if (delivery.Status == DeliveryStatus.Delivered || delivery.Status == DeliveryStatus.Cancelled)
                throw new InvalidOperationException("Cannot cancel a delivered or already cancelled delivery");

            delivery.Status = DeliveryStatus.Cancelled;
            delivery.CancellationReason = reason;
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.UpdatedBy = updatedBy;

            await _unitOfWork.SaveChangesAsync();
            return delivery;
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByDriverAsync(string driverId)
        {
            var specification = new DeliverySpecification();
            specification.ApplyDriverFilter(driverId);
            specification.ApplyOrderByDeliveryDate(true);
            return await _deliveryRepository.ListAsync(specification);
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByAddressAsync(int addressId)
        {
            var specification = new DeliverySpecification();
            specification.ApplyAddressFilter(addressId);
            specification.ApplyOrderByDeliveryDate(true);
            return await _deliveryRepository.ListAsync(specification);
        }

        public async Task<IEnumerable<Delivery>> GetDeliveriesByFoodPackAsync(int foodPackId)
        {
            var specification = new DeliverySpecification();
            specification.ApplyFoodPackFilter(foodPackId);
            specification.ApplyOrderByDeliveryDate(true);
            return await _deliveryRepository.ListAsync(specification);
        }
    }
} 