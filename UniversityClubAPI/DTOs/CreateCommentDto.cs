namespace UniversityClubAPI.DTOs
{
    public class CreateCommentDto
    {
        public int PostId { get; set; }
        public string? Content { get; set; }
        public int? ParentCommentId { get; set; }
    }
}
