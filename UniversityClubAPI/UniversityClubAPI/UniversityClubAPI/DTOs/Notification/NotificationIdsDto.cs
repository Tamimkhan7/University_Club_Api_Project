using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Notification
{
    public class NotificationIdsDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one notification ID is required")]
        public List<int> NotificationIds { get; set; } = new();
    }
}
