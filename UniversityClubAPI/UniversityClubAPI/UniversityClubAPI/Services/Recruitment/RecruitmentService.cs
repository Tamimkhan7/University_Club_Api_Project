using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Notification;
using UniversityClubAPI.DTOs.Recruitment;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.NotificationService;

namespace UniversityClubAPI.Services.RecruitmentService
{
    public class RecruitmentService : IRecruitmentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RecruitmentService> _logger;
        private readonly INotificationService _notificationService;

        public RecruitmentService(AppDbContext context, ILogger<RecruitmentService> logger, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        private static ApplicationResponseDto ToDto(ClubApplication a) => new()
        {
            Id = a.Id,
            ClubId = a.ClubId,
            ClubName = a.Club?.Name,
            UserId = a.UserId,
            UserName = a.User?.Name,
            UserProfileImage = a.User?.ProfileImage,
            Message = a.Message,
            Status = a.Status,
            ReviewedBy = a.ReviewedBy,
            ReviewerName = a.Reviewer?.Name,
            ReviewNote = a.ReviewNote,
            AppliedAt = a.AppliedAt,
            ReviewedAt = a.ReviewedAt
        };

        public async Task<ApiResponse<ApplicationResponseDto>> ApplyAsync(int userId, int clubId, CreateApplicationDto dto)
        {
            var club = await _context.Clubs.FirstOrDefaultAsync(x => x.Id == clubId);
            if (club == null)
                return ApiResponse<ApplicationResponseDto>.Fail("Club not found.");

            if (await _context.GetMembershipAsync(userId, clubId) != null)
                return ApiResponse<ApplicationResponseDto>.Fail("You are already a member of this club.");

            var isBlocked = await _context.BlockedUsers
                .AnyAsync(x => x.BlockerId == club.CreatedBy && x.BlockedUserId == userId);
            if (isBlocked)
                return ApiResponse<ApplicationResponseDto>.Fail("You are not allowed to apply to this club.");

            var existingPending = await _context.ClubApplications
                .AnyAsync(x => x.UserId == userId && x.ClubId == clubId && x.Status == ApplicationStatus.Pending);

            if (existingPending)
                return ApiResponse<ApplicationResponseDto>.Fail("You already have a pending application for this club.");

            var application = new ClubApplication
            {
                ClubId = clubId,
                UserId = userId,
                Message = dto.Message,
                Status = ApplicationStatus.Pending
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ClubApplications.Add(application);
                await _context.SaveChangesAsync();

                var allMembers = await _context.ClubMembers
                    .Where(x => x.ClubId == clubId)
                    .Select(x => new { x.UserId, x.Role })
                    .ToListAsync();

                var reviewerIds = allMembers
                    .Where(x => ClubPermissionHelper.CanManage(x.Role))
                    .Select(x => x.UserId)
                    .ToList();

                foreach (var reviewerId in reviewerIds)
                {
                    await _notificationService.CreateAndPushAsync(new CreateNotificationDto
                    {
                        SenderId = userId,
                        ReceiverId = reviewerId,
                        Type = NotificationType.NewApplication,
                        Message = $"New application to join {club.Name}"
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            _logger.LogInformation("User {UserId} applied to Club {ClubId}", userId, clubId);

            var result = await _context.ClubApplications
                .Include(x => x.Club)
                .Include(x => x.User)
                .FirstAsync(x => x.Id == application.Id);

            return ApiResponse<ApplicationResponseDto>.Ok(ToDto(result), "Application submitted successfully.");
        }

        public async Task<ApiResponse<string>> WithdrawApplicationAsync(int userId, int applicationId)
        {
            var application = await _context.ClubApplications
                .FirstOrDefaultAsync(x => x.Id == applicationId && x.UserId == userId);

            if (application == null)
                return ApiResponse<string>.Fail("Application not found.");

            if (application.Status != ApplicationStatus.Pending)
                return ApiResponse<string>.Fail("Only pending applications can be withdrawn.");

            application.Status = ApplicationStatus.Withdrawn;
            application.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse<string>.Ok("Application withdrawn successfully.");
        }

        public async Task<ApiResponse<PagedResultDto<ApplicationResponseDto>>> GetMyApplicationsAsync(
            int userId, PaginationParamsDto pagination)
        {
            var query = _context.ClubApplications
                .AsNoTracking()
                .Include(x => x.Club)
                .Include(x => x.User)
                .Include(x => x.Reviewer)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AppliedAt);

            var paged = await PaginationHelper.ToPagedResultAsync(query, pagination);

            return ApiResponse<PagedResultDto<ApplicationResponseDto>>.Ok(new PagedResultDto<ApplicationResponseDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(ToDto).ToList()
            });
        }

        public async Task<ApiResponse<PagedResultDto<ApplicationResponseDto>>> GetClubApplicationsAsync(
            int currentUserId, int clubId, ApplicationStatus? status, PaginationParamsDto pagination)
        {
            var reviewer = await _context.GetMembershipAsync(currentUserId, clubId);
            if (reviewer == null || !ClubPermissionHelper.CanManage(reviewer.Role))
                return ApiResponse<PagedResultDto<ApplicationResponseDto>>.Fail("Only Admins or Moderators can view applications.");

            var query = _context.ClubApplications
                .AsNoTracking()
                .Include(x => x.Club)
                .Include(x => x.User)
                .Include(x => x.Reviewer)
                .Where(x => x.ClubId == clubId);

            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);

            var ordered = query.OrderByDescending(x => x.AppliedAt);

            var paged = await PaginationHelper.ToPagedResultAsync(ordered, pagination);

            return ApiResponse<PagedResultDto<ApplicationResponseDto>>.Ok(new PagedResultDto<ApplicationResponseDto>
            {
                Page = paged.Page,
                PageSize = paged.PageSize,
                TotalCount = paged.TotalCount,
                TotalPages = paged.TotalPages,
                Items = paged.Items.Select(ToDto).ToList()
            });
        }

        public async Task<ApiResponse<ApplicationResponseDto>> ApproveApplicationAsync(
            int currentUserId, int applicationId, ReviewApplicationDto dto)
        {
            var application = await _context.ClubApplications
                .Include(x => x.Club)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == applicationId);

            if (application == null)
                return ApiResponse<ApplicationResponseDto>.Fail("Application not found.");

            var reviewer = await _context.GetMembershipAsync(currentUserId, application.ClubId);
            if (reviewer == null || !ClubPermissionHelper.CanManage(reviewer.Role))
                return ApiResponse<ApplicationResponseDto>.Fail("Only Admins or Moderators can review applications.");

            if (application.Status != ApplicationStatus.Pending)
                return ApiResponse<ApplicationResponseDto>.Fail("This application has already been reviewed.");

            application.Status = ApplicationStatus.Approved;
            application.ReviewedBy = currentUserId;
            application.ReviewNote = dto.Note;
            application.ReviewedAt = DateTime.UtcNow;

            var alreadyMember = await _context.GetMembershipAsync(application.UserId, application.ClubId);
            if (alreadyMember == null)
            {
                _context.ClubMembers.Add(new ClubMember
                {
                    ClubId = application.ClubId,
                    UserId = application.UserId,
                    Role = "Member",
                    IsApproved = true
                });
            }

            await _notificationService.CreateAndPushAsync(new CreateNotificationDto
            {
                SenderId = currentUserId,
                ReceiverId = application.UserId,
                Type = NotificationType.ApplicationApproved,
                Message = $"Your application to join {application.Club?.Name} was approved"
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("Application {AppId} approved by {ReviewerId}", applicationId, currentUserId);

            return ApiResponse<ApplicationResponseDto>.Ok(ToDto(application), "Application approved successfully.");
        }

        public async Task<ApiResponse<ApplicationResponseDto>> RejectApplicationAsync(
            int currentUserId, int applicationId, ReviewApplicationDto dto)
        {
            var application = await _context.ClubApplications
                .Include(x => x.Club)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == applicationId);

            if (application == null)
                return ApiResponse<ApplicationResponseDto>.Fail("Application not found.");

            var reviewer = await _context.GetMembershipAsync(currentUserId, application.ClubId);
            if (reviewer == null || !ClubPermissionHelper.CanManage(reviewer.Role))
                return ApiResponse<ApplicationResponseDto>.Fail("Only Admins or Moderators can review applications.");

            if (application.Status != ApplicationStatus.Pending)
                return ApiResponse<ApplicationResponseDto>.Fail("This application has already been reviewed.");

            application.Status = ApplicationStatus.Rejected;
            application.ReviewedBy = currentUserId;
            application.ReviewNote = dto.Note;
            application.ReviewedAt = DateTime.UtcNow;

            await _notificationService.CreateAndPushAsync(new CreateNotificationDto
            {
                SenderId = currentUserId,
                ReceiverId = application.UserId,
                Type = NotificationType.ApplicationRejected,
                Message = $"Your application to join {application.Club?.Name} was rejected"
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("Application {AppId} rejected by {ReviewerId}", applicationId, currentUserId);

            return ApiResponse<ApplicationResponseDto>.Ok(ToDto(application), "Application rejected.");
        }

        public async Task<ApiResponse<int>> GetPendingCountAsync(int currentUserId, int clubId)
        {
            var reviewer = await _context.GetMembershipAsync(currentUserId, clubId);
            if (reviewer == null || !ClubPermissionHelper.CanManage(reviewer.Role))
                return ApiResponse<int>.Fail("Only Admins or Moderators can view this.");

            var count = await _context.ClubApplications
                .CountAsync(x => x.ClubId == clubId && x.Status == ApplicationStatus.Pending);

            return ApiResponse<int>.Ok(count);
        }
    }
}