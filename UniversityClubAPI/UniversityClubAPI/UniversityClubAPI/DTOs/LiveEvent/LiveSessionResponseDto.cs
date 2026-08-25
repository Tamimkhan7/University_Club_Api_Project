using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.LiveEvent
{
    public class LiveSessionResponseDto
    {
        public int EventId { get; set; }
        public string? Title { get; set; }

        public int ClubId { get; set; }
        public string? ClubName { get; set; }

        public LiveStatus Status { get; set; }

        public string? MeetingLink { get; set; }

        public DateTime? LiveStartedAt { get; set; }
        public DateTime? LiveEndedAt { get; set; }

        public int CurrentViewerCount { get; set; }
    }
}
