using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentitySandboxApp.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace IdentitySandboxApp.Infrastructure
{
    //public class DigitsTokenProvider : IUserTwoFactorTokenProvider<User>
    public class DigitsTokenProvider<TUser, TKey> : IUserTwoFactorTokenProvider<TUser>
        where TUser : IdentityUser<TKey>
        where TKey  : IEquatable<TKey>
    {
        private static readonly Dictionary<string, string> KeysStore = new Dictionary<string, string>();

        public Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            string token = new Random().Next(100000, 999999).ToString();

            string key = $"{purpose}:{user.Id}";

            if (KeysStore.ContainsKey(key))
            {
                KeysStore[key] = token;
            }
            else
            {
                KeysStore.Add(key, token);
            }

            return Task.FromResult(token);
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            string key = $"{purpose}:{user.Id}";

            if (!KeysStore.ContainsKey(key)) return Task.FromResult(false);

            if (KeysStore[key] == token)
            {
                KeysStore.Remove(key);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(true);
        }
    }

    public class DigitsTokenProvider : DigitsTokenProvider<User, long> { }
}
