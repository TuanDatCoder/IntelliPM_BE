using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.Security
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            // Convert the password string to a byte array
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Compute the hash value of the password bytes
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);

            // Convert the hash bytes to a hexadecimal string
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }

            // Return the hashed password as a string
            return sb.ToString();
        }
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        string hashedInput = HashPassword(password);
        return string.Equals(hashedInput, hashedPassword, StringComparison.OrdinalIgnoreCase);
    }
}
}
