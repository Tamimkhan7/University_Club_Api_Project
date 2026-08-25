using UniversityClubAPI.DTOs.Group;
using UniversityClubAPI.DTOs.Message;
using UniversityClubAPI.DTOs.VoiceMessage;
using UniversityClubAPI.Helpers;
namespace UniversityClubAPI.Services.VoiceMessageService
{
    public interface IVoiceMessageService
    {
        Task<ApiResponse<MessageResponseDto>> SendDirectVoiceMessageAsync(int senderId, int receiverId, SendVoiceMessageDto dto);
        Task<ApiResponse<GroupMessageDto>> SendGroupVoiceMessageAsync(int senderId, int groupId, SendVoiceMessageDto dto);
        Task<ApiResponse<bool>> DeleteDirectVoiceMessageAsync(int userId, int messageId);
        Task<ApiResponse<bool>> DeleteGroupVoiceMessageAsync(int userId, int messageId);
    }
}