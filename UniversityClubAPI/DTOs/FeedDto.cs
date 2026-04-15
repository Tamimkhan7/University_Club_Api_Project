namespace UniversityClubAPI.DTOs
{
    public class FeedDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserName { get; set; }
        public string UserImage { get; set; }

        public int CommentCount { get; set; }
        public int ReactionCount { get; set; }
    }
}
