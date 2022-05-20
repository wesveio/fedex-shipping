using FedExRateServiceReference;
using System;

namespace FedexShipping.Models
{
    public class RateReplyWrapper
    {
        public RateReply rateReply { get; set; }

        public TimeSpan timeSpan { get; set; }

        public string message { get; set; }
    }
}
