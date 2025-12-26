using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YouTubeKurator.Api.Services;

namespace YouTubeKurator.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] SendAuthCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Email))
            {
                return BadRequest(new { error = "Email is required" });
            }

            var (success, message) = await _authService.SendAuthCodeAsync(request.Email);

            if (success)
            {
                return Ok(new { success = true, message = message });
            }

            return BadRequest(new { success = false, message = message });
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrWhiteSpace(request?.Code))
            {
                return BadRequest(new { error = "Email and code are required" });
            }

            var (success, tokenOrError, userId) = await _authService.VerifyCodeAsync(request.Email, request.Code);

            if (success)
            {
                return Ok(new {
                    success = true,
                    token = tokenOrError,
                    userId = userId,
                    email = request.Email
                });
            }

            return BadRequest(new { success = false, message = tokenOrError });
        }
    }

    public class SendAuthCodeRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }

    public class VerifyCodeRequest
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 6)]
        public required string Code { get; set; }
    }
}
