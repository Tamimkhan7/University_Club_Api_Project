using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs
{
    public class SetGroupAdminDto
    {
        [Required]
        public int UserId { get; set; }
        [Required]
        public bool IsAdmin { get; set; }
    }
}
