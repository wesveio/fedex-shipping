using System.Threading.Tasks;
using TrackServiceReference;

namespace ShippingUtilities.Services
{
    public interface IFedExTrackRequest
    {
        Task<TrackReply> Track(string trackingNumber);
    }
}