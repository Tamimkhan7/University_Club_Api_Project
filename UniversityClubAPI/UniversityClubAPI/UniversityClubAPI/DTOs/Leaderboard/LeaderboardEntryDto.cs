namespace UniversityClubAPI.DTOs.Leaderboard
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? ProfileImage { get; set; }
        public string? Department { get; set; }

        public double Points { get; set; }

        public int PostCount { get; set; }
        public int EventCount { get; set; }
        public int BadgeCount { get; set; }
        public int FollowerCount { get; set; }

        public bool IsCurrentUser { get; set; }
    }
}
