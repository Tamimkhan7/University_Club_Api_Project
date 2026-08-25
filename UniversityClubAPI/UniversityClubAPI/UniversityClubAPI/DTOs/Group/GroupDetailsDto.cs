namespace UniversityClubAPI.DTOs.Group
{
    public class GroupDetailsDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<GroupMemberDto> Members { get; set; } = new();
    }
}
