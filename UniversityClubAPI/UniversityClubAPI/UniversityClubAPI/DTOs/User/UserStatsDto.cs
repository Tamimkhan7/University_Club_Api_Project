namespace UniversityClubAPI.DTOs.User
{
    public class UserStatsDto
    {
        public int UserId { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        public int Posts { get; set; }
        public int ProfileViews { get; set; }
    }
}
