using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Message;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.MessageService;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }


        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendMessageDto dto)
        {
            var result = await _messageService.SendAsync(UserHelper.GetUserId(User), dto);
            return Ok(ApiResponse<object>.Ok(result, "Message sent"));
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var result = await _messageService.GetConversationsAsync(UserHelper.GetUserId(User));
            return Ok(ApiResponse<object>.Ok(result));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _messageService.GetUnreadCountAsync(UserHelper.GetUserId(User));
            return Ok(ApiResponse<object>.Ok(new { unreadCount = count }));
        }


        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string keyword,
            [FromQuery] PaginationParamsDto pagination)
        {
            var result = await _messageService.SearchMessagesAsync(UserHelper.GetUserId(User), keyword, pagination);
            return Ok(ApiResponse<object>.Ok(result));
        }


        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetChat(int userId, [FromQuery] MessageQueryDto query)
        {
            var result = await _messageService.GetChatAsync(UserHelper.GetUserId(User), userId, query);
            return Ok(ApiResponse<object>.Ok(result));
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> Edit(int id, [FromBody] EditMessageDto dto)
        {
            var result = await _messageService.EditAsync(id, UserHelper.GetUserId(User), dto);
            return Ok(ApiResponse<object>.Ok(result, "Message updated"));
        }


        [HttpDelete("{id:int}/for-everyone")]
        public async Task<IActionResult> DeleteForEveryone(int id)
        {
            await _messageService.DeleteForEveryoneAsync(id, UserHelper.GetUserId(User));
            return Ok(ApiResponse<object>.Ok("Message deleted for everyone"));
        }


        [HttpDelete("{id:int}/for-me")]
        public async Task<IActionResult> DeleteForMe(int id)
        {
            await _messageService.DeleteForMeAsync(id, UserHelper.GetUserId(User));
            return Ok(ApiResponse<object>.Ok("Message removed from your view"));
        }


        [HttpPut("seen/{senderId:int}")]
        public async Task<IActionResult> MarkSeen(int senderId)
        {
            await _messageService.MarkAsSeenAsync(UserHelper.GetUserId(User), senderId);
            return Ok(ApiResponse<object>.Ok("Messages marked as seen"));
        }
    }
}
