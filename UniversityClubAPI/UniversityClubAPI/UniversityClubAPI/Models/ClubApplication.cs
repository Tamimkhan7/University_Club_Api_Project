using UniversityClubAPI.Enums;

namespace UniversityClubAPI.Models
{
    public class ClubApplication
    {
        public int Id { get; set; }

        public int ClubId { get; set; }
        public Club? Club { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public string? Message { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;


        public int? ReviewedBy { get; set; }
        public User? Reviewer { get; set; }

        public string? ReviewNote { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }
}
