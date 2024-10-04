namespace YatriSewa.Services
{
    public class PasswordHashingService
    {
        // Method to hash the password
        public static string HashPassword(string password)
        {
            // Use BCrypt to hash the password
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Method to verify the password against a stored hash
        public static bool VerifyPassword(string password, string storedHash)
        {
            // Use BCrypt to verify the password against the stored hash
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
