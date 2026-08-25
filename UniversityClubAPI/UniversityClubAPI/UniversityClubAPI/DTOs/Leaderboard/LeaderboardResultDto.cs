using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Leaderboard
{
    public class LeaderboardResultDto
    {
        public LeaderboardCategory Category { get; set; }
        public LeaderboardPeriod Period { get; set; }

        public List<LeaderboardEntryDto> TopEntries { get; set; } = new();

        public LeaderboardEntryDto? MyEntry { get; set; }
    }
}
