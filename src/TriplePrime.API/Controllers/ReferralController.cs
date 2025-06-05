//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using TriplePrime.Data.Entities;
//using TriplePrime.Data.Models;
//using TriplePrime.Data.Services;

//namespace TriplePrime.API.Controllers
//{
//    [Authorize]
//    public class ReferralController : BaseController
//    {
//        private readonly ReferralService _referralService;

//        public ReferralController(ReferralService referralService)
//        {
//            _referralService = referralService;
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateReferral([FromBody] CreateReferralRequest request)
//        {
//            try
//            {
//                var referral = new Referral
//                {
//                    MarketerId = request.MarketerId,
//                    ReferredUserId = request.ReferredUserId,
//                    ReferralDate = DateTime.UtcNow,
//                    Status = ReferralStatus.Pending
//                };
                
//                var createdReferral = await _referralService.CreateReferralAsync(referral);
//                return HandleResponse(ApiResponse<Referral>.SuccessResponse(createdReferral));
//            }
//            catch (System.Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetReferralById(int id)
//        {
//            try
//            {
//                var referral = await _referralService.GetReferralByIdAsync(id);
//                if (referral == null)
//                {
//                    return HandleResponse(ApiResponse<Referral>.ErrorResponse("Referral not found"));
//                }
//                return HandleResponse(ApiResponse<Referral>.SuccessResponse(referral));
//            }
//            catch (System.Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        [HttpGet("marketer/{marketerId}")]
//        public async Task<IActionResult> GetReferralsByMarketer(string marketerId)
//        {
//            try
//            {
//                if (!int.TryParse(marketerId, out int marketerIdInt))
//                {
//                    return HandleResponse(ApiResponse<IEnumerable<Referral>>.ErrorResponse("Invalid marketer ID format"));
//                }
                
//                var referrals = await _referralService.GetReferralsByMarketerAsync(marketerIdInt);
//                return HandleResponse(ApiResponse<IEnumerable<Referral>>.SuccessResponse(referrals));
//            }
//            catch (System.Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        [HttpGet("user/{userId}")]
//        public async Task<IActionResult> GetReferralsByUser(string userId)
//        {
//            try
//            {
//                var referrals = await _referralService.GetReferralsByUserAsync(userId);
//                return HandleResponse(ApiResponse<IEnumerable<Referral>>.SuccessResponse(referrals));
//            }
//            catch (System.Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        [HttpGet("code/{code}")]
//        public async Task<IActionResult> GetReferralByCode(string code)
//        {
//            try
//            {
//                var referral = await _referralService.GetReferralByCodeAsync(code);
//                if (referral == null)
//                {
//                    return HandleResponse(ApiResponse<Referral>.ErrorResponse("Referral not found"));
//                }
//                return HandleResponse(ApiResponse<Referral>.SuccessResponse(referral));
//            }
//            catch (System.Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        [HttpGet("generate-code/{marketerId}")]
//        public async Task<IActionResult> GenerateReferralCode(int marketerId)
//        {
//            try
//            {
//                var code = await _referralService.GenerateReferralCodeAsync(marketerId);
//                return HandleResponse(ApiResponse<string>.SuccessResponse(code));
//            }
//            catch (System.Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        [HttpPost("validate-code")]
//        public async Task<IActionResult> ValidateReferralCode([FromBody] ValidateReferralCodeRequest request)
//        {
//            try
//            {
//                var isValid = await _referralService.ValidateReferralCodeAsync(request.Code);
//                return HandleResponse(ApiResponse<bool>.SuccessResponse(isValid));
//            }
//            catch (System.Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }

//        [HttpPost("process")]
//        public async Task<IActionResult> ProcessReferral([FromBody] ProcessReferralRequest request)
//        {
//            try
//            {
//                var result = await _referralService.ProcessReferralAsync(request.Code, request.UserId);
//                if (!result)
//                {
//                    return HandleResponse(ApiResponse.ErrorResponse("Failed to process referral"));
//                }
//                return HandleResponse(ApiResponse.SuccessResponse("Referral processed successfully"));
//            }
//            catch (System.Exception ex)
//            {
//                return HandleException(ex);
//            }
//        }
//    }

//    public class CreateReferralRequest
//    {
//        public int MarketerId { get; set; }
//        public string ReferredUserId { get; set; }
//    }

//    public class ValidateReferralCodeRequest
//    {
//        public string Code { get; set; }
//    }

//    public class ProcessReferralRequest
//    {
//        public string Code { get; set; }
//        public string UserId { get; set; }
//    }
//} 