using UniversityClubAPI.Enums;

namespace UniversityClubAPI.Models
{
    public class ClubInvite
    {
        public int Id { get; set; }

        public int ClubId { get; set; }
        public Club? Club { get; set; }

        public int InvitedUserId { get; set; }
        public User? InvitedUser { get; set; }

        public int InvitedBy { get; set; }
        public User? Inviter { get; set; }

        public InviteStatus Status { get; set; } = InviteStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}
