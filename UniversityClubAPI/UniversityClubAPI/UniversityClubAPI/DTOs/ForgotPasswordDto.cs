using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}