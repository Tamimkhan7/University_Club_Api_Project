namespace UniversityClubAPI.DTOs.Event
{
    public class EventResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public int ClubId { get; set; }
        public Models.Club? Club { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalAttendees { get; set; }
    }
}