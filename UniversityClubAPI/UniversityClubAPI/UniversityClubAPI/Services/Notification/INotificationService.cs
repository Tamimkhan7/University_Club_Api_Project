using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Notification;
namespace UniversityClubAPI.Services.NotificationService
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateAndPushAsync(CreateNotificationDto dto, bool allowSelfNotify = false);
        Task<PagedResultDto<NotificationDto>> GetPagedAsync(int userId, NotificationQueryDto query);
        Task<List<NotificationDto>> GetUnreadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<NotificationDto?> GetByIdAsync(int userId, int notificationId);
        Task<bool> MarkAsReadAsync(int userId, int notificationId);
        Task<int> MarkSelectedAsReadAsync(int userId, List<int> notificationIds);
        Task<int> MarkAllAsReadAsync(int userId);
        Task<bool> DeleteAsync(int userId, int notificationId);
        Task<int> DeleteSelectedAsync(int userId, List<int> notificationIds);
        Task<int> DeleteAllAsync(int userId);
    }
}