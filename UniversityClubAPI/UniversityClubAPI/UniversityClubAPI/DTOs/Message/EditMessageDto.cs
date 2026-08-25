using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Message
{
    public class EditMessageDto
    {
        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = null!;
    }
}