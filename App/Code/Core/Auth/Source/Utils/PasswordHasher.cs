using System.Security.Cryptography;

namespace LLE.Auth.Utils
{
    /// <summary>
    /// Provides password hashing and verification using PBKDF2 (SHA-256) with a
    /// per-password random salt, in the format <c>{iterations}.{salt}.{hash}</c>
    /// (salt and hash are Base64-encoded).
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>Size of the randomly generated salt, in bytes (128-bit).</summary>
        private const int SaltSize = 16;      // 128-bit

        /// <summary>Size of the derived hash, in bytes (256-bit).</summary>
        private const int HashSize = 32;      // 256-bit

        /// <summary>Number of PBKDF2 iterations applied when hashing.</summary>
        private const int Iterations = 100_000;

        /// <summary>
        /// Hashes a plaintext password using PBKDF2 with a newly generated random salt.
        /// </summary>
        /// <param name="password">The plaintext password to hash.</param>
        /// <returns>
        /// A string encoding the iteration count, salt, and hash as
        /// <c>{iterations}.{base64 salt}.{base64 hash}</c>, suitable for storage and later
        /// verification via <see cref="Verify"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="password"/> is <c>null</c>.</exception>
        public static string Hash(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            Span<byte> salt = stackalloc byte[SaltSize];
            RandomNumberGenerator.Fill(salt);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return string.Join(
                '.',
                Iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        /// <summary>
        /// Verifies a plaintext password against a hash previously produced by <see cref="Hash"/>,
        /// using a constant-time comparison to avoid timing attacks.
        /// </summary>
        /// <param name="password">The plaintext password to verify.</param>
        /// <param name="storedHash">The previously stored hash string, in the format produced by <see cref="Hash"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="password"/> matches <paramref name="storedHash"/>; otherwise <c>false</c>.
        /// Returns <c>false</c> (rather than throwing) if <paramref name="storedHash"/> is malformed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="password"/> or <paramref name="storedHash"/> is <c>null</c>.
        /// </exception>
        public static bool Verify(string password, string storedHash)
        {
            ArgumentNullException.ThrowIfNull(password);
            ArgumentNullException.ThrowIfNull(storedHash);

            var parts = storedHash.Split('.');

            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out var iterations))
                return false;

            byte[] salt;
            byte[] expectedHash;

            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expectedHash = Convert.FromBase64String(parts[2]);
            }
            catch
            {
                return false;
            }

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(
                actualHash,
                expectedHash);
        }
    }
}