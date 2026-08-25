using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Event;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.EventService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            var result = await _eventService.CreateAsync(UserHelper.GetUserId(User), dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateEventDto dto)
        {
            var result = await _eventService.UpdateAsync(UserHelper.GetUserId(User), id, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _eventService.DeleteAsync(UserHelper.GetUserId(User), id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("join/{eventId:int}")]
        public async Task<IActionResult> Join(int eventId)
        {
            var result = await _eventService.JoinAsync(UserHelper.GetUserId(User), eventId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("leave/{eventId:int}")]
        public async Task<IActionResult> Leave(int eventId)
        {
            var result = await _eventService.LeaveAsync(UserHelper.GetUserId(User), eventId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:int}/join-status")]
        public async Task<IActionResult> JoinStatus(int id)
        {
            var result = await _eventService.GetJoinStatusAsync(UserHelper.GetUserId(User), id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{eventId:int}/join-requests")]
        public async Task<IActionResult> GetJoinRequests(int eventId)
        {
            var result = await _eventService.GetJoinRequestsAsync(UserHelper.GetUserId(User), eventId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{eventId:int}/join-requests/{requestId:int}/approve")]
        public async Task<IActionResult> ApproveJoinRequest(int eventId, int requestId)
        {
            var result = await _eventService.RespondToJoinRequestAsync(UserHelper.GetUserId(User), eventId, requestId, true);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{eventId:int}/join-requests/{requestId:int}/reject")]
        public async Task<IActionResult> RejectJoinRequest(int eventId, int requestId)
        {
            var result = await _eventService.RespondToJoinRequestAsync(UserHelper.GetUserId(User), eventId, requestId, false);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _eventService.GetAllAsync(pagination.Page, pagination.PageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _eventService.GetByIdAsync(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> Upcoming([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _eventService.GetUpcomingAsync(pagination.Page, pagination.PageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string keyword,
            [FromQuery] int? clubId,
            [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _eventService.SearchAsync(keyword, clubId, pagination.Page, pagination.PageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("club/{clubId:int}")]
        public async Task<IActionResult> GetByClub(int clubId, [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _eventService.GetByClubAsync(clubId, pagination.Page, pagination.PageSize);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> MyEvents([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _eventService.GetMyEventsAsync(UserHelper.GetUserId(User), pagination.Page, pagination.PageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("joined")]
        public async Task<IActionResult> MyJoinedEvents([FromQuery] PaginationParamsDto pagination)
        {
            var result = await _eventService.GetMyJoinedEventsAsync(UserHelper.GetUserId(User), pagination.Page, pagination.PageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my-clubs-upcoming")]
        public async Task<IActionResult> MyClubsUpcoming()
        {
            var result = await _eventService.GetMyClubsUpcomingAsync(UserHelper.GetUserId(User));
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{eventId:int}/attendees")]
        public async Task<IActionResult> GetAttendees(int eventId)
        {
            var result = await _eventService.GetAttendeesAsync(eventId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("{id:int}/stats")]
        public async Task<IActionResult> GetStats(int id)
        {
            var result = await _eventService.GetStatsAsync(UserHelper.GetUserId(User), id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}