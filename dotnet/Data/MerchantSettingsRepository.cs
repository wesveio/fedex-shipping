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
    using System.Collections.Generic;
    using Vtex.Api.Context;

    public class MerchantSettingsRepository : IMerchantSettingsRepository
    {
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _clientFactory;
        private readonly string _applicationName;
        private readonly IIOServiceContext _context;

        public MerchantSettingsRepository(IVtexEnvironmentVariableProvider environmentVariableProvider, IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory, IIOServiceContext context)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                                                throw new ArgumentNullException(nameof(environmentVariableProvider));

            this._httpContextAccessor = httpContextAccessor ??
                                        throw new ArgumentNullException(nameof(httpContextAccessor));

            this._clientFactory = clientFactory ??
                                  throw new ArgumentNullException(nameof(clientFactory));

            this._applicationName =
                $"{this._environmentVariableProvider.ApplicationVendor}.{this._environmentVariableProvider.ApplicationName}";
            this._context = context 
                ?? throw new ArgumentNullException(nameof(context));

        }

        public async Task<MerchantSettings> GetMerchantSettings()
        {
            MerchantSettings merchantSettings = new MerchantSettings();

            try
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
                merchantSettings = JsonConvert.DeserializeObject<MerchantSettings>(responseContent);
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

                if (merchantSettings.SlaSettings.Count == 19)
                {
                    merchantSettings.SlaSettings.Add(new SlaSettings("International Economy Freight", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("International Priority", false, 0, 0));
                }

                if (merchantSettings.SlaSettings.Count < 19)
                {
                    merchantSettings.SlaSettings = new List<SlaSettings>();
                    merchantSettings.SlaSettings.Add(new SlaSettings("International Economy Freight", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("International Priority", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("FedEx Ground", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("Priority Overnight", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("Express Saver", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("2DAY AM", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("First Overnight", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("Standard Overnight", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("2Day", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("FedEx Home Delivery", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("First Overnight Freight", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("1 Day Freight", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("2 Day Freight", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("3 Day Freight", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("International Economy", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("International Priority Express (IP EXP)", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("International Priority EOD (IP EOD)", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("International Connect Plus", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("International First", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("International Two Day", false, 0, 0));
                    merchantSettings.SlaSettings.Add(new SlaSettings("International Priority Freight", false, 0, 0));
                }
                if (merchantSettings.PackingAccessKey == null) 
                {
                    merchantSettings.PackingAccessKey = "";
                }

            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetMerchantSettings", null, "Error:", ex);
            }

            return merchantSettings;
        }

        public async Task<bool> SetMerchantSettings(MerchantSettings merchantSettings)
        {

            bool IsSuccess = false;

            try
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
                IsSuccess = response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("SetMerchantSettings", null, 
                "Error:", ex,
                new[]
                {
                    ( "merchantSettings", JsonConvert.SerializeObject(merchantSettings) ),
                });
            }

            return IsSuccess;
        }
    }
}
