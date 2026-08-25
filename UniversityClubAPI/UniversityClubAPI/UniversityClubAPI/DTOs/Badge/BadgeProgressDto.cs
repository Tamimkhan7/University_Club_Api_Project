namespace UniversityClubAPI.DTOs.Badge
{
    public class BadgeProgressDto
    {
        public string BadgeCode { get; set; } = string.Empty;
        public string BadgeName { get; set; } = string.Empty;
        public string IconEmoji { get; set; } = string.Empty;
        public int Current { get; set; }
        public int Target { get; set; }
        public double PercentComplete { get; set; }
        public bool Earned { get; set; }
    }
}
