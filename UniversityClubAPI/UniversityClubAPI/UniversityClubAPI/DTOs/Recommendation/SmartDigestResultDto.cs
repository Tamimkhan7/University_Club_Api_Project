namespace UniversityClubAPI.DTOs.Recommendation
{
    public class SmartDigestResultDto
    {
        public ClubRecommendationDto? TopClub { get; set; }
        public EventRecommendationDto? TopEvent { get; set; }
        public bool NotificationSent { get; set; }
    }
}
