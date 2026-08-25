namespace UniversityClubAPI.DTOs.Recommendation
{
    public class PersonRecommendationDto
    {
        public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Department { get; set; }
        public int MutualFollowCount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}