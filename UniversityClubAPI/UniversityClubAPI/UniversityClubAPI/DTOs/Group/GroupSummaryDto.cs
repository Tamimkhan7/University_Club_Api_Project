namespace UniversityClubAPI.DTOs.Group
{
    public class GroupSummaryDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAdmin { get; set; }
        public int MemberCount { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }

}
