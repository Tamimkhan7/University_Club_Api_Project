using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Group
{
    public class GroupMessageDto
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? Text { get; set; }
        public MessageMediaType MediaType { get; set; } = MessageMediaType.Text;
        public string? MediaUrl { get; set; }
        public int? DurationSeconds { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}