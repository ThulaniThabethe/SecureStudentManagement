namespace SecureStudentManagement.Helpers
{
    public static class StudentAccessHelper
    {
        public static bool IsAdministrator(System.Security.Claims.ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }
    }
}
