using System.ComponentModel.DataAnnotations;

namespace UniversityClubAPI.DTOs.Common
{
    public class PaginationParamsDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "page must be at least 1")]
        public int Page { get; set; } = 1;
        [Range(1, 100, ErrorMessage = "pageSize must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        public int Skip => (Page - 1) * PageSize;
    }
}
