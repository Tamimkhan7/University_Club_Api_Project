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
    public class ReactionController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReactionController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("react")]
        public async Task<IActionResult> React([FromBody] ReactDto dto)
        {
            if (dto == null) return BadRequest("Invalid request");
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return Unauthorized();

            var post = await _context.Reactions
                .FirstOrDefaultAsync(x => x.PostId == dto.PostId && x.UserId == user.Id);

            if (post != null)
                post.Type = dto.Type;
            else
            {
                var reaction = new Reaction
                {
                    PostId = dto.PostId,
                    Type = dto.Type,
                    UserId = user.Id
                };
                _context.Reactions.Add(reaction);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpDelete("remove/{postId}")]
        public async Task<IActionResult> Remove(int postId)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return Unauthorized();

            var reaction = await _context.Reactions
                .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == user.Id);

            if (reaction == null) return NotFound("Reaction not found");

            _context.Reactions.Remove(reaction);
            await _context.SaveChangesAsync();

            return Ok("Reaction Removed");
        }

        [HttpGet("count/{postId}")]
        public async Task<IActionResult> Count(int postId)
        {
            var count = await _context.Reactions
                .Where(x => x.PostId == postId)
                .CountAsync();

            return Ok(count);
        }

        [Authorize]
        [HttpGet("my/{postId}")]
        public async Task<IActionResult> MyReaction(int postId)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return Unauthorized();

            var reaction = await _context.Reactions
                .FirstOrDefaultAsync(x => x.PostId == postId && x.UserId == user.Id);

            if (reaction == null) return Ok(null);

            return Ok(reaction.Type);
        }

        [Authorize]
        [HttpGet("all/{postId}")]
        public async Task<IActionResult> GetPostReactions(int postId)
        {
            var reactions = await _context.Reactions
                .Include(x => x.User)
                .Where(x => x.PostId == postId)
                .Select(r => new ReactionResponseDto
                {
                    UserName = r.User.Name,
                    UserImage = r.User.ProfileImage,
                    Type = r.Type,
                    CreatedAt = r.CreatedAt
                }).ToListAsync();

            return Ok(reactions);
        }
    }
}
