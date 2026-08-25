using System.ComponentModel.DataAnnotations;
using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Poll
{
    public class CreatePollDto
    {
        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public PollType Type { get; set; } = PollType.General;

        public bool IsMultipleChoice { get; set; } = false;

        [Required]
        public DateTime EndDate { get; set; }

        [Required, MinLength(2, ErrorMessage = "A poll needs at least 2 options.")]
        public List<string> Options { get; set; } = new();
    }
}
