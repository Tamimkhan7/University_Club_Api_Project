using UniversityClubAPI.DTOs.Common;

namespace UniversityClubAPI.DTOs.Post
{
    public class PostQueryDto : PaginationParamsDto
    {

        public int? ClubId { get; set; }

        public int? UserId { get; set; }

        public string? Query { get; set; }
    }
}
