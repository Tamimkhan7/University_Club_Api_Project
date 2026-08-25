using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Message
{
    public class SendMessageDto
    {
        [Required]
        public int ReceiverId { get; set; }
        [Required]
        [MaxLength(1000)]
        public string? Text { get; set; }
    }
}
