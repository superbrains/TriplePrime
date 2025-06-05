using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Models;
using TriplePrime.Data.Services;

namespace TriplePrime.API.Controllers
{
  
    public class FoodPackController : BaseController
    {
        private readonly FoodPackService _foodPackService;

        public FoodPackController(FoodPackService foodPackService)
        {
            _foodPackService = foodPackService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllFoodPacks()
        {
            try
            {
                var foodPacks = await _foodPackService.GetAllFoodPacksAsync();
                return HandleResponse(ApiResponse<IReadOnlyList<FoodPack>>.SuccessResponse(foodPacks));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFoodPackById(int id)
        {
            try
            {
                var foodPack = await _foodPackService.GetFoodPackByIdAsync(id);
                if (foodPack == null)
                {
                    return HandleResponse(ApiResponse<FoodPack>.ErrorResponse("Food pack not found"));
                }
                return HandleResponse(ApiResponse<FoodPack>.SuccessResponse(foodPack));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateFoodPack([FromBody] CreateFoodPackRequest request)
        {
            try
            {
                var createdFoodPack = await _foodPackService.CreateFoodPackAsync(request);
                return HandleResponse(ApiResponse<FoodPack>.SuccessResponse(createdFoodPack, "Food pack created successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateFoodPack(int id, [FromBody] UpdateFoodPackRequest request)
        {
            try
            {
                var updatedFoodPack = await _foodPackService.UpdateFoodPackAsync(id, request);
                return HandleResponse(ApiResponse<FoodPack>.SuccessResponse(updatedFoodPack, "Food pack updated successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteFoodPack(int id)
        {
            try
            {
                await _foodPackService.DeleteFoodPackAsync(id);
                return HandleResponse(ApiResponse.SuccessResponse("Food pack deleted successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchFoodPacks(
            [FromQuery] string searchQuery = null,
            [FromQuery] string category = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? available = null,
            [FromQuery] bool? featured = null,
            [FromQuery] string sortBy = "name",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var (items, totalCount) = await _foodPackService.SearchFoodPacksAsync(
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

                return HandleResponse(ApiResponse<SearchResult<FoodPack>>.SuccessResponse(
                    new SearchResult<FoodPack>
                    {
                        Items = items,
                        TotalCount = totalCount,
                        Page = page,
                        PageSize = pageSize
                    }
                ));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("adjust-prices")]
        [Authorize]
        public async Task<IActionResult> AdjustPrices([FromQuery] decimal percentageIncrease)
        {
            try
            {
                var updatedPacks = await _foodPackService.AdjustPricesAsync(percentageIncrease);
                return HandleResponse(ApiResponse<IReadOnlyList<FoodPack>>.SuccessResponse(
                    updatedPacks,
                    $"Prices adjusted by {percentageIncrease}% successfully"
                ));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }
    }

    public class SearchResult<T>
    {
        public IReadOnlyList<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
} 