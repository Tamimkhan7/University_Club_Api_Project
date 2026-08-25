using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Presence;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.PresenceService;
namespace UniversityClubAPI.Controllers
{
    [Route("api/presence")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class PresenceController : ControllerBase
    {
        private readonly IPresenceService _presenceService;
        public PresenceController(IPresenceService presenceService)
        {
            _presenceService = presenceService;
        }
        [HttpGet("users/{userId:int}")]
        public async Task<IActionResult> GetStatus(int userId)
        {
            var result = await _presenceService.GetStatusAsync(userId);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }
        [HttpPost("users/bulk")]
        public async Task<IActionResult> GetBulkStatus([FromBody] BulkPresenceRequestDto dto)
        {
            var result = await _presenceService.GetBulkStatusAsync(dto.UserIds);
            return Ok(result);
        }
        [HttpGet("online-following")]
        public async Task<IActionResult> GetOnlineFollowing([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _presenceService.GetOnlineFollowingAsync(UserHelper.GetUserId(User), pagination);
            return Ok(result);
        }
    }
}