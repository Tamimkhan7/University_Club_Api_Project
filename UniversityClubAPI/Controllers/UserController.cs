using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET PROFILE
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> Profile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Unauthorized();

            var user = await _context.Users
                .Where(x => x.Email == email)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Email,
                    x.Bio,
                    x.ProfileImage,
                    x.Department,
                    x.Batch,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        // UPDATE PROFILE (complete profile system)
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update(UpdateUserDTO model)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound("User not found");

            user.Name = model.Name ?? user.Name;
            user.Bio = model.Bio;
            user.ProfileImage = model.ProfileImage;
            user.Department = model.Department;
            user.Batch = model.Batch;

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        // DELETE PROFILE
        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Profile deleted successfully");
        }
    }
}