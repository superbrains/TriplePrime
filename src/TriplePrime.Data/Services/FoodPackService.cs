using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Interfaces;
using TriplePrime.Data.Repositories;
using TriplePrime.Data.Models;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace TriplePrime.Data.Services
{
    public class FoodPackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGlobalPricingService _globalPricingService;
        private readonly string _imageBasePath;
        private readonly string _apiBaseUrl;

        public FoodPackService(IUnitOfWork unitOfWork, string imageBasePath, string apiBaseUrl, IGlobalPricingService globalPricingService = null)
        {
            _unitOfWork = unitOfWork;
            _imageBasePath = imageBasePath;
            _apiBaseUrl = apiBaseUrl.TrimEnd('/');
            _globalPricingService = globalPricingService;
        }

        /// <summary>
        /// Resolves the effective interest rate for a food pack's pricing tier.
        /// If the food pack has an override (UseGlobalRate = false), returns that rate.
        /// Otherwise, returns the global rate for the duration.
        /// </summary>
        public async Task<decimal> ResolveEffectiveRateAsync(int foodPackId, int durationMonths)
        {
            // Check for food pack specific pricing
            var pricing = await _unitOfWork.Repository<FoodPackPricing>()
                .GetEntityWithSpec(new FoodPackPricingSpecification(foodPackId, durationMonths));

            if (pricing != null && !pricing.UseGlobalRate)
            {
                // Food pack has an override - use it
                return pricing.InterestRate;
            }

            // Fall back to global rate
            if (_globalPricingService != null)
            {
                return await _globalPricingService.GetGlobalRateAsync(durationMonths);
            }

            // If no global service, use the pricing rate or default to 0
            return pricing?.InterestRate ?? 0m;
        }

        private string GetFullImageUrl(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            return $"{_apiBaseUrl}{relativePath}";
        }

        public async Task<FoodPack> CreateFoodPackAsync(CreateFoodPackRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Save image
                string imageUrl = await SaveFoodPackImage(request.ImageBase64);

                // Create food pack
                var foodPack = new FoodPack
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    OriginalPrice = request.OriginalPrice,
                    Savings = request.OriginalPrice - request.Price,
                    Available = request.Available,
                    Featured = request.Featured,
                    ImageUrl = imageUrl,
                    Inventory = request.Inventory,
                    Duration = request.Duration,
                    Category = request.Category,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };

                await _unitOfWork.Repository<FoodPack>().AddAsync(foodPack);
                await _unitOfWork.SaveChangesAsync();

                // Add items
                foreach (var item in request.Items)
                {
                    await _unitOfWork.Repository<FoodPackItem>().AddAsync(new FoodPackItem
                    {
                        FoodPackId = foodPack.Id,
                        Item = item,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Set full image URL before returning

                foodPack.ImageUrl = GetFullImageUrl(foodPack.ImageUrl);
                return foodPack;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<FoodPack> UpdateFoodPackAsync(int id, UpdateFoodPackRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var foodPack = await _unitOfWork.Repository<FoodPack>().GetByIdAsync(id);
                if (foodPack == null)
                    throw new ArgumentException($"Food pack with ID {id} not found");

                // Update basic info
                foodPack.Name = request.Name ?? foodPack.Name;
                foodPack.Description = request.Description ?? foodPack.Description;
                foodPack.Price = request.Price ?? foodPack.Price;
                foodPack.OriginalPrice = request.OriginalPrice ?? foodPack.OriginalPrice;
                foodPack.Savings = (request.OriginalPrice ?? foodPack.OriginalPrice) - (request.Price ?? foodPack.Price);
                foodPack.Available = request.Available ?? foodPack.Available;
                foodPack.Featured = request.Featured ?? foodPack.Featured;
                foodPack.Inventory = request.Inventory ?? foodPack.Inventory;
                foodPack.Duration = request.Duration ?? foodPack.Duration;
                foodPack.Category = request.Category ?? foodPack.Category;
                foodPack.UpdatedAt = DateTime.UtcNow;
                foodPack.UpdatedBy = request.UpdatedBy;

                // Update image if provided
                if (!string.IsNullOrEmpty(request.ImageBase64))
                {
                    foodPack.ImageUrl = await SaveFoodPackImage(request.ImageBase64);
                }

                _unitOfWork.Repository<FoodPack>().Update(foodPack);

                // Update items if provided
                if (request.Items != null)
                {
                    // Remove existing items
                    var existingItems = await _unitOfWork.Repository<FoodPackItem>()
                        .ListAsync(new FoodPackItemSpecification(id));
                    foreach (var item in existingItems)
                    {
                        _unitOfWork.Repository<FoodPackItem>().Remove(item);
                    }

                    // Add new items
                    foreach (var item in request.Items)
                    {
                        await _unitOfWork.Repository<FoodPackItem>().AddAsync(new FoodPackItem
                        {
                            FoodPackId = foodPack.Id,
                            Item = item,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Set full image URL before returning
                foodPack.ImageUrl = GetFullImageUrl(foodPack.ImageUrl);
                return foodPack;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task DeleteFoodPackAsync(int id)
        {
            var foodPack = await _unitOfWork.Repository<FoodPack>().GetByIdAsync(id);
            if (foodPack == null)
                throw new ArgumentException($"Food pack with ID {id} not found");

            // Delete associated image
            if (!string.IsNullOrEmpty(foodPack.ImageUrl))
            {
                var imagePath = Path.Combine(_imageBasePath, foodPack.ImageUrl.TrimStart('/'));
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }

            _unitOfWork.Repository<FoodPack>().Remove(foodPack);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<FoodPack>> GetAllFoodPacksAsync()
        {
            var spec = new FoodPackSpecification();
            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            var result = foodPacks.ToList();
            
            // Set full image URLs
            foreach (var pack in result)
            {
                pack.ImageUrl = GetFullImageUrl(pack.ImageUrl);
            }
            
            return result;
        }

        public async Task<FoodPack> GetFoodPackByIdAsync(int id)
        {
            var spec = new FoodPackSpecification(id);
            var foodPack = await _unitOfWork.Repository<FoodPack>().GetEntityWithSpec(spec);
            
            if (foodPack != null)
            {
                foodPack.ImageUrl = GetFullImageUrl(foodPack.ImageUrl);
            }
            
            return foodPack;
        }

        public async Task<(IReadOnlyList<FoodPack> Items, int TotalCount)> SearchFoodPacksAsync(
            string searchQuery = null,
            string category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? available = null,
            bool? featured = null,
            string sortBy = "name",
            string sortOrder = "asc",
            int page = 1,
            int pageSize = 10)
        {
            var spec = new FoodPackSpecification(
                searchQuery,
                category,
                minPrice,
                maxPrice,
                available,
                featured,
                sortBy,
                sortOrder,
                page,
                pageSize
            );

            var items = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            var count = await _unitOfWork.Repository<FoodPack>().CountAsync(spec);

            var result = items.ToList();
            
            // Set full image URLs
            foreach (var pack in result)
            {
                pack.ImageUrl = GetFullImageUrl(pack.ImageUrl);
            }

            return (result, count);
        }

        public async Task<IReadOnlyList<FoodPack>> AdjustPricesAsync(decimal percentageIncrease)
        {
            var spec = new FoodPackSpecification();
            var foodPacks = await _unitOfWork.Repository<FoodPack>().ListAsync(spec);
            var updatedPacks = new List<FoodPack>();

            foreach (var pack in foodPacks)
            {
                var increaseFactor = 1 + (percentageIncrease / 100);
                pack.Price = Math.Round(pack.Price * increaseFactor, 2);
                pack.OriginalPrice = Math.Round(pack.OriginalPrice * increaseFactor, 2);
                pack.Savings = pack.OriginalPrice - pack.Price;
                pack.UpdatedAt = DateTime.UtcNow;
                pack.UpdatedBy = "System";

                _unitOfWork.Repository<FoodPack>().Update(pack);
                updatedPacks.Add(pack);
            }

            await _unitOfWork.SaveChangesAsync();

            // Set full image URLs
            foreach (var pack in updatedPacks)
            {
                pack.ImageUrl = GetFullImageUrl(pack.ImageUrl);
            }

            return updatedPacks;
        }

        // Pricing Management Methods
        public async Task<FoodPackPricing> CreatePricingAsync(CreateFoodPackPricingDto request)
        {
            // Check if pricing already exists for this food pack and duration
            var existingPricing = await _unitOfWork.Repository<FoodPackPricing>()
                .GetEntityWithSpec(new FoodPackPricingSpecification(request.FoodPackId, request.DurationMonths));

            if (existingPricing != null)
                throw new InvalidOperationException($"Pricing for {request.DurationMonths} month(s) already exists for this food pack");

            var pricing = new FoodPackPricing
            {
                FoodPackId = request.FoodPackId,
                DurationMonths = request.DurationMonths,
                InterestRate = request.InterestRate,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FoodPackPricing>().AddAsync(pricing);
            await _unitOfWork.SaveChangesAsync();

            return pricing;
        }

        public async Task<List<FoodPackPricing>> CreateBulkPricingAsync(BulkCreateFoodPackPricingDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var foodPack = await _unitOfWork.Repository<FoodPack>().GetByIdAsync(request.FoodPackId);
                if (foodPack == null)
                    throw new ArgumentException($"Food pack with ID {request.FoodPackId} not found");

                // Remove existing pricing
                var existingPricings = await _unitOfWork.Repository<FoodPackPricing>()
                    .ListAsync(new FoodPackPricingSpecification(request.FoodPackId));

                foreach (var existing in existingPricings)
                {
                    _unitOfWork.Repository<FoodPackPricing>().Remove(existing);
                }

                // Create new pricing tiers
                var createdPricings = new List<FoodPackPricing>();
                foreach (var tier in request.PricingTiers)
                {
                    var pricing = new FoodPackPricing
                    {
                        FoodPackId = request.FoodPackId,
                        DurationMonths = tier.DurationMonths,
                        InterestRate = tier.InterestRate,
                        UseGlobalRate = tier.UseGlobalRate,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _unitOfWork.Repository<FoodPackPricing>().AddAsync(pricing);
                    createdPricings.Add(pricing);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return createdPricings;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<FoodPackPricing> UpdatePricingAsync(int pricingId, UpdateFoodPackPricingDto request)
        {
            var pricing = await _unitOfWork.Repository<FoodPackPricing>().GetByIdAsync(pricingId);
            if (pricing == null)
                throw new ArgumentException($"Pricing with ID {pricingId} not found");

            pricing.InterestRate = request.InterestRate;
            pricing.UseGlobalRate = request.UseGlobalRate;
            pricing.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<FoodPackPricing>().Update(pricing);
            await _unitOfWork.SaveChangesAsync();

            return pricing;
        }

        public async Task DeletePricingAsync(int pricingId)
        {
            var pricing = await _unitOfWork.Repository<FoodPackPricing>().GetByIdAsync(pricingId);
            if (pricing == null)
                throw new ArgumentException($"Pricing with ID {pricingId} not found");

            _unitOfWork.Repository<FoodPackPricing>().Remove(pricing);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<FoodPackPricingDto>> GetPricingsByFoodPackIdAsync(int foodPackId)
        {
            var foodPack = await _unitOfWork.Repository<FoodPack>().GetByIdAsync(foodPackId);
            if (foodPack == null)
                throw new ArgumentException($"Food pack with ID {foodPackId} not found");

            var pricings = await _unitOfWork.Repository<FoodPackPricing>()
                .ListAsync(new FoodPackPricingSpecification(foodPackId));

            var basePrice = foodPack.Price;
            var result = new List<FoodPackPricingDto>();

            foreach (var p in pricings.OrderBy(x => x.DurationMonths))
            {
                // Resolve effective rate: use global rate if UseGlobalRate is true
                var effectiveRate = p.InterestRate;
                if (p.UseGlobalRate && _globalPricingService != null)
                {
                    effectiveRate = await _globalPricingService.GetGlobalRateAsync(p.DurationMonths);
                }

                var totalPrice = basePrice * (1 + effectiveRate);
                var dailyPayment = totalPrice / (p.DurationMonths * 30);

                result.Add(new FoodPackPricingDto
                {
                    Id = p.Id,
                    FoodPackId = p.FoodPackId,
                    DurationMonths = p.DurationMonths,
                    InterestRate = effectiveRate, // Return the effective rate, not the stored rate
                    UseGlobalRate = p.UseGlobalRate,
                    TotalPrice = totalPrice,
                    DailyPaymentAmount = dailyPayment,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                });
            }

            return result;
        }

        public async Task<FoodPackPricingDto> GetPricingByDurationAsync(int foodPackId, int durationMonths)
        {
            var foodPack = await _unitOfWork.Repository<FoodPack>().GetByIdAsync(foodPackId);
            if (foodPack == null)
                throw new ArgumentException($"Food pack with ID {foodPackId} not found");

            var pricing = await _unitOfWork.Repository<FoodPackPricing>()
                .GetEntityWithSpec(new FoodPackPricingSpecification(foodPackId, durationMonths));

            if (pricing == null)
                throw new ArgumentException($"Pricing for {durationMonths} month(s) not found for this food pack");

            var basePrice = foodPack.Price;

            // Resolve effective rate: use global rate if UseGlobalRate is true
            var effectiveRate = pricing.InterestRate;
            if (pricing.UseGlobalRate && _globalPricingService != null)
            {
                effectiveRate = await _globalPricingService.GetGlobalRateAsync(durationMonths);
            }

            var totalPrice = basePrice * (1 + effectiveRate);
            var dailyPayment = totalPrice / (durationMonths * 30);

            return new FoodPackPricingDto
            {
                Id = pricing.Id,
                FoodPackId = pricing.FoodPackId,
                DurationMonths = pricing.DurationMonths,
                InterestRate = effectiveRate, // Return the effective rate, not the stored rate
                UseGlobalRate = pricing.UseGlobalRate,
                TotalPrice = totalPrice,
                DailyPaymentAmount = dailyPayment,
                CreatedAt = pricing.CreatedAt,
                UpdatedAt = pricing.UpdatedAt
            };
        }

        public async Task<FoodPackWithPricingDto> GetFoodPackWithPricingAsync(int foodPackId)
        {
            var foodPack = await _unitOfWork.Repository<FoodPack>().GetByIdAsync(foodPackId);
            if (foodPack == null)
                throw new ArgumentException($"Food pack with ID {foodPackId} not found");

            var pricings = await GetPricingsByFoodPackIdAsync(foodPackId);

            return new FoodPackWithPricingDto
            {
                Id = foodPack.Id,
                Name = foodPack.Name,
                Description = foodPack.Description,
                BasePrice = foodPack.Price,
                OriginalPrice = foodPack.OriginalPrice,
                Savings = foodPack.Savings,
                Available = foodPack.Available,
                Featured = foodPack.Featured,
                ImageUrl = GetFullImageUrl(foodPack.ImageUrl),
                Inventory = foodPack.Inventory,
                Category = foodPack.Category,
                Pricings = pricings
            };
        }

        private async Task<string> SaveFoodPackImage(string base64Image)
        {
            if (string.IsNullOrEmpty(base64Image))
                return null;

            // Remove data:image/jpeg;base64, prefix
            var base64Data = base64Image.Substring(base64Image.IndexOf(",") + 1);
            
            // Convert base64 to bytes
            var imageBytes = Convert.FromBase64String(base64Data);
            
            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(_imageBasePath, "foodpacks", fileName);
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            
            // Save file
            await File.WriteAllBytesAsync(filePath, imageBytes);
            
            // Return relative URL
            return $"/images/foodpacks/{fileName}";
        }
    }

    public class CreateFoodPackRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal OriginalPrice { get; set; }
        public bool Available { get; set; }
        public bool Featured { get; set; }
        public string? ImageBase64 { get; set; }
        public int Inventory { get; set; }
        public List<string> Items { get; set; }
        public int Duration { get; set; }
        public string Category { get; set; }
        public string CreatedBy { get; set; }
    }

    public class UpdateFoodPackRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public bool? Available { get; set; }
        public bool? Featured { get; set; }
        public string? ImageBase64 { get; set; }
        public int? Inventory { get; set; }
        public List<string> Items { get; set; }
        public int? Duration { get; set; }
        public string Category { get; set; }
        public string UpdatedBy { get; set; }
    }
} 