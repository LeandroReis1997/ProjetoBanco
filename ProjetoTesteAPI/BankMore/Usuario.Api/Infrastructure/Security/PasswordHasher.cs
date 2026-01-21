using System.Security.Cryptography;
using System.Text;

namespace Usuario.Api.Infrastructure.Security
{
    public class PasswordHasher
    {
        public string GerarSalt()
        {
            var bytes = RandomNumberGenerator.GetBytes(16);
            return Convert.ToBase64String(bytes);
        }

        public string Hash(string senha, string salt)
        {
            var bytes = Encoding.UTF8.GetBytes(salt + senha);
            var hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool Verificar(string senha, string salt, string hash)
        {
            return Hash(senha, salt) == hash;
        }
    }
}
