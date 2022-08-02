using System.Collections.Specialized;
using System;

namespace FedexShipping.Data
{
    public interface ICachedKeys
    {
        void AddCacheKey(int cacheKey);
        void RemoveCacheKey(int cacheKey);
        int ListExpiredKeys();
        OrderedDictionary GetOrderedDictionary();
        int BinarySearch(OrderedDictionary orderedDictionary, DateTime currentTime);
    }
}