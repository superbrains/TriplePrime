using Microsoft.AspNetCore.Mvc;
using TriplePrime.Data.Models;
using System;
using System.Collections.Generic;

namespace TriplePrime.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleResponse<T>(ApiResponse<T> response)
        {
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        protected IActionResult HandleResponse(ApiResponse response)
        {
            if (response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        protected IActionResult HandleException(System.Exception ex)
        {
            var response = ApiResponse.ErrorResponse(
                "An error occurred while processing your request",
                new List<string> { ex.Message }
            );
            return StatusCode(500, response);
        }
    }
} 