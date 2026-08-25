using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Reaction
{
    public class ReactionResponseDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserImage { get; set; }
        public ReactionType Type { get; set; }
        public string TypeLebel => Type.ToString();
        public DateTime CreatedAt { get; set; }
    }
}
