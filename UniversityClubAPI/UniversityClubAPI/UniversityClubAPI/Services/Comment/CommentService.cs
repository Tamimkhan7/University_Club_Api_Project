using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs.Comment;
using UniversityClubAPI.DTOs.Common;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Services.CommentService
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CommentDto>> CreateAsync(int userId, CreateCommentDto dto)
        {
            var user = await _context.GetUserOrThrowAsync(userId);

            var post = await _context.Posts.FindAsync(dto.PostId);
            if (post == null)
                throw new KeyNotFoundException("Post not found");

            var blocked = await _context.BlockedUsers.AnyAsync(x =>
                (x.BlockerId == userId && x.BlockedUserId == post.UserId) ||
                (x.BlockerId == post.UserId && x.BlockedUserId == userId));

            if (blocked)
                throw new UnauthorizedAccessException("Not allowed to comment on this post");

            if (dto.ParentCommentId != null)
            {
                var parent = await _context.Comments.FirstOrDefaultAsync(x => x.Id == dto.ParentCommentId);

                if (parent == null)
                    throw new KeyNotFoundException("Parent comment not found");

                if (parent.PostId != dto.PostId)
                    throw new ArgumentException("Invalid parent comment");
            }

            var comment = new Comment
            {
                Content = dto.Content,
                PostId = dto.PostId,
                ParentCommentId = dto.ParentCommentId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return ApiResponse<CommentDto>.Ok(new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content!,
                UserName = user.Name,
                UserImage = user.ProfileImage,
                CreatedAt = comment.CreatedAt
            }, "Comment created successfully");
        }

        public async Task<ApiResponse<string>> UpdateAsync(int userId, int id, CreateCommentDto dto)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteAsync(int userId, int id)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Deleted successfully");
        }

        public async Task<ApiResponse<PagedResultDto<CommentDto>>> GetPostCommentsAsync(int userId, int postId, int page, int pageSize)
        {
            var blockedUsers = await _context.BlockedUsers
                .Where(x => x.BlockerId == userId || x.BlockedUserId == userId)
                .Select(x => x.BlockerId == userId ? x.BlockedUserId : x.BlockerId)
                .ToListAsync();

            var query = _context.Comments
                .Include(x => x.User)
                .Where(x =>
                    x.PostId == postId &&
                    x.ParentCommentId == null &&
                    !blockedUsers.Contains(x.UserId))
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new CommentDto
                {
                    Id = x.Id,
                    Content = x.Content!,
                    UserName = x.User!.Name,
                    UserImage = x.User.ProfileImage,
                    CreatedAt = x.CreatedAt
                });

            var result = await PaginationHelper.ToPagedResultAsync(query, page, pageSize);
            return ApiResponse<PagedResultDto<CommentDto>>.Ok(result);
        }

        public async Task<ApiResponse<CommentDto>> GetCommentByIdAsync(int userId, int id)
        {
            var comment = await _context.Comments
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found");

            return ApiResponse<CommentDto>.Ok(new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content!,
                UserName = comment.User!.Name,
                UserImage = comment.User.ProfileImage,
                CreatedAt = comment.CreatedAt
            });
        }

        public async Task<ApiResponse<List<CommentDto>>> GetRepliesAsync(int userId, int commentId)
        {
            var replies = await _context.Comments
                .Include(x => x.User)
                .Where(x => x.ParentCommentId == commentId)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new CommentDto
                {
                    Id = x.Id,
                    Content = x.Content!,
                    UserName = x.User!.Name,
                    UserImage = x.User.ProfileImage,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            return ApiResponse<List<CommentDto>>.Ok(replies);
        }

        public async Task<ApiResponse<string>> ToggleLikeAsync(int userId, int commentId)
        {
            var exists = await _context.CommentReactions
                .FirstOrDefaultAsync(x => x.CommentId == commentId && x.UserId == userId);

            if (exists != null)
            {
                _context.CommentReactions.Remove(exists);
                await _context.SaveChangesAsync();
                return ApiResponse<string>.Ok("Unliked");
            }

            _context.CommentReactions.Add(new CommentReaction
            {
                CommentId = commentId,
                UserId = userId
            });

            await _context.SaveChangesAsync();
            return ApiResponse<string>.Ok("Liked");
        }

        public async Task<ApiResponse<int>> GetLikeCountAsync(int commentId)
        {
            var count = await _context.CommentReactions
                .CountAsync(x => x.CommentId == commentId);

            return ApiResponse<int>.Ok(count);
        }
    }
}