using FedExRateServiceReference;
using System;
using System.Collections.Generic;

namespace FedexShipping.Models
{
    public class GetRatesResponseWrapper
    {
        public List<GetRatesResponse> GetRatesResponses { get; set; }
        public List<string> HighestSeverity { get; set; } = new List<string>();
        public List<Notification>  Notifications { get; set; }
        public List<string> Error { get; set; } = new List<string>();
        public bool Success { get; set; }
        public TimeSpan timeSpan { get; set; }

        public GetRatesResponseWrapper()
        {
            this.GetRatesResponses = new List<GetRatesResponse>();
            this.Notifications = new List<Notification>();
            this.Success = false;
        }
    }
}
