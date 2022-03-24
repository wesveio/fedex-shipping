namespace FedexShipping.Services
{
    using System;
    using FedexShipping.Models;
    public class FedExEstimateDeliveryRequest : IFedExEstimateDeliveryRequest
    {

        public GetEstimateDeliveryResponse getEstimateDelivery(GetEstimateDeliveryRequest request) {

            DateTimeOffset newEstimate = DateTimeOffset.UtcNow + request.previousEstimation.transitTime;
            GetEstimateDeliveryResponse getEstimateDeliveryResponse = new GetEstimateDeliveryResponse(request.carrierId, request.shippingMethod, newEstimate);

            return getEstimateDeliveryResponse;
        }
    }
}