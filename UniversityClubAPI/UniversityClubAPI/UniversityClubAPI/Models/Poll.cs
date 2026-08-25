using UniversityClubAPI.Enums;

namespace UniversityClubAPI.Models
{
    public class Poll
    {
        public int Id { get; set; }

        public int ClubId { get; set; }
        public Club? Club { get; set; }

        public int CreatedBy { get; set; }
        public User? Creator { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public PollType Type { get; set; } = PollType.General;


        public bool IsMultipleChoice { get; set; } = false;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }


        public bool IsClosed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
    }
}
