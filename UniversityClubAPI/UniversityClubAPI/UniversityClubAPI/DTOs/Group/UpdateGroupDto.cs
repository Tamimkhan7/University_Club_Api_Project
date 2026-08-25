using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Group
{
    public class UpdateGroupDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
    }
}
