using System.Collections.Generic;

namespace FedexShipping.Models
{
    public class LogisticsDockWrapper
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> ShippingRatesProviders { get; set; } = new List<string>();

        public LogisticsDockWrapper(string Id, string Name, List<string> ShippingRatesProviders) {
            this.Id = Id;
            this.Name = Name;
            this.ShippingRatesProviders = ShippingRatesProviders;
        }
    }

    public class LogisticsDocksListWrapper
    {
        public List<LogisticsDockWrapper> DocksList { get; set; } = new List<LogisticsDockWrapper>();

        public LogisticsDocksListWrapper() {}
    }
}
