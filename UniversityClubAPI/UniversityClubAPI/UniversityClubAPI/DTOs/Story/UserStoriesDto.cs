namespace UniversityClubAPI.DTOs.Story
{
    public class UserStoriesDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImage { get; set; }


        public bool HasUnviewed { get; set; }

        public List<StoryResponseDto> Stories { get; set; } = new();
    }
}
