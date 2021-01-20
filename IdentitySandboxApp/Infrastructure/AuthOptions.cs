using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace IdentitySandboxApp.Infrastructure
{
    public class AuthOptions
    {
        /// <summary>
        /// Издатель токена
        /// </summary>
        public const string ISSUER = "JwtAuthServer";
        /// <summary>
        /// Потребитель токена
        /// </summary>
        public const string AUDIENCE = "JwtAuthClient";
        /// <summary>
        /// Время жизни токена в минутах
        /// </summary>
        public const int LIFETIME = 120;


        /// <summary>
        /// Ключ для шифрования
        /// </summary>
        private const string Key = "abcdefjhijklmnopqrstuvwxyz";


        public static SymmetricSecurityKey GetKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}
