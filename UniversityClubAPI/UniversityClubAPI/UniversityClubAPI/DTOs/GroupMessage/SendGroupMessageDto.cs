using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.GroupMessage
{
    public class SendGroupMessageDto
    {
        [Required]
        public int GroupId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = null!;
    }
}