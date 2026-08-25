using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Group
{
    public class AddGroupMemberDto
    {
        [Required]
        public int UserId { get; set; }
    }
}