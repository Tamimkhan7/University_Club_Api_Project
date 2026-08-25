using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Poll
{
    public class PollResponseDto
    {
        public int Id { get; set; }

        public int ClubId { get; set; }
        public string? ClubName { get; set; }

        public int CreatedBy { get; set; }
        public string? CreatorName { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public PollType Type { get; set; }
        public bool IsMultipleChoice { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsClosed { get; set; }
        public bool IsActive { get; set; }

        public int TotalVotes { get; set; }
        public bool HasVoted { get; set; }

        public List<PollOptionResultDto> Options { get; set; } = new();
    }
}
