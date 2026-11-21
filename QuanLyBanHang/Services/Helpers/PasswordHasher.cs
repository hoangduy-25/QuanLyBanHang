using System;
using System.Linq;
using System.Security.Cryptography;

namespace QuanLyBanHang.Services.Helpers
{
    /// <summary>
    /// Băm và xác minh mật khẩu / câu trả lời bảo mật bằng PBKDF2-HMACSHA256 (.NET Framework 4.7.2 native).
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public static string Hash(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException("Giá trị cần băm không được rỗng.", "plainText");

            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(plainText, salt, Iterations, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(HashSize);
                return string.Format("{0}.{1}.{2}", Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
            }
        }

        public static bool Verify(string plainText, string storedHash)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            var parts = storedHash.Split('.');
            if (parts.Length != 3)
                return false;

            int iterations;
            if (!int.TryParse(parts[0], out iterations))
                return false;

            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);

            using (var pbkdf2 = new Rfc2898DeriveBytes(plainText, salt, iterations, HashAlgorithmName.SHA256))
            {
                var actualHash = pbkdf2.GetBytes(expectedHash.Length);
                return ConstantTimeEquals(actualHash, expectedHash);
            }
        }

        public static string HashSecurityAnswer(string answer)
        {
            var normalized = (answer ?? string.Empty).Trim().ToLowerInvariant();
            return Hash(normalized);
        }

        public static bool VerifySecurityAnswer(string answer, string storedHash)
        {
            var normalized = (answer ?? string.Empty).Trim().ToLowerInvariant();
            return Verify(normalized, storedHash);
        }

        public static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            return hasUpper && hasLower && hasDigit;
        }

        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }
    }
}
