namespace UniversityClubAPI.DTOs
{
    public class BlockedUserDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ProfileImage { get; set; }
        public DateTime BlockedAt { get; set; }
    }
}
