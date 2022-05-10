using System.Threading.Tasks;
using FedexShipping.Models;

namespace FedexShipping.Data
{
    public interface IFedExCacheRepository
    {
        bool TryGetCache(int cacheKey, out GetRatesResponseWrapper fedexRatesCache);
        Task<bool> SetCache(int cacheKey, GetRatesResponseWrapper fedexRatesCache);
    }
}