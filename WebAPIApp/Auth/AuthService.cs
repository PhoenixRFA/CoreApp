using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPIApp.Auth
{
    public class AuthService
    {
        private static List<Person> PeopleDb = new List<Person>
        {
            new Person
            {
                Login = "admin",
                Password = "Qwe123",
                Role = "admin"
            },
            new Person
            {
                Login = "user1",
                Password = "123123",
                Role = "user"
            }
        };

        public ClaimsIdentity GetIdentity(string username, string password)
        {
            Person person = PeopleDb.FirstOrDefault(x => x.Login == username && x.Password == password);
            if (person == null) return null;
            
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, person.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, person.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            
            return claimsIdentity;
        }

        public string GetToken(ClaimsIdentity identity)
        {
            var now = DateTime.Now;
            var credentials = new SigningCredentials(AuthOptions.GetKey(), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    claims: identity.Claims,
                    notBefore: now,
                    expires: now.AddMinutes(AuthOptions.LIFETIME),
                    signingCredentials: credentials
                );
            
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
