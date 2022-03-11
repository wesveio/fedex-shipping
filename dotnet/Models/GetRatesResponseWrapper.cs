using FedExRateServiceReference;
using System;
using System.Collections.Generic;
using System.Text;

namespace FedexShipping.Models
{
    public class GetRatesResponseWrapper
    {
        public List<GetRatesResponse> GetRatesResponses { get; set; }
        public string HighestSeverity { get; set; }
        public List<Notification>  Notifications { get; set; }
        public string Error { get; set; }
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
