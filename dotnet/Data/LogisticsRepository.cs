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

    public class LogisticsRepository : ILogisticsRepository
    {
        private readonly string _env;

        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IIOServiceContext _context;

        public LogisticsRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context, ICachedKeys cachedKeys) {
            this._environmentVariableProvider = environmentVariableProvider ??
                                    throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                            throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                                    throw new ArgumentNullException(nameof(clientFactory));
        
            this._context = context ??
                               throw new ArgumentNullException(nameof(context));

            this._env = string.Equals(this._environmentVariableProvider.Workspace, "master") ? "vtexcommercestable" : "vtexcommercebeta";
        }

        public async Task<LogisticsDocksListWrapper> GetDocks() {
            LogisticsDocksListWrapper dockList = new LogisticsDocksListWrapper();

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._context.Vtex.Account}.{this._env}.com.br/api/logistics/pvt/configuration/docks")
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
                    JArray array = JArray.Parse(responseContent);

                    foreach (JObject obj in array.Children<JObject>())
                    {
                        var parsedObject = obj;
                        dockList.DocksList.Add(new LogisticsDockWrapper(
                            parsedObject["id"].ToString(),
                            parsedObject["name"].ToString(),
                            ((JArray) parsedObject["shippingRatesProviders"]).ToObject<List<string>>()
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetDocks", null, "Error:", ex);
            }


            return dockList;
        }

        public async Task<bool> ChangeShippingProviders(UpdateDockRequest updateDockRequest, JObject requestBody) {

            bool IsSuccess = false;

            try
            {
                List<string> shippingRatesProviders = ((JArray) requestBody["shippingRatesProviders"]).ToObject<List<string>>();

                if (updateDockRequest.ToRemove) {
                    shippingRatesProviders.RemoveAll(provider => string.Equals(provider, "vtexus.fedex-shipping"));
                } else {
                    shippingRatesProviders.Add("vtexus.fedex-shipping");
                }

                requestBody["shippingRatesProviders"] = JToken.Parse(JsonConvert.SerializeObject(shippingRatesProviders));

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"http://{this._context.Vtex.Account}.{this._env}.com.br/api/logistics/pvt/configuration/docks"),
                    Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, Constants.APPLICATION_JSON)
                };

                string authToken = this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_CREDENTIAL];
                if (authToken != null)
                {
                    request.Headers.Add(Constants.VTEX_ID_HEADER_NAME, authToken);
                    request.Headers.Add(Constants.AUTHORIZATION_HEADER_NAME, authToken);
                }

                var client = _clientFactory.CreateClient();
                var response = await client.SendAsync(request);
                IsSuccess = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("ChangeShippingProviders", null, 
                "Error:", ex,
                new[]
                {
                    ( "updateDockRequest", JsonConvert.SerializeObject(updateDockRequest) ),
                    ( "requestBody", JsonConvert.SerializeObject(requestBody) ),
                });
            }

            return IsSuccess;
        }

        public async Task<bool> SetDocks(UpdateDockRequest updateDockRequest) {
            bool IsSuccess = false;

            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"http://{this._context.Vtex.Account}.{this._env}.com.br/api/logistics/pvt/configuration/docks/{updateDockRequest.DockId}")
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
                    JObject parsedObject = JObject.Parse(responseContent);
                    var isSuccessChange = await ChangeShippingProviders(updateDockRequest, parsedObject);
                    IsSuccess = isSuccessChange;
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("SetDocks", null, 
                "Error:", ex,
                new[]
                {
                    ( "updateDockRequest", JsonConvert.SerializeObject(updateDockRequest) ),
                });
            }

            return IsSuccess;
        }
    }
}