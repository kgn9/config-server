using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Config.Server.Application.Utils;

// TODO Get rid of magic numbers if possible
public static class PasswordEncoder
{
    public static string Encode(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

        return Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password,
            salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
    }
}