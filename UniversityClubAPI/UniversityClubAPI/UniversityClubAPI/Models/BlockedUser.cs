namespace UniversityClubAPI.Models
{
    public class BlockedUser
    {
        public int Id { get; set; }
        public int BlockerId { get; set; }
        public User? Blocker { get; set; }
        public int BlockedUserId { get; set; }
        public User? BlockedUserInfo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}