namespace ShippingUtilities.Data
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using ShippingUtilities.Models;

    public class InMemoryMerchantSettingsRepository : IMerchantSettingsRepository
    {
        private readonly IDictionary<string, MerchantSettings> _inMemorySettings = new Dictionary<string, MerchantSettings>();

        public InMemoryMerchantSettingsRepository()
        {
            //Required for All Web Services
            //Developer Test Key:	 614CcXUEx89qLL6S
            //Required for FedEx Web Services for Intra Country Shipping in US and Global
            //Test Account Number:	 510087240
            //Test Meter Number:	 114036294
            //Required for FedEx Web Services for Office and Print
            //Test FedEx Office Integrator ID:	 123
            //Test Client Product ID:	 TEST
            //Test Client Product Version:	 9999

            //Test Account Information
            //Test URL: https://wsbeta.fedex.com:443/web-services
            //Test Password: O6Ar42a8ZavOQsmCCGi1j5Z5f
            //FedEx Shipping Account Number: 510087240
            //FedEx Meter Number: 114036294

            MerchantSettings merchantSettings = new MerchantSettings()
            {
                ClientDetailAccountNumber = "510087240",
                ClientDetailMeterNumber = "114036294",
                ParentCredentialKey = "",
                ParentCredentialPassword = "",
                UserCredentialKey = "614CcXUEx89qLL6S",
                UserCredentialPassword = "O6Ar42a8ZavOQsmCCGi1j5Z5f"
            };

            this.SetMerchantSettings("FedEx", merchantSettings);
        }

        public Task SetMerchantSettings(string carrier, MerchantSettings merchantSettings)
        {
            this._inMemorySettings.Remove("settings");
            this._inMemorySettings.Add("settings", merchantSettings);

            return Task.CompletedTask;
        }

        public Task<MerchantSettings> GetMerchantSettings(string carrier)
        {
            if (!this._inMemorySettings.TryGetValue("settings", out var merchantSettings))
            {
                return Task.FromResult((MerchantSettings)null);
            }

            return Task.FromResult(merchantSettings);
        }
    }
}
