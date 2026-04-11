using Microsoft.AspNetCore.Mvc;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs;
using UniversityClubAPI.Models;

namespace UniversityClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO dto)
        {
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }


        [HttpPost("login")]
        public IActionResult Login(LoginDTO dto)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == dto.Email);
            if (user == null) return Unauthorized();

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
            if ((!isPasswordValid)) return Unauthorized();

            var token = JwtHelper.GenerateToken(user.Email, user.Role, _config);
            return Ok(token);

        }
    }
}


