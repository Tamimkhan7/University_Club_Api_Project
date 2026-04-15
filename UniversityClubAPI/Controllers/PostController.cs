using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UniversityClubAPI.Data;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PostController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(Post post)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            if (user == null) return Unauthorized();

            post.UserId = user.Id;
            post.CreatedAt = DateTime.UtcNow;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, Post updatedPost)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            if (user == null) return Unauthorized();

            var post = _context.Posts.FirstOrDefault(x => x.Id == id && x.UserId == user.Id);
            if (post == null) return NotFound("Post not found"); //send custome message

            post.Content = updatedPost.Content;
            post.ImageUrl = updatedPost.ImageUrl;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            if (user == null) return Unauthorized();

            var post = _context.Posts.FirstOrDefault(x => x.Id == id && x.UserId == user.Id);
            if (post == null) return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok("Post deleted successfully");
        }

        [HttpGet("all")]
        public IActionResult All()
        {
            var posts = _context.Posts.OrderByDescending(x => x.CreatedAt).ToList();
            return Ok(posts);
        }

        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            if (user == null) return Unauthorized();

            var post = _context.Posts.FirstOrDefault(x => x.Id == id);
            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpGet("feed")]
        public IActionResult Feed()
        {
            var feed = _context.Posts.OrderByDescending(x => x.CreatedAt).Select(p => new
            {
                p.Id,
                p.Content,
                p.ImageUrl,
                p.CreatedAt,

                User = _context.Users.Where(u => u.Id == p.UserId).Select(u => new
                {
                    u.Name,
                    u.ProfileImage
                }).FirstOrDefault(),

                CommentCount = _context.Comments.Count(c => c.PostId == p.Id),
                ReactionCount = _context.Reactions.Count(r => r.PostId == p.Id)
            }).ToList();

            return Ok(feed);
        }
    }
}
