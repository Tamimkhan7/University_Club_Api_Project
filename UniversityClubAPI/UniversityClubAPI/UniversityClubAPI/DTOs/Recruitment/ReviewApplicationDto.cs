using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Recruitment
{
    public class ReviewApplicationDto
    {
        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
        public string? Note { get; set; }
    }
}
