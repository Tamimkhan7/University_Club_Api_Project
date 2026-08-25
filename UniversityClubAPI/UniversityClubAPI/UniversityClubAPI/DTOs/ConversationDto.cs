namespace UniversityClubAPI.DTOs.Message
{
    public class ConversationDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? ProfileImage { get; set; }
        public string? LastMessage { get; set; }
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public bool IsOnline { get; set; }
    }
}
