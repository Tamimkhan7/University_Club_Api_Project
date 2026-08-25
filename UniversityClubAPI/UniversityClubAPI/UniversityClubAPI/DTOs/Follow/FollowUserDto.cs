namespace UniversityClubAPI.DTOs.Follow
{
    public class FollowUserDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ProfileImage { get; set; }
        public DateTime FollowedAt { get; set; }
    }
}
