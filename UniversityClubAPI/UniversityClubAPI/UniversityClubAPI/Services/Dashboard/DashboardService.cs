using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<object>> GetSummaryAsync(int userId)
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalPosts = await _context.Posts.CountAsync();
            var totalClubs = await _context.Clubs.CountAsync();
            var totalComments = await _context.Comments.CountAsync();
            var totalReactions = await _context.Reactions.CountAsync();

            var myPosts = await _context.Posts
                .CountAsync(x => x.UserId == userId);

            var myClubs = await _context.ClubMembers
                .CountAsync(x => x.UserId == userId);


            return ApiResponse<object>.Ok(new
            {
                totalUsers,
                totalPosts,
                totalClubs,
                totalComments,
                totalReactions,
                myPosts,
                myClubs
            });
        }

        public async Task<ApiResponse<object>> GetRecentPostsAsync(int userId)
        {
            var posts = await _context.Posts
                .AsNoTracking()
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new
                {
                    x.Id,
                    x.Content,
                    x.CreatedAt,
                    UserName = x.User != null ? x.User.Name : "Unknown",
                    UserImage = x.User != null ? x.User.ProfileImage : null
                })
                .ToListAsync();

            return ApiResponse<object>.Ok(posts);
        }

        public async Task<ApiResponse<object>> GetRecentClubsAsync(int userId)
        {
            var clubs = await _context.Clubs
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Take(10)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Description,
                    x.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<object>.Ok(clubs);
        }

        public async Task<ApiResponse<object>> GetAiInsightAsync(int userId)
        {
            var myPosts = await _context.Posts.CountAsync(x => x.UserId == userId);
            var myClubs = await _context.ClubMembers.CountAsync(x => x.UserId == userId);

            var totalPosts = await _context.Posts.CountAsync();
            var totalUsers = await _context.Users.CountAsync();

            string insight;

            if (myPosts == 0)
                insight = "You haven't posted anything yet. Start engaging with the community!";
            else if (myPosts < 5)
                insight = "You are getting started. Try posting more to grow your presence.";
            else if (myPosts < 20)
                insight = "Good activity! You are an active member of the platform.";
            else
                insight = "Excellent! You are a top contributor in the community.";

            return ApiResponse<object>.Ok(new
            {
                insight,
                stats = new
                {
                    myPosts,
                    myClubs,
                    totalPosts,
                    totalUsers
                }
            });
        }

        public async Task<ApiResponse<object>> GetStatsAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var last7Days = now.AddDays(-7);


            var totalPosts = await _context.Posts.CountAsync();

            var totalClubs = await _context.Clubs.CountAsync();

            var totalComments = await _context.Comments.CountAsync();

            var totalReactions = await _context.Reactions.CountAsync();


            var myPosts = await _context.Posts
                .CountAsync(x => x.UserId == userId);


            var myClubs = await _context.ClubMembers
                .CountAsync(x => x.UserId == userId);


            var newUsers = await _context.Users
                .CountAsync(x => x.CreatedAt >= last7Days);


            var newPosts = await _context.Posts
                .CountAsync(x => x.CreatedAt >= last7Days);



            return ApiResponse<object>.Ok(new
            {
                totalPosts,
                totalClubs,
                totalComments,
                totalReactions,

                myPosts,
                myClubs,

                recentActivity = new
                {
                    newUsers,
                    newPosts
                }
            });
        }
        public async Task<ApiResponse<object>> GetTrendingPostsAsync()
        {
            var posts = await _context.Posts
                .AsNoTracking()
                .Select(p => new
                {
                    p.Id,
                    Content = p.Content ?? "",
                    p.CreatedAt,
                    UserName = p.User != null ? p.User.Name : "Unknown",
                    UserImage = p.User != null ? p.User.ProfileImage : null,
                    ReactionCount = p.Reactions.Count(),
                    CommentCount = p.Comments.Count()
                })
                .OrderByDescending(x => x.ReactionCount)
                .ThenByDescending(x => x.CommentCount)
                .ThenByDescending(x => x.CreatedAt)
                .Take(5)
                .ToListAsync();

            return ApiResponse<object>.Ok(posts);
        }
    }
}