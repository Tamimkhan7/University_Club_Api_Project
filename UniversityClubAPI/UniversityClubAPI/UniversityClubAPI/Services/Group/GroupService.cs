using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.DTOs.Group;
using UniversityClubAPI.DTOs.GroupMessage;
using UniversityClubAPI.Enums;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Hubs;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Services.GroupService
{
    public class GroupService : IGroupService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<GroupHub> _groupHub;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public GroupService(AppDbContext context, IHubContext<GroupHub> groupHub, IHubContext<NotificationHub> notificationHub)
        {
            _context = context;
            _groupHub = groupHub;
            _notificationHub = notificationHub;
        }

        public async Task<ApiResponse<GroupSummaryDto>> CreateAsync(int userId, CreateGroupDto dto)
        {
            var group = new Group
            {
                Name = dto.Name.Trim(),
                CreatedBy = userId
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = group.Id,
                UserId = userId,
                IsAdmin = true
            });

            var memberIds = dto.MemberIds.Where(x => x != userId).Distinct().ToList();

            if (memberIds.Any())
            {
                var validUserIds = await _context.Users
                    .Where(x => memberIds.Contains(x.Id))
                    .Select(x => x.Id)
                    .ToListAsync();

                foreach (var id in validUserIds)
                {
                    _context.GroupMembers.Add(new GroupMember
                    {
                        GroupId = group.Id,
                        UserId = id,
                        IsAdmin = false
                    });
                }
            }

            await _context.SaveChangesAsync();

            var memberCount = await _context.GroupMembers.CountAsync(x => x.GroupId == group.Id);

            foreach (var id in dto.MemberIds.Where(x => x != userId).Distinct())
            {
                await _notificationHub.Clients
                    .Group($"notification-{id}")
                    .SendAsync("AddedToGroup", new { groupId = group.Id, groupName = group.Name });
            }

            return ApiResponse<GroupSummaryDto>.Ok(new GroupSummaryDto
            {
                Id = group.Id,
                Name = group.Name,
                CreatedBy = group.CreatedBy,
                CreatedAt = group.CreatedAt,
                IsAdmin = true,
                MemberCount = memberCount
            }, "Group created successfully.");
        }

        public async Task<ApiResponse<GroupSummaryDto>> UpdateAsync(int userId, int groupId, UpdateGroupDto dto)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == groupId);
            if (group == null)
                throw new KeyNotFoundException("Group not found.");

            var membership = await _context.GroupMembers
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.UserId == userId);

            if (membership == null || !membership.IsAdmin)
                throw new UnauthorizedAccessException("Only an admin can update this group.");

            group.Name = dto.Name.Trim();
            await _context.SaveChangesAsync();

            await _groupHub.Clients.Group($"group-{groupId}")
                .SendAsync("GroupUpdated", new { groupId, name = group.Name });

            var memberCount = await _context.GroupMembers.CountAsync(x => x.GroupId == groupId);

            return ApiResponse<GroupSummaryDto>.Ok(new GroupSummaryDto
            {
                Id = group.Id,
                Name = group.Name,
                CreatedBy = group.CreatedBy,
                CreatedAt = group.CreatedAt,
                IsAdmin = true,
                MemberCount = memberCount
            }, "Group updated successfully.");
        }

        public async Task<ApiResponse<string>> DeleteGroupAsync(int userId, int groupId)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == groupId);
            if (group == null)
                throw new KeyNotFoundException("Group not found.");

            if (group.CreatedBy != userId)
                throw new UnauthorizedAccessException("Only the creator can delete this group.");

            var members = _context.GroupMembers.Where(x => x.GroupId == groupId);
            var messages = _context.GroupMessages.Where(x => x.GroupId == groupId);

            _context.GroupMembers.RemoveRange(members);
            _context.GroupMessages.RemoveRange(messages);
            _context.Groups.Remove(group);

            await _context.SaveChangesAsync();

            await _groupHub.Clients.Group($"group-{groupId}")
                .SendAsync("GroupDeleted", new { groupId });

            return ApiResponse<string>.Ok("Group deleted successfully.");
        }

        public async Task<ApiResponse<GroupMessageDto>> SendMessageAsync(int userId, SendGroupMessageDto dto)
        {
            if (!await _context.Groups.AnyAsync(x => x.Id == dto.GroupId))
                throw new KeyNotFoundException("Group not found.");

            var isMember = await _context.GroupMembers
                .AnyAsync(x => x.GroupId == dto.GroupId && x.UserId == userId);

            if (!isMember)
                throw new UnauthorizedAccessException("You are not a member of this group.");

            var message = new GroupMessage
            {
                GroupId = dto.GroupId,
                SenderId = userId,
                Text = dto.Text.Trim()
            };

            _context.GroupMessages.Add(message);
            await _context.SaveChangesAsync();

            var senderName = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.Name)
                .FirstOrDefaultAsync();

            var resultDto = new GroupMessageDto
            {
                Id = message.Id,
                GroupId = message.GroupId,
                SenderId = message.SenderId,
                SenderName = senderName,
                Text = message.Text,
                CreatedAt = message.CreatedAt
            };

            await _groupHub.Clients.Group($"group-{dto.GroupId}")
                .SendAsync("ReceiveGroupMessage", resultDto);

            var otherMemberIds = await _context.GroupMembers
                .Where(x => x.GroupId == dto.GroupId && x.UserId != userId)
                .Select(x => x.UserId)
                .ToListAsync();

            foreach (var memberId in otherMemberIds)
            {
                await _notificationHub.Clients
                    .Group($"notification-{memberId}")
                    .SendAsync("NewGroupMessage", new
                    {
                        groupId = dto.GroupId,
                        senderId = userId,
                        senderName,
                        preview = message.Text.Length > 60 ? message.Text[..60] + "…" : message.Text
                    });
            }

            return ApiResponse<GroupMessageDto>.Ok(resultDto, "Message sent successfully.");
        }

        public async Task<ApiResponse<PagedResultDto<GroupMessageDto>>> GetMessagesAsync(
            int userId, int groupId, PaginationParamsDto pagination)
        {
            var isMember = await _context.GroupMembers
                .AnyAsync(x => x.GroupId == groupId && x.UserId == userId);

            if (!isMember)
                throw new UnauthorizedAccessException("You are not a member of this group.");

            var query = _context.GroupMessages
                .AsNoTracking()
                .Where(x => x.GroupId == groupId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new GroupMessageDto
                {
                    Id = x.Id,
                    GroupId = x.GroupId,
                    SenderId = x.SenderId,
                    SenderName = x.Sender != null ? x.Sender.Name : null,
                    Text = x.Text,
                    MediaType = x.MediaType,
                    MediaUrl = x.IsDeletedForEveryone ? null : x.MediaUrl,
                    DurationSeconds = x.DurationSeconds,
                    CreatedAt = x.CreatedAt
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, pagination);
            return ApiResponse<PagedResultDto<GroupMessageDto>>.Ok(result);
        }

        public async Task<ApiResponse<string>> LeaveGroupAsync(int userId, int groupId)
        {
            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.UserId == userId);

            if (member == null)
                throw new KeyNotFoundException("You are not in this group.");

            var wasAdmin = member.IsAdmin;
            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();

            var remaining = await _context.GroupMembers
                .Where(x => x.GroupId == groupId)
                .ToListAsync();

            if (!remaining.Any())
            {
                var group = await _context.Groups.FirstOrDefaultAsync(x => x.Id == groupId);
                var msgs = _context.GroupMessages.Where(x => x.GroupId == groupId);
                _context.GroupMessages.RemoveRange(msgs);

                if (group != null)
                    _context.Groups.Remove(group);

                await _context.SaveChangesAsync();
            }
            else if (wasAdmin && !remaining.Any(x => x.IsAdmin))
            {
                var nextAdmin = remaining.OrderBy(x => x.JoinedAt).First();
                nextAdmin.IsAdmin = true;
                await _context.SaveChangesAsync();

                await _notificationHub.Clients
                    .Group($"notification-{nextAdmin.UserId}")
                    .SendAsync("PromotedToGroupAdmin", new { groupId });
            }

            await _groupHub.Clients.Group($"group-{groupId}")
                .SendAsync("MemberLeft", new { groupId, userId });

            return ApiResponse<string>.Ok("Left group successfully.");
        }

        public async Task<ApiResponse<string>> AddMemberAsync(int userId, int groupId, AddGroupMemberDto dto)
        {
            var admin = await _context.GroupMembers
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.UserId == userId);

            if (admin == null || !admin.IsAdmin)
                throw new UnauthorizedAccessException("Only an admin can add members.");

            await _context.EnsureUserExistsAsync(dto.UserId);

            if (await _context.GroupMembers.AnyAsync(x => x.GroupId == groupId && x.UserId == dto.UserId))
                throw new ArgumentException("User is already in this group.");

            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = groupId,
                UserId = dto.UserId,
                IsAdmin = false
            });

            await _context.SaveChangesAsync();

            var groupName = await _context.Groups
                .Where(x => x.Id == groupId)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();

            await _notificationHub.Clients
                .Group($"notification-{dto.UserId}")
                .SendAsync("AddedToGroup", new { groupId, groupName });

            await _groupHub.Clients.Group($"group-{groupId}")
                .SendAsync("MemberAdded", new { groupId, userId = dto.UserId });

            return ApiResponse<string>.Ok("Member added successfully.");
        }

        public async Task<ApiResponse<string>> RemoveMemberAsync(int userId, int groupId, int memberId)
        {
            var admin = await _context.GroupMembers
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.UserId == userId);

            if (admin == null || !admin.IsAdmin)
                throw new UnauthorizedAccessException("Only an admin can remove members.");

            if (memberId == userId)
                throw new ArgumentException("Admin cannot remove themselves. Use leave instead.");

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.UserId == memberId);

            if (member == null)
                throw new KeyNotFoundException("Member not found.");

            _context.GroupMembers.Remove(member);
            await _context.SaveChangesAsync();

            await _notificationHub.Clients
                .Group($"notification-{memberId}")
                .SendAsync("RemovedFromGroup", new { groupId });

            await _groupHub.Clients.Group($"group-{groupId}")
                .SendAsync("MemberRemoved", new { groupId, userId = memberId });

            return ApiResponse<string>.Ok("Member removed successfully.");
        }

        public async Task<ApiResponse<string>> SetAdminAsync(int userId, int groupId, SetGroupAdminDto dto)
        {
            var requester = await _context.GroupMembers
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.UserId == userId);

            if (requester == null || !requester.IsAdmin)
                throw new UnauthorizedAccessException("Only an admin can change admin status.");

            var target = await _context.GroupMembers
                .FirstOrDefaultAsync(x => x.GroupId == groupId && x.UserId == dto.UserId);

            if (target == null)
                throw new KeyNotFoundException("Member not found.");

            if (!dto.IsAdmin && target.IsAdmin)
            {
                var adminCount = await _context.GroupMembers
                    .CountAsync(x => x.GroupId == groupId && x.IsAdmin);

                if (adminCount <= 1)
                    throw new ArgumentException("Cannot demote the last remaining admin.");
            }

            target.IsAdmin = dto.IsAdmin;
            await _context.SaveChangesAsync();

            await _notificationHub.Clients
                .Group($"notification-{dto.UserId}")
                .SendAsync(dto.IsAdmin ? "PromotedToGroupAdmin" : "DemotedFromGroupAdmin", new { groupId });

            return ApiResponse<string>.Ok(dto.IsAdmin ? "Member promoted to admin." : "Admin rights revoked.");
        }

        public async Task<ApiResponse<List<GroupMemberDto>>> GetMembersAsync(int userId, int groupId)
        {
            var isMember = await _context.GroupMembers
                .AnyAsync(x => x.GroupId == groupId && x.UserId == userId);

            if (!isMember)
                throw new UnauthorizedAccessException("You are not a member of this group.");

            var members = await _context.GroupMembers
                .AsNoTracking()
                .Where(x => x.GroupId == groupId)
                .OrderByDescending(x => x.IsAdmin)
                .ThenBy(x => x.JoinedAt)
                .Select(x => new GroupMemberDto
                {
                    UserId = x.UserId,
                    Name = x.User != null ? x.User.Name : null,
                    Email = x.User != null ? x.User.Email : null,
                    IsAdmin = x.IsAdmin,
                    JoinedAt = x.JoinedAt
                })
                .ToListAsync();

            return ApiResponse<List<GroupMemberDto>>.Ok(members);
        }

        public async Task<ApiResponse<List<GroupSummaryDto>>> GetMyGroupsAsync(int userId)
        {
            var groups = await _context.GroupMembers
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => new GroupSummaryDto
                {
                    Id = x.Group!.Id,
                    Name = x.Group.Name,
                    CreatedBy = x.Group.CreatedBy,
                    CreatedAt = x.Group.CreatedAt,
                    IsAdmin = x.IsAdmin,
                    MemberCount = x.Group.Members.Count,
                    LastMessage = x.Group.Messages
                                    .OrderByDescending(m => m.CreatedAt)
                                    .Select(m => m.MediaType == MessageMediaType.Voice ? "🎤 Voice message" : m.Text)
                                    .FirstOrDefault(),
                    LastMessageAt = x.Group.Messages
                                    .OrderByDescending(m => m.CreatedAt)
                                    .Select(m => (DateTime?)m.CreatedAt)
                                    .FirstOrDefault()
                })
                .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
                .ToListAsync();

            return ApiResponse<List<GroupSummaryDto>>.Ok(groups);
        }

        public async Task<ApiResponse<GroupDetailsDto>> GetByIdAsync(int userId, int groupId)
        {
            var isMember = await _context.GroupMembers
                .AnyAsync(x => x.GroupId == groupId && x.UserId == userId);

            if (!isMember)
                throw new UnauthorizedAccessException("You are not a member of this group.");

            var group = await _context.Groups
                .AsNoTracking()
                .Where(x => x.Id == groupId)
                .Select(x => new GroupDetailsDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    CreatedBy = x.CreatedBy,
                    CreatorName = x.Creator != null ? x.Creator.Name : null,
                    CreatedAt = x.CreatedAt,
                    Members = x.Members.Select(m => new GroupMemberDto
                    {
                        UserId = m.UserId,
                        Name = m.User != null ? m.User.Name : null,
                        Email = m.User != null ? m.User.Email : null,
                        IsAdmin = m.IsAdmin,
                        JoinedAt = m.JoinedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (group == null)
                throw new KeyNotFoundException("Group not found.");

            return ApiResponse<GroupDetailsDto>.Ok(group);
        }
    }
}