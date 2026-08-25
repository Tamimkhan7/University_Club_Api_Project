namespace UniversityClubAPI.DTOs.Event
{
    public class EventStatsDto
    {
        public int EventId { get; set; }
        public string? Title { get; set; }
        public DateTime EventDate { get; set; }
        public int TotalAttendees { get; set; }
        public bool IsUpcoming { get; set; }

    }
}
