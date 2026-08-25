namespace UniversityClubAPI.DTOs.LiveEvent
{
    public class LiveChatMessageDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImage { get; set; }

        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
