using System.Threading.Tasks;
using FedexShipping.Models;

namespace FedexShipping.Data
{
    public interface ILogisticsRepository
    {
        Task<LogisticsDocksListWrapper> GetDocks();
        bool SetDocks();
    }
}