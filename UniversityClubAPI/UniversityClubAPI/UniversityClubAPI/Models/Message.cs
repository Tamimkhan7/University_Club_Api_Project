using System.ComponentModel.DataAnnotations;
using UniversityClubAPI.Enums;
namespace UniversityClubAPI.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public User? Sender { get; set; }
        public int ReceiverID { get; set; }
        public User? Receiver { get; set; }
        [StringLength(1000)]
        public string? Text { get; set; }
        public MessageMediaType MediaType { get; set; } = MessageMediaType.Text;
        public string? MediaUrl { get; set; }
        public int? DurationSeconds { get; set; }
        public bool IsSeen { get; set; } = false;
        public bool IsDeletedBySender { get; set; } = false;
        public bool IsDeletedByReceiver { get; set; } = false;
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
        public bool IsDeletedForEveryone { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}