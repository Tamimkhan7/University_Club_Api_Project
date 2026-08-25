using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.BadgeService;
namespace UniversityClubAPI.Controllers
{
    [Route("api/badges")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class BadgeController : ControllerBase
    {
        private readonly IBadgeService _badgeService;
        public BadgeController(IBadgeService badgeService)
        {
            _badgeService = badgeService;
        }
        [HttpGet("catalog")]
        public async Task<IActionResult> GetCatalog()
        {
            var result = await _badgeService.GetCatalogAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyBadges()
        {
            var result = await _badgeService.GetMyBadgesAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }

        [HttpGet("users/{userId:int}")]
        public async Task<IActionResult> GetUserBadges(int userId)
        {
            var result = await _badgeService.GetUserBadgesAsync(userId);
            return Ok(result);
        }

        [HttpPost("evaluate")]
        public async Task<IActionResult> Evaluate()
        {
            var result = await _badgeService.EvaluateAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }

        [HttpGet("clubs/{clubId:int}/leaderboard")]
        public async Task<IActionResult> GetClubLeaderboard(int clubId, [FromQuery] int count = 10)
        {
            var result = await _badgeService.GetClubLeaderboardAsync(clubId, count);
            return Ok(result);
        }

        [HttpPost("clubs/{clubId:int}/recalculate-top-contributor")]
        public async Task<IActionResult> RecalculateTopContributor(int clubId)
        {
            var result = await _badgeService.RecalculateTopContributorAsync(UserHelper.GetUserId(User), clubId);
            return Ok(result);
        }


        [HttpGet("progress")]
        public async Task<IActionResult> GetProgress()
        {
            var result = await _badgeService.GetProgressAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }


        [HttpGet("leaderboard/global")]
        public async Task<IActionResult> GetGlobalLeaderboard([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _badgeService.GetGlobalLeaderboardAsync(page, pageSize);
            return Ok(result);
        }


        [HttpGet("{badgeCode}/holders")]
        public async Task<IActionResult> GetBadgeHolders(string badgeCode, [FromQuery] int? clubId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _badgeService.GetBadgeHoldersAsync(badgeCode, clubId, page, pageSize);
            return Ok(result);
        }

        [HttpDelete("users/{userId:int}/{badgeCode}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RevokeBadge(int userId, string badgeCode, [FromQuery] int? clubId = null)
        {
            var result = await _badgeService.RevokeBadgeAsync(UserHelper.GetUserId(User), userId, badgeCode, clubId);
            return Ok(result);
        }
    }
}
