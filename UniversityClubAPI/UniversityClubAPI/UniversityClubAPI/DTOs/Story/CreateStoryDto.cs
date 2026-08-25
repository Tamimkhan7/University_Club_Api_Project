using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Story
{
    public class CreateStoryDto
    {
        [Required(ErrorMessage = "Media file is required.")]
        public IFormFile Media { get; set; } = null!;

        [StringLength(300)]
        public string? Caption { get; set; }
    }
}
