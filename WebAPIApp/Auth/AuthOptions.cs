using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIApp.Auth
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
        public const int LIFETIME = 3;
        

        /// <summary>
        /// Ключ для шифрования
        /// </summary>
        private const string key = "abcdefjhijklmnopqrstuvwxyz";


        public static SymmetricSecurityKey GetKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
        }
    }
}
