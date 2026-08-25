using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using System.Text;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.DTOs.Recommendation;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.AI;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.RecommendationService
{
    public class RecommendationService : IRecommendationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecommendationService> _logger;
        private readonly IGeminiService _geminiService;
        private readonly IMemoryCache _cache;
        private readonly INotificationService _notificationService;

        private const double WeightFriendInClub = 3.0;
        private const double WeightSameDepartment = 2.0;
        private const double WeightPopularity = 1.0;

        private const double WeightMyClubEvent = 5.0;
        private const double WeightFriendAttending = 2.0;
        private const double WeightSoonness = 1.5;

        private const double WeightMutualFollow = 3.0;
        private const double WeightSameDepartmentPerson = 1.5;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);
        private const int MaxNotificationMessageLength = 480;

        private static readonly ConcurrentDictionary<int, int> _cacheVersions = new();

        public RecommendationService(
            AppDbContext context,
            ILogger<RecommendationService> logger,
            IGeminiService geminiService,
            IMemoryCache cache,
            INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _geminiService = geminiService;
            _cache = cache;
            _notificationService = notificationService;
        }

        private static int GetCacheVersion(int userId) => _cacheVersions.GetOrAdd(userId, 0);

        private static void BumpCacheVersion(int userId) =>
            _cacheVersions.AddOrUpdate(userId, 1, (_, v) => v + 1);

        public async Task<ApiResponse<List<ClubRecommendationDto>>> GetRecommendedClubsAsync(int userId, int count = 10)
        {
            var version = GetCacheVersion(userId);
            var cacheKey = $"club-recs:{userId}:v{version}:{count}";
            if (_cache.TryGetValue(cacheKey, out List<ClubRecommendationDto>? cached) && cached != null)
                return ApiResponse<List<ClubRecommendationDto>>.Ok(cached);

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return ApiResponse<List<ClubRecommendationDto>>.Fail("User not found.");

            var joinedClubIds = await _context.ClubMembers
                .Where(x => x.UserId == userId)
                .Select(x => x.ClubId)
                .ToListAsync();

            var dismissedClubIds = await _context.ClubRecommendationDismissals
                .Where(x => x.UserId == userId)
                .Select(x => x.ClubId)
                .ToListAsync();

            var followingIds = await _context.Follows
                .Where(x => x.FollowerId == userId)
                .Select(x => x.FollowingId)
                .ToListAsync();

            var normalizedDept = string.IsNullOrWhiteSpace(user.Department) ? null : user.Department.ToLower();

            var candidateClubs = await _context.Clubs
                .AsNoTracking()
                .Where(c => !joinedClubIds.Contains(c.Id) && !dismissedClubIds.Contains(c.Id))
                .Select(c => new
                {
                    Club = c,
                    TotalMembers = c.Members!.Count,
                    FriendMemberCount = c.Members!.Count(m => followingIds.Contains(m.UserId)),
                    SameDeptCount = normalizedDept == null
                        ? 0
                        : c.Members!.Count(m => m.User != null && m.User.Department.ToLower() == normalizedDept)
                })
                .ToListAsync();

            var results = new List<ClubRecommendationDto>();

            foreach (var item in candidateClubs)
            {
                var club = item.Club;
                var totalMembers = item.TotalMembers;
                var friendMemberCount = item.FriendMemberCount;
                var sameDeptCount = item.SameDeptCount;

                var popularityScore = Math.Log(totalMembers + 1);

                var score =
                    friendMemberCount * WeightFriendInClub +
                    sameDeptCount * WeightSameDepartment +
                    popularityScore * WeightPopularity;

                if (score <= 0 && totalMembers == 0)
                    continue;

                string reason;
                if (friendMemberCount > 0)
                    reason = friendMemberCount == 1
                        ? "1 person you follow is a member"
                        : $"{friendMemberCount} people you follow are members";
                else if (sameDeptCount > 0)
                    reason = $"Popular among {user.Department} students";
                else
                    reason = totalMembers > 0
                        ? $"Trending club with {totalMembers} members"
                        : "New club you might like";

                results.Add(new ClubRecommendationDto
                {
                    ClubId = club.Id,
                    ClubName = club.Name,
                    Description = club.Description,
                    MemberCount = totalMembers,
                    Reason = reason,
                    Score = Math.Round(score, 2)
                });
            }

            var top = results.OrderByDescending(x => x.Score).Take(count).ToList();
            _cache.Set(cacheKey, top, CacheDuration);
            return ApiResponse<List<ClubRecommendationDto>>.Ok(top);
        }

        public async Task<ApiResponse<List<EventRecommendationDto>>> GetRecommendedEventsAsync(int userId, int count = 10)
        {
            var version = GetCacheVersion(userId);
            var cacheKey = $"event-recs:{userId}:v{version}:{count}";
            if (_cache.TryGetValue(cacheKey, out List<EventRecommendationDto>? cached) && cached != null)
                return ApiResponse<List<EventRecommendationDto>>.Ok(cached);

            var now = DateTime.UtcNow;

            var myClubIds = await _context.ClubMembers
                .Where(x => x.UserId == userId)
                .Select(x => x.ClubId)
                .ToListAsync();

            var followingIds = await _context.Follows
                .Where(x => x.FollowerId == userId)
                .Select(x => x.FollowingId)
                .ToListAsync();

            var joinedEventIds = await _context.EventAttendances
                .Where(x => x.UserId == userId)
                .Select(x => x.EventId)
                .ToListAsync();

            var upcomingEvents = await _context.Events
                .AsNoTracking()
                .Include(e => e.club)
                .Include(e => e.Attendances)
                .Where(e => e.EventDate >= now && !joinedEventIds.Contains(e.Id))
                .ToListAsync();

            var results = new List<EventRecommendationDto>();

            foreach (var ev in upcomingEvents)
            {
                var isMyClub = myClubIds.Contains(ev.ClubId);
                var friendAttendeeCount = ev.Attendances.Count(a => followingIds.Contains(a.UserId));

                if (!isMyClub && friendAttendeeCount == 0)
                    continue;

                var daysUntil = Math.Max(1, (ev.EventDate - now).TotalDays);
                var soonnessScore = 1.0 / daysUntil;

                var score =
                    (isMyClub ? WeightMyClubEvent : 0) +
                    friendAttendeeCount * WeightFriendAttending +
                    soonnessScore * WeightSoonness;

                string reason;
                if (isMyClub && friendAttendeeCount > 0)
                    reason = $"In your club, and {friendAttendeeCount} people you follow are attending";
                else if (isMyClub)
                    reason = "Upcoming event in a club you're a member of";
                else
                    reason = friendAttendeeCount == 1
                        ? "1 person you follow is attending"
                        : $"{friendAttendeeCount} people you follow are attending";

                results.Add(new EventRecommendationDto
                {
                    EventId = ev.Id,
                    Title = ev.Title,
                    EventDate = ev.EventDate,
                    ClubId = ev.ClubId,
                    ClubName = ev.club?.Name,
                    Reason = reason,
                    Score = Math.Round(score, 2)
                });
            }

            var top = results.OrderByDescending(x => x.Score).ThenBy(x => x.EventDate).Take(count).ToList();
            _cache.Set(cacheKey, top, CacheDuration);
            return ApiResponse<List<EventRecommendationDto>>.Ok(top);
        }

        public async Task<ApiResponse<List<PersonRecommendationDto>>> GetRecommendedPeopleAsync(int userId, int count = 10)
        {
            var version = GetCacheVersion(userId);
            var cacheKey = $"people-recs:{userId}:v{version}:{count}";
            if (_cache.TryGetValue(cacheKey, out List<PersonRecommendationDto>? cached) && cached != null)
                return ApiResponse<List<PersonRecommendationDto>>.Ok(cached);

            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return ApiResponse<List<PersonRecommendationDto>>.Fail("User not found.");

            var followingIds = await _context.Follows
                .Where(x => x.FollowerId == userId)
                .Select(x => x.FollowingId)
                .ToListAsync();

            var blockedByMeIds = await _context.BlockedUsers
                .Where(x => x.BlockerId == userId)
                .Select(x => x.BlockedUserId)
                .ToListAsync();

            var blockedMeIds = await _context.BlockedUsers
                .Where(x => x.BlockedUserId == userId)
                .Select(x => x.BlockerId)
                .ToListAsync();

            var excludedIds = followingIds
                .Concat(blockedByMeIds)
                .Concat(blockedMeIds)
                .Distinct()
                .ToList();

            var normalizedDept = string.IsNullOrWhiteSpace(user.Department) ? null : user.Department.ToLower();

            var candidates = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id != userId && !excludedIds.Contains(u.Id))
                .Select(u => new
                {
                    Candidate = u,
                    MutualCount = _context.Follows.Count(f => f.FollowingId == u.Id && followingIds.Contains(f.FollowerId)),
                    SameDept = normalizedDept != null && u.Department != null && u.Department.ToLower() == normalizedDept
                })
                .Where(x => x.MutualCount > 0 || x.SameDept)
                .ToListAsync();

            var results = new List<PersonRecommendationDto>();

            foreach (var item in candidates)
            {
                var score =
                    item.MutualCount * WeightMutualFollow +
                    (item.SameDept ? WeightSameDepartmentPerson : 0);

                string reason;
                if (item.MutualCount > 0)
                    reason = item.MutualCount == 1
                        ? "Followed by 1 person you follow"
                        : $"Followed by {item.MutualCount} people you follow";
                else
                    reason = $"Also in {item.Candidate.Department}";

                results.Add(new PersonRecommendationDto
                {
                    UserId = item.Candidate.Id,
                    FullName = item.Candidate.Name,
                    Department = item.Candidate.Department,
                    MutualFollowCount = item.MutualCount,
                    Reason = reason,
                    Score = Math.Round(score, 2)
                });
            }

            var top = results.OrderByDescending(x => x.Score).Take(count).ToList();
            _cache.Set(cacheKey, top, CacheDuration);
            return ApiResponse<List<PersonRecommendationDto>>.Ok(top);
        }

        public async Task<ApiResponse<bool>> DismissClubRecommendationAsync(int userId, int clubId)
        {
            var alreadyDismissed = await _context.ClubRecommendationDismissals
                .AnyAsync(x => x.UserId == userId && x.ClubId == clubId);

            if (!alreadyDismissed)
            {
                _context.ClubRecommendationDismissals.Add(new ClubRecommendationDismissal
                {
                    UserId = userId,
                    ClubId = clubId
                });

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogWarning(ex, "Duplicate dismiss for user {UserId} club {ClubId} - already recorded.", userId, clubId);
                }
            }

            BumpCacheVersion(userId);

            return ApiResponse<bool>.Ok(true, "Recommendation dismissed.");
        }

        public async Task<ApiResponse<SmartDigestResultDto>> RunSmartDigestAsync(int userId)
        {
            var clubsResult = await GetRecommendedClubsAsync(userId, 1);
            var eventsResult = await GetRecommendedEventsAsync(userId, 1);

            var topClub = clubsResult.Data?.FirstOrDefault();
            var topEvent = eventsResult.Data?.FirstOrDefault();

            var cutoff = DateTime.UtcNow.AddHours(-24);
            var recentlyNotified = await _context.Notifications
                .AnyAsync(x => x.ReceiverId == userId &&
                               x.Type == NotificationType.SmartRecommendation &&
                               x.CreatedAt >= cutoff);

            var digest = new SmartDigestResultDto
            {
                TopClub = topClub,
                TopEvent = topEvent,
                NotificationSent = false
            };

            if (recentlyNotified || (topClub == null && topEvent == null))
                return ApiResponse<SmartDigestResultDto>.Ok(digest);

            var message = await BuildDigestMessageAsync(topClub, topEvent);

            await _notificationService.CreateAndPushAsync(new CreateNotificationDto
            {
                SenderId = userId,
                ReceiverId = userId,
                Type = NotificationType.SmartRecommendation,
                Message = message
            }, allowSelfNotify: true);

            digest.NotificationSent = true;

            _logger.LogInformation("Smart digest sent to user {UserId}", userId);
            return ApiResponse<SmartDigestResultDto>.Ok(digest, "Smart digest generated.");
        }

        private async Task<string> BuildDigestMessageAsync(ClubRecommendationDto? topClub, EventRecommendationDto? topEvent)
        {
            var fallbackParts = new List<string>();
            if (topClub != null)
                fallbackParts.Add($"Check out \"{topClub.ClubName}\" — {topClub.Reason}");
            if (topEvent != null)
                fallbackParts.Add($"Don't miss \"{topEvent.Title}\" — {topEvent.Reason}");
            var fallbackMessage = string.Join(" | ", fallbackParts);

            var prompt = BuildGeminiPrompt(topClub, topEvent);
            var aiMessage = await _geminiService.GenerateTextAsync(prompt);

            var finalMessage = string.IsNullOrWhiteSpace(aiMessage) ? fallbackMessage : aiMessage;
            return Truncate(finalMessage, MaxNotificationMessageLength);
        }

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text[..(maxLength - 1)].TrimEnd() + "…";
        }

        private static string BuildGeminiPrompt(ClubRecommendationDto? topClub, EventRecommendationDto? topEvent)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Write one short, friendly push-notification message (max 2 sentences, no emojis, no quotation marks) for a university student, based on this data:");
            if (topClub != null)
                sb.AppendLine($"- Recommended club: \"{topClub.ClubName}\" ({topClub.MemberCount} members). Reason: {topClub.Reason}");
            if (topEvent != null)
                sb.AppendLine($"- Recommended event: \"{topEvent.Title}\" on {topEvent.EventDate:MMM d}. Reason: {topEvent.Reason}");
            sb.AppendLine("Return only the message text, nothing else.");
            return sb.ToString();
        }
    }
}