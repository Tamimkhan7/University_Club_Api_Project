using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.DTOs.Reaction;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Hubs;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.ReactionService
{
    public class ReactionService : IReactionService
    {
        private readonly AppDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<ReactionService> _logger;

        public ReactionService(
            AppDbContext context,
            IServiceScopeFactory scopeFactory,
            IHubContext<NotificationHub> hubContext,
            ILogger<ReactionService> logger)
        {
            _context = context;
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task<ReactionSummaryDto> ReactAsync(int callerId, ReactDto dto)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.PostId)
                ?? throw new KeyNotFoundException("Post not found.");

            var isBlocked = await _context.BlockedUsers.AnyAsync(x =>
                (x.BlockerId == callerId && x.BlockedUserId == post.UserId) ||
                (x.BlockerId == post.UserId && x.BlockedUserId == callerId));

            if (isBlocked)
                throw new InvalidOperationException("Reaction not allowed.");

            bool isNewReaction = false;
            bool removed = false;

            var existing = await _context.Reactions
                .FirstOrDefaultAsync(x => x.PostId == dto.PostId && x.UserId == callerId);

            if (existing != null)
            {
                if (existing.Type == dto.Type)
                {
                    _context.Reactions.Remove(existing);
                    removed = true;
                }
                else
                {
                    existing.Type = dto.Type;
                    isNewReaction = true;
                }
            }
            else
            {
                _context.Reactions.Add(new Reaction
                {
                    PostId = dto.PostId,
                    UserId = callerId,
                    Type = dto.Type
                });
                isNewReaction = true;
            }

            await _context.SaveChangesAsync();

            var summary = await BuildSummaryAsync(callerId, dto.PostId);

            await PushReactionUpdateAsync(dto.PostId, summary);

            if (isNewReaction && post.UserId != callerId)
            {
                var reactorName = await _context.Users
                    .AsNoTracking()
                    .Where(x => x.Id == callerId)
                    .Select(x => x.Name)
                    .FirstOrDefaultAsync() ?? "Someone";

                var receiverId = post.UserId;


                _ = Task.Run(async () =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    try
                    {
                        await notificationService.CreateAndPushAsync(new CreateNotificationDto
                        {
                            SenderId = callerId,
                            ReceiverId = receiverId,
                            Type = NotificationType.Reaction,
                            Message = $"{reactorName} reacted to your post."
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to push Reaction notification to user {UserId}", receiverId);
                    }
                });
            }

            return summary;
        }

        public async Task<ReactionSummaryDto> RemoveAsync(int callerId, int postId)
        {
            var reaction = await _context.Reactions
                .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == callerId)
                ?? throw new KeyNotFoundException("Reaction not found.");

            _context.Reactions.Remove(reaction);
            await _context.SaveChangesAsync();

            var summary = await BuildSummaryAsync(callerId, postId);

            await PushReactionUpdateAsync(postId, summary);

            return summary;
        }

        public async Task<ReactionSummaryDto> GetSummaryAsync(int callerId, int postId)
            => await BuildSummaryAsync(callerId, postId);

        public async Task<int> GetCountAsync(int postId)
            => await _context.Reactions.CountAsync(x => x.PostId == postId);

        public async Task<ReactionType?> GetMyReactionAsync(int callerId, int postId)
        {
            return await _context.Reactions
                .AsNoTracking()
                .Where(x => x.PostId == postId && x.UserId == callerId)
                .Select(x => (ReactionType?)x.Type)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResultDto<ReactionResponseDto>> GetAllAsync(
            int postId,
            PaginationParamsDto pagination)
        {
            var q = _context.Reactions
                .AsNoTracking()
                .Where(x => x.PostId == postId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ReactionResponseDto
                {
                    UserId = x.UserId,
                    UserName = x.User!.Name,
                    UserImage = x.User.ProfileImage,
                    Type = x.Type,
                    CreatedAt = x.CreatedAt
                });

            return await PaginationHelper.ToPagedResultAsync(q, pagination);
        }

        public async Task<PagedResultDto<ReactionUserDto>> GetByTypeAsync(
            int postId,
            ReactionType type,
            PaginationParamsDto pagination)
        {
            var q = _context.Reactions
                .AsNoTracking()
                .Where(x => x.PostId == postId && x.Type == type)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ReactionUserDto
                {
                    UserId = x.UserId,
                    UserName = x.User!.Name,
                    UserImage = x.User.ProfileImage,
                    Type = x.Type
                });

            return await PaginationHelper.ToPagedResultAsync(q, pagination);
        }

        private async Task<ReactionSummaryDto> BuildSummaryAsync(int callerId, int postId)
        {
            var counts = await _context.Reactions
                .AsNoTracking()
                .Where(x => x.PostId == postId)
                .GroupBy(x => x.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();
            var myReaction = await _context.Reactions
                .AsNoTracking()
                .Where(x => x.PostId == postId && x.UserId == callerId)
                .Select(x => (ReactionType?)x.Type)
                .FirstOrDefaultAsync();

            int Get(ReactionType t) => counts.FirstOrDefault(c => c.Type == t)?.Count ?? 0;

            return new ReactionSummaryDto
            {
                PostId = postId,
                Total = counts.Sum(c => c.Count),
                Like = Get(ReactionType.Like),
                Love = Get(ReactionType.Love),
                Haha = Get(ReactionType.Haha),
                Wow = Get(ReactionType.Wow),
                Sad = Get(ReactionType.Sad),
                Angry = Get(ReactionType.Angry),
                MyReaction = myReaction
            };
        }

        private async Task PushReactionUpdateAsync(int postId, ReactionSummaryDto summary)
        {
            try
            {
                await _hubContext.Clients
                    .Group($"post-{postId}")
                    .SendAsync("ReactionUpdated", summary);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to push ReactionUpdated for post {PostId}", postId);
            }
        }
    }
}