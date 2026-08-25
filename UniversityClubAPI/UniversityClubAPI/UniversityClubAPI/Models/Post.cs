namespace UniversityClubAPI.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }

        public int ClubId { get; set; }
        public Club? Club { get; set; }

        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
        public ICollection<PostShare> Shares { get; set; } = new List<PostShare>();
        public ICollection<SavedPost> SavedByUsers { get; set; } = new List<SavedPost>();

    }
}
