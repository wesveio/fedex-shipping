using System;
using System.Collections.Specialized;

namespace FedexShipping.Data
{
    public class CachedKeys : ICachedKeys
    {
        // k,v as cacheKey: int, currentTime: DateTime
        private readonly OrderedDictionary _orderedCacheKeys;

        public CachedKeys()
        {
            this._orderedCacheKeys = new OrderedDictionary();
        }

        public void AddCacheKey(int cacheKey)
        {
            this._orderedCacheKeys.Add(cacheKey, DateTime.Now);
        }

        public void RemoveCacheKey(int cacheKey)
        {
            // Removes element at index
            this._orderedCacheKeys.RemoveAt(cacheKey);
        }

        // Example: [1,2,5,9,10,11,23,44,68,90] and if the lookup is 50
        // Then all elements from index 0 to 7 will be removed
        public int ListExpiredKeys()
        {
            int removalIndex = BinarySearch(_orderedCacheKeys, DateTime.Now.AddMinutes(-10));
            return removalIndex;
        }

        public OrderedDictionary GetOrderedDictionary()
        {
            return _orderedCacheKeys;
        }

        // Binary searches the list to find the maximum element
        // Which is less than or equal to the searching element
        public int BinarySearch(OrderedDictionary orderedDictionary, DateTime searchTime)
        {
            int left = 0;
            int right = orderedDictionary.Count - 1;
            
            while (left <= right) {
                int mid = left + ((right - left) / 2);
                
                if ((DateTime) orderedDictionary[mid] == searchTime) {
                    return mid;
                } else if ((DateTime) orderedDictionary[mid] < searchTime) {
                    left = mid + 1;
                } else {
                    right = mid - 1;
                }
            }
            
            // If there is no match, returns 1 index
            // behind start of final pivot between
            return left - 1;
                
        }
    }
}