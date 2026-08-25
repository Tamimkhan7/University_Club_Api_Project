using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Badge
{
    public class UserBadgeDto
    {
        public int Id { get; set; }
        public string BadgeCode { get; set; } = string.Empty;
        public string BadgeName { get; set; } = string.Empty;
        public string IconEmoji { get; set; } = string.Empty;
        public BadgeCategory Category { get; set; }

        public int? ClubId { get; set; }
        public string? ClubName { get; set; }

        public DateTime EarnedAt { get; set; }
    }
}
