using UniversityClubAPI.Enums;

namespace UniversityClubAPI.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int SenderId { get; set; }
        public User? Sender { get; set; }

        public int ReceiverId { get; set; }
        public User? Receiver { get; set; }

        public NotificationType Type { get; set; }

        public string? Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}