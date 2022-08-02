using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;

namespace FedexShipping.Data
{
    public class CachedKeys : ICachedKeys
    {
        private readonly Dictionary<int, DateTime> _cacheKeys;

        // k,v as cacheKey: int, currentTime: DateTime
        private readonly OrderedDictionary _orderedCacheKeys;

        public CachedKeys()
        {
            this._cacheKeys = new Dictionary<int, DateTime>();
            this._orderedCacheKeys = new OrderedDictionary();
        }

        public void AddCacheKey(int cacheKey)
        {
            this._cacheKeys.Add(cacheKey, DateTime.Now);
            this._orderedCacheKeys.Add(cacheKey, DateTime.Now);
        }

        public void RemoveCacheKey(int cacheKey)
        {
            this._cacheKeys.Remove(cacheKey);
            this._orderedCacheKeys.Remove(cacheKey);
        }

        public List<int> ListExpiredKeys()
        {
            Dictionary<int, DateTime> keysToRemove = this._cacheKeys.Where(k => k.Value < DateTime.Now.AddMinutes(-10)).ToDictionary(c => c.Key, c => c.Value);

            return keysToRemove.Select(k => k.Key).ToList();
        }

        private int BinarySearch(OrderedDictionary orderedDictionary, DateTime searchTime) {
            int left = 0;
            int right = orderedDictionary.Count - 1;
            
            while (left <= right) {
                int mid = left + (right - left) / 2;
                
                if ((DateTime) orderedDictionary[mid] == searchTime) {
                    return mid;
                } else if ((DateTime) orderedDictionary[mid] < searchTime) {
                    left = mid + 1;
                } else {
                    right = mid - 1;
                }
            }
            
            return -1;
                
        }
    }
}


/*

using System;
using System.Threading;

using System.Collections.Specialized;
public class HelloWorld
{
    public static void Main(string[] args)
    {
        OrderedDictionary myDict = new OrderedDictionary();
  
        // Adding key and value in myDict
        myDict.Add("key0", DateTime.Now);
        myDict.Add("key1", DateTime.Now);
        myDict.Add("key2", DateTime.Now);
        myDict.Add("key3", DateTime.Now);
        DateTime x = DateTime.Now;
        myDict.Add("key4", x);
        myDict.Add("key5", DateTime.Now);
        int ans = BinarySearch(myDict, x);
        Console.WriteLine(ans);
    }
    
    public static int BinarySearch(OrderedDictionary orderedDictionary, DateTime searchTime) {
            int left = 0;
            int right = orderedDictionary.Count - 1;
            
            while (left <= right) {
                int mid = left + (right - left) / 2;
                
                if ((DateTime) orderedDictionary[mid] == searchTime) {
                    Console.WriteLine(searchTime);
                    return mid;
                } else if ((DateTime) orderedDictionary[mid] < searchTime) {
                    left = mid + 1;
                } else {
                    right = mid - 1;
                }
            }
            
            return -1;
        }
}
*/