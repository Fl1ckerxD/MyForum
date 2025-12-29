using System.Security.Cryptography;
using System.Text;
using MyForum.Api.Core.Interfaces.Services;

namespace MyForum.Api.Infrastructure.Services
{
    public class SHA256IPHasher : IIPHasher
    {
        public string HashIP(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "unknown")
                return "unknown";

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(ipAddress + "my-secret-salt");
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}