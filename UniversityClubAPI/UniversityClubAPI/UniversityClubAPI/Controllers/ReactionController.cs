using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Reaction;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.ReactionService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class ReactionController : ControllerBase
    {
        private readonly IReactionService _reactionService;

        public ReactionController(IReactionService reactionService)
        {
            _reactionService = reactionService;
        }

        [HttpPost("react")]
        public async Task<IActionResult> React([FromBody] ReactDto dto)
        {
            var callerId = UserHelper.GetUserId(User);
            var summary = await _reactionService.ReactAsync(callerId, dto);
            return Ok(ApiResponse<ReactionSummaryDto>.Ok(summary));
        }

        [HttpDelete("remove/{postId:int}")]
        public async Task<IActionResult> Remove(int postId)
        {
            var callerId = UserHelper.GetUserId(User);
            var summary = await _reactionService.RemoveAsync(callerId, postId);
            return Ok(ApiResponse<ReactionSummaryDto>.Ok(summary, "Reaction removed"));
        }

        [HttpGet("summary/{postId:int}")]
        public async Task<IActionResult> Summary(int postId)
        {
            var callerId = UserHelper.GetUserId(User);
            var summary = await _reactionService.GetSummaryAsync(callerId, postId);
            return Ok(ApiResponse<ReactionSummaryDto>.Ok(summary));
        }


        [HttpGet("count/{postId:int}")]
        public async Task<IActionResult> Count(int postId)
        {
            var count = await _reactionService.GetCountAsync(postId);
            return Ok(ApiResponse<object>.Ok(new { postId, count }));
        }


        [HttpGet("my/{postId:int}")]
        public async Task<IActionResult> MyReaction(int postId)
        {
            var callerId = UserHelper.GetUserId(User);
            var myReaction = await _reactionService.GetMyReactionAsync(callerId, postId);
            return Ok(ApiResponse<object>.Ok(new { postId, myReaction }));
        }


        [HttpGet("all/{postId:int}")]
        public async Task<IActionResult> GetAll(int postId, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _reactionService.GetAllAsync(postId, pagination);
            return Ok(ApiResponse<object>.Ok(result));
        }


        [HttpGet("by-type/{postId:int}/{type}")]
        public async Task<IActionResult> GetByType(
          int postId,
          ReactionType type,
          [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _reactionService.GetByTypeAsync(postId, type, pagination);
            return Ok(ApiResponse<object>.Ok(result));
        }
    }
}
