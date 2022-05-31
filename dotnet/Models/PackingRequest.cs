using System.Collections.Generic;

namespace FedexShipping.Models
{
    public class PackingRequest
    {
        public List<RequestItems> ItemsToPack { get; set; } = new List<RequestItems>();
    }

    public class RequestItems
    {
        public int ID { get; set; }
        public int Dim1 { get; set; }
        public int Dim2 { get; set; }
        public int Dim3 { get; set; }
        public int Quantity { get; set; }

        public RequestItems(int ID, int Dim1, int Dim2, int Dim3, int Quantity)
        {
            this.ID = ID;
            this.Dim1 = Dim1;
            this.Dim2 = Dim2;
            this.Dim3 = Dim3;
            this.Quantity = Quantity;
        }
    }
}