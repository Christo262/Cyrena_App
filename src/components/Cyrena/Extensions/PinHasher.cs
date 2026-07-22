using System.Security.Cryptography;

namespace Cyrena.Extensions
{
    public static class PinHasher
    {
        private const int SaltSize = 16; // 128 bits
        private const int HashSize = 32; // 256 bits
        private const int Iterations = 100000;

        /// <summary>
        /// Hashes a PIN using PBKDF2 with a random salt.
        /// Returns a base64 string containing salt + hash.
        /// </summary>
        public static string HashPin(string pin)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            byte[] hashBytes = new byte[SaltSize + HashSize];
            Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifies a PIN against a stored hash.
        /// </summary>
        public static bool VerifyPin(string pin, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            byte[] hashBytes = Convert.FromBase64String(storedHash);
            if (hashBytes.Length != SaltSize + HashSize)
                return false;

            byte[] salt = new byte[SaltSize];
            byte[] expectedHash = new byte[HashSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(hashBytes, SaltSize, expectedHash, 0, HashSize);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(pin, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
    }
}
