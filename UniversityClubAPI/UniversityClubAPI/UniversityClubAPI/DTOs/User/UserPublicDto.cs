namespace UniversityClubAPI.DTOs.User
{
    public class UserPublicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImage { get; set; }
        public string? CoverPhoto { get; set; }
        public string? Department { get; set; }
        public string? Batch { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsFollowing { get; set; }
        public bool IsBlocked { get; set; }
    }
}
