using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.LeaderboardService;
namespace UniversityClubAPI.Controllers
{
    [Route("api/leaderboard")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard(
            [FromQuery] LeaderboardCategory category = LeaderboardCategory.Overall,
            [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.AllTime,
            [FromQuery] int count = 20)
        {
            var result = await _leaderboardService.GetLeaderboardAsync(
                UserHelper.GetUserId(User), category, period, count);
            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyRank(
            [FromQuery] LeaderboardCategory category = LeaderboardCategory.Overall,
            [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.AllTime)
        {
            var result = await _leaderboardService.GetMyLeaderboardEntryAsync(
                UserHelper.GetUserId(User), category, period);
            return Ok(result);
        }

        [HttpGet("user/{userId:int}")]
        [Authorize(Policy = "ModeratorOnly")]
        public async Task<IActionResult> GetUserRank(
            int userId,
            [FromQuery] LeaderboardCategory category = LeaderboardCategory.Overall,
            [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.AllTime)
        {
            var result = await _leaderboardService.GetUserLeaderboardEntryAsync(
                UserHelper.GetUserId(User), userId, category, period);
            return Ok(result);
        }

        [HttpGet("insight")]
        public async Task<IActionResult> GetLeaderboardInsight(
            [FromQuery] LeaderboardCategory category = LeaderboardCategory.Overall,
            [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.AllTime)
        {
            var result = await _leaderboardService.GetLeaderboardInsightAsync(
                UserHelper.GetUserId(User), category, period);
            return Ok(result);
        }
    }
}