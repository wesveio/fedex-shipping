using FedExRateServiceReference;
using ShippingUtilities.Models;
using System.Threading.Tasks;

namespace ShippingUtilities.Services
{
    public interface IFedExRateRequest
    {
        Task<GetRatesResponseWrapper> GetRates(GetRatesRequest getRatesRequest);
        Task<RateReplyWrapper> GetRawRates(RateRequest rateRequest);
    }
}