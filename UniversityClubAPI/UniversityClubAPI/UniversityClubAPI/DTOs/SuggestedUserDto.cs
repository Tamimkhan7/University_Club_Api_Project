namespace UniversityClubAPI.DTOs
{
    public class SuggestedUserDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ProfileImage { get; set; }
        public int MutualCount { get; set; }
    }
}
