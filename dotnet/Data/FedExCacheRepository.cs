namespace FedexShipping.Data
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Text;
    using System.Net.Http;
    using System.Collections.Generic;
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
                _context.Vtex.Logger.Error("TryGetCache", null, "Error getting cache", ex);
            }

            return success;
        }

        public async Task<bool> SetCache(int cacheKey, GetRatesResponseWrapper fedexRatesCache) {
            bool success = false;

            try
            {
                success = await CacheRatesResponse(cacheKey, fedexRatesCache);
                if (success)
                {
                    await _cachedKeys.AddCacheKey(cacheKey);
                }

                List<int> keysToRemove = await _cachedKeys.ListExpiredKeys();
                foreach (int cacheKeyToRemove in keysToRemove)
                {
                    await CacheRatesResponse(cacheKey, null);
                    await _cachedKeys.RemoveCacheKey(cacheKey);
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("TryGetCache", null, "Error setting cache", ex);
            }

            return success;
        }

        public async Task<GetRatesResponseWrapper> GetCachedRatesResponse(int cacheKey)
        {
            GetRatesResponseWrapper getRatesResponseWrapper = null;

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

            return getRatesResponseWrapper;
        }

        public async Task<bool> CacheRatesResponse(int cacheKey, GetRatesResponseWrapper fedexRatesCache)
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

            return response.IsSuccessStatusCode;
        }
    }
}