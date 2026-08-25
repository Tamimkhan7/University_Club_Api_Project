using System.Security.Claims;

namespace UniversityClubAPI.Helpers
{
    public static class UserHelper
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(claim, out int userId))
                throw new UnauthorizedAccessException(
                    "Invalid or missing user identity claim");

            return userId;
        }

        public static string GetUserName(ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        public static string GetUserEmail(ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        public static string GetUserRole(ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }
}
