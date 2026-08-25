namespace UniversityClubAPI.DTOs.Badge
{
    public class BadgeHoldersResponseDto
    {
        public string BadgeCode { get; set; } = string.Empty;
        public string BadgeName { get; set; } = string.Empty;
        public string IconEmoji { get; set; } = string.Empty;
        public PagedResultDto<BadgeHolderDto> Holders { get; set; } = new();
    }
}
