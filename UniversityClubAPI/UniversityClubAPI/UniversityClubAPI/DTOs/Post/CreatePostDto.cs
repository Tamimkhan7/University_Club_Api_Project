using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Post
{
    public class CreatePostDto
    {
        [Required(ErrorMessage = "ClubId is required")]
        public int ClubId { get; set; }

        [MaxLength(3000, ErrorMessage = "Content cannot exceed 300 characters")]
        public string? Content { get; set; }

        public IFormFile? Image { get; set; }
    }
}