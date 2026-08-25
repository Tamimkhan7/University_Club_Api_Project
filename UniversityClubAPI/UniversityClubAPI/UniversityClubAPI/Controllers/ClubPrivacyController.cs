using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.ClubPrivacy;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.ClubPrivacyService;
namespace UniversityClubAPI.Controllers
{
    [Route("api/club-privacy")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class ClubPrivacyController : ControllerBase
    {
        private readonly IClubPrivacyService _clubPrivacyService;
        public ClubPrivacyController(IClubPrivacyService clubPrivacyService)
        {
            _clubPrivacyService = clubPrivacyService;
        }

        [HttpPut("clubs/{clubId:int}/visibility")]
        public async Task<IActionResult> UpdateVisibility(int clubId, [FromBody] UpdateVisibilityDto dto)
        {
            var result = await _clubPrivacyService.UpdateVisibilityAsync(UserHelper.GetUserId(User), clubId, dto);
            return Ok(result);
        }

        [HttpPost("clubs/{clubId:int}/invites")]
        public async Task<IActionResult> CreateInvite(int clubId, [FromBody] CreateInviteDto dto)
        {
            var result = await _clubPrivacyService.CreateInviteAsync(UserHelper.GetUserId(User), clubId, dto);
            return Ok(result);
        }

        [HttpDelete("invites/{inviteId:int}")]
        public async Task<IActionResult> RevokeInvite(int inviteId)
        {
            var result = await _clubPrivacyService.RevokeInviteAsync(UserHelper.GetUserId(User), inviteId);
            return Ok(result);
        }

        [HttpGet("clubs/{clubId:int}/invites")]
        public async Task<IActionResult> GetClubInvites(
            int clubId,
            [FromQuery] PaginationParamsDto pagination,
            [FromQuery] InviteStatus? status)
        {
            var result = await _clubPrivacyService.GetClubInvitesAsync(UserHelper.GetUserId(User), clubId, pagination, status);
            return Ok(result);
        }

        [HttpGet("invites/{inviteId:int}")]
        public async Task<IActionResult> GetInviteById(int inviteId)
        {
            var result = await _clubPrivacyService.GetInviteByIdAsync(UserHelper.GetUserId(User), inviteId);
            return Ok(result);
        }

        [HttpGet("invites/my")]
        public async Task<IActionResult> GetMyInvites()
        {
            var result = await _clubPrivacyService.GetMyInvitesAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }

        [HttpPost("invites/{inviteId:int}/accept")]
        public async Task<IActionResult> AcceptInvite(int inviteId)
        {
            var result = await _clubPrivacyService.AcceptInviteAsync(UserHelper.GetUserId(User), inviteId);
            return Ok(result);
        }


        [HttpPost("invites/{inviteId:int}/decline")]
        public async Task<IActionResult> DeclineInvite(int inviteId)
        {
            var result = await _clubPrivacyService.DeclineInviteAsync(UserHelper.GetUserId(User), inviteId);
            return Ok(result);
        }
    }
}