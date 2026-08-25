using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.Dashboard;
namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> Summary()
            => Ok(await _dashboardService.GetSummaryAsync(UserHelper.GetUserId(User)));

        [HttpGet("recent-posts")]
        public async Task<IActionResult> RecentPosts()
            => Ok(await _dashboardService.GetRecentPostsAsync(UserHelper.GetUserId(User)));

        [HttpGet("recent-clubs")]
        public async Task<IActionResult> RecentClubs()
            => Ok(await _dashboardService.GetRecentClubsAsync(UserHelper.GetUserId(User)));

        [HttpGet("ai-insight")]
        public async Task<IActionResult> AiInsight()
            => Ok(await _dashboardService.GetAiInsightAsync(UserHelper.GetUserId(User)));

        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
            => Ok(await _dashboardService.GetStatsAsync(UserHelper.GetUserId(User)));

        [HttpGet("trending")]
        public async Task<IActionResult> Trending()
            => Ok(await _dashboardService.GetTrendingPostsAsync());
    }
}