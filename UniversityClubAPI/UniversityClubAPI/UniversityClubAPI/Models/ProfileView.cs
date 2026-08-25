namespace UniversityClubAPI.Models
{
    public class ProfileView
    {
        public int Id { get; set; }
        public int ViewerId { get; set; }
        public int ProfileOwnerId { get; set; }
        public User? user { get; set; }
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
