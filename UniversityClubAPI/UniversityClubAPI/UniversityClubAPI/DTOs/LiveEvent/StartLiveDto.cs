using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.LiveEvent
{
    public class StartLiveDto
    {
        [Required, StringLength(500), Url]
        public string MeetingLink { get; set; } = string.Empty;
    }
}
