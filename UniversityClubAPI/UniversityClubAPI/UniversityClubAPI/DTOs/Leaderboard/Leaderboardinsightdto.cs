namespace UniversityClubAPI.DTOs.Leaderboard
{
    public class LeaderboardInsightDto
    {
        public LeaderboardEntryDto? MyEntry { get; set; }
        public LeaderboardEntryDto? NextRankEntry { get; set; }
        public int? PointsToNextRank { get; set; }
        public string Suggestion { get; set; } = string.Empty;
    }
}