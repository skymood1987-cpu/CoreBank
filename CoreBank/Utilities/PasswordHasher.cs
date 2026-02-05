using System.Security.Cryptography;
using System.Text;

namespace MinCoreBank.Utilities
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA256;
        private const char Delimiter = ';';

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                _hashAlgorithm,
                KeySize);

            return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public static bool Verify(string passwordHash, string inputPassword)
        {
            var elements = passwordHash.Split(Delimiter);
            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);

            var hashInput = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(inputPassword),
                salt,
                Iterations,
                _hashAlgorithm,
                KeySize);

            return CryptographicOperations.FixedTimeEquals(hash, hashInput);
        }
    }
}