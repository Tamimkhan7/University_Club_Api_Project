using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Recruitment
{
    public class ApplicationResponseDto
    {
        public int Id { get; set; }

        public int ClubId { get; set; }
        public string? ClubName { get; set; }

        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImage { get; set; }

        public string? Message { get; set; }
        public ApplicationStatus Status { get; set; }

        public int? ReviewedBy { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewNote { get; set; }

        public DateTime AppliedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
