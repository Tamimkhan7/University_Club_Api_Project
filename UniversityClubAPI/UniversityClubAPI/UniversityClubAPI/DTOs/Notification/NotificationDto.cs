using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Notification
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public NotificationType Type { get; set; }
        public string TypeLabel => Type.ToString();
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
