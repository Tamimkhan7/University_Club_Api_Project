namespace UniversityClubAPI.DTOs.Feed
{
    public class FeedItemDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserImage { get; set; }

        public int? ClubId { get; set; }
        public string? ClubName { get; set; }

        public int CommentCount { get; set; }
        public int ReactionCount { get; set; }
        public int ShareCount { get; set; }

        public bool IsSaved { get; set; }
        public string? MyReaction { get; set; }
    }
}
