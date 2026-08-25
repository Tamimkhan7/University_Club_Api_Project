using UniversityClubAPI.Enums;

namespace UniversityClubAPI.Models
{
    public class EventJoinRequest
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event? Event { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public JoinRequestStatus Status { get; set; } = JoinRequestStatus.Pending;

        public int? RespondedBy { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }
}