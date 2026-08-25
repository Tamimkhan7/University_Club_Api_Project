namespace UniversityClubAPI.DTOs.Event
{
    public class EventAttendeeDto
    {
        public int EventId { get; set; }
        public string? EventTitle { get; set; }
        public int UserId { get; set; }
        public Models.User? User { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public DateTime? JoinedAt { get; set; }
    }
}