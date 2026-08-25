namespace UniversityClubAPI.DTOs.Badge
{
    public class GlobalBadgeLeaderboardDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImage { get; set; }
        public int BadgeCount { get; set; }
    }
}
