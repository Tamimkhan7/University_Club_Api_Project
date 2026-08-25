using UniversityClubAPI.DTOs;
using UniversityClubAPI.DTOs.User;
using UniversityClubAPI.Helpers;

namespace UniversityClubAPI.Services.Auth
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> RegisterAsync(RegisterDTO dto);
        Task<ApiResponse<object>> LoginAsync(LoginDTO dto);
        Task<ApiResponse<string>> VerifyEmailAsync(string token);
        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordDto dto);
        Task<ApiResponse<object>> RefreshTokenAsync(TokenDto dto);
        Task<ApiResponse<object>> GetMeAsync(int userId);
        Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordDto dto);
    }
}