using AliseaTorrent.Peering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Tracking
{
    public interface ITrackingResultListener
    {
        void OnTrackingResult(List<Peer> peerList);
    }
}
