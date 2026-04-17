namespace UniversityClubAPI.DTOs
{
    public class CommentResponseDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? UserName { get; set; }
        public string? UserImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
