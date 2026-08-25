using UniversityClubAPI.Enums;

namespace UniversityClubAPI.Models
{
    public class Badge
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string IconEmoji { get; set; } = "🏅";

        public BadgeCategory Category { get; set; } = BadgeCategory.Participation;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserBadge> AwardedTo { get; set; } = new List<UserBadge>();
    }
}
