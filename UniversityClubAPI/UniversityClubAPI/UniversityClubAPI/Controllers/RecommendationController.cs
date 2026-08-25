using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.RecommendationService;
namespace UniversityClubAPI.Controllers
{
    [Route("api/recommendations")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class RecommendationController : ControllerBase
    {
        private const int MinCount = 1;
        private const int MaxCount = 50;
        private readonly IRecommendationService _recommendationService;
        public RecommendationController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }
        [HttpGet("clubs")]
        public async Task<IActionResult> GetClubs([FromQuery] int count = 10)
        {
            count = Math.Clamp(count, MinCount, MaxCount);
            var result = await _recommendationService.GetRecommendedClubsAsync(UserHelper.GetUserId(User), count);
            return result.Success ? Ok(result) : NotFound(result);
        }
        [HttpGet("events")]
        public async Task<IActionResult> GetEvents([FromQuery] int count = 10)
        {
            count = Math.Clamp(count, MinCount, MaxCount);
            var result = await _recommendationService.GetRecommendedEventsAsync(UserHelper.GetUserId(User), count);
            return result.Success ? Ok(result) : NotFound(result);
        }
        [HttpGet("people")]
        public async Task<IActionResult> GetPeople([FromQuery] int count = 10)
        {
            count = Math.Clamp(count, MinCount, MaxCount);
            var result = await _recommendationService.GetRecommendedPeopleAsync(UserHelper.GetUserId(User), count);
            return result.Success ? Ok(result) : NotFound(result);
        }
        [HttpPost("clubs/{clubId:int}/dismiss")]
        public async Task<IActionResult> DismissClub(int clubId)
        {
            var result = await _recommendationService.DismissClubRecommendationAsync(UserHelper.GetUserId(User), clubId);
            return Ok(result);
        }
        [HttpPost("smart-digest")]
        public async Task<IActionResult> RunSmartDigest()
        {
            var result = await _recommendationService.RunSmartDigestAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }
    }
}