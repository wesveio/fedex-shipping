namespace ShippingUtilities.Data
{
    using ShippingUtilities.Models;
    using System.Threading.Tasks;

    public interface IMerchantSettingsRepository
    {
        Task SetMerchantSettings(string carrier, MerchantSettings merchantSettings);

        Task<MerchantSettings> GetMerchantSettings(string carrier);
    }
}
