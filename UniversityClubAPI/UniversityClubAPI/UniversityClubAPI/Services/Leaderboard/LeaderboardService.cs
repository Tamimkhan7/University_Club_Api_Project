using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Leaderboard;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Services.AI;

namespace UniversityClubAPI.Services.LeaderboardService
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IGeminiService _geminiService;
        private readonly ILogger<LeaderboardService> _logger;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

        public LeaderboardService(
            AppDbContext context,
            IMemoryCache cache,
            IGeminiService geminiService,
            ILogger<LeaderboardService> logger)
        {
            _context = context;
            _cache = cache;
            _geminiService = geminiService;
            _logger = logger;
        }

        private static DateTime? PeriodStart(LeaderboardPeriod period) => period switch
        {
            LeaderboardPeriod.Weekly => DateTime.UtcNow.AddDays(-7),
            LeaderboardPeriod.Monthly => DateTime.UtcNow.AddDays(-30),
            _ => null
        };

        private static int ClampCount(int count) => Math.Clamp(count, 1, 100);

        private static string CacheKey(LeaderboardCategory category, LeaderboardPeriod period)
            => $"leaderboard:{category}:{period}";

        private async Task<List<LeaderboardEntryDto>> GetScoredListAsync(LeaderboardCategory category, LeaderboardPeriod period)
        {
            var cacheKey = CacheKey(category, period);

            if (_cache.TryGetValue(cacheKey, out List<LeaderboardEntryDto>? cached) && cached != null)
                return cached;

            var since = PeriodStart(period);

            var postCounts = await _context.Posts
                .Where(p => since == null || p.CreatedAt >= since)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var commentCounts = await _context.Comments
                .Where(c => since == null || c.CreatedAt >= since)
                .GroupBy(c => c.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var reactionsReceived = await _context.Reactions
                .Where(r => (since == null || r.CreatedAt >= since) && r.Post != null)
                .GroupBy(r => r.Post!.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var eventCounts = await _context.EventAttendances
                .Where(a => since == null || a.JoinedAt >= since)
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var badgeCounts = await _context.UserBadges
                .Where(b => since == null || b.EarnedAt >= since)
                .GroupBy(b => b.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var followerCounts = await _context.Follows
                .Where(f => since == null || f.CreatedAt >= since)
                .GroupBy(f => f.FollowingId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var involvedUserIds = postCounts.Keys
                .Union(commentCounts.Keys)
                .Union(reactionsReceived.Keys)
                .Union(eventCounts.Keys)
                .Union(badgeCounts.Keys)
                .Union(followerCounts.Keys)
                .ToList();

            if (involvedUserIds.Count == 0)
            {
                var empty = new List<LeaderboardEntryDto>();
                _cache.Set(cacheKey, empty, CacheDuration);
                return empty;
            }

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => involvedUserIds.Contains(u.Id) && !u.IsDeleted)
                .Select(u => new { u.Id, u.Name, u.ProfileImage, u.Department })
                .ToListAsync();

            int Get(Dictionary<int, int> dict, int userId) => dict.TryGetValue(userId, out var v) ? v : 0;

            var scored = users.Select(u =>
            {
                var posts = Get(postCounts, u.Id);
                var comments = Get(commentCounts, u.Id);
                var reactions = Get(reactionsReceived, u.Id);
                var events = Get(eventCounts, u.Id);
                var badges = Get(badgeCounts, u.Id);
                var followers = Get(followerCounts, u.Id);

                double points = category switch
                {
                    LeaderboardCategory.Posts => posts,
                    LeaderboardCategory.Events => events,
                    LeaderboardCategory.Badges => badges,
                    LeaderboardCategory.Followers => followers,
                    _ => posts * 3 + comments * 1.5 + reactions * 1 + events * 2 + badges * 5 + followers * 1
                };

                return new LeaderboardEntryDto
                {
                    UserId = u.Id,
                    UserName = u.Name,
                    ProfileImage = u.ProfileImage,
                    Department = u.Department,
                    Points = Math.Round(points, 1),
                    PostCount = posts,
                    EventCount = events,
                    BadgeCount = badges,
                    FollowerCount = followers,
                    IsCurrentUser = false
                };
            })
            .Where(e => e.Points > 0)
            .OrderByDescending(e => e.Points)
            .ToList();

            for (int i = 0; i < scored.Count; i++)
                scored[i].Rank = i + 1;

            _cache.Set(cacheKey, scored, CacheDuration);
            return scored;
        }

        private static LeaderboardEntryDto Clone(LeaderboardEntryDto e, int currentUserId) => new()
        {
            Rank = e.Rank,
            UserId = e.UserId,
            UserName = e.UserName,
            ProfileImage = e.ProfileImage,
            Department = e.Department,
            Points = e.Points,
            PostCount = e.PostCount,
            EventCount = e.EventCount,
            BadgeCount = e.BadgeCount,
            FollowerCount = e.FollowerCount,
            IsCurrentUser = e.UserId == currentUserId
        };

        public async Task<ApiResponse<LeaderboardResultDto>> GetLeaderboardAsync(
            int currentUserId, LeaderboardCategory category, LeaderboardPeriod period, int count = 20)
        {
            count = ClampCount(count);
            var scored = await GetScoredListAsync(category, period);

            var top = scored.Take(count).Select(e => Clone(e, currentUserId)).ToList();
            var myRaw = scored.FirstOrDefault(e => e.UserId == currentUserId);
            var myEntry = myRaw == null ? null : Clone(myRaw, currentUserId);

            return ApiResponse<LeaderboardResultDto>.Ok(new LeaderboardResultDto
            {
                Category = category,
                Period = period,
                TopEntries = top,
                MyEntry = myEntry
            });
        }

        public async Task<ApiResponse<LeaderboardEntryDto?>> GetMyLeaderboardEntryAsync(
            int currentUserId, LeaderboardCategory category, LeaderboardPeriod period)
        {
            var scored = await GetScoredListAsync(category, period);
            var mine = scored.FirstOrDefault(e => e.UserId == currentUserId);
            return ApiResponse<LeaderboardEntryDto?>.Ok(mine == null ? null : Clone(mine, currentUserId));
        }

        public async Task<ApiResponse<LeaderboardEntryDto?>> GetUserLeaderboardEntryAsync(
            int currentUserId, int targetUserId, LeaderboardCategory category, LeaderboardPeriod period)
        {
            var scored = await GetScoredListAsync(category, period);
            var entry = scored.FirstOrDefault(e => e.UserId == targetUserId);
            return ApiResponse<LeaderboardEntryDto?>.Ok(entry == null ? null : Clone(entry, currentUserId));
        }

        public async Task<ApiResponse<LeaderboardInsightDto>> GetLeaderboardInsightAsync(
            int currentUserId, LeaderboardCategory category, LeaderboardPeriod period)
        {
            var scored = await GetScoredListAsync(category, period);
            var myRaw = scored.FirstOrDefault(e => e.UserId == currentUserId);
            var myEntry = myRaw == null ? null : Clone(myRaw, currentUserId);

            var nextRaw = myRaw == null
                ? scored.OrderBy(e => e.Rank).FirstOrDefault()
                : scored.Where(e => e.Rank < myRaw.Rank).OrderByDescending(e => e.Rank).FirstOrDefault();

            var nextEntry = nextRaw == null ? null : Clone(nextRaw, currentUserId);

            int? gap = (myEntry != null && nextEntry != null)
                ? (int)Math.Ceiling(nextEntry.Points - myEntry.Points)
                : null;

            var prompt = BuildInsightPrompt(category, period, myEntry, nextEntry, gap);

            string? aiSuggestion = null;
            try
            {
                aiSuggestion = await _geminiService.GenerateTextAsync(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unexpected error requesting Gemini leaderboard insight, using fallback tip.");
            }

            var suggestion = string.IsNullOrWhiteSpace(aiSuggestion)
                ? BuildFallbackSuggestion(myEntry, nextEntry, gap)
                : aiSuggestion.Trim();

            return ApiResponse<LeaderboardInsightDto>.Ok(new LeaderboardInsightDto
            {
                MyEntry = myEntry,
                NextRankEntry = nextEntry,
                PointsToNextRank = gap,
                Suggestion = suggestion
            });
        }

        private static string BuildInsightPrompt(
            LeaderboardCategory category, LeaderboardPeriod period,
            LeaderboardEntryDto? myEntry, LeaderboardEntryDto? nextEntry, int? gap)
        {
            if (myEntry == null)
            {
                return $"A university club platform user has no leaderboard activity yet in the '{category}' " +
                       $"({period}) leaderboard. Write one short, encouraging sentence (max 25 words) suggesting " +
                       "a simple first action (like posting, joining an event, or commenting) to get started.";
            }

            if (nextEntry == null)
            {
                return $"A university club platform user is ranked #{myEntry.Rank} and currently #1 in the " +
                       $"'{category}' ({period}) leaderboard with {myEntry.Points} points. Write one short, " +
                       "congratulatory sentence (max 25 words) encouraging them to keep their lead.";
            }

            return $"A university club platform user is ranked #{myEntry.Rank} with {myEntry.Points} points in " +
                   $"the '{category}' ({period}) leaderboard. The user ranked #{nextEntry.Rank} just above them " +
                   $"has {nextEntry.Points} points ({gap} points ahead). Their current stats: {myEntry.PostCount} " +
                   $"posts, {myEntry.EventCount} events, {myEntry.BadgeCount} badges, {myEntry.FollowerCount} " +
                   "followers. Write one short, specific, motivating sentence (max 30 words) suggesting the most " +
                   "effective action to close the gap and move up a rank.";
        }

        private static string BuildFallbackSuggestion(
            LeaderboardEntryDto? myEntry, LeaderboardEntryDto? nextEntry, int? gap)
        {
            if (myEntry == null)
                return "You're not on the leaderboard yet — post something, join an event, or leave a comment to get on the board!";

            if (nextEntry == null)
                return "You're #1! Keep posting and engaging to hold onto the top spot.";

            return $"You're {gap} points behind #{nextEntry.Rank}. Try attending an event or posting more to close the gap.";
        }
    }
}