namespace UniversityClubAPI.DTOs.LiveEvent
{
    public class LiveViewerDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImage { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsMuted { get; set; }
        public bool IsBanned { get; set; }
    }
}