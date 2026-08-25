using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Story;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.StoryService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/stories")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class StoryController : ControllerBase
    {
        private readonly IStoryService _storyService;

        public StoryController(IStoryService storyService)
        {
            _storyService = storyService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateStoryDto dto)
        {
            var result = await _storyService.CreateStoryAsync(UserHelper.GetUserId(User), dto);
            return Ok(result);
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeed()
        {
            var result = await _storyService.GetFeedStoriesAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyStories()
        {
            var result = await _storyService.GetMyStoriesAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }

        [HttpGet("user/{targetUserId:int}")]
        public async Task<IActionResult> GetUserStories(int targetUserId)
        {
            var result = await _storyService.GetUserStoriesAsync(UserHelper.GetUserId(User), targetUserId);
            return Ok(result);
        }

        [HttpPost("{storyId:int}/view")]
        public async Task<IActionResult> View(int storyId)
        {
            var result = await _storyService.ViewStoryAsync(UserHelper.GetUserId(User), storyId);
            return Ok(result);
        }


        [HttpGet("{storyId:int}/viewers")]
        public async Task<IActionResult> GetViewers(int storyId)
        {
            var result = await _storyService.GetStoryViewersAsync(UserHelper.GetUserId(User), storyId);
            return Ok(result);
        }


        [HttpDelete("{storyId:int}")]
        public async Task<IActionResult> Delete(int storyId)
        {
            var result = await _storyService.DeleteStoryAsync(UserHelper.GetUserId(User), storyId);
            return Ok(result);
        }
    }
}
