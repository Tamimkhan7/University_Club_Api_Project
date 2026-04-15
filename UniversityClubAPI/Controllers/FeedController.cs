using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedController : ControllerBase
    {

        private readonly AppDbContext _context;
        public FeedController(AppDbContext Context)
        {
            _context = Context;
        }


        [HttpGet("feed")]
        public async Task<IActionResult> Feed()
        {
            var feed = await _context.Posts.Include(x => x.User)
                .Include(x => x.Comments)
                .Include(x => x.Reactions)
                .OrderByDescending(x => x.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Content,
                    p.ImageUrl,
                    p.CreatedAt,

                    User = new
                    {
                        p.User.Name,
                        p.User.ProfileImage
                    },

                    CommentCount = p.Comments.Count,
                    ReactionCount = p.Reactions.Count
                }).ToListAsync();

            return Ok(feed);
        }
    }
}
