using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Presence
{
    public class BulkPresenceRequestDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one user id is required.")]
        [MaxLength(100, ErrorMessage = "You can request presence for a maximum of 100 users at a time.")]
        public List<int> UserIds { get; set; } = new();
    }
}
