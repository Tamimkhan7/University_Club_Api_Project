namespace UniversityClubAPI.DTOs.Badge
{
    public class ContributorLeaderboardDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImage { get; set; }

        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int ReactionsReceived { get; set; }

        public double Score { get; set; }
        public bool HoldsTopContributorBadge { get; set; }
    }
}
