using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.Models
{
    public class PostReport
    {
        [Key]
        public int Id { get; set; }

        public int ReporterId { get; set; }
        public User? Reporter { get; set; }

        public int PostId { get; set; }
        public Post? Post { get; set; }

        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
