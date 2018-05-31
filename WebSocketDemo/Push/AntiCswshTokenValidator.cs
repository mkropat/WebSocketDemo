using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace WebSocketDemo.Push
{
    public class AntiCswshTokenValidator
    {
        const string _purpose = "AntiCswshToken";
        readonly IDataProtector _dataProtector;
        readonly ILogger _logger;

        public AntiCswshTokenValidator(IDataProtectionProvider dataProtection, ILogger<AntiCswshTokenValidator> logger)
        {
            _dataProtector = dataProtection.CreateProtector(_purpose);
            _logger = logger;
        }

        public string GenerateToken(string userId, DateTime expires)
        {
            using (var buffer = new MemoryStream())
            using (var writer = new BinaryWriter(buffer))
            {
                writer.Write(GenerateNonce());
                writer.Write(expires.Ticks);
                writer.Write(userId ?? string.Empty);

                var secureBytes = _dataProtector.Protect(buffer.ToArray());
                return WebEncoders.Base64UrlEncode(secureBytes);
            }
        }

        public bool IsValid(string token, string expectedUserId)
        {
            expectedUserId = expectedUserId ?? string.Empty;

            try
            {
                var secureBytes = WebEncoders.Base64UrlDecode(token);

                using (var buffer = new MemoryStream(_dataProtector.Unprotect(secureBytes)))
                using (var reader = new BinaryReader(buffer))
                {
                    reader.ReadInt64(); // nonce
                    var expires = new DateTime(reader.ReadInt64());
                    var userId = reader.ReadString();

                    if (DateTime.UtcNow > expires)
                        throw new Exception($"Token expired at {expires}");

                    if (userId != expectedUserId)
                        throw new Exception($"Token userId '{userId}' does not match expected userId '{expectedUserId}'");

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Failed to deserialize anti-CSWSH token");
                return false;
            }
        }

        static long GenerateNonce()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[sizeof(long)];
                rng.GetBytes(buffer);
                return BitConverter.ToInt64(buffer);
            }
        }
    }
}