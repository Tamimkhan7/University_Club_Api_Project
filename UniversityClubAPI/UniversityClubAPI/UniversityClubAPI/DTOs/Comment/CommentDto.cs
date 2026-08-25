namespace UniversityClubAPI.DTOs.Comment
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
