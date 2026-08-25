using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.FeedService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class FeedController : ControllerBase
    {
        private readonly IFeedService _feedService;

        public FeedController(IFeedService feedService)
        {
            _feedService = feedService;
        }

        [HttpGet("global")]
        public async Task<IActionResult> Global([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _feedService.GetGlobalFeedAsync(UserHelper.GetUserId(User), pagination.Page, pagination.PageSize);
            return Ok(result);
        }

        [HttpGet("trending")]
        public async Task<IActionResult> Trending([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _feedService.GetTrendingAsync(UserHelper.GetUserId(User), pagination.Page, pagination.PageSize);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> Personalized([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _feedService.GetPersonalizedFeedAsync(UserHelper.GetUserId(User), pagination.Page, pagination.PageSize);
            return Ok(result);
        }

        [HttpGet("following")]
        public async Task<IActionResult> Following([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _feedService.GetFollowingFeedAsync(UserHelper.GetUserId(User), pagination.Page, pagination.PageSize);
            return Ok(result);
        }

        [HttpGet("my-clubs-trending")]
        public async Task<IActionResult> MyClubsTrending([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _feedService.GetMyClubsTrendingAsync(UserHelper.GetUserId(User), pagination.Page, pagination.PageSize);
            return Ok(result);
        }

        [HttpGet("club/{clubId:int}")]
        public async Task<IActionResult> Club(int clubId, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _feedService.GetClubFeedAsync(UserHelper.GetUserId(User), clubId, pagination.Page, pagination.PageSize);
            return Ok(result);
        }

        [HttpGet("user/{targetUserId:int}")]
        public async Task<IActionResult> UserFeed(int targetUserId, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _feedService.GetUserFeedAsync(UserHelper.GetUserId(User), targetUserId, pagination.Page, pagination.PageSize);
            return Ok(result);
        }

        [HttpGet("saved")]
        public async Task<IActionResult> Saved([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _feedService.GetSavedFeedAsync(UserHelper.GetUserId(User), pagination.Page, pagination.PageSize);
            return Ok(result);
        }
    }
}