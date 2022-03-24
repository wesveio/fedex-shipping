using System;

namespace FedexShipping.Models
{
    public class GetEstimateDeliveryResponse
    {
        public string carrierId { get; set; } = "FEDEX";

        public string shippingMethod { get; set; }

        public DateTimeOffset estimateDateUtc { get; set; }
        public GetEstimateDeliveryResponse(string carrierId, string shippingMethod, DateTimeOffset estimateDateUtc) {
            this.carrierId = carrierId;
            this.shippingMethod = shippingMethod;
            this.estimateDateUtc = estimateDateUtc;
        }
    }
}



