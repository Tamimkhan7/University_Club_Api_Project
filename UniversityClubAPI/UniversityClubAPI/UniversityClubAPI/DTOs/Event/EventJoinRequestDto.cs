namespace UniversityClubAPI.DTOs
{
    public class EventJoinRequestDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImage { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; }
    }
}