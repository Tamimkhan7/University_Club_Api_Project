using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.File
{
    public class FileUploadDto
    {
        [Required]
        public IFormFile? File { get; set; }

    }
}
