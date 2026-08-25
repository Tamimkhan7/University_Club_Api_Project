namespace UniversityClubAPI.DTOs.User
{
    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? ProfileImage { get; set; }
        public string? Department { get; set; }
        public string? Batch { get; set; }
        public bool IsFollowing { get; set; }
    }
}
