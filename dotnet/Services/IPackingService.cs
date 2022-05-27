using FedexShipping.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FedexShipping.Services
{
    public interface IPackingService
    {
        Task<List<Item>> packingMap(List<Item> items);
    }
}