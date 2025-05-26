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
    [Authorize]
    public class FoodPackController : BaseController
    {
        private readonly FoodPackService _foodPackService;

        public FoodPackController(FoodPackService foodPackService)
        {
            _foodPackService = foodPackService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFoodPacks()
        {
            try
            {
                var foodPacks = await _foodPackService.GetFoodPacksByDateRangeAsync(DateTime.MinValue, DateTime.MaxValue);
                return HandleResponse(ApiResponse<IReadOnlyList<FoodPack>>.SuccessResponse(foodPacks));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("{id}")]
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
        public async Task<IActionResult> CreateFoodPack([FromBody] FoodPack foodPack)
        {
            try
            {
                var createdFoodPack = await _foodPackService.CreateFoodPackAsync(foodPack);
                return HandleResponse(ApiResponse<FoodPack>.SuccessResponse(createdFoodPack, "Food pack created successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFoodPack(int id, [FromBody] FoodPack foodPack)
        {
            try
            {
                foodPack.Id = id;
                await _foodPackService.UpdateFoodPackAsync(foodPack);
                return HandleResponse(ApiResponse.SuccessResponse("Food pack updated successfully"));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpDelete("{id}")]
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
        public async Task<IActionResult> SearchFoodPacks([FromQuery] string searchTerm)
        {
            try
            {
                var foodPacks = await _foodPackService.SearchFoodPacksAsync(searchTerm);
                return HandleResponse(ApiResponse<IReadOnlyList<FoodPack>>.SuccessResponse(foodPacks));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetFoodPacksByCategory(string category)
        {
            try
            {
                var foodPacks = await _foodPackService.GetFoodPacksByCategoryAsync(category);
                return HandleResponse(ApiResponse<IReadOnlyList<FoodPack>>.SuccessResponse(foodPacks));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("price-range")]
        public async Task<IActionResult> GetFoodPacksByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
        {
            try
            {
                var foodPacks = await _foodPackService.GetFoodPacksByPriceRangeAsync(minPrice, maxPrice);
                return HandleResponse(ApiResponse<IReadOnlyList<FoodPack>>.SuccessResponse(foodPacks));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularFoodPacks()
        {
            try
            {
                var foodPacks = await _foodPackService.GetFoodPacksByPopularityAsync();
                return HandleResponse(ApiResponse<IReadOnlyList<FoodPack>>.SuccessResponse(foodPacks));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }

        [HttpGet("top-rated")]
        public async Task<IActionResult> GetTopRatedFoodPacks()
        {
            try
            {
                var foodPacks = await _foodPackService.GetFoodPacksByRatingAsync();
                return HandleResponse(ApiResponse<IReadOnlyList<FoodPack>>.SuccessResponse(foodPacks));
            }
            catch (System.Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
} 