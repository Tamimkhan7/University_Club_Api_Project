using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Group;
using UniversityClubAPI.DTOs.GroupMessage;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.GroupService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupDto dto)
        {
            var result = await _groupService.CreateAsync(UserHelper.GetUserId(User), dto);
            return Ok(result);
        }

        [HttpPut("{groupId:int}")]
        public async Task<IActionResult> Update(int groupId, [FromBody] UpdateGroupDto dto)
        {
            var result = await _groupService.UpdateAsync(UserHelper.GetUserId(User), groupId, dto);
            return Ok(result);
        }

        [HttpDelete("{groupId:int}")]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var result = await _groupService.DeleteGroupAsync(UserHelper.GetUserId(User), groupId);
            return Ok(result);
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] SendGroupMessageDto dto)
        {
            var result = await _groupService.SendMessageAsync(UserHelper.GetUserId(User), dto);
            return Ok(result);
        }

        [HttpGet("{groupId:int}/messages")]
        public async Task<IActionResult> GetMessages(int groupId, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _groupService.GetMessagesAsync(UserHelper.GetUserId(User), groupId, pagination);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> MyGroups()
        {
            var result = await _groupService.GetMyGroupsAsync(UserHelper.GetUserId(User));
            return Ok(result);
        }

        [HttpGet("{groupId:int}")]
        public async Task<IActionResult> GetById(int groupId)
        {
            var result = await _groupService.GetByIdAsync(UserHelper.GetUserId(User), groupId);
            return Ok(result);
        }

        [HttpGet("{groupId:int}/members")]
        public async Task<IActionResult> GetMembers(int groupId)
        {
            var result = await _groupService.GetMembersAsync(UserHelper.GetUserId(User), groupId);
            return Ok(result);
        }

        [HttpDelete("{groupId:int}/leave")]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
            var result = await _groupService.LeaveGroupAsync(UserHelper.GetUserId(User), groupId);
            return Ok(result);
        }

        [HttpPost("{groupId:int}/members")]
        public async Task<IActionResult> AddMember(int groupId, [FromBody] AddGroupMemberDto dto)
        {
            var result = await _groupService.AddMemberAsync(UserHelper.GetUserId(User), groupId, dto);
            return Ok(result);
        }

        [HttpDelete("{groupId:int}/members/{memberId:int}")]
        public async Task<IActionResult> RemoveMember(int groupId, int memberId)
        {
            var result = await _groupService.RemoveMemberAsync(UserHelper.GetUserId(User), groupId, memberId);
            return Ok(result);
        }

        [HttpPatch("{groupId:int}/members/admin")]
        public async Task<IActionResult> SetAdmin(int groupId, [FromBody] SetGroupAdminDto dto)
        {
            var result = await _groupService.SetAdminAsync(UserHelper.GetUserId(User), groupId, dto);
            return Ok(result);
        }
    }
}