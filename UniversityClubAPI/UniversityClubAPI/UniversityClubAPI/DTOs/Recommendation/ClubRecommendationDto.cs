namespace UniversityClubAPI.DTOs.Recommendation
{
    public class ClubRecommendationDto
    {
        public int ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? Description { get; set; }
        public int MemberCount { get; set; }


        public string Reason { get; set; } = string.Empty;

        public double Score { get; set; }
    }
}
