namespace UniversityClubAPI.Models
{
    public class PostShare
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PostId { get; set; }
        public User? user { get; set; }
        public Post? post { get; set; }
        public DateTime CreatdAt { get; set; } = DateTime.UtcNow;
    }
}
