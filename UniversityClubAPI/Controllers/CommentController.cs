using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CommentController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateCommentDto dto)
        {
            if (dto == null) return BadRequest("Invalid request");
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return Unauthorized();

            var comment = new Comment
            {
                Content = dto.Content,
                PostId = dto.PostId,
                ParentCommentId = dto.ParentCommentId,
                UserId = user.Id
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var response = new CommentResponseDto
            {
                Id = comment.Id,
                Content = comment.Content,
                UserName = user.Name,
                UserImage = user.ProfileImage,
                CreatedAt = comment.CreatedAt
            };

            return Ok(response);
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, CreateCommentDto dto)
        {
            if (dto == null) return BadRequest("Invalid request");
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return Unauthorized();

            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);
            if (comment == null) return NotFound();

            comment.Content = dto.Content;
            await _context.SaveChangesAsync();

            return Ok("Comment Updated Successfully");
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return Unauthorized();

            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);
            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok("Comment Deleted Successfully");

        }

        [Authorize]
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _context.Comments
                .Include(x => x.User)
                .Where(x => x.PostId == postId && x.ParentCommentId == null)
                .OrderByDescending(x => x.CreatedAt)
                .Select(c => new
                {
                    Id = c.Id,
                    Content = c.Content,
                    UserName = c.User.Name,
                    UserImage = c.User.ProfileImage,
                    CreatedAt = c.CreatedAt,

                    Replies = _context.Comments
                    .Include(r => r.User)
                    .Where(r => r.ParentCommentId == c.Id)
                    .Select(r => new
                    {
                        Id = r.Id,
                        Content = r.Content,
                        UserName = r.User.Name,
                        UserImage = r.User.ProfileImage,
                        CreatedAt = r.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return Ok(comments);
        }
    }
}
