namespace UniversityClubAPI.Helpers
{
    public static class ClubPermissionHelper
    {
        public static bool IsAdmin(string role)
        {
            return role == "Admin";
        }

        public static bool CanManage(string role)
        {
            return role == "Admin" || role == "Moderator";
        }
    }
}
