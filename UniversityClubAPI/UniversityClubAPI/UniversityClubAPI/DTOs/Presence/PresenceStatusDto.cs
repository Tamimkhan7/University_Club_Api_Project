namespace UniversityClubAPI.DTOs.Presence
{
    public class PresenceStatusDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? ProfileImage { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastSeenAt { get; set; }
    }
}
