using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Story
{
    public class StoryResponseDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImage { get; set; }

        public string MediaUrl { get; set; } = string.Empty;
        public StoryMediaType MediaType { get; set; }
        public string? Caption { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public int ViewCount { get; set; }
        public bool ViewedByMe { get; set; }
    }
}
