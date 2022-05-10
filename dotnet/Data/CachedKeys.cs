using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FedexShipping.Data
{
    public class CachedKeys : ICachedKeys
    {
        private Dictionary<int, DateTime> _cacheKeys;

        public CachedKeys()
        {
            this._cacheKeys = new Dictionary<int, DateTime>();
        }

        public async Task AddCacheKey(int cacheKey)
        {
            this._cacheKeys.Add(cacheKey, DateTime.Now);
        }

        public async Task RemoveCacheKey(int cacheKey)
        {
            bool removed = this._cacheKeys.Remove(cacheKey);
        }

        public async Task<List<int>> ListExpiredKeys()
        {
            Dictionary<int, DateTime> keysToRemove = this._cacheKeys.Where(k => k.Value < DateTime.Now.AddMinutes(-10)).ToDictionary(c => c.Key, c => c.Value);

            return keysToRemove.Select(k => k.Key).ToList();
        }
    }
}