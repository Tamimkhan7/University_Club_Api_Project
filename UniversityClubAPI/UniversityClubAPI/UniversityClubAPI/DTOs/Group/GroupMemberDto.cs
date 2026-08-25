namespace UniversityClubAPI.DTOs.Group
{
    public class GroupMemberDto
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
