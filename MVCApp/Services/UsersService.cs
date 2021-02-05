using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace MVCApp.Services
{
    public interface IUsersService
    {
        List<User> GetUsers();
        User GetUser(int id, out bool isFromCache);
    }

    public class UsersService : IUsersService
    {
        private readonly TestappdbContext _db;
        private readonly IMemoryCache _cache;

        public UsersService(TestappdbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public List<User> GetUsers()
        {
            return _db.Users.ToList();
        }

        public User GetUser(int id, out bool isFromCache)
        {
            string key = _getKey(id);
            
            if (_cache.TryGetValue(key, out User res))
            {
                isFromCache = true;
                return res;
            }

            isFromCache = false;

            //res = _db.Users.FirstOrDefault(x => x.Id == id);
            //using ICacheEntry entry = _cache.CreateEntry(id);
            //entry.Value = res;
            //entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

            //return res;

            return _cache.GetOrCreate(key, entry =>
            {
                User item = _db.Users.FirstOrDefault(x => x.Id == id);
                if (item == null) return item;

                entry.Value = item;
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

                return item;
            });
        }

        private string _getKey(int id) => $"{typeof(User).FullName}_{id}";
    }
}
