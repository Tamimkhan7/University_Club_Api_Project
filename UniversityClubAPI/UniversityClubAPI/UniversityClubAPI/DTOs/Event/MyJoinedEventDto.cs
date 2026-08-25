namespace UniversityClubAPI.DTOs.Event
{
    public class MyJoinedEventDto
    {
        public int EventId { get; set; }
        public string? EventTitle { get; set; }
        public string? EventDescription { get; set; }
        public DateTime EventDate { get; set; }
        public int ClubId { get; set; }
        public Models.Club? Club { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsUpcoming { get; set; }
    }
}