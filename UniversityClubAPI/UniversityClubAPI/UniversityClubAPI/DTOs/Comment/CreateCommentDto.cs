using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Comment
{
    public class CreateCommentDto
    {
        public int PostId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public int? ParentCommentId { get; set; }
    }
}