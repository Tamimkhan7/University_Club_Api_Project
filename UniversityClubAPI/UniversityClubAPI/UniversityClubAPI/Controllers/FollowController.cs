using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.FollowService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpPost("{targetUserId:int}")]
        public async Task<IActionResult> Follow(int targetUserId)
        {
            var result = await _followService.FollowAsync(UserHelper.GetUserId(User), targetUserId);
            return Ok(result);
        }

        [HttpDelete("{targetUserId:int}")]
        public async Task<IActionResult> Unfollow(int targetUserId)
        {
            var result = await _followService.UnfollowAsync(UserHelper.GetUserId(User), targetUserId);
            return Ok(result);
        }

        [HttpGet("followers")]
        public async Task<IActionResult> GetMyFollowers([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _followService.GetMyFollowersAsync(UserHelper.GetUserId(User), pagination);
            return Ok(result);
        }

        [HttpGet("following")]
        public async Task<IActionResult> GetMyFollowing([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _followService.GetMyFollowingAsync(UserHelper.GetUserId(User), pagination);
            return Ok(result);
        }

        [HttpGet("followers/{targetUserId:int}")]
        public async Task<IActionResult> GetUserFollowers(int targetUserId, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _followService.GetUserFollowersAsync(UserHelper.GetUserId(User), targetUserId, pagination);
            return Ok(result);
        }

        [HttpGet("following/{targetUserId:int}")]
        public async Task<IActionResult> GetUserFollowing(int targetUserId, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _followService.GetUserFollowingAsync(UserHelper.GetUserId(User), targetUserId, pagination);
            return Ok(result);
        }

        [HttpGet("status/{targetUserId:int}")]
        public async Task<IActionResult> GetFollowStatus(int targetUserId)
        {
            var result = await _followService.GetFollowStatusAsync(UserHelper.GetUserId(User), targetUserId);
            return Ok(result);
        }

        [HttpGet("counts/{targetUserId:int}")]
        public async Task<IActionResult> GetFollowCounts(int targetUserId)
        {
            var result = await _followService.GetFollowCountsAsync(UserHelper.GetUserId(User), targetUserId);
            return Ok(result);
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions()
        {
            var result = await _followService.GetSuggestionsAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }

        [HttpGet("suggestions/common")]
        public async Task<IActionResult> GetCommonSuggestions()
        {
            var result = await _followService.GetCommonSuggestionsAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }

        [HttpGet("mutual/{targetUserId:int}")]
        public async Task<IActionResult> GetMutualFollowing(int targetUserId)
        {
            var result = await _followService.GetMutualFollowingAsync(UserHelper.GetUserId(User), targetUserId);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string query,
            [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _followService.SearchUsersAsync(UserHelper.GetUserId(User), query, pagination);
            return Ok(result);
        }

        [HttpPost("block/{targetUserId:int}")]
        public async Task<IActionResult> BlockUser(int targetUserId)
        {
            var result = await _followService.BlockUserAsync(UserHelper.GetUserId(User), targetUserId);
            return Ok(result);
        }

        [HttpDelete("unblock/{targetUserId:int}")]
        public async Task<IActionResult> UnblockUser(int targetUserId)
        {
            var result = await _followService.UnblockUserAsync(UserHelper.GetUserId(User), targetUserId);
            return Ok(result);
        }

        [HttpGet("blocked")]
        public async Task<IActionResult> GetBlockedUsers([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _followService.GetBlockedUsersAsync(UserHelper.GetUserId(User), pagination);
            return Ok(result);
        }

        [HttpGet("block-status/{targetUserId:int}")]
        public async Task<IActionResult> GetBlockStatus(int targetUserId)
        {
            var result = await _followService.GetBlockStatusAsync(UserHelper.GetUserId(User), targetUserId);
            return Ok(result);
        }
    }
}