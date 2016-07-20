using AliseaTorrent.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Peering
{
    public interface IPeerMessengerCallback
    {
        // Mantain thwe connection alive
        void KeepAliveMessage(PeerMessenger sender);

        // Choke the destination peer
        void ChokeMessage(PeerMessenger sender);

        // Unchoke the destination peer
        void UnchokeMessage(PeerMessenger sender);

        // Set interest to the destination peer
        void InterestedMessage(PeerMessenger sender);

        // Remove interest in the destination peer
        void NotInterestedMessage(PeerMessenger sender);

        // Notify to destination an owned piece id
        void HaveMessage(PeerMessenger sender, int pieceIndex);

        // Include all the owned pieces
        void BitfieldMessage(PeerMessenger sender, byte[] bitfield);

        // Request a piece's block 
        void RequestMessage(PeerMessenger sender, int pieceIndex, int begin, int size);

        // Send a piece's block (after a request)
        void PieceMessage(PeerMessenger sender, DataTransferUnit dataUnit);

        // Cancel a piece request
        void CancelMessage(PeerMessenger sender, int pieceIndex, int begin, int size);

        // Boh
        void PortMessage();
    }
}
