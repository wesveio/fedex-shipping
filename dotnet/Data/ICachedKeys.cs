using System.Threading.Tasks;
using System.Collections.Generic;

namespace FedexShipping.Data
{
    public interface ICachedKeys
    {
        Task AddCacheKey(int cacheKey);
        Task RemoveCacheKey(int cacheKey);
        Task<List<int>> ListExpiredKeys();
    }
}