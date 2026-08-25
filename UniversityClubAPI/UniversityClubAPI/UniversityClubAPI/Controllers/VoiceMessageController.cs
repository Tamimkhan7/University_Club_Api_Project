using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UniversityClubAPI.DTOs.VoiceMessage;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.VoiceMessageService;
namespace UniversityClubAPI.Controllers
{
    [Route("api/voice-messages")]
    [ApiController]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class VoiceMessageController : ControllerBase
    {
        private readonly IVoiceMessageService _voiceMessageService;
        public VoiceMessageController(IVoiceMessageService voiceMessageService)
        {
            _voiceMessageService = voiceMessageService;
        }
        [HttpPost("direct/{receiverId:int}")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> SendDirect(int receiverId, [FromForm] SendVoiceMessageDto dto)
        {
            var result = await _voiceMessageService.SendDirectVoiceMessageAsync(UserHelper.GetUserId(User), receiverId, dto);
            return Ok(result);
        }
        [HttpPost("group/{groupId:int}")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> SendGroup(int groupId, [FromForm] SendVoiceMessageDto dto)
        {
            var result = await _voiceMessageService.SendGroupVoiceMessageAsync(UserHelper.GetUserId(User), groupId, dto);
            return Ok(result);
        }
        [HttpDelete("direct/{messageId:int}")]
        public async Task<IActionResult> DeleteDirect(int messageId)
        {
            var result = await _voiceMessageService.DeleteDirectVoiceMessageAsync(UserHelper.GetUserId(User), messageId);
            return Ok(result);
        }
        [HttpDelete("group/{messageId:int}")]
        public async Task<IActionResult> DeleteGroup(int messageId)
        {
            var result = await _voiceMessageService.DeleteGroupVoiceMessageAsync(UserHelper.GetUserId(User), messageId);
            return Ok(result);
        }
    }
}