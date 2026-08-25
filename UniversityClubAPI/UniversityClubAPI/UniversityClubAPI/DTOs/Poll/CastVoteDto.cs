using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Poll
{
    public class CastVoteDto
    {

        [Required, MinLength(1, ErrorMessage = "Select at least one option.")]
        public List<int> OptionIds { get; set; } = new();
    }
}
