using UniversityClubAPI.DTOs.Common;

namespace UniversityClubAPI.DTOs.Message
{



    public class MessageQueryDto : PaginationParamsDto
    {

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Search { get; set; }
        public string SortOrder { get; set; } = "asc";
    }
}
