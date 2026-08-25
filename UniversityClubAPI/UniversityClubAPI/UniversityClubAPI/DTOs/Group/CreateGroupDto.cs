using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Group
{
    public class CreateGroupDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        public List<int> MemberIds { get; set; } = new();
    }
}