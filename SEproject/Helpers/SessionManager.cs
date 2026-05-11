using System.Security.Cryptography;
using System.Text;

namespace AcademicProjectSystem.Helpers
{
    public static class SessionManager
    {
        public static int    CurrentUserID   { get; private set; }
        public static string CurrentUsername { get; private set; }
        public static string CurrentFullName { get; private set; }
        public static string CurrentRole     { get; private set; }
        public static bool   IsLoggedIn      => CurrentUserID > 0;

        public static void Login(int id, string username, string fullName, string role)
        {
            CurrentUserID   = id;
            CurrentUsername = username;
            CurrentFullName = fullName;
            CurrentRole     = role;
        }

        public static void Logout()
        {
            CurrentUserID   = 0;
            CurrentUsername = null;
            CurrentFullName = null;
            CurrentRole     = null;
        }

        // Simple SHA-1 hex hash (good enough for a local academic system)
        public static string HashPassword(string password)
        {
            using (var sha = SHA1.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var sb = new StringBuilder();
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
