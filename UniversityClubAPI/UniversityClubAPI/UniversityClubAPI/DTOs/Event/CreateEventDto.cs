using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Event
{
    public class CreateEventDto
    {
        [Required]
        [MaxLength(150)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
        [Required]
        public int ClubId { get; set; }
    }
}
