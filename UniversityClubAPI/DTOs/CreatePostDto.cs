namespace UniversityClubAPI.DTOs
{
    public class CreatePostDto
    {
        public int ClubId { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
    }
}
