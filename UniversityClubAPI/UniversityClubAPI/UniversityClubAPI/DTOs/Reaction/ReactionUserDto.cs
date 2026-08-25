using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Reaction
{
    public class ReactionUserDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserImage { get; set; }
        public ReactionType Type { get; set; }
        public string TypeLabel => Type.ToString();
    }
}
