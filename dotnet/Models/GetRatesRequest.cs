using System;
using System.Collections.Generic;
using System.Text;

namespace ShippingUtilities.Models
{
    public class UnitDimension
    {
        public int weight { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public int length { get; set; }
    }

    public class Availability
    {
        public string warehouseId { get; set; }
        public string dockId { get; set; }
        public int availableQuantity { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public int quantity { get; set; }
        public object modal { get; set; }
        public object groupId { get; set; }
        public double unitPrice { get; set; }
        public UnitDimension unitDimension { get; set; }
        public List<Availability> availability { get; set; }
    }

    public class Location
    {
        public object lat { get; set; }
        public object lon { get; set; }
    }

    public class Origin
    {
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipCode { get; set; }
        public string country { get; set; }
        public Location location { get; set; }
    }

    public class Destination
    {
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipCode { get; set; }
        public string country { get; set; }
        public Location location { get; set; }
    }

    public class GetRatesRequest
    {
        public List<Item> items { get; set; }
        public Origin origin { get; set; }
        public Destination destination { get; set; }
    }
}
