using System;
using System.Collections.Generic;
using System.Linq;

namespace FedexShipping.Data
{
    public class CachedKeys : ICachedKeys
    {
        private readonly Dictionary<int, DateTime> _cacheKeys;

        public CachedKeys()
        {
            this._cacheKeys = new Dictionary<int, DateTime>();
        }

        public void AddCacheKey(int cacheKey)
        {
            this._cacheKeys.Add(cacheKey, DateTime.Now);
        }

        public void RemoveCacheKey(int cacheKey)
        {
            this._cacheKeys.Remove(cacheKey);
        }

        public List<int> ListExpiredKeys()
        {
            Dictionary<int, DateTime> keysToRemove = this._cacheKeys.Where(k => k.Value < DateTime.Now.AddMinutes(-10)).ToDictionary(c => c.Key, c => c.Value);

            return keysToRemove.Select(k => k.Key).ToList();
        }
    }
}