using System;
using System.Collections.Generic;
using System.Text;

namespace FedexShipping.Models
{
    public class MerchantSettings
    {
        public string UserCredentialKey { get; set; }
        public string UserCredentialPassword { get; set; }
        public string ParentCredentialKey { get; set; }
        public string ParentCredentialPassword { get; set; }
        public string ClientDetailAccountNumber { get; set; }
        public string ClientDetailMeterNumber { get; set; }
        public bool IsLive { get; set; }
    }
}
