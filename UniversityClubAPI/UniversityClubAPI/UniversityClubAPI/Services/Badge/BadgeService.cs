using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Badge;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.AI;
using UniversityClubAPI.Services.NotificationService;
using BadgeModel = UniversityClubAPI.Models.Badge;

namespace UniversityClubAPI.Services.BadgeService
{
    public class BadgeService : IBadgeService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BadgeService> _logger;
        private readonly IGeminiService _geminiService;
        private readonly INotificationService _notificationService;


        private static readonly (string Code, string Name, string Description, string Icon, BadgeCategory Category)[] DefaultBadges =
        {
            (Enums.BadgeCode.FirstPost, "First Post", "Made your very first post.", "📝", BadgeCategory.Contribution),
            (Enums.BadgeCode.ActiveContributor, "Active Contributor", "Created 10 or more posts.", "✍️", BadgeCategory.Contribution),
            (Enums.BadgeCode.EventEnthusiast, "Event Enthusiast", "Attended 5 or more events.", "🎉", BadgeCategory.Participation),
            (Enums.BadgeCode.SuperAttendee, "Super Attendee", "Attended 20 or more events.", "🌟", BadgeCategory.Participation),
            (Enums.BadgeCode.ClubFounder, "Club Founder", "Created a club on the platform.", "🏛️", BadgeCategory.Leadership),
            (Enums.BadgeCode.SocialButterfly, "Social Butterfly", "Reached 10 or more followers.", "🦋", BadgeCategory.Social),
            (Enums.BadgeCode.PollParticipant, "Poll Participant", "Voted in 5 or more polls.", "🗳️", BadgeCategory.Participation),
            (Enums.BadgeCode.TopContributor, "Top Contributor", "Most active member in a club.", "🏆", BadgeCategory.Special),
        };


        private sealed record UserStatsSnapshot(
            int PostCount,
            int EventCount,
            bool CreatedClub,
            int FollowerCount,
            int PollVoteCount);

        public BadgeService(AppDbContext context, ILogger<BadgeService> logger, IGeminiService geminiService, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _geminiService = geminiService;
            _notificationService = notificationService;
        }

        public async Task SeedDefaultBadgesAsync()
        {
            var existingCodes = await _context.Badges.Select(x => x.Code).ToListAsync();

            foreach (var b in DefaultBadges)
            {
                if (existingCodes.Contains(b.Code)) continue;

                _context.Badges.Add(new BadgeModel
                {
                    Code = b.Code,
                    Name = b.Name,
                    Description = b.Description,
                    IconEmoji = b.Icon,
                    Category = b.Category
                });
            }

            await _context.SaveChangesAsync();
        }

        private static UserBadgeDto ToDto(UserBadge ub) => new()
        {
            Id = ub.Id,
            BadgeCode = ub.Badge?.Code ?? string.Empty,
            BadgeName = ub.Badge?.Name ?? string.Empty,
            IconEmoji = ub.Badge?.IconEmoji ?? string.Empty,
            Category = ub.Badge?.Category ?? BadgeCategory.Participation,
            ClubId = ub.ClubId,
            ClubName = ub.Club?.Name,
            EarnedAt = ub.EarnedAt
        };

        public async Task<ApiResponse<List<BadgeDto>>> GetCatalogAsync(int userId)
        {
            var allBadges = await _context.Badges.AsNoTracking().OrderBy(x => x.Category).ToListAsync();

            var earned = await _context.UserBadges
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.ClubId == null)
                .ToDictionaryAsync(x => x.BadgeId, x => x.EarnedAt);

            var result = allBadges.Select(b => new BadgeDto
            {
                Id = b.Id,
                Code = b.Code,
                Name = b.Name,
                Description = b.Description,
                IconEmoji = b.IconEmoji,
                Category = b.Category,
                Earned = earned.ContainsKey(b.Id),
                EarnedAt = earned.TryGetValue(b.Id, out var date) ? date : null
            }).ToList();

            return ApiResponse<List<BadgeDto>>.Ok(result);
        }

        public async Task<ApiResponse<List<UserBadgeDto>>> GetMyBadgesAsync(int userId)
            => await GetUserBadgesAsync(userId);

        public async Task<ApiResponse<List<UserBadgeDto>>> GetUserBadgesAsync(int targetUserId)
        {
            var badges = await _context.UserBadges
                .AsNoTracking()
                .Include(x => x.Badge)
                .Include(x => x.Club)
                .Where(x => x.UserId == targetUserId)
                .OrderByDescending(x => x.EarnedAt)
                .ToListAsync();

            return ApiResponse<List<UserBadgeDto>>.Ok(badges.Select(ToDto).ToList());
        }


        private async Task<UserStatsSnapshot> GetUserStatsAsync(int userId)
        {
            var postCount = await _context.Posts.CountAsync(x => x.UserId == userId);
            var eventCount = await _context.EventAttendances.CountAsync(x => x.UserId == userId);
            var createdClub = await _context.Clubs.AnyAsync(x => x.CreatedBy == userId);
            var followerCount = await _context.Follows.CountAsync(x => x.FollowingId == userId);
            var pollVoteCount = await _context.PollVotes
                .Where(x => x.UserId == userId)
                .Select(x => x.PollId)
                .Distinct()
                .CountAsync();

            return new UserStatsSnapshot(postCount, eventCount, createdClub, followerCount, pollVoteCount);
        }

        private async Task<string> BuildCongratsMessageAsync(string badgeName, string icon)
        {
            var fallback = $"You earned the \"{badgeName}\" badge {icon}";
            try
            {
                var prompt =
                    $"Write one short, upbeat congratulations notification (max 20 words) for a " +
                    $"university club app user who just earned the \"{badgeName}\" badge. " +
                    "No quotes, no hashtags, just the sentence.";

                var generated = await _geminiService.GenerateTextAsync(prompt);
                return string.IsNullOrWhiteSpace(generated) ? fallback : generated.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Gemini congrats message generation failed for badge {BadgeName}; using fallback text.", badgeName);
                return fallback;
            }
        }

        private async Task<UserBadge?> AwardIfMissingAsync(int userId, string badgeCode, int? clubId = null)
        {
            var badge = await _context.Badges.FirstOrDefaultAsync(x => x.Code == badgeCode);
            if (badge == null) return null;

            var alreadyHas = await _context.UserBadges
                .AnyAsync(x => x.UserId == userId && x.BadgeId == badge.Id && x.ClubId == clubId);

            if (alreadyHas) return null;

            var userBadge = new UserBadge
            {
                UserId = userId,
                BadgeId = badge.Id,
                ClubId = clubId
            };

            _context.UserBadges.Add(userBadge);

            var message = await BuildCongratsMessageAsync(badge.Name, badge.IconEmoji);

            await _notificationService.CreateAndPushAsync(new CreateNotificationDto
            {
                SenderId = userId,
                ReceiverId = userId,
                Type = NotificationType.BadgeEarned,
                Message = message
            }, allowSelfNotify: true);

            userBadge.Badge = badge;
            return userBadge;
        }

        public async Task<ApiResponse<List<UserBadgeDto>>> EvaluateAsync(int userId)
        {
            await SeedDefaultBadgesAsync();

            var newlyAwarded = new List<UserBadge>();
            var stats = await GetUserStatsAsync(userId);

            if (stats.PostCount >= 1)
            {
                var b = await AwardIfMissingAsync(userId, Enums.BadgeCode.FirstPost);
                if (b != null) newlyAwarded.Add(b);
            }
            if (stats.PostCount >= 10)
            {
                var b = await AwardIfMissingAsync(userId, Enums.BadgeCode.ActiveContributor);
                if (b != null) newlyAwarded.Add(b);
            }

            if (stats.EventCount >= 5)
            {
                var b = await AwardIfMissingAsync(userId, Enums.BadgeCode.EventEnthusiast);
                if (b != null) newlyAwarded.Add(b);
            }
            if (stats.EventCount >= 20)
            {
                var b = await AwardIfMissingAsync(userId, Enums.BadgeCode.SuperAttendee);
                if (b != null) newlyAwarded.Add(b);
            }

            if (stats.CreatedClub)
            {
                var b = await AwardIfMissingAsync(userId, Enums.BadgeCode.ClubFounder);
                if (b != null) newlyAwarded.Add(b);
            }

            if (stats.FollowerCount >= 10)
            {
                var b = await AwardIfMissingAsync(userId, Enums.BadgeCode.SocialButterfly);
                if (b != null) newlyAwarded.Add(b);
            }

            if (stats.PollVoteCount >= 5)
            {
                var b = await AwardIfMissingAsync(userId, Enums.BadgeCode.PollParticipant);
                if (b != null) newlyAwarded.Add(b);
            }

            if (newlyAwarded.Count > 0)
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User {UserId} earned {Count} new badge(s)", userId, newlyAwarded.Count);
            }

            return ApiResponse<List<UserBadgeDto>>.Ok(
                newlyAwarded.Select(ToDto).ToList(),
                newlyAwarded.Count > 0 ? $"{newlyAwarded.Count} new badge(s) earned!" : "No new badges yet.");
        }
        public async Task<ApiResponse<List<BadgeProgressDto>>> GetProgressAsync(int userId)
        {
            await SeedDefaultBadgesAsync();

            var stats = await GetUserStatsAsync(userId);

            var earnedCodes = (await _context.UserBadges
                .Where(x => x.UserId == userId && x.ClubId == null)
                .Join(_context.Badges, ub => ub.BadgeId, b => b.Id, (ub, b) => b.Code)
                .ToListAsync())
                .ToHashSet();

            var progress = new List<BadgeProgressDto>
            {
                BuildProgress(Enums.BadgeCode.FirstPost, "First Post", "📝", stats.PostCount, 1, earnedCodes),
                BuildProgress(Enums.BadgeCode.ActiveContributor, "Active Contributor", "✍️", stats.PostCount, 10, earnedCodes),
                BuildProgress(Enums.BadgeCode.EventEnthusiast, "Event Enthusiast", "🎉", stats.EventCount, 5, earnedCodes),
                BuildProgress(Enums.BadgeCode.SuperAttendee, "Super Attendee", "🌟", stats.EventCount, 20, earnedCodes),
                BuildProgress(Enums.BadgeCode.ClubFounder, "Club Founder", "🏛️", stats.CreatedClub ? 1 : 0, 1, earnedCodes),
                BuildProgress(Enums.BadgeCode.SocialButterfly, "Social Butterfly", "🦋", stats.FollowerCount, 10, earnedCodes),
                BuildProgress(Enums.BadgeCode.PollParticipant, "Poll Participant", "🗳️", stats.PollVoteCount, 5, earnedCodes),
            };

            return ApiResponse<List<BadgeProgressDto>>.Ok(progress);
        }

        private static BadgeProgressDto BuildProgress(
            string code, string name, string icon, int current, int target, HashSet<string> earnedCodes)
        {
            var clamped = Math.Min(current, target);
            return new BadgeProgressDto
            {
                BadgeCode = code,
                BadgeName = name,
                IconEmoji = icon,
                Current = clamped,
                Target = target,
                PercentComplete = target == 0 ? 100 : Math.Round((double)clamped / target * 100, 0),
                Earned = earnedCodes.Contains(code)
            };
        }

        public async Task<ApiResponse<List<ContributorLeaderboardDto>>> GetClubLeaderboardAsync(int clubId, int count = 10)
        {
            count = count is < 1 or > 100 ? Math.Clamp(count, 1, 100) : count;

            if (!await _context.Clubs.AnyAsync(x => x.Id == clubId))
                return ApiResponse<List<ContributorLeaderboardDto>>.Fail("Club not found.");

            var memberIds = await _context.ClubMembers
                .Where(x => x.ClubId == clubId)
                .Select(x => x.UserId)
                .ToListAsync();

            if (memberIds.Count == 0)
                return ApiResponse<List<ContributorLeaderboardDto>>.Ok(new List<ContributorLeaderboardDto>());

            var badge = await _context.Badges.FirstOrDefaultAsync(x => x.Code == Enums.BadgeCode.TopContributor);
            var currentHolderIds = badge == null
                ? new HashSet<int>()
                : (await _context.UserBadges
                    .Where(x => x.BadgeId == badge.Id && x.ClubId == clubId)
                    .Select(x => x.UserId)
                    .ToListAsync()).ToHashSet();


            var postCounts = await _context.Posts
                .Where(p => p.ClubId == clubId && memberIds.Contains(p.UserId))
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var commentCounts = await _context.Comments
                .Where(c => memberIds.Contains(c.UserId) && c.Post != null && c.Post.ClubId == clubId)
                .GroupBy(c => c.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var reactionCounts = await _context.Reactions
                .Where(r => r.Post != null && r.Post.ClubId == clubId && memberIds.Contains(r.Post.UserId))
                .GroupBy(r => r.Post!.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var users = await _context.Users
                .AsNoTracking()
                .Where(u => memberIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var results = new List<ContributorLeaderboardDto>();

            foreach (var memberId in memberIds)
            {
                postCounts.TryGetValue(memberId, out var postCount);
                commentCounts.TryGetValue(memberId, out var commentCount);
                reactionCounts.TryGetValue(memberId, out var reactionsReceived);

                if (postCount == 0 && commentCount == 0 && reactionsReceived == 0)
                    continue;

                var score = postCount * 3 + commentCount * 1.5 + reactionsReceived * 1;
                users.TryGetValue(memberId, out var user);

                results.Add(new ContributorLeaderboardDto
                {
                    UserId = memberId,
                    UserName = user?.Name,
                    UserProfileImage = user?.ProfileImage,
                    PostCount = postCount,
                    CommentCount = commentCount,
                    ReactionsReceived = reactionsReceived,
                    Score = Math.Round(score, 1),
                    HoldsTopContributorBadge = currentHolderIds.Contains(memberId)
                });
            }

            var top = results.OrderByDescending(x => x.Score).Take(count).ToList();
            return ApiResponse<List<ContributorLeaderboardDto>>.Ok(top);
        }

        public async Task<ApiResponse<PagedResultDto<GlobalBadgeLeaderboardDto>>> GetGlobalLeaderboardAsync(int page = 1, int pageSize = 10)
        {
            var groupedQuery = _context.UserBadges
                .GroupBy(x => x.UserId)
                .Select(g => new { UserId = g.Key, BadgeCount = g.Count() })
                .OrderByDescending(x => x.BadgeCount);

            var pagedGrouped = await PaginationHelper.ToPagedResultAsync(groupedQuery, page, pageSize);

            var userIds = pagedGrouped.Items.Select(x => x.UserId).ToList();
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var items = pagedGrouped.Items.Select(g =>
            {
                users.TryGetValue(g.UserId, out var user);
                return new GlobalBadgeLeaderboardDto
                {
                    UserId = g.UserId,
                    UserName = user?.Name,
                    UserProfileImage = user?.ProfileImage,
                    BadgeCount = g.BadgeCount
                };
            }).ToList();

            return ApiResponse<PagedResultDto<GlobalBadgeLeaderboardDto>>.Ok(new PagedResultDto<GlobalBadgeLeaderboardDto>
            {
                Items = items,
                Page = pagedGrouped.Page,
                PageSize = pagedGrouped.PageSize,
                TotalCount = pagedGrouped.TotalCount,
                HasMore = (long)pagedGrouped.Page * pagedGrouped.PageSize < pagedGrouped.TotalCount
            });
        }

        public async Task<ApiResponse<BadgeHoldersResponseDto>> GetBadgeHoldersAsync(string badgeCode, int? clubId = null, int page = 1, int pageSize = 20)
        {
            badgeCode = (badgeCode ?? string.Empty).Trim().ToUpperInvariant();

            var badge = await _context.Badges.AsNoTracking().FirstOrDefaultAsync(x => x.Code == badgeCode);
            if (badge == null)
                return ApiResponse<BadgeHoldersResponseDto>.Fail("Badge not found.");

            var query = _context.UserBadges
                .AsNoTracking()
                .Include(x => x.Club)
                .Where(x => x.BadgeId == badge.Id && x.ClubId == clubId)
                .OrderByDescending(x => x.EarnedAt);

            var pagedHolders = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);

            var userIds = pagedHolders.Items.Select(x => x.UserId).ToList();
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var items = pagedHolders.Items.Select(h =>
            {
                users.TryGetValue(h.UserId, out var user);
                return new BadgeHolderDto
                {
                    UserId = h.UserId,
                    UserName = user?.Name,
                    UserProfileImage = user?.ProfileImage,
                    ClubId = h.ClubId,
                    ClubName = h.Club?.Name,
                    EarnedAt = h.EarnedAt
                };
            }).ToList();

            var result = new BadgeHoldersResponseDto
            {
                BadgeCode = badge.Code,
                BadgeName = badge.Name,
                IconEmoji = badge.IconEmoji,
                Holders = new PagedResultDto<BadgeHolderDto>
                {
                    Items = items,
                    Page = pagedHolders.Page,
                    PageSize = pagedHolders.PageSize,
                    TotalCount = pagedHolders.TotalCount,
                    HasMore = (long)pagedHolders.Page * pagedHolders.PageSize < pagedHolders.TotalCount
                }
            };

            return ApiResponse<BadgeHoldersResponseDto>.Ok(result);
        }

        public async Task<ApiResponse<string>> RecalculateTopContributorAsync(int currentUserId, int clubId)
        {
            var reviewer = await _context.ClubMembers
                .FirstOrDefaultAsync(x => x.ClubId == clubId && x.UserId == currentUserId);

            if (reviewer == null || !ClubPermissionHelper.CanManage(reviewer.Role))
                return ApiResponse<string>.Fail("Only Admins or Moderators can recalculate Top Contributor.");

            await SeedDefaultBadgesAsync();

            var leaderboard = await GetClubLeaderboardAsync(clubId, 1);
            var topUser = leaderboard.Data?.FirstOrDefault();

            if (topUser == null)
                return ApiResponse<string>.Ok("Not enough activity in this club yet to determine a Top Contributor.");

            var awarded = await AwardIfMissingAsync(topUser.UserId, Enums.BadgeCode.TopContributor, clubId);

            if (awarded == null)
                return ApiResponse<string>.Ok($"{topUser.UserName} is still the Top Contributor (already awarded).");

            await _context.SaveChangesAsync();
            _logger.LogInformation("Top Contributor badge awarded to {UserId} in Club {ClubId}", topUser.UserId, clubId);

            return ApiResponse<string>.Ok($"{topUser.UserName} has been awarded Top Contributor for this club!");
        }


        public async Task<ApiResponse<string>> RevokeBadgeAsync(int currentUserId, int targetUserId, string badgeCode, int? clubId = null)
        {
            badgeCode = (badgeCode ?? string.Empty).Trim().ToUpperInvariant();

            var badge = await _context.Badges.FirstOrDefaultAsync(x => x.Code == badgeCode);
            if (badge == null)
                return ApiResponse<string>.Fail("Badge not found.");

            var userBadge = await _context.UserBadges
                .FirstOrDefaultAsync(x => x.UserId == targetUserId && x.BadgeId == badge.Id && x.ClubId == clubId);

            if (userBadge == null)
                return ApiResponse<string>.Fail("This user does not hold that badge.");

            _context.UserBadges.Remove(userBadge);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Admin {AdminId} revoked badge {BadgeCode} (clubId={ClubId}) from user {UserId}",
                currentUserId, badgeCode, clubId, targetUserId);

            return ApiResponse<string>.Ok($"Badge \"{badge.Name}\" revoked from user {targetUserId}.");
        }
    }
}