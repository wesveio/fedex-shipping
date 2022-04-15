using System;
using System.Collections.Generic;

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
        public bool OptimizeShipping { get; set; }
        public string UnitWeight { get; set; }
        public string UnitDimension { get; set; }
        public List<String> HiddenSLA { get; set; } = new List<String>();
        public List<ModalMap> ItemModals { get; set; } = new List<ModalMap>();
    }

    public class ModalMap {
        public string Modal { get; set; }
        public string FedexHandling { get; set; }
        public ModalMap() {}
        public ModalMap(string Modal, string FedexHandling) {
            this.Modal = Modal;
            this.FedexHandling = FedexHandling;
        }
    }

}
