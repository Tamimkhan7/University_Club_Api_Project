using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.User
{
    public class UpdateUserDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(50)]
        public string? Batch { get; set; }

        [StringLength(50)]
        public string? UserName { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public IFormFile? CoverPhoto { get; set; }
    }
}
