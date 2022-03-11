using System.Threading.Tasks;
using TrackServiceReference;

namespace FedexShipping.Services
{
    public interface IFedExTrackRequest
    {
        Task<TrackReply> Track(string trackingNumber);
    }
}