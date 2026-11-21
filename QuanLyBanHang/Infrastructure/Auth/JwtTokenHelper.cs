using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace QuanLyBanHang.Infrastructure.Auth
{
    /// <summary>
    /// Tạo và xác thực JWT HS256 thuần .NET Framework 4.7.2 + Newtonsoft.Json.
    /// Đọc khóa bí mật từ Web.config (appSettings["JwtSecret"]).
    /// </summary>
    public static class JwtTokenHelper
    {
        private static string SecretKey
        {
            get
            {
                var key = ConfigurationManager.AppSettings["JwtSecret"];
                return string.IsNullOrWhiteSpace(key)
                    ? "QuanLyBanHang_Default_Jwt_Secret_Key_2026_ChangeInProduction!"
                    : key;
            }
        }

        public static string GenerateToken(Guid taiKhoanId, string hoTen, string email, string vaiTro, DateTimeOffset expiresAt)
        {
            var header = new Dictionary<string, object>
            {
                { "alg", "HS256" },
                { "typ", "JWT" }
            };

            var payload = new Dictionary<string, object>
            {
                { "sub", taiKhoanId.ToString() },
                { "name", hoTen ?? string.Empty },
                { "email", email ?? string.Empty },
                { "role", vaiTro ?? string.Empty },
                { "exp", expiresAt.ToUnixTimeSeconds() },
                { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
            };

            var headerPart = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(header)));
            var payloadPart = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)));
            var signingInput = headerPart + "." + payloadPart;
            var signaturePart = ComputeHmacSha256(signingInput, SecretKey);

            return signingInput + "." + signaturePart;
        }

        public static ClaimsPrincipal ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            var signingInput = parts[0] + "." + parts[1];
            var expectedSig = ComputeHmacSha256(signingInput, SecretKey);
            if (!SlowEquals(parts[2], expectedSig))
                return null;

            try
            {
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                var payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadJson);
                if (payload == null || !payload.ContainsKey("exp") || !payload.ContainsKey("sub"))
                    return null;

                long exp = Convert.ToInt64(payload["exp"]);
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp)
                    return null;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, Convert.ToString(payload["sub"])),
                    new Claim(ClaimTypes.Name, payload.ContainsKey("name") ? Convert.ToString(payload["name"]) : string.Empty),
                    new Claim(ClaimTypes.Email, payload.ContainsKey("email") ? Convert.ToString(payload["email"]) : string.Empty),
                    new Claim(ClaimTypes.Role, payload.ContainsKey("role") ? Convert.ToString(payload["role"]) : string.Empty)
                };

                var identity = new ClaimsIdentity(claims, "Bearer");
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return null;
            }
        }

        private static string ComputeHmacSha256(string input, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Base64UrlEncode(hash);
            }
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string input)
        {
            var s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }

        private static bool SlowEquals(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
