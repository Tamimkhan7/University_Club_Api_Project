using System.ComponentModel.DataAnnotations;
namespace UniversityClubAPI.DTOs.Recruitment
{
    public class CreateApplicationDto
    {
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string? Message { get; set; }
    }
}