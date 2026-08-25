namespace UniversityClubAPI.Models
{

    public class ClubRecommendationDismissal
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ClubId { get; set; }
        public DateTime DismissedAt { get; set; } = DateTime.UtcNow;
    }
}