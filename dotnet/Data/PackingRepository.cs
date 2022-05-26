namespace FedexShipping.Data
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Text;
    using Vtex.Api.Context;
    using FedexShipping.Models;
    using FedexShipping.Services;
    using System.Collections.Generic;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class PackingRepository : IPackingRepository
    {
        private readonly string _applicationName;
        private readonly string _env;

        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IIOServiceContext _context;
        private readonly ICachedKeys _cachedKeys;

        public PackingRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context, ICachedKeys cachedKeys) {
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

        public async Task<PackingResponseWrapper> PackItems(PackingRequest packingRequest) {
            PackingResponseWrapper packingResponse = new PackingResponseWrapper();

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"http://{this._environmentVariableProvider.Workspace}--{this._context.Vtex.Account}.myvtex.com/packAll"),
                Content = new StringContent(JsonConvert.SerializeObject(packingRequest), Encoding.UTF8, Constants.APPLICATION_JSON),
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(Constants.VTEX_ID_HEADER_NAME, authToken);
                request.Headers.Add(Constants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                packingResponse = JsonConvert.DeserializeObject<PackingResponseWrapper>(responseContent);
            }

            return packingResponse;
        }
    }
}