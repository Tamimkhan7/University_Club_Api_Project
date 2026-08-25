using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.LiveEvent
{
    public class MuteRequestDto
    {
        [Required]
        public bool Mute { get; set; }
    }

    public class KickRequestDto
    {


        public bool Ban { get; set; } = false;
    }

    public class LiveModerationStatusDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public bool IsMuted { get; set; }
        public bool IsBanned { get; set; }
    }
}