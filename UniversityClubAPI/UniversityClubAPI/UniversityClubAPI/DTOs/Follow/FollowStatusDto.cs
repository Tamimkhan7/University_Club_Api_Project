namespace UniversityClubAPI.DTOs.Follow
{
    public class FollowStatusDto
    {
        public bool IsFollowing { get; set; }
        public bool IsFollowedBy { get; set; }
        public bool IsMutual { get; set; }
    }
}
