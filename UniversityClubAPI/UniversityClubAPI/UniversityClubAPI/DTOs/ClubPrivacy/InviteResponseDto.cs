using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.ClubPrivacy
{
    public class InviteResponseDto
    {
        public int Id { get; set; }

        public int ClubId { get; set; }
        public string? ClubName { get; set; }

        public int InvitedUserId { get; set; }
        public string? InvitedUserName { get; set; }

        public int InvitedBy { get; set; }
        public string? InviterName { get; set; }

        public InviteStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}
