using UniversityClubAPI.DTOs.Feed;

namespace UniversityClubAPI.DTOs.Post
{
    public class TrendingPostDto : FeedItemDto
    {
        public int TrendingScore { get; set; }
    }
}
