using System.ComponentModel.DataAnnotations;
namespace UniversityClubAPI.DTOs.VoiceMessage
{
    public class SendVoiceMessageDto
    {
        [Required(ErrorMessage = "Audio file is required.")]
        public IFormFile Audio { get; set; } = null!;
        [Range(1, 600, ErrorMessage = "Voice messages must be between 1 and 600 seconds.")]
        public int DurationSeconds { get; set; }
    }
}