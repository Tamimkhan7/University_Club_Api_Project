using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Poll;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.PollService;
namespace UniversityClubAPI.Controllers
{
    [Route("api/polls")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class PollController : ControllerBase
    {
        private readonly IPollService _pollService;
        public PollController(IPollService pollService)
        {
            _pollService = pollService;
        }

        [HttpPost("clubs/{clubId:int}")]
        public async Task<IActionResult> Create(int clubId, [FromBody] CreatePollDto dto)
        {
            var result = await _pollService.CreatePollAsync(UserHelper.GetUserId(User), clubId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("clubs/{clubId:int}")]
        public async Task<IActionResult> GetClubPolls(
            int clubId, [FromQuery] bool activeOnly, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _pollService.GetClubPollsAsync(UserHelper.GetUserId(User), clubId, activeOnly, pagination);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("{pollId:int}")]
        public async Task<IActionResult> GetById(int pollId)
        {
            var result = await _pollService.GetPollByIdAsync(UserHelper.GetUserId(User), pollId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("{pollId:int}/vote")]
        public async Task<IActionResult> Vote(int pollId, [FromBody] CastVoteDto dto)
        {
            var result = await _pollService.VoteAsync(UserHelper.GetUserId(User), pollId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{pollId:int}/close")]
        public async Task<IActionResult> Close(int pollId)
        {
            var result = await _pollService.ClosePollAsync(UserHelper.GetUserId(User), pollId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{pollId:int}")]
        public async Task<IActionResult> Delete(int pollId)
        {
            var result = await _pollService.DeletePollAsync(UserHelper.GetUserId(User), pollId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}