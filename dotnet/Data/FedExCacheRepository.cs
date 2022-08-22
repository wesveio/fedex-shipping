namespace FedexShipping.Data
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Text;
    using System.Net.Http;
    using System.Collections;
    using System.Threading.Tasks;
    using Vtex.Api.Context;
    using FedexShipping.Models;
    using FedexShipping.Services;

    using Newtonsoft.Json;

    public class FedExCacheRespository : IFedExCacheRepository
    {
        private readonly string _applicationName;

        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IIOServiceContext _context;
        private readonly ICachedKeys _cachedKeys;

        public FedExCacheRespository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context, ICachedKeys cachedKeys) {
            this._environmentVariableProvider = environmentVariableProvider ??
                                    throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                            throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                                    throw new ArgumentNullException(nameof(clientFactory));
        
            this._context = context ??
                               throw new ArgumentNullException(nameof(context));

            this._cachedKeys = cachedKeys ??
                               throw new ArgumentNullException(nameof(cachedKeys));
                               
            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
        }
        public bool TryGetCache(int cacheKey, out GetRatesResponseWrapper fedexRatesCache)
        {
            bool success = false;
            fedexRatesCache = null;
            try
            {
                fedexRatesCache = GetCachedRatesResponse(cacheKey).Result;
                success = fedexRatesCache != null;
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("TryGetCache", null, 
                "Error:", ex,
                new[]
                {
                    ( "cacheKey", cacheKey.ToString() ),
                    ( "fedexRatesCache", JsonConvert.SerializeObject(fedexRatesCache) )
                });
            }

            return success;
        }

        public async Task<bool> SetCache(int cacheKey, GetRatesResponseWrapper fedexRatesCache)
        {
            bool success = false;

            try
            {
                success = await CacheRatesResponse(cacheKey, fedexRatesCache);
                if (success)
                {
                    _cachedKeys.AddCacheKey(cacheKey);
                }

                int removeKeyMax = _cachedKeys.ListExpiredKeys();
                int removeCount = 0;
                if (removeKeyMax >= 0)
                {
                    foreach (DictionaryEntry entry in _cachedKeys.GetOrderedDictionary())
                    {
                        // Once removeCount reaches removeKeyMax, we stop the iteration
                        if (removeCount == removeKeyMax) {
                            break;
                        }
                        await CacheRatesResponse((int) entry.Key, null);
                        removeCount += 1;
                    }

                    // Removes all elements of index keysToRemove to 0
                    for (int index = removeKeyMax; index >= 0; index--) {
                        _cachedKeys.RemoveCacheKey(index);
                    }
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("SetCache", null, 
                "Error:", ex,
                new[]
                {
                    ( "cacheKey", cacheKey.ToString() ),
                    ( "fedexRatesCache", JsonConvert.SerializeObject(fedexRatesCache) )
                });
            }

            return success;
        }

        public async Task<GetRatesResponseWrapper> GetCachedRatesResponse(int cacheKey)
        {
            GetRatesResponseWrapper getRatesResponseWrapper = null;

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://infra.io.vtex.com/vbase/v2/{this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_ACCOUNT]}/{this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{Constants.RATES_BUCKET}/files/{cacheKey}")
                };

                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(Constants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(Constants.VTEX_ID_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    getRatesResponseWrapper = JsonConvert.DeserializeObject<GetRatesResponseWrapper>(responseContent);
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetCachedRatesResponse", null, 
                "Error:", ex,
                new[]
                {
                    ( "cacheKey", cacheKey.ToString() )
                });
            }


            return getRatesResponseWrapper;
        }

        public async Task<bool> CacheRatesResponse(int cacheKey, GetRatesResponseWrapper fedexRatesCache)
        {
            bool IsScuccess = false;

            try
            {
                var jsonSerializedTaxResponse = JsonConvert.SerializeObject(fedexRatesCache);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri($"http://infra.io.vtex.com/vbase/v2/{this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_ACCOUNT]}/{this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{Constants.RATES_BUCKET}/files/{cacheKey}"),
                    Content = new StringContent(jsonSerializedTaxResponse, Encoding.UTF8, Constants.APPLICATION_JSON)
                };

                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(Constants.AUTHORIZATION_HEADER_NAME, authToken);
                    request.Headers.Add(Constants.VTEX_ID_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                IsScuccess = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("CacheRatesResponse", null, 
                "Error:", ex,
                new[]
                {
                    ( "cacheKey", cacheKey.ToString() ),
                    ( "fedexRatesCache", JsonConvert.SerializeObject(fedexRatesCache) )
                });
            }

            return IsScuccess;
        }
    }
}