using System.Collections.Generic;

namespace MVCApp.Services
{
    public class RequestStoreService : IRequestStoreService
    {
        private readonly Dictionary<string, string> _store;

        public RequestStoreService()
        {
            _store = new Dictionary<string, string>();
        }

        public void Add(string key, string value)
        {
            if (_store.ContainsKey(key))
            {
                _store[key] = value;
            }
            else
            {
                _store.Add(key, value);
            }
        }

        public string Get(string key)
        {
            return _store.ContainsKey(key) ? _store[key] : string.Empty;
        }
    }

    public interface IRequestStoreService
    {
        void Add(string key, string value);
        string Get(string key);
    }
}
