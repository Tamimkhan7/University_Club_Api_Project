using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Message;

namespace UniversityClubAPI.Services.MessageService
{

    public interface IMessageService
    {
        Task<MessageResponseDto> SendAsync(int senderId, SendMessageDto dto);
        Task<List<ConversationDto>> GetConversationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<PagedResultDto<MessageResponseDto>> SearchMessagesAsync(int userId, string keyword, PaginationParamsDto pagination);
        Task<PagedResultDto<MessageResponseDto>> GetChatAsync(int currentUserId, int otherUserId, MessageQueryDto query);
        Task<MessageResponseDto> EditAsync(int messageId, int userId, EditMessageDto dto);
        Task DeleteForEveryoneAsync(int messageId, int userId);
        Task DeleteForMeAsync(int messageId, int userId);
        Task MarkAsSeenAsync(int receiverId, int senderId);
    }
}
