namespace FedexShipping.Data
{
    using FedexShipping.Models;
    using System.Threading.Tasks;

    public interface IMerchantSettingsRepository
    {
        Task<bool> SetMerchantSettings(MerchantSettings merchantSettings);

        Task<MerchantSettings> GetMerchantSettings();
    }
}
