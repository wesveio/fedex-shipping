using System.Threading.Tasks;
using FedexShipping.Models;

namespace FedexShipping.Data
{
    public interface IPackingRepository
    {
        Task<PackingResponseWrapper> PackItems(PackingRequest packingRequest, string accessKey);
    }
}