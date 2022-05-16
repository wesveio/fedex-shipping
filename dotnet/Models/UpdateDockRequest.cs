namespace FedexShipping.Models
{
    public class UpdateDockRequest
    {
        public string DockId { get; set; }
        public bool ToRemove { get; set; }

        public UpdateDockRequest() {}
    }
}
