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
    public class ClubController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClubController(AppDbContext context)
        {
            _context = context;
        }

        // CREATE CLUB
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create(CreateClubDTO dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound("User not found");

            var exists = await _context.Clubs.AnyAsync(x => x.Name == dto.Name);

            if (exists)
                return BadRequest("Club already exists");

            var club = new Club
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedBy = user.Id
            };

            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();

            return Ok(club);
        }

        // JOIN CLUB
        [Authorize]
        [HttpPost("join")]
        public async Task<IActionResult> Join(JoinClubDTO dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound("User not found");

            var exists = await _context.ClubMembers
                .AnyAsync(x => x.UserId == user.Id && x.ClubId == dto.ClubId);

            if (exists)
                return BadRequest("Already joined");

            var member = new ClubMember
            {
                UserId = user.Id,
                ClubId = dto.ClubId
            };

            _context.ClubMembers.Add(member);
            await _context.SaveChangesAsync();

            return Ok(member);
        }

        // LEAVE CLUB
        [Authorize]
        [HttpDelete("leave/{clubId}")]
        public async Task<IActionResult> Leave(int clubId)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound("User not found");

            var member = await _context.ClubMembers
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.ClubId == clubId);

            if (member == null)
                return NotFound("Not a member");

            _context.ClubMembers.Remove(member);
            await _context.SaveChangesAsync();

            return Ok("Left club");
        }

        // GET ALL CLUBS

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var clubs = await _context.Clubs.ToListAsync();
            return Ok(clubs);
        }

        // UPDATE CLUB (NEW)
        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, CreateClubDTO dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound("User not found");

            var club = await _context.Clubs.FirstOrDefaultAsync(x => x.Id == id);

            if (club == null)
                return NotFound("Club not found");

            if (club.CreatedBy != user.Id)
                return Forbid();

            club.Name = dto.Name ?? club.Name;
            club.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(club);
        }

        // DELETE CLUB (NEW)
        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null)
                return NotFound("User not found");

            var club = await _context.Clubs.FirstOrDefaultAsync(x => x.Id == id);

            if (club == null)
                return NotFound("Club not found");

            if (club.CreatedBy != user.Id)
                return Forbid();

            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();

            return Ok("Club deleted successfully");
        }
    }
}