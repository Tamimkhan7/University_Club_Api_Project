using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.ClubPrivacy
{
    public class CreateInviteDto
    {
        [Required]
        public int InvitedUserId { get; set; }
    }
}
