namespace UniversityClubAPI.DTOs.Story
{
    public class StoryViewerDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImage { get; set; }
        public DateTime ViewedAt { get; set; }
    }
}
