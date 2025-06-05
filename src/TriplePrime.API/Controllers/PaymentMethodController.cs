using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using TriplePrime.Data.Entities;
using TriplePrime.Data.Services;

namespace TriplePrime.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentMethodController : ControllerBase
    {
        private readonly IPaymentMethodService _paymentMethodService;

        public PaymentMethodController(IPaymentMethodService paymentMethodService)
        {
            _paymentMethodService = paymentMethodService;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentPaymentMethod()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var paymentMethod = await _paymentMethodService.GetUserPaymentMethodAsync(userId);
            if (paymentMethod == null)
            {
                return NotFound();
            }

            return Ok(paymentMethod);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePaymentMethod([FromBody] PaymentMethod paymentMethod)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            paymentMethod.UserId = userId;
            var updatedMethod = await _paymentMethodService.UpdatePaymentMethodAsync(paymentMethod);
            return Ok(updatedMethod);
        }
    }
}