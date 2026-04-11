using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
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

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Club club)
        {
            if (club == null)
                return BadRequest("Club data is required");

            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();

            return Ok(club);
        }

        // Modify a club
        [HttpPut("modified/{clubId}")]
        public async Task<IActionResult> ModifiedClub(int clubId, [FromBody] Club updatedClub)
        {

            if (updatedClub == null) return BadRequest("Club data is required");

            var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == clubId);
            if (club == null)
                return NotFound("Club not found");

            // Update fields
            club.Name = updatedClub.Name;
            club.Description = updatedClub.Description;
            club.CreatedBy = updatedClub.CreatedBy;

            await _context.SaveChangesAsync();

            return Ok(club);
        }

        // Delete a club
        [HttpDelete("delete/{clubId}")]
        public async Task<IActionResult> Delete(int clubId)
        {

            var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == clubId);
            if (club == null)
                return NotFound("Club not found");

            _context.Clubs.Remove(club);
            await _context.SaveChangesAsync();

            return Ok("Club removed successfully");
        }

        // Join a club
        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] ClubMember member)
        {
            if (member == null)
                return BadRequest("Member data is required");

            var exists = await _context.ClubMembers
                .FirstOrDefaultAsync(x => x.UserId == member.UserId && x.ClubId == member.ClubId);

            if (exists != null)
                return BadRequest("Already joined");

            _context.ClubMembers.Add(member);
            await _context.SaveChangesAsync();

            return Ok(member);
        }

        // Modify a club member
        [HttpPut("clubmembermodified")]
        public async Task<IActionResult> ModifiedMember([FromBody] ClubMember member)
        {
            if (member == null)
                return BadRequest("Member data is required");

            var exists = await _context.ClubMembers
                .FirstOrDefaultAsync(x => x.UserId == member.UserId && x.ClubId == member.ClubId);

            if (exists == null)
                return NotFound("Member not found");

            // Update fields if needed
            exists.Role = member.Role;

            _context.ClubMembers.Update(exists);
            await _context.SaveChangesAsync();

            return Ok(exists);
        }

        // Leave a club
        [HttpDelete("leave/{userId}/{clubId}")]
        public async Task<IActionResult> Leave(int userId, int clubId)
        {
            var member = await _context.ClubMembers
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ClubId == clubId);

            if (member == null)
                return BadRequest("User is not a member of this club");

            _context.ClubMembers.Remove(member);
            await _context.SaveChangesAsync();

            return Ok("Left the club successfully");
        }
    }
}