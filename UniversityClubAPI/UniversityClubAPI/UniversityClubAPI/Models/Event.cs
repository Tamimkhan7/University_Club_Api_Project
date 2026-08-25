using UniversityClubAPI.Enums;

namespace UniversityClubAPI.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }

        public int ClubId { get; set; }
        public Club? club { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<EventAttendance> Attendances { get; set; } = new List<EventAttendance>();

        public LiveStatus LiveStatus { get; set; } = LiveStatus.NotStarted;
        public string? MeetingLink { get; set; }
        public DateTime? LiveStartedAt { get; set; }
        public DateTime? LiveEndedAt { get; set; }
        public ICollection<LiveParticipant> LiveParticipants { get; set; } = new List<LiveParticipant>();
        public ICollection<LiveChatMessage> LiveChatMessages { get; set; } = new List<LiveChatMessage>();
    }
}