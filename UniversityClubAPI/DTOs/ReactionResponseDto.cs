using UniversityClubAPI.Models;

namespace UniversityClubAPI.DTOs
{
    public class ReactionResponseDto
    {
        public string? UserName { get; set; }
        public string? UserImage { get; set; }
        public ReactionType Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
