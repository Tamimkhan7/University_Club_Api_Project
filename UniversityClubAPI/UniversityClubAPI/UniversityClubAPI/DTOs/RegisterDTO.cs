using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs
{
    public class RegisterDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}