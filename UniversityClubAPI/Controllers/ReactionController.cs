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
    public class ReactionController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReactionController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("react")]
        public async Task<IActionResult> React(Reaction reaction)
        {

            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null) return Unauthorized();

            var existing = await _context.Reactions.FirstOrDefaultAsync(x => x.PostId == reaction.PostId && x.UserId == reaction.UserId);

            if (existing != null) existing.Type = reaction.Type;
            else
            {
                reaction.UserId = user.Id;
                _context.Reactions.Add(reaction);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
