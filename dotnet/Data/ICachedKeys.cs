using System.Collections.Generic;
using System.Collections.Specialized;
using System;

namespace FedexShipping.Data
{
    public interface ICachedKeys
    {
        void AddCacheKey(int cacheKey);
        void RemoveCacheKey(int cacheKey);
        List<int> ListExpiredKeys();
        int BinarySearch(OrderedDictionary orderedDictionary, DateTime currentTime);
    }
}