namespace UniversityClubAPI.DTOs.Badge
{
    public class BadgeHolderDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserProfileImage { get; set; }
        public int? ClubId { get; set; }
        public string? ClubName { get; set; }
        public DateTime EarnedAt { get; set; }
    }
}
