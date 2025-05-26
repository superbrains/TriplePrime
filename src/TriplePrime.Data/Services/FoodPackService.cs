using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;

namespace TriplePrime.Data.Services
{
    public class FoodPackService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FoodPackService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<FoodPack> CreateFoodPackAsync(FoodPack foodPack)
        {
            foodPack.CreatedAt = DateTime.UtcNow;
            foodPack.Status = FoodPackStatus.Active;

            await _unitOfWork.Repository<FoodPack>().AddAsync(foodPack);
            await _unitOfWork.SaveChangesAsync();

            return foodPack;
        }

        public async Task<FoodPack> GetFoodPackByIdAsync(int id)
        {
            var spec = new FoodPackSpecification(id);
            return await _unitOfWork.Repository<FoodPack>().GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<FoodPack>> GetFoodPacksByStatusAsync(FoodPackStatus status)
        {
            var spec = new FoodPackSpecification();
            spec.ApplyStatusFilter(status);
            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            return foodPacks.ToList();
        }

        public async Task<IReadOnlyList<FoodPack>> GetFoodPacksByCategoryAsync(string category)
        {
            var spec = new FoodPackSpecification();
            spec.ApplyCategoryFilter(category);
            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            return foodPacks.ToList();
        }

        public async Task<IReadOnlyList<FoodPack>> GetFoodPacksByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            var spec = new FoodPackSpecification();
            spec.ApplyPriceRangeFilter(minPrice, maxPrice);
            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            return foodPacks.ToList();
        }

        public async Task UpdateFoodPackAsync(FoodPack foodPack)
        {
            var existingPack = await GetFoodPackByIdAsync(foodPack.Id);
            if (existingPack == null)
            {
                throw new ArgumentException($"Food pack with ID {foodPack.Id} not found");
            }

            existingPack.Name = foodPack.Name;
            existingPack.Description = foodPack.Description;
            existingPack.Price = foodPack.Price;
            existingPack.Category = foodPack.Category;
            existingPack.Status = foodPack.Status;
            existingPack.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<FoodPack>().Update(existingPack);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateFoodPackStatusAsync(int id, FoodPackStatus status)
        {
            var foodPack = await GetFoodPackByIdAsync(id);
            if (foodPack == null)
            {
                throw new ArgumentException($"Food pack with ID {id} not found");
            }

            foodPack.Status = status;
            foodPack.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<FoodPack>().Update(foodPack);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteFoodPackAsync(int id)
        {
            var foodPack = await GetFoodPackByIdAsync(id);
            if (foodPack == null)
            {
                throw new ArgumentException($"Food pack with ID {id} not found");
            }

            _unitOfWork.Repository<FoodPack>().Remove(foodPack);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<FoodPack>> SearchFoodPacksAsync(string searchTerm)
        {
            var spec = new FoodPackSpecification();
            spec.ApplySearchFilter(searchTerm);
            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            return foodPacks.ToList();
        }

        public async Task<IReadOnlyList<FoodPack>> GetFoodPacksByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var spec = new FoodPackSpecification();
            spec.ApplyDateRangeFilter(startDate, endDate);
            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            return foodPacks.ToList();
        }

        public async Task<IReadOnlyList<FoodPack>> GetFoodPacksByPopularityAsync()
        {
            var spec = new FoodPackSpecification();
            spec.ApplyOrderByPopularity();
            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            return foodPacks.ToList();
        }

        public async Task<IReadOnlyList<FoodPack>> GetFoodPacksByRatingAsync()
        {
            var spec = new FoodPackSpecification();
            spec.ApplyOrderByRating();
            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            return foodPacks.ToList();
        }

        public async Task<IReadOnlyList<FoodPack>> GetUserFoodPacksAsync(string userId, bool includeItems = false)
        {
            var spec = new FoodPackSpecification(userId, includeItems);
            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            return foodPacks.ToList();
        }

        public async Task<(IReadOnlyList<FoodPack> Items, int TotalCount)> GetPagedFoodPacksAsync(
            int pageNumber,
            int pageSize,
            FoodPackStatus? status = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var spec = new FoodPackSpecification(true);
            
            if (status.HasValue)
            {
                spec.ApplyStatusFilter(status.Value);
            }

            if (minPrice.HasValue && maxPrice.HasValue)
            {
                spec.ApplyPriceRangeFilter(minPrice.Value, maxPrice.Value);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                spec.ApplyDeliveryDateRangeFilter(startDate.Value, endDate.Value);
            }

            spec.ApplyOrderByDeliveryDate();
            spec.ApplyPagination(pageNumber, pageSize);

            var items = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            var count = await _unitOfWork.Repository<FoodPack>().CountAsync(spec);

            return (items.ToList(), count);
        }

        public async Task<FoodPack> CreateFoodPackWithItemsAsync(FoodPack foodPack, List<FoodPackItem> items)
        {
            foodPack.CreatedAt = DateTime.UtcNow;
            foodPack.Status = FoodPackStatus.Active;

            // Add the food pack
            await _unitOfWork.Repository<FoodPack>().AddAsync(foodPack);
            await _unitOfWork.SaveChangesAsync();

            // Add the items
            foreach (var item in items)
            {
                item.FoodPackId = foodPack.Id;
                await _unitOfWork.Repository<FoodPackItem>().AddAsync(item);
            }

            await _unitOfWork.SaveChangesAsync();

            return foodPack;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var spec = new FoodPackSpecification();
            spec.ApplyDeliveryDateRangeFilter(startDate, endDate);
            spec.ApplyStatusFilter(FoodPackStatus.Active); // Changed from 'Delivered' as it's not in the enum

            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            return foodPacks.Sum(fp => fp.Price);
        }

        public async Task<IReadOnlyList<FoodPack>> GetUpcomingDeliveriesAsync(int daysAhead)
        {
            var spec = new FoodPackSpecification(true);
            spec.ApplyDeliveryDateRangeFilter(
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(daysAhead)
            );
            spec.ApplyStatusFilter(FoodPackStatus.Pending);
            spec.ApplyOrderByDeliveryDate();

            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            return foodPacks.ToList();
        }
    }
} 