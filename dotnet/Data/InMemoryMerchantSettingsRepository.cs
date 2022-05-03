namespace FedexShipping.Data
{
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using FedexShipping.Models;

    public class InMemoryMerchantSettingsRepository : IMerchantSettingsRepository
    {
        private readonly IDictionary<string, MerchantSettings> _inMemorySettings = new Dictionary<string, MerchantSettings>();

        public InMemoryMerchantSettingsRepository()
        {
            MerchantSettings merchantSettings = new MerchantSettings()
            {
                ClientDetailAccountNumber = "9876543210",
                ClientDetailMeterNumber = "1234567890",
                ParentCredentialKey = "",
                ParentCredentialPassword = "",
                UserCredentialKey = "key",
                UserCredentialPassword = "password"
            };

            this.SetMerchantSettings("FedEx", merchantSettings);
        }

        public Task<bool> SetMerchantSettings(string carrier, MerchantSettings merchantSettings)
        {
            this._inMemorySettings.Remove("settings");
            this._inMemorySettings.Add("settings", merchantSettings);

            return Task.FromResult(true);
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
