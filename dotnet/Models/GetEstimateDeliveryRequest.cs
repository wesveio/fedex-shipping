using System;

namespace FedexShipping.Models
{
    public class GetEstimateDeliveryRequest : GetRatesRequest
    {
        public string carrierId { get; set; }

        public string shippingMethod { get; set; }

        public PreviousEstimation previousEstimation { get; set; }
    }

    public class PreviousEstimation
    {
        public DateTimeOffset estimateDateUtc;
        public DateTimeOffset calculationDateUtc;
        public TimeSpan transitTime;
    }
}



