using FedexShipping.Models;

namespace FedexShipping.Services
{
    public interface IFedExEstimateDeliveryRequest
    {
        GetEstimateDeliveryResponse getEstimateDelivery(GetEstimateDeliveryRequest request);
    }
}