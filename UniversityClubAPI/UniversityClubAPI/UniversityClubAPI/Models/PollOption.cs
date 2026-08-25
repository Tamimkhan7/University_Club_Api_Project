namespace UniversityClubAPI.Models
{
    public class PollOption
    {
        public int Id { get; set; }

        public int PollId { get; set; }
        public Poll? Poll { get; set; }

        public string Text { get; set; } = string.Empty;

        public ICollection<PollVote> Votes { get; set; } = new List<PollVote>();
    }
}
