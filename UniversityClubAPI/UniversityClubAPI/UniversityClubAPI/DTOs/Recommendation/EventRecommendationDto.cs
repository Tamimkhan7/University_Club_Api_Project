namespace UniversityClubAPI.DTOs.Recommendation
{
    public class EventRecommendationDto
    {
        public int EventId { get; set; }
        public string? Title { get; set; }
        public DateTime EventDate { get; set; }

        public int ClubId { get; set; }
        public string? ClubName { get; set; }

        public string Reason { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
