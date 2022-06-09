namespace FedexShipping.Data
{
    using Microsoft.AspNetCore.Http;
    using FedexShipping.Models;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using FedexShipping.Services;
    using Newtonsoft.Json;

    public class MerchantSettingsRepository : IMerchantSettingsRepository
    {
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _applicationName;

        public MerchantSettingsRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                                  throw new ArgumentNullException(nameof(clientFactory));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";

        }

        public async Task<MerchantSettings> GetMerchantSettings()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_ACCOUNT]}/{this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{Constants.SETTINGS_BUCKET}/files/{Constants.SETTINGS_NAME}{Constants.CARRIER.ToUpper()}"),
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(Constants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();
            MerchantSettings merchantSettings = JsonConvert.DeserializeObject<MerchantSettings>(responseContent);
            if (merchantSettings.ItemModals.Count == 0)
            {
                merchantSettings.ItemModals.Add(new ModalMap("CHEMICALS", "HAZARDOUS_MATERIALS", false));
                merchantSettings.ItemModals.Add(new ModalMap("ELECTRONICS", "BATTERY", false));
                merchantSettings.ItemModals.Add(new ModalMap("FURNITURE", "NONE", false));
                merchantSettings.ItemModals.Add(new ModalMap("GLASS", "NONE", false));
                merchantSettings.ItemModals.Add(new ModalMap("LIQUID", "HAZARDOUS_MATERIALS", false));
                merchantSettings.ItemModals.Add(new ModalMap("MATTRESSES", "NONE", false));
                merchantSettings.ItemModals.Add(new ModalMap("REFRIGERATED", "NONE", false));
                merchantSettings.ItemModals.Add(new ModalMap("TIRES", "NONE", false));
                merchantSettings.ItemModals.Add(new ModalMap("WHITE_GOODS", "NONE", false));
                merchantSettings.ItemModals.Add(new ModalMap("FIREARMS", "ORM_D", false));
            }
            if (merchantSettings.SlaSettings.Count == 0) 
            {
                merchantSettings.SlaSettings.Add(new SlaSettings("FedEx Ground", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("Priority Overnight", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("Express Saver", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("2DAY AM", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("First Overnight", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("Standard Overnight", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("2Day", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("FedEx Home Delivery", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("International Economy", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("International Priority Express (IP EXP)", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("International Priority EOD (IP EOD)", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("International Connect Plus", false, 0, 0));
                merchantSettings.SlaSettings.Add(new SlaSettings("International First", false, 0, 0));

            }
            if (merchantSettings.PackingAccessKey == null) 
            {
                merchantSettings.PackingAccessKey = "";
            }
            return merchantSettings;
        }

        public async Task<bool> SetMerchantSettings(MerchantSettings merchantSettings)
        {
            if (merchantSettings == null)
            {
                merchantSettings = new MerchantSettings();
            }

            var jsonSerializedMerchantSettings = JsonConvert.SerializeObject(merchantSettings);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"http://vbase.{this._environmentVariableProvider.Region}.vtex.io/{this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_ACCOUNT]}/{this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_WORKSPACE]}/buckets/{this._applicationName}/{Constants.SETTINGS_BUCKET}/files/{Constants.SETTINGS_NAME}{Constants.CARRIER.ToUpper()}"),
                Content = new StringContent(jsonSerializedMerchantSettings, Encoding.UTF8, Constants.APPLICATION_JSON)
            };

            string authToken = this._httpContextAccessor.HttpContext.Request.Headers[Constants.HEADER_VTEX_CREDENTIAL];
            if (authToken != null)
            {
                request.Headers.Add(Constants.AUTHORIZATION_HEADER_NAME, authToken);
            }

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
