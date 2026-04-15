using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniversityClubAPI.Data;
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
        public async Task<IActionResult> Create(Comment comment)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return Unauthorized();

            comment.UserId = user.Id;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }


        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _context.Comments
                .Include(x => x.user)
                .Where(x => x.PostId == postId)
                .ToListAsync();

            return Ok(comments);
        }
    }
}
