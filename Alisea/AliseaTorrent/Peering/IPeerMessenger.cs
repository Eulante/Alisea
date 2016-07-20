using AliseaTorrent.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Peering
{
    interface IPeerMessenger
    {


        void StartListening();

        bool IsValid();

        bool IsChocking();

        bool OwnPiece(int pieceId);

        // First message sent
        void HandshakeMessage(byte[] torrentSha, byte[] clientId);
        
        // Mantain thwe connection alive
        void KeepAliveMessage();
        
        // Choke the destination peer
        void ChokeMessage();
        
        // Unchoke the destination peer
        void UnchokeMessage();
        
        // Set interest to the destination peer
        void InterestedMessage();
        
        // Remove interest in the destination peer
        void NotInterestedMessage();
        
        // Notify to destination an owned piece id
        void HaveMessage(int pieceIndex);
        
        // Include all the owned pieces
        void BitfieldMessage();
        
        // Request a piece's block 
        void RequestMessage(int pieceIndex, int begin, int size);
        
        // Send a piece's block (after a request)
        void PieceMessage(DataTransferUnit dataUnit);
        
        // Cancel a piece request
        void CancelMessage(int pieceIndex, int begin, int size);
        
        // Boh
        void PortMessage();

    }
}
