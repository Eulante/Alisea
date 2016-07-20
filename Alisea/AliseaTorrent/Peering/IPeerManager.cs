using AliseaTorrent.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Peering
{

    public interface IPeerManager
    {        
        void AddPeers(List<Peer> peers);

        void StartPeerCommunication();

        void StopPeerCommunication();

        void RequestData(Int32 pieceId);

        void Reset();

        void SetResultListener(IPeeringResultListener resultListener);

        void PieceCompleted(int pieceId);
    }
}
