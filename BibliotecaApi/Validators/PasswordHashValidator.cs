
using Isopoh.Cryptography.Argon2;

namespace BibliotecaApi.Validators
{
    public class PasswordHashValidator
    {
        public bool ValidatePassword(string storedHash, string currentHash) {
            return Argon2.Verify(storedHash, currentHash);
        }

        public string GenerateHash(string password)
        {
            return Argon2.Hash(password);
        }
    }
}
