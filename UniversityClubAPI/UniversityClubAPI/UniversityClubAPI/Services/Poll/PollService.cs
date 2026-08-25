using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.DTOs.Poll;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.PollService
{
    public class PollService : IPollService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PollService> _logger;
        private readonly INotificationService _notificationService;

        public PollService(AppDbContext context, ILogger<PollService> logger, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        private static PollResponseDto ToDto(Poll poll, int currentUserId)
        {
            var totalVotes = poll.Options.Sum(o => o.Votes.Count);
            var now = DateTime.UtcNow;

            return new PollResponseDto
            {
                Id = poll.Id,
                ClubId = poll.ClubId,
                ClubName = poll.Club?.Name,
                CreatedBy = poll.CreatedBy,
                CreatorName = poll.Creator?.Name,
                Title = poll.Title,
                Description = poll.Description,
                Type = poll.Type,
                IsMultipleChoice = poll.IsMultipleChoice,
                StartDate = poll.StartDate,
                EndDate = poll.EndDate,
                IsClosed = poll.IsClosed,
                IsActive = !poll.IsClosed && now >= poll.StartDate && now <= poll.EndDate,
                TotalVotes = totalVotes,
                HasVoted = poll.Options.Any(o => o.Votes.Any(v => v.UserId == currentUserId)),
                Options = poll.Options.Select(o => new PollOptionResultDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    VoteCount = o.Votes.Count,
                    Percentage = totalVotes == 0 ? 0 : Math.Round(o.Votes.Count * 100.0 / totalVotes, 1),
                    VotedByMe = o.Votes.Any(v => v.UserId == currentUserId)
                }).ToList()
            };
        }

        private IQueryable<Poll> BaseQuery()
            => _context.Polls
                .AsNoTracking()
                .Include(x => x.Club)
                .Include(x => x.Creator)
                .Include(x => x.Options)
                    .ThenInclude(o => o.Votes);

        public async Task<ApiResponse<PollResponseDto>> CreatePollAsync(int userId, int clubId, CreatePollDto dto)
        {
            var club = await _context.Clubs.FirstOrDefaultAsync(x => x.Id == clubId);
            if (club == null)
                return ApiResponse<PollResponseDto>.Fail("Club not found.");

            var member = await _context.GetMembershipAsync(userId, clubId);
            if (member == null || !ClubPermissionHelper.CanManage(member.Role))
                return ApiResponse<PollResponseDto>.Fail("Only Admins or Moderators can create a poll.");

            if (dto.EndDate <= DateTime.UtcNow)
                return ApiResponse<PollResponseDto>.Fail("End date must be in the future.");

            var cleanOptions = dto.Options
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Trim())
                .Distinct()
                .ToList();

            if (cleanOptions.Count < 2)
                return ApiResponse<PollResponseDto>.Fail("A poll needs at least 2 distinct options.");

            var poll = new Poll
            {
                ClubId = clubId,
                CreatedBy = userId,
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                IsMultipleChoice = dto.IsMultipleChoice,
                StartDate = DateTime.UtcNow,
                EndDate = dto.EndDate,
                Options = cleanOptions.Select(text => new PollOption { Text = text }).ToList()
            };

            _context.Polls.Add(poll);
            await _context.SaveChangesAsync();


            var memberIds = await _context.ClubMembers
                .Where(x => x.ClubId == clubId && x.UserId != userId)
                .Select(x => x.UserId)
                .ToListAsync();

            foreach (var memberId in memberIds)
            {
                await _notificationService.CreateAndPushAsync(new CreateNotificationDto
                {
                    SenderId = userId,
                    ReceiverId = memberId,
                    Type = NotificationType.NewPoll,
                    Message = $"New poll in {club.Name}: {poll.Title}"
                });
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Poll {PollId} created in Club {ClubId} by {UserId}", poll.Id, clubId, userId);

            var result = await BaseQuery().FirstAsync(x => x.Id == poll.Id);
            return ApiResponse<PollResponseDto>.Ok(ToDto(result, userId), "Poll created successfully.");
        }

        public async Task<ApiResponse<PagedResultDto<PollResponseDto>>> GetClubPollsAsync(
            int userId, int clubId, bool activeOnly, PaginationParamsDto pagination)
        {
            if (!await _context.Clubs.AnyAsync(x => x.Id == clubId))
                return ApiResponse<PagedResultDto<PollResponseDto>>.Fail("Club not found.");

            var now = DateTime.UtcNow;
            var query = BaseQuery().Where(x => x.ClubId == clubId);

            if (activeOnly)
                query = query.Where(x => !x.IsClosed && x.EndDate >= now);

            var ordered = query.OrderByDescending(x => x.CreatedAt);
            var paged = await PaginationHelper.ToPagedResultAsync(ordered, pagination);

            return ApiResponse<PagedResultDto<PollResponseDto>>.Ok(new PagedResultDto<PollResponseDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(p => ToDto(p, userId)).ToList()
            });
        }

        public async Task<ApiResponse<PollResponseDto>> GetPollByIdAsync(int userId, int pollId)
        {
            var poll = await BaseQuery().FirstOrDefaultAsync(x => x.Id == pollId);
            if (poll == null)
                return ApiResponse<PollResponseDto>.Fail("Poll not found.");

            return ApiResponse<PollResponseDto>.Ok(ToDto(poll, userId));
        }
        public async Task<ApiResponse<PollResponseDto>> VoteAsync(int userId, int pollId, CastVoteDto dto)
        {

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

            try
            {
                var poll = await _context.Polls
                    .Include(x => x.Options)
                        .ThenInclude(o => o.Votes)
                    .FirstOrDefaultAsync(x => x.Id == pollId);

                if (poll == null)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<PollResponseDto>.Fail("Poll not found.");
                }

                var member = await _context.GetMembershipAsync(userId, poll.ClubId);
                if (member == null)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<PollResponseDto>.Fail("Only club members can vote on this poll.");
                }

                var now = DateTime.UtcNow;
                if (poll.IsClosed || now > poll.EndDate)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<PollResponseDto>.Fail("This poll is closed.");
                }

                if (now < poll.StartDate)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<PollResponseDto>.Fail("This poll has not started yet.");
                }

                var validOptionIds = poll.Options.Select(o => o.Id).ToHashSet();
                var requestedIds = dto.OptionIds.Distinct().ToList();

                if (requestedIds.Any(id => !validOptionIds.Contains(id)))
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<PollResponseDto>.Fail("One or more options do not belong to this poll.");
                }

                if (!poll.IsMultipleChoice && requestedIds.Count > 1)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<PollResponseDto>.Fail("This poll only allows selecting a single option.");
                }


                var alreadyVoted = await _context.PollVotes
                    .AnyAsync(v => v.PollId == pollId && v.UserId == userId);

                if (alreadyVoted)
                {
                    await transaction.RollbackAsync();
                    return ApiResponse<PollResponseDto>.Fail("You have already voted on this poll.");
                }

                foreach (var optionId in requestedIds)
                {
                    _context.PollVotes.Add(new PollVote
                    {
                        PollId = poll.Id,
                        PollOptionId = optionId,
                        UserId = userId
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("User {UserId} voted on Poll {PollId}", userId, pollId);

                var result = await BaseQuery().FirstAsync(x => x.Id == pollId);
                return ApiResponse<PollResponseDto>.Ok(ToDto(result, userId), "Vote recorded successfully.");
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return ApiResponse<PollResponseDto>.Fail("Could not record your vote due to a conflict, please try again.");
            }
        }
        public async Task<ApiResponse<string>> ClosePollAsync(int userId, int pollId)
        {
            var poll = await _context.Polls
                .Include(x => x.Club)
                .FirstOrDefaultAsync(x => x.Id == pollId);

            if (poll == null)
                return ApiResponse<string>.Fail("Poll not found.");

            var member = await _context.GetMembershipAsync(userId, poll.ClubId);
            if (member == null || !ClubPermissionHelper.CanManage(member.Role))
                return ApiResponse<string>.Fail("Only Admins or Moderators can close a poll.");

            if (poll.IsClosed)
                return ApiResponse<string>.Fail("This poll is already closed.");

            poll.IsClosed = true;

            var voterIds = await _context.PollVotes
                .Where(x => x.PollId == pollId)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var voterId in voterIds)
            {
                await _notificationService.CreateAndPushAsync(new CreateNotificationDto
                {
                    SenderId = userId,
                    ReceiverId = voterId,
                    Type = NotificationType.PollClosed,
                    Message = $"The poll \"{poll.Title}\" in {poll.Club?.Name} has closed"
                });
            }

            await _context.SaveChangesAsync();
            return ApiResponse<string>.Ok("Poll closed successfully.");
        }

        public async Task<ApiResponse<string>> DeletePollAsync(int userId, int pollId)
        {
            var poll = await _context.Polls.FirstOrDefaultAsync(x => x.Id == pollId);
            if (poll == null)
                return ApiResponse<string>.Fail("Poll not found.");

            var member = await _context.GetMembershipAsync(userId, poll.ClubId);
            if (member == null || !ClubPermissionHelper.IsAdmin(member.Role))
                return ApiResponse<string>.Fail("Only the Club Admin can delete a poll.");

            _context.Polls.Remove(poll);
            await _context.SaveChangesAsync();
            return ApiResponse<string>.Ok("Poll deleted successfully.");
        }
    }
}
