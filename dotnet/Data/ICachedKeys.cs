using System.Collections.Generic;

namespace FedexShipping.Data
{
    public interface ICachedKeys
    {
        void AddCacheKey(int cacheKey);
        void RemoveCacheKey(int cacheKey);
        List<int> ListExpiredKeys();
    }
}