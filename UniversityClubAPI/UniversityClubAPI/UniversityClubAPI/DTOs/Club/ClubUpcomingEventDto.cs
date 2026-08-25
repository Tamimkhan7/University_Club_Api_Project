namespace UniversityClubAPI.DTOs.Club
{
    public class ClubUpcomingEventDto
    {
        public int EventId { get; set; }
        public string? Title { get; set; }
        public DateTime EventDate { get; set; }
        public int ClubId { get; set; }
        public Models.Club? Club { get; set; }
        public string? ClubName { get; set; }
        public int TotalAttendees { get; set; }
    }
}