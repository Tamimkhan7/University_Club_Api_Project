using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Club
{
    public class CreateClubDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(50)]
        public string? Description { get; set; }
    }
}
