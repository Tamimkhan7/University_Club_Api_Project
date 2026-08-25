using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Post
{
    public class UpdatePostDto
    {
        [MaxLength(3000, ErrorMessage = "Content cannot exceed 3000 characters")]
        public string? Content { get; set; }

        public IFormFile? Image { get; set; }
    }
}