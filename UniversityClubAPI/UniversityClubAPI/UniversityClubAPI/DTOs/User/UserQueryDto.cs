using UniversityClubAPI.DTOs.Common;

namespace UniversityClubAPI.DTOs.User
{
    public class UserQueryDto : PaginationParamsDto
    {
        public string? Query { get; set; }
        public string? Department { get; set; }
        public string? Batch { get; set; }
    }
}
