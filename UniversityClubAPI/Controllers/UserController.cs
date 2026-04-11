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
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var user = _context.Users.FirstOrDefault(x => x.Email == email);

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

        //create user profile -> for new users
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update(User model)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound("User not found");

            user.Name = model.Name;
            user.Bio = model.Bio;
            user.Department = model.Department;
            user.Batch = model.Batch;

            await _context.SaveChangesAsync();

            return Ok(user);
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("Profile deleted successfully");
        }
    }
}