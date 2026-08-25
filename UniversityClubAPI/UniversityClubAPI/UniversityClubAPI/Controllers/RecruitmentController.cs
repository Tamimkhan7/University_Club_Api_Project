using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Recruitment;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.RecruitmentService;
namespace UniversityClubAPI.Controllers
{
    [Route("api/recruitment")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class RecruitmentController : ControllerBase
    {
        private readonly IRecruitmentService _recruitmentService;
        public RecruitmentController(IRecruitmentService recruitmentService)
        {
            _recruitmentService = recruitmentService;
        }

        [HttpPost("clubs/{clubId:int}/apply")]
        public async Task<IActionResult> Apply(int clubId, [FromBody] CreateApplicationDto dto)
        {
            var result = await _recruitmentService.ApplyAsync(UserHelper.GetUserId(User), clubId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{applicationId:int}")]
        public async Task<IActionResult> Withdraw(int applicationId)
        {
            var result = await _recruitmentService.WithdrawApplicationAsync(UserHelper.GetUserId(User), applicationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyApplications([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _recruitmentService.GetMyApplicationsAsync(UserHelper.GetUserId(User), pagination);
            return Ok(result);
        }

        [HttpGet("clubs/{clubId:int}")]
        public async Task<IActionResult> GetClubApplications(
            int clubId, [FromQuery] ApplicationStatus? status, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _recruitmentService.GetClubApplicationsAsync(
                UserHelper.GetUserId(User), clubId, status, pagination);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{applicationId:int}/approve")]
        public async Task<IActionResult> Approve(int applicationId, [FromBody] ReviewApplicationDto dto)
        {
            var result = await _recruitmentService.ApproveApplicationAsync(UserHelper.GetUserId(User), applicationId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{applicationId:int}/reject")]
        public async Task<IActionResult> Reject(int applicationId, [FromBody] ReviewApplicationDto dto)
        {
            var result = await _recruitmentService.RejectApplicationAsync(UserHelper.GetUserId(User), applicationId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("clubs/{clubId:int}/pending-count")]
        public async Task<IActionResult> GetPendingCount(int clubId)
        {
            var result = await _recruitmentService.GetPendingCountAsync(UserHelper.GetUserId(User), clubId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}