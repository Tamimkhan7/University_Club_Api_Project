namespace UniversityClubAPI.Models
{
    public class LiveChatMessage
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
