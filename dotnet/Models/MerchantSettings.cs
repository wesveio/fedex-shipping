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
        public bool Residential { get; set; }
        public int OptimizeShippingType { get; set; }
        public string UnitWeight { get; set; }
        public string UnitDimension { get; set; }
        public string PackingAccessKey { get; set; } = "";
        public List<ModalMap> ItemModals { get; set; } = new List<ModalMap>();
        public List<SlaSettings> SlaSettings { get; set; } = new List<SlaSettings>();
        public string DefaultDeliveryEstimateInDays { get; set; }
    }

    public class ModalMap {
        public string Modal { get; set; }
        public string FedexHandling { get; set; }
        public bool ShipAlone { get; set; }
        public ModalMap() {}
        public ModalMap(string Modal, string FedexHandling, Boolean ShipAlone) {
            this.Modal = Modal;
            this.FedexHandling = FedexHandling;
            this.ShipAlone = ShipAlone;
        }
    }

    public class SlaSettings {
        public string Sla { get; set; }
        public bool Hidden { get; set; }
        public double SurchargePercent { get; set; }
        public double SurchargeFlatRate { get; set; }
        public SlaSettings() {}
        public SlaSettings(string Sla, bool Hidden, double SurchargePercent, double SurchargeFlatRate) {
            this.Sla = Sla;
            this.Hidden = Hidden;
            this.SurchargePercent = SurchargePercent;
            this.SurchargeFlatRate = SurchargeFlatRate;
        }
    }

}
