using Microsoft.EntityFrameworkCore;
using UniversityClubAPI.Data;
using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.User;
using UniversityClubAPI.Helpers;
using UniversityClubAPI.Models;
using UniversityClubAPI.Services.Email;

namespace UniversityClubAPI.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthService(AppDbContext context, IConfiguration config, IEmailService emailService)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
        }


        public async Task<ApiResponse<string>> RegisterAsync(RegisterDTO dto)
        {
            dto.Email = dto.Email.Trim().ToLower();
            dto.Name = dto.Name.Trim();

            if (await _context.Users.AnyAsync(x => x.Email == dto.Email))
                throw new ArgumentException("Email already registered");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                ProfileImage = $"https://ui-avatars.com/api/?name={dto.Name}",

                IsEmailVerified = false,

                EmailVerificationToken = Guid.NewGuid().ToString(),
                EmailVerificationExpiry = DateTime.UtcNow.AddHours(24)
            };



            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var link = $"http://localhost:5173/verify-email?token={user.EmailVerificationToken}";

            await _emailService.SendEmailAsync(user.Email, "Verify Email", $"<a href='{link}'>Verify</a>");

            return ApiResponse<string>.Ok("Registered successfully");
        }

        public async Task<ApiResponse<object>> LoginAsync(LoginDTO dto)
        {
            dto.Email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            if (!user.IsEmailVerified)
                throw new UnauthorizedAccessException("Email not verified");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                throw new UnauthorizedAccessException("Invalid email or password");

            var accessToken = JwtHelper.GenerateAccessToken(user, _config);
            var refreshToken = JwtHelper.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return ApiResponse<object>.Ok(new
            {
                accessToken,
                refreshToken,
                user = new { user.Id, user.Name, user.Email, user.Role, user.ProfileImage }
            }, "Login successful");
        }

        public async Task<ApiResponse<string>> VerifyEmailAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.EmailVerificationToken == token);

            if (user == null || user.EmailVerificationExpiry < DateTime.UtcNow)
                throw new ArgumentException("Invalid or expired verification token");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationExpiry = null;

            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Email verified successfully");
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            dto.Email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (user != null)
            {
                user.ResetPasswordToken = Guid.NewGuid().ToString();
                user.ResetPasswordExpiry = DateTime.UtcNow.AddHours(1);

                await _context.SaveChangesAsync();

                var link = $"http://localhost:5173/reset-password?token={user.ResetPasswordToken}";
                await _emailService.SendEmailAsync(user.Email, "Reset Password", $"<a href='{link}'>Reset</a>");
            }


            return ApiResponse<string>.Ok("If an account with that email exists, a reset link has been sent");
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.ResetPasswordToken == dto.Token);

            if (user == null || user.ResetPasswordExpiry < DateTime.UtcNow)
                throw new ArgumentException("Invalid or expired reset token");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;

            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Password reset successfully");
        }

        public async Task<ApiResponse<object>> RefreshTokenAsync(TokenDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var accessToken = JwtHelper.GenerateAccessToken(user, _config);
            var refreshToken = JwtHelper.GenerateRefreshToken();

            user.RefreshToken = refreshToken;

            await _context.SaveChangesAsync();

            return ApiResponse<object>.Ok(new { accessToken, refreshToken });
        }

        public async Task<ApiResponse<object>> GetMeAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Email,
                    x.Role,
                    x.ProfileImage,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                throw new KeyNotFoundException("User not found");

            return ApiResponse<object>.Ok(user);
        }

        public async Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
                throw new UnauthorizedAccessException("Current password is incorrect");

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ApiResponse<string>.Ok("Password changed successfully");
        }
    }
}