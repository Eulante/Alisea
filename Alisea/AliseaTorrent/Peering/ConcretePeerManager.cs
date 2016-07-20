using AliseaTorrent.Data;
using AliseaTorrent.Metadata;
using AliseaTorrent.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace AliseaTorrent.Peering
{
    public class ConcretePeerManager : IPeerManager
    {

        #region VARIABILI

        // Max size for block request
        private const int MAX_BLOCK_SIZE = 8192;

        // Size of the requests' window
        private const int MAX_SLIDING_WINDOW_SIZE = 10;

        // Max pending request
        private const int MAX_PENDING_BLOCK_REQUEST = 100;

        // Max number of active connection
        private const int MAX_ACTIVE_CONNECTION = 50;

        // Time to wait before resend a request (in seconds)
        private const int TIME_TO_RESEND_REQUEST = 15;

        // Set to used the active connection upper bound
        private bool limitActiveConnection = true;

        // Client Id
        byte[] clientId = null;

        // Torrent info
        TorrentMetaData torrentInfo = null;

        // It associates every peer with its own output stream (Tcp)
        Dictionary<string, IPeerMessenger> peerStreamSockets = null;

        // Synchronize update on peerStreamSockets dictionary
        Mutex messengersMutex;
        
        // Defines the pieces' status (Missing, Requested, Owned)
        byte[] pieceStatus = null;
        enum PieceState:byte { Missing, Requested, Owned}

        // Default pieces size
        int pieceSize;

        // Store Peer owners for every piece
        List<IPeerMessenger>[] piecesOwners = null;

        //  Synchronize update on pieceOwners list
        Mutex ownersMutex;

        // Index of the next Piece that will be requested
        int nextRequest = 0;

        // Number of current requests
        int numberOfRequest = 0;

        // Number of added block Request (Not sent)
        int addedBlockRequst = 0;

        // Number of current block request
        int pendingBlockRequest = 0;

        // Number of current active connections
        int numberOfConnection = 0;

        // Total number of sent request
        int requestCounter = 0;

        // It associates every requested piece to its block status
        // TODO create structures
        Dictionary<string, BlockRequest> currentRequestedBlocks = null;

        Mutex blockRequestMutex;


        List<int> missingPieces = null;

        // It stores all the arrived request from other peers 
        List<BlockRequest> incomingRequests = null;

        // Object where result is forwarded 
        IPeeringResultListener resultListener = null;


        StreamSocketListener socketListener = null;

        int socketPort;

        // Listening task (listen from socket input stream
        List<Task> listeningTask = null;

        // To stop data exchange routine
        bool continueExchange;

        #endregion



        #region INITIALIZATION

        public ConcretePeerManager(TorrentMetaData torrentInfo, int socketPort, byte[] clientId)
        {
            this.clientId = clientId;
            this.socketPort = socketPort;
            this.torrentInfo = torrentInfo;

            InitializeData(torrentInfo);
            InitializeSocket();

        }


        void InitializeData(TorrentMetaData torrentInfo)
        {
            int total_pieces_number = torrentInfo.MetaInfo.Sha1.Count();

            pieceSize = torrentInfo.MetaInfo.PieceLength;

            peerStreamSockets = new Dictionary<string, IPeerMessenger>();
            listeningTask = new List<Task>();

            piecesOwners = new List<IPeerMessenger>[total_pieces_number];

            pieceStatus = new byte[total_pieces_number];
            for (int i = 0; i < total_pieces_number; ++i)
            {
                pieceStatus[i] = (int)PieceState.Missing;
                piecesOwners[i] = new List<IPeerMessenger>();
            }

            incomingRequests = new List<BlockRequest>();
            currentRequestedBlocks = new Dictionary<string, BlockRequest>();

            missingPieces = new List<int>(total_pieces_number);
            for (int i = 0; i < total_pieces_number; ++i)
                missingPieces.Add(i);

            messengersMutex = new Mutex();
            ownersMutex = new Mutex();
            blockRequestMutex = new Mutex();


        }


        async void InitializeSocket()
        {
            socketListener = new StreamSocketListener();
            socketListener.ConnectionReceived += SocketListener_ConnectionReceived;

            await socketListener.BindServiceNameAsync(socketPort.ToString());
        }

        #endregion




        ~ConcretePeerManager()
        {
            if (socketListener != null)
                socketListener.Dispose();
        }



        #region LISTENING

        private void SocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {


           /* StreamSocket socket = args.Socket;
            string remoteIp = socket.Information.RemoteAddress.ToString();
            int remotePort = Int32.Parse(socket.Information.RemotePort);

            IPeerMessenger peerMessenger = new PeerMessenger(new Peer(remoteIp, remotePort), socket, new MessengersListener(this));




            messengersMutex.WaitOne();

            if (!peerStreamSockets.ContainsKey(remoteIp))
                peerStreamSockets.Add(remoteIp, peerMessenger);

            messengersMutex.ReleaseMutex();



            peerMessenger.HandshakeMessage(SHA1.GenerateHash(torrentInfo.InfoBytes), clientId);
            peerMessenger.StartListening();

            ++numberOfConnection;*/

            //listeningTask.Add(listenTask);
        }

        #endregion



        #region DATA EXCHANGE

        async void DataExchangeRoutine()
        {

            while(continueExchange)
            {
                //AGGIUNTA RICHIESTE IN CODA
                bool op = AddPieceRequests();

                //RICHIESTE PACCHETTI
                op = SendBlockRequests();


                //RISPOSTE PACCHETTI
                /*if (incomingRequests.Count() > 0)
                {
                    BlockRequest request = incomingRequests[0];
                    DataTransferUnit dtu = resultListener.GiveMeData(request.pieceId, request.offset, request.size);
                    request.peerMessenger.PieceMessage(dtu);

                    somethingHappen = true;
                }
                */
                
                await Task.Delay(TimeSpan.FromMilliseconds(5));
                
            }
        }


        private bool AddPieceRequests()
        {
            bool somethingHappen = false;
            bool ownerFound = false;
            bool moveNextRequest = true;
            

            while (addedBlockRequst < MAX_PENDING_BLOCK_REQUEST)
            {
                bool idFound = false;
                int idx = nextRequest % pieceStatus.Count();

                // Cerchiamo il nuovo pezzo da chiedere
                for(int c = 0; c<pieceStatus.Count(); ++c)
                {
                    // Troviamo un pezzo mancante
                    if (pieceStatus[idx] == (int)PieceState.Missing)
                    {
                        int count = 0;

                        ownersMutex.WaitOne();

                        foreach (IPeerMessenger peer in piecesOwners[idx])
                            if(peer.IsValid() && !peer.IsChocking())
                                ++count;

                        ownersMutex.ReleaseMutex();

                        if (count == 0)
                            break;

                        // abbiamo trovato un owner
                        ownerFound = true;

                        pieceStatus[idx] = (int)PieceState.Requested;

                        // creiamo le richieste diblocchi da effettuare
                        blockRequestMutex.WaitOne();

                        int added = 0;
                        int total = pieceSize;
                        int rid = 0;
                        while (added < pieceSize)
                        {                       
                            int addsize = MAX_BLOCK_SIZE;
                            if (pieceSize < added + addsize)
                                addsize = pieceSize - added;
                            
                            BlockRequest block = new BlockRequest() { pieceId = idx, offset = added, size = addsize, responseWaiting = false };
                            if(!currentRequestedBlocks.ContainsKey(block.GetId()))
                                currentRequestedBlocks.Add(block.GetId(), block);

                            added += addsize;
                            ++addedBlockRequst;
                            ++rid;
                        }

                        blockRequestMutex.ReleaseMutex();

                        ++numberOfRequest;
                        ++requestCounter;
                        somethingHappen = true;

                        // se abbiamo trovato un piece da richiedere non ne richiediamo altri (per questo giro)
                        idFound = true;
                        break;
                    }
                    
                    // se il pezzo non è da richiedere proviamo con il successivo
                    idx = (idx + 1) % pieceStatus.Count();
                }

                // se non lo troviamo usciamo dalla richiesta (IMPORTANTE ALTRIMENTI ENTRIAMO IN LOOP)
                if (!idFound)
                    break;

                // Se il piece richiesto era il primo provato mandiamo avanti l'indice di richiesta
                moveNextRequest = moveNextRequest && ownerFound;

                if (moveNextRequest)
                    nextRequest = (nextRequest + 1) % pieceStatus.Count();

            }

            return somethingHappen;
        }


        private bool SendBlockRequests()
        {
            bool somethingHappen = false;

            List<BlockRequest> reqs = currentRequestedBlocks.Values.ToList();

            foreach (BlockRequest req in reqs)
            {

                bool needRequest = false;

                if (!req.responseWaiting)
                {
                    needRequest = true;

                    if (pendingBlockRequest >= MAX_PENDING_BLOCK_REQUEST)
                        continue;
                }
                else
                {
                    long offset = DateTimeOffset.Now.ToUnixTimeSeconds() - req.requestTime.ToUnixTimeSeconds();
                    if (offset > TIME_TO_RESEND_REQUEST)
                        needRequest = true;
                }

                if(needRequest)
                {
                    int idx = req.pieceId;

                    ownersMutex.WaitOne();

                    List<IPeerMessenger> owners = piecesOwners[idx];
                    List<IPeerMessenger> winners = new List<IPeerMessenger>();

                    foreach (IPeerMessenger peer in owners)
                        if (!peer.IsChocking() && peer.IsValid())
                            winners.Add(peer);

                    ownersMutex.ReleaseMutex();


                    int winnerscount = winners.Count();
                    if (winnerscount >= 0)
                    {
                        int ntry = Math.Min(4, winnerscount);

                        for (int i = 0; i < ntry; ++i)
                        {
                            int index = requestCounter % winners.Count();
                            IPeerMessenger peer = winners[index];

                            peer.RequestMessage(idx, req.offset, req.size);
                            
                            ++requestCounter;
                            somethingHappen = true;

                        }

                        BlockRequest blr = req;
                        blr.responseWaiting = true;
                        blr.requestTime = DateTimeOffset.Now;
                        
                        blockRequestMutex.WaitOne();

                        currentRequestedBlocks.Remove(req.GetId());
                        currentRequestedBlocks.Add(req.GetId(), blr);
                        
                        blockRequestMutex.ReleaseMutex();
                        
                        ++pendingBlockRequest;
                        --addedBlockRequst;
                    }
                }

            }

            return somethingHappen;

        }


        #endregion



        #region KEEP ALIVE ROUTINE

        private async void KeepAliveRoutine()
        {
            while(continueExchange)
            {
                messengersMutex.WaitOne();

                foreach (IPeerMessenger peer in peerStreamSockets.Values)
                    try { peer.KeepAliveMessage(); }
                    catch (Exception) { }

                messengersMutex.ReleaseMutex();

                await Task.Delay(TimeSpan.FromSeconds(60));
            }
        }

        #endregion



        #region CLEAN SOCKET

        async void UpdateSocketListRoutine()
        {
            while(continueExchange)
            {            
                messengersMutex.WaitOne();

                try
                {
                    foreach (var badItem in peerStreamSockets.Where(removeItem => !removeItem.Value.IsValid()).ToList())
                        peerStreamSockets.Remove(badItem.Key);

                    numberOfConnection = peerStreamSockets.Count();
                    
                }
                catch (Exception) { }

                messengersMutex.ReleaseMutex();

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        #endregion




        #region METODI IINTERFACE PEER MANAGER

        public void StartPeerCommunication()
        {
            continueExchange = true;
            Task.Run(() => DataExchangeRoutine());
            Task.Run(() => KeepAliveRoutine());
            Task.Run(() => UpdateSocketListRoutine());
        }


        public void StopPeerCommunication()
        {
            continueExchange = false;
        }


        public void AddPeers(List<Peer> peers)
        {


            foreach (Peer peer in peers)
                if (!limitActiveConnection || numberOfConnection < MAX_ACTIVE_CONNECTION)
                    if (!peerStreamSockets.ContainsKey(peer.Address))
                        ConnectToNewPeer(peer);

        }


        public void RequestData(int pieceId)
        {
            pieceStatus[pieceId] = (int)PieceState.Missing;
            nextRequest = pieceId;
        }


        public void SetResultListener(IPeeringResultListener resultListener)
        {
            this.resultListener = resultListener;
        }


        public void Reset()
        {
            StopPeerCommunication();
        }


        public void PieceCompleted(int pid)
        {
            --numberOfRequest;
            pieceStatus[pid] = (byte)PieceState.Owned;
        }

        #endregion



        #region UTIL

        async Task ConnectToNewPeer(Peer peer)
        {
            StreamSocket socket = new StreamSocket();

            try
            {
                await socket.ConnectAsync(new HostName(peer.Address), peer.Port.ToString());
                ++numberOfConnection;

                IPeerMessenger peerMessenger = new PeerMessenger(peer, socket, new MessengersListener(this));

                //handshake message
                peerMessenger.HandshakeMessage(SHA1.GenerateHash(torrentInfo.InfoBytes), clientId);
                peerMessenger.StartListening();

                // add peermessenger to list
                messengersMutex.WaitOne();
                peerStreamSockets.Add(peer.Address, peerMessenger);
                messengersMutex.ReleaseMutex();


                peerMessenger.InterestedMessage();
                peerMessenger.UnchokeMessage();

                Debug.Write("Connessione RIUSCITA\n");

            }
            catch (Exception)
            {
                Debug.Write("Connessione fallita\n");
            }

        }

        #endregion





        struct BlockRequest
        {
            public IPeerMessenger peerMessenger;
            public int pieceId;
            public int offset;
            public int size;
            public bool responseWaiting;
            public DateTimeOffset requestTime;

            public string GetId()
            {
                return pieceId + "b" + offset;
            }
        }



        private class MessengersListener : IPeerMessengerCallback
        {
            private ConcretePeerManager peerManager = null;

            public MessengersListener(ConcretePeerManager peerManager) { this.peerManager = peerManager; }


            public void BitfieldMessage(PeerMessenger sender, byte[] bitfield)
            {
                peerManager.ownersMutex.WaitOne();

                int pieceNumber = peerManager.pieceStatus.Count();

                if (bitfield.Count() * 8 < pieceNumber)
                    return;
                                
                for (int idx = 0; idx < pieceNumber; ++idx)
                {
                    int cell = idx / 8;
                    int bitcount = idx % 8;

                    int p = 1 << (7 - bitcount);

                    int test = bitfield[cell] & p;

                    if (test != 0)
                        peerManager.piecesOwners[idx].Add(sender);
                }

                peerManager.ownersMutex.ReleaseMutex();
            }


            public void CancelMessage(PeerMessenger sender, int pieceIndex, int begin, int size)
            {

                foreach (BlockRequest r in peerManager.incomingRequests)
                    if (r.peerMessenger.Equals(sender))
                        if (r.pieceId == pieceIndex && r.offset == begin && r.size == size)
                            peerManager.incomingRequests.Remove(r);
            }


            public void HaveMessage(PeerMessenger sender, int pieceIndex)
            {
                peerManager.ownersMutex.WaitOne();

                if (!peerManager.piecesOwners[pieceIndex].Contains(sender))
                    peerManager.piecesOwners[pieceIndex].Add(sender);

                peerManager.ownersMutex.ReleaseMutex();
            }


            public void RequestMessage(PeerMessenger sender, int pieceIndex, int begin, int size)
            {
                peerManager.incomingRequests.Add(new BlockRequest() { peerMessenger = sender, pieceId = pieceIndex, offset = begin, size = size });
            }

            
            public void PieceMessage(PeerMessenger sender, DataTransferUnit dataUnit)
            {
                peerManager.resultListener.OnPeeringResult(dataUnit);

                BlockRequest request = new BlockRequest() { pieceId = dataUnit.pieceId, offset = dataUnit.inPieceOffset };
                peerManager.blockRequestMutex.WaitOne();
                peerManager.currentRequestedBlocks.Remove(request.GetId());
                peerManager.blockRequestMutex.ReleaseMutex();
                --peerManager.pendingBlockRequest;

            }


            public void KeepAliveMessage(PeerMessenger sender) { }

            public void ChokeMessage(PeerMessenger sender) { }

            public void UnchokeMessage(PeerMessenger sender) { }

            public void InterestedMessage(PeerMessenger sender) { }

            public void NotInterestedMessage(PeerMessenger sender) { }

            public void PortMessage() { }

        }

    }
}
