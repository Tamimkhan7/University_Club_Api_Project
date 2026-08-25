namespace UniversityClubAPI.Models
{

    public class LiveModeration
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public bool IsMuted { get; set; }
        public bool IsBanned { get; set; }

        public int ModeratedBy { get; set; }
        public User? Moderator { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}