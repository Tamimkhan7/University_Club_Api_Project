using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Notification
{
    public class CreateNotificationDto
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public NotificationType Type { get; set; }
        public string? Message { get; set; }
    }
}
