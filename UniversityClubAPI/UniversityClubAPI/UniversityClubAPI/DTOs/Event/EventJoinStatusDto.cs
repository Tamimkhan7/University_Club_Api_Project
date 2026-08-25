namespace UniversityClubAPI.DTOs.Event
{
    public class EventJoinStatusDto
    {
        public int EventId { get; set; }
        public bool HasJoined { get; set; }
        public DateTime? JoinedAt { get; set; }
    }
}
