using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.LiveEvent;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.LiveEventService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/live-events")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class LiveEventController : ControllerBase
    {
        private readonly ILiveEventService _liveEventService;
        public LiveEventController(ILiveEventService liveEventService)
        {
            _liveEventService = liveEventService;
        }

        [HttpPost("{eventId:int}/start")]
        public async Task<IActionResult> Start(int eventId, [FromBody] StartLiveDto dto)
        {
            var result = await _liveEventService.StartLiveAsync(UserHelper.GetUserId(User), eventId, dto);
            return Ok(result);
        }

        [HttpPut("{eventId:int}/end")]
        public async Task<IActionResult> End(int eventId)
        {
            var result = await _liveEventService.EndLiveAsync(UserHelper.GetUserId(User), eventId);
            return Ok(result);
        }

        [HttpGet("{eventId:int}/status")]
        public async Task<IActionResult> GetStatus(int eventId)
        {
            var result = await _liveEventService.GetStatusAsync(UserHelper.GetUserId(User), eventId);
            return Ok(result);
        }

        [HttpGet("{eventId:int}/chat")]
        public async Task<IActionResult> GetChatHistory(int eventId, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _liveEventService.GetChatHistoryAsync(UserHelper.GetUserId(User), eventId, pagination);
            return Ok(result);
        }

        [HttpGet("{eventId:int}/viewers")]
        public async Task<IActionResult> GetActiveViewers(int eventId)
        {
            var result = await _liveEventService.GetActiveViewersAsync(UserHelper.GetUserId(User), eventId);
            return Ok(result);
        }


        [HttpPut("{eventId:int}/moderation/{userId:int}/mute")]
        public async Task<IActionResult> MuteUser(int eventId, int userId, [FromBody] MuteRequestDto dto)
        {
            var result = await _liveEventService.MuteUserAsync(UserHelper.GetUserId(User), eventId, userId, dto);
            return Ok(result);
        }

        [HttpPost("{eventId:int}/moderation/{userId:int}/kick")]
        public async Task<IActionResult> KickUser(int eventId, int userId, [FromBody] KickRequestDto dto)
        {
            var result = await _liveEventService.KickUserAsync(UserHelper.GetUserId(User), eventId, userId, dto);
            return Ok(result);
        }

        [HttpPost("{eventId:int}/moderation/{userId:int}/unban")]
        public async Task<IActionResult> UnbanUser(int eventId, int userId)
        {
            var result = await _liveEventService.UnbanUserAsync(UserHelper.GetUserId(User), eventId, userId);
            return Ok(result);
        }
    }
}