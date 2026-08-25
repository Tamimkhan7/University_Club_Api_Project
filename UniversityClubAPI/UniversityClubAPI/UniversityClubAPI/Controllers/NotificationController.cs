using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int GetUserId()
        {
            return UserHelper.GetUserId(User);
        }


        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] NotificationQueryDto query)
        {
            var userId = GetUserId();
            var result = await _notificationService.GetPagedAsync(userId, query);
            return Ok(ApiResponse<object>.Ok(result));
        }


        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
        {
            var userId = GetUserId();
            var result = await _notificationService.GetUnreadAsync(userId);
            return Ok(ApiResponse<object>.Ok(result));
        }


        [HttpGet("count")]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = GetUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(ApiResponse<object>.Ok(new { unreadCount = count }));
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = GetUserId();
            var notification = await _notificationService.GetByIdAsync(userId, id);

            if (notification is null)
                return NotFound(ApiResponse<object>.Fail("Notification not found"));

            return Ok(ApiResponse<object>.Ok(notification));
        }


        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();
            var success = await _notificationService.MarkAsReadAsync(userId, id);

            if (!success)
                return NotFound(ApiResponse<object>.Fail("Notification not found"));

            return Ok(ApiResponse<object>.Ok("Notification marked as read"));
        }


        [HttpPut("read-selected")]
        public async Task<IActionResult> MarkSelectedAsRead([FromBody] NotificationIdsDto dto)
        {
            if (dto == null || dto.NotificationIds == null || !dto.NotificationIds.Any())
                return BadRequest(ApiResponse<object>.Fail("NotificationIds are required."));

            var userId = GetUserId();
            var updated = await _notificationService.MarkSelectedAsReadAsync(userId, dto.NotificationIds);

            return Ok(ApiResponse<object>.Ok(new { updated }));
        }


        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            var updated = await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(ApiResponse<object>.Ok(new { updated }));
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var success = await _notificationService.DeleteAsync(userId, id);

            if (!success)
                return NotFound(ApiResponse<object>.Fail("Notification not found"));

            return Ok(ApiResponse<object>.Ok("Notification deleted successfully"));
        }


        [HttpDelete("delete-selected")]
        public async Task<IActionResult> DeleteSelected([FromBody] NotificationIdsDto dto)
        {
            if (dto == null || dto.NotificationIds == null || !dto.NotificationIds.Any())
                return BadRequest(ApiResponse<object>.Fail("NotificationIds are required."));

            var userId = GetUserId();
            var deleted = await _notificationService.DeleteSelectedAsync(userId, dto.NotificationIds);

            return Ok(ApiResponse<object>.Ok(new { deleted }));
        }


        [HttpDelete("delete-all")]
        public async Task<IActionResult> DeleteAll()
        {
            var userId = GetUserId();
            var deleted = await _notificationService.DeleteAllAsync(userId);

            return Ok(ApiResponse<object>.Ok(new { deleted }));
        }
    }
}