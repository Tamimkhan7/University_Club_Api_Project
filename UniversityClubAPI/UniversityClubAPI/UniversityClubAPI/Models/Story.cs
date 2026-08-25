using UniversityClubAPI.Enums;

namespace UniversityClubAPI.Models
{
    public class Story
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public string MediaUrl { get; set; } = string.Empty;
        public StoryMediaType MediaType { get; set; } = StoryMediaType.Image;

        public string? Caption { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public ICollection<StoryView> Views { get; set; } = new List<StoryView>();
    }
}
