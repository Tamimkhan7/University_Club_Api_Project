namespace UniversityClubAPI.DTOs.Event
{
    public class EventAttendanceResponseDto
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public Models.User? User { get; set; }
        public string? EventTitle { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}