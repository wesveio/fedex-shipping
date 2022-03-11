namespace FedexShipping.Data
{
    using FedexShipping.Models;
    using System.Threading.Tasks;

    public interface IMerchantSettingsRepository
    {
        Task SetMerchantSettings(string carrier, MerchantSettings merchantSettings);

        Task<MerchantSettings> GetMerchantSettings(string carrier);
    }
}
