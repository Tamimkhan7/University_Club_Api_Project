using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Message
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderProfileImage { get; set; }
        public int ReceiverId { get; set; }
        public string? Text { get; set; }
        public MessageMediaType MediaType { get; set; } = MessageMediaType.Text;
        public string? MediaUrl { get; set; }
        public int? DurationSeconds { get; set; }
        public bool IsSeen { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeletedForEveryone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EditedAt { get; set; }
    }
}