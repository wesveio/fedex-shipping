using FedExRateServiceReference;
using FedexShipping.Models;
using System.Threading.Tasks;

namespace FedexShipping.Services
{
    public interface IFedExRateRequest
    {
        Task<GetRatesResponseWrapper> GetRates(GetRatesRequest getRatesRequest);
        Task<RateReplyWrapper> GetRawRates(RateRequest rateRequest);
    }
}