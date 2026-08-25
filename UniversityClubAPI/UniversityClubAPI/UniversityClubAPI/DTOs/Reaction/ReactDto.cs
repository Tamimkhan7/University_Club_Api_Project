using System.ComponentModel.DataAnnotations;
using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Reaction
{
    public class ReactDto
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        public ReactionType Type { get; set; }
    }
}