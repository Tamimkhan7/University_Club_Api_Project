using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Badge
{
    public class BadgeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string IconEmoji { get; set; } = string.Empty;
        public BadgeCategory Category { get; set; }

        public bool Earned { get; set; }
        public DateTime? EarnedAt { get; set; }
    }
}
