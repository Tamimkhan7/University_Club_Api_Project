using UniversityClubAPI.DTOs.Common;

namespace UniversityClubAPI.DTOs.Notification
{
    public class NotificationQueryDto : PaginationParamsDto
    {
        public string? Type { get; set; }
    }
}
