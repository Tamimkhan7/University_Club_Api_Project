using System.ComponentModel.DataAnnotations;
using UniversityClubAPI.Enums;
namespace UniversityClubAPI.Models
{
    public class GroupMessage
    {
        [Key]
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group? Group { get; set; }
        public int SenderId { get; set; }
        public User? Sender { get; set; }
        public string? Text { get; set; }
        public MessageMediaType MediaType { get; set; } = MessageMediaType.Text;
        public string? MediaUrl { get; set; }
        public int? DurationSeconds { get; set; }
        public bool IsDeletedForEveryone { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}