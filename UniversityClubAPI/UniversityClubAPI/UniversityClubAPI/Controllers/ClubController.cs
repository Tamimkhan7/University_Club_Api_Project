using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Club;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.ClubService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class ClubController : ControllerBase
    {
        private readonly IClubService _clubService;

        public ClubController(IClubService clubService)
        {
            _clubService = clubService;
        }

        private int GetUserId() => UserHelper.GetUserId(User);


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateClubDTO dto)
            => Ok(await _clubService.CreateClubAsync(GetUserId(), dto));

        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] JoinClubDTO dto)
            => Ok(await _clubService.JoinClubAsync(GetUserId(), dto));


        [HttpDelete("leave/{clubId:int}")]
        public async Task<IActionResult> Leave(int clubId)
            => Ok(await _clubService.LeaveClubAsync(GetUserId(), clubId));


        [HttpGet("all")]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParamsDto pagination)
            => Ok(await _clubService.GetAllClubsAsync(pagination));


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
            => Ok(await _clubService.GetClubByIdAsync(GetUserId(), id));


        [HttpPut("update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateClubDTO dto)
            => Ok(await _clubService.UpdateClubAsync(GetUserId(), id, dto));


        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
            => Ok(await _clubService.DeleteClubAsync(GetUserId(), id));


        [HttpGet("{clubId:int}/members")]
        public async Task<IActionResult> Members(int clubId, [FromQuery] PaginationParamsDto pagination)
            => Ok(await _clubService.GetMembersAsync(GetUserId(), clubId, pagination));


        [HttpPut("{clubId:int}/role")]
        public async Task<IActionResult> UpdateRole(int clubId, [FromBody] UpdateClubRoleDto dto)
            => Ok(await _clubService.UpdateRoleAsync(GetUserId(), clubId, dto));


        [HttpDelete("{clubId:int}/members/{userId:int}")]
        public async Task<IActionResult> RemoveMember(int clubId, int userId)
            => Ok(await _clubService.RemoveMemberAsync(GetUserId(), clubId, userId));


        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string query,
            [FromQuery] PaginationParamsDto pagination)
            => Ok(await _clubService.SearchClubsAsync(query, pagination));


        [HttpGet("my")]
        public async Task<IActionResult> MyClubs()
            => Ok(await _clubService.GetMyClubsAsync(GetUserId()));


        [HttpGet("{clubId:int}/membership")]
        public async Task<IActionResult> MembershipStatus(int clubId)
            => Ok(await _clubService.GetMembershipStatusAsync(GetUserId(), clubId));


        [HttpGet("{clubId:int}/posts")]
        public async Task<IActionResult> ClubPosts(int clubId, [FromQuery] PaginationParamsDto pagination)
            => Ok(await _clubService.GetClubPostsAsync(GetUserId(), clubId, pagination));


        [HttpGet("{clubId:int}/members/search")]
        public async Task<IActionResult> SearchMembers(
            int clubId,
            [FromQuery] string query,
            [FromQuery] PaginationParamsDto pagination)
            => Ok(await _clubService.SearchMembersAsync(GetUserId(), clubId, query, pagination));
    }
}