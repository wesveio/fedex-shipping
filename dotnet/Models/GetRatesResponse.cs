using System;
using System.Collections.Generic;
using System.Text;

namespace ShippingUtilities.Models
{
    public class GetRatesResponse
    {
        //public string itemId { get; set; }
        //public int quantity { get; set; }
        public string slaType { get; set; }
        public string carrierName { get; set; }
        public string carrierId { get; set; }
        public decimal price { get; set; }
        public string time { get; set; }
        //public string wareHouseId { get; set; }
        //public string dockId { get; set; }
        //public bool deliveryOnWeekends { get; set; }
    }
}
