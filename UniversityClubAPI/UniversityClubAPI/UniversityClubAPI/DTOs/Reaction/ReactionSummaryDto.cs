using UniversityClubAPI.Enums;

namespace UniversityClubAPI.DTOs.Reaction
{


    public class ReactionSummaryDto
    {
        public int PostId { get; set; }
        public int Total { get; set; }
        public int Like { get; set; }
        public int Love { get; set; }
        public int Haha { get; set; }
        public int Wow { get; set; }
        public int Sad { get; set; }
        public int Angry { get; set; }


        public ReactionType? MyReaction { get; set; }
    }
}
