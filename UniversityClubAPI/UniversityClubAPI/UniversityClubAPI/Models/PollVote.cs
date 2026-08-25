namespace UniversityClubAPI.Models
{
    public class PollVote
    {
        public int Id { get; set; }

        public int PollId { get; set; }
        public Poll? Poll { get; set; }

        public int PollOptionId { get; set; }
        public PollOption? PollOption { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime VotedAt { get; set; } = DateTime.UtcNow;
    }
}
