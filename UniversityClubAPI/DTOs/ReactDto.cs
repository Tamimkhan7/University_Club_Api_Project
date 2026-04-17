using UniversityClubAPI.Models;

namespace UniversityClubAPI.DTOs
{
    public class ReactDto
    {
        public int PostId { get; set; }
        public ReactionType Type { get; set; }
    }
}
