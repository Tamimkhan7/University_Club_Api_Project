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
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PostController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreatePostDto dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return Unauthorized();

            var club = await _context.Clubs.FindAsync(dto.ClubId);
            if (club == null) return NotFound("Club not found");

            var post = new Post
            {
                Content = dto.Content,
                ImageUrl = dto.ImageUrl,
                ClubId = dto.ClubId,
                UserId = user.Id
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(post);
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, UpdatePostDto dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value; //User means, currently logged in user
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email); //Users means, all users in database, and we are trying to find the user with the email of currently logged in user
            if (user == null) return Unauthorized();

            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id);
            if (post == null) return NotFound("Post not found"); //send custome message


            post.Content = dto.Content;
            post.ImageUrl = dto.ImageUrl;
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

        [Authorize]
        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            var posts = await _context.Posts
                .Include(x => x.User)
                .Include(x => x.Comments)
                .Include(x => x.Reactions)
                .OrderByDescending(x => x.CreatedAt)
                .Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    CreatedAt = p.CreatedAt,
                    UserName = p.User.Name,
                    UserImage = p.User.ProfileImage,
                    CommentCount = p.Comments.Count,
                    ReactionCount = p.Reactions.Count
                }).ToListAsync();

            return Ok(posts);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            if (user == null) return Unauthorized();

            //var post = _context.Posts.FirstOrDefault(x => x.Id == id);
            var post = await _context.Posts
                .Include(x => x.User)
                .Include(x => x.Comments)
                .Include(x => x.Reactions)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (post == null) return NotFound();

            return Ok(post);
        }


    }
}
