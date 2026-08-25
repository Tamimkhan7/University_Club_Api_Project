using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.User;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.Auth;
namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
            => Ok(await _authService.RegisterAsync(dto));

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
            => Ok(await _authService.LoginAsync(dto));

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
            => Ok(await _authService.VerifyEmailAsync(token));

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
            => Ok(await _authService.ForgotPasswordAsync(dto));

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
            => Ok(await _authService.ResetPasswordAsync(dto));

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenDto dto)
            => Ok(await _authService.RefreshTokenAsync(dto));

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = UserHelper.GetUserId(User);
            return Ok(await _authService.GetMeAsync(userId));
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = UserHelper.GetUserId(User);
            return Ok(await _authService.ChangePasswordAsync(userId, dto));
        }
    }
}