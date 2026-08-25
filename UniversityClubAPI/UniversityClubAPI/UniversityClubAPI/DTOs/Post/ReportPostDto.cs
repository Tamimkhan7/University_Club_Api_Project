using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Post
{
    public class ReportPostDto
    {
        [Required]
        public int PostId { get; set; }
        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
