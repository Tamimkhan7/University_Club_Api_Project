namespace UniversityClubAPI.DTOs.Poll
{
    public class PollOptionResultDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int VoteCount { get; set; }
        public double Percentage { get; set; }
        public bool VotedByMe { get; set; }
    }
}
