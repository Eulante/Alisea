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

        // Used to manage the socket's inputstream listening
        bool continueListeningFromSocketInputStream = true;

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
                //bool somethingHappen = false;

                //DebugPrinter.Print("non bloccato\n");

                //AGGIUNTA RICHIESTE IN CODA
                bool op = AddPieceRequests();
                //somethingHappen = somethingHappen || op;

                //RICHIESTE PACCHETTI
                op = SendBlockRequests();
                //somethingHappen = somethingHappen || op;


                //RISPOSTE PACCHETTI
                /*if (incomingRequests.Count() > 0)
                {
                    BlockRequest request = incomingRequests[0];
                    DataTransferUnit dtu = resultListener.GiveMeData(request.pieceId, request.offset, request.size);
                    request.peerMessenger.PieceMessage(dtu);

                    somethingHappen = true;
                }
                */

                //if (!somethingHappen)
                await Task.Delay(TimeSpan.FromMilliseconds(5));
                
            }
        }




        private bool AddPieceRequests()
        {
            bool somethingHappen = false;
            bool ownerFound = false;
            //int steps = 0;
            bool moveNextRequest = true;

            //while (addedBlockRequst numberOfRequest < MAX_SLIDING_WINDOW_SIZE)
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
                        /*
                        
                        messengersMutex.WaitOne();

                        //prendiamo la lista dei peer che hanno il piece e non ci strozzano
                        var owners = peerStreamSockets.Where(ownsock => (!ownsock.Value.IsChocking() && ownsock.Value.OwnPiece(idx))).ToList();

                        messengersMutex.ReleaseMutex();

                        // se nessuno ha quel pezzo usciamo e proviamo a cercarne un altro
                        if (owners.Count() == 0)
                            break;

                        */

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
                        //List<BlockRequest> blockRequests = new List<BlockRequest>();
                        blockRequestMutex.WaitOne();

                        int added = 0;
                        int total = pieceSize;
                        int rid = 0;
                        while (added < pieceSize)
                        {                       
                            int addsize = MAX_BLOCK_SIZE;
                            if (pieceSize < added + addsize)
                                addsize = pieceSize - added;

                            //blockRequests.Add(new BlockRequest() { pieceId = idx, offset = added, size = addsize, responseWaiting = false, peerMessenger = owners[rid].Value});
                            BlockRequest block = new BlockRequest() { pieceId = idx, offset = added, size = addsize, responseWaiting = false }; /*, peerMessenger = owners[rid].Value */
                            if(!currentRequestedBlocks.ContainsKey(block.GetId()))
                                currentRequestedBlocks.Add(block.GetId(), block);

                            added += addsize;
                            ++addedBlockRequst;
                            ++rid;
                        }

                        blockRequestMutex.ReleaseMutex();
                        // mandiamo le richieste ai peer
                        /*int rid = 0;
                        foreach(BlockRequest br in blockRequests)
                        {
                            try
                            {
                                owners[rid % owners.Count].Value.RequestMessage(br.pieceId, br.offset, br.size);
                            }
                            catch(Exception e)
                            {
                                DebugPrinter.Print("ERRORE DURANTE RICHIESTA");
                            }
                            
                            ++rid;
                        }*/


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


                #region OLD
                /*foreach (IPeerMessenger peer in piecesOwners[idx])
                {
                    if (peer != null)
                    {
                        if (!peer.IsChocking())
                        {
                            pieceStatus[idx] = (int)PieceState.Requested;

                            int added = 0;
                            int total = pieceSize;

                            while (added < pieceSize)
                            {
                                int addsize = MAX_BLOCK_SIZE;
                                if (pieceSize < added + addsize)
                                    addsize = pieceSize - added;

                                BlockRequest block = new BlockRequest() { pieceId = idx, offset = added, size = addsize, responseWaiting = false };
                                currentRequestedBlocks.Add(block.GetId(), block);
                                added += addsize;
                            }

                            ++numberOfRequest;
                            ++requestCounter;

                            ownerFound = true;
                            somethingHappen = true;

                            break;
                        }
                    }

                }

                moveNextRequest = moveNextRequest && ownerFound;

                if (moveNextRequest)
                    ++nextRequest;

                ++steps;
                if (steps == pieceStatus.Count())
                    break;*/

                #endregion
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

            #region OLD
            /*            bool somethingHappen = false;

                        List<BlockRequest> reqs = currentRequestedBlocks.Values.ToList();

                        foreach (BlockRequest req in reqs)
                        {
                            if (pendingBlockRequest >= MAX_PENDING_BLOCK_REQUEST)
                                break;



                            if (!req.responseWaiting)
                            {
                                int idx = req.pieceId;

                                List<IPeerMessenger> owners = piecesOwners[idx];
                                List<IPeerMessenger> winners = new List<IPeerMessenger>();

                                foreach (IPeerMessenger peer in owners)
                                    if (!peer.IsChocking())
                                        winners.Add(peer);

                                if (winners.Count() > 0)
                                {
                                    int index = requestCounter % winners.Count();
                                    IPeerMessenger peer = winners[index];

                                    peer.RequestMessage(idx, req.offset, req.size);

                                    ++pendingBlockRequest;
                                    ++requestCounter;

                                    somethingHappen = true;

                                    BlockRequest blr = req;
                                    blr.responseWaiting = true;
                                    currentRequestedBlocks.Remove(req.GetId());
                                    currentRequestedBlocks.Add(req.GetId(), blr);

                                }

                            }

                        }

                        return somethingHappen;
            */
            #endregion
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

                    DebugPrinter.Print("ACTIVE CONNECTION: " + numberOfConnection);
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
            // TODO to be completed
        }


        public void PieceCompleted(int pid)
        {
            --numberOfRequest;
            pieceStatus[pid] = (byte)PieceState.Owned;

            /*
            List<BlockRequest> cancelRequests = new List<BlockRequest>();

            int added = 0;
            int total = pieceSize;
            while (added < pieceSize)
            {
                int addsize = MAX_BLOCK_SIZE;
                if (pieceSize < added + addsize)
                    addsize = pieceSize - added;
                
                BlockRequest block = new BlockRequest() { pieceId = pid, offset = added, size = addsize, responseWaiting = false };
                added += addsize;
            }

            messengersMutex.WaitOne();

            foreach (IPeerMessenger peer in peerStreamSockets.Values.ToList())
                foreach (BlockRequest block in cancelRequests)
                    peer.CancelMessage(block.pieceId, block.offset, block.size);

            messengersMutex.ReleaseMutex();
            */
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

                //listeningTask.Add(listenTask);

                DebugPrinter.Print("Connessione RIUSCITA\n");

            }
            catch (Exception)
            {
                //Debug.Write("Connessione fallita\n");
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

                DebugPrinter.Print("BitfieldMessage\n");

                peerManager.ownersMutex.ReleaseMutex();
            }


            public void CancelMessage(PeerMessenger sender, int pieceIndex, int begin, int size)
            {
                DebugPrinter.Print("CancelMessage\n");
                
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
                DebugPrinter.Print("RequestMessage\n");

                peerManager.incomingRequests.Add(new BlockRequest() { peerMessenger = sender, pieceId = pieceIndex, offset = begin, size = size });
            }

            
            public void PieceMessage(PeerMessenger sender, DataTransferUnit dataUnit)
            {
                DebugPrinter.Print("PieceMessage pid:" + dataUnit.pieceId + " start:" + dataUnit.inPieceOffset + " size:" + dataUnit.data.Length + "\n");

                peerManager.resultListener.OnPeeringResult(dataUnit);

                BlockRequest request = new BlockRequest() { pieceId = dataUnit.pieceId, offset = dataUnit.inPieceOffset };
                peerManager.blockRequestMutex.WaitOne();
                peerManager.currentRequestedBlocks.Remove(request.GetId());
                peerManager.blockRequestMutex.ReleaseMutex();
                --peerManager.pendingBlockRequest;

                /*if(peerManager.currentRequestedBlocks.TryGetValue(request.GetId(), out request))
                {
                    if (request.offset == peerManager.pieceSize)
                    {
                        --peerManager.numberOfRequest;
                        peerManager.currentRequestedBlocks.Remove(request.GetId());
                        
                    }                        
                    else
                    {
                        int size = MAX_BLOCK_SIZE;
                        if (size > peerManager.pieceSize - request.offset)
                            size = peerManager.pieceSize - request.offset;

                        //sender.RequestMessage(request.pieceId, request.offset, size);

                        request.offset += size;
                        peerManager.currentRequestedBlocks.Remove(request.GetId());
                        request.responseWaiting = false;
                        peerManager.currentRequestedBlocks.Add(request.GetId(), request);

                    }
                }*/
            }


            public void KeepAliveMessage(PeerMessenger sender) { DebugPrinter.Print("KeepAliveMessage: " + sender.peer.Address + "\n"); }

            public void ChokeMessage(PeerMessenger sender) { DebugPrinter.Print("ChokeMessage: " + sender.peer.Address + "\n"); }

            public void UnchokeMessage(PeerMessenger sender) { DebugPrinter.Print("UnchokeMessage: " + sender.peer.Address + "\n"); }

            public void InterestedMessage(PeerMessenger sender) { DebugPrinter.Print("InterestedMessage: " + sender.peer.Address + "\n"); }

            public void NotInterestedMessage(PeerMessenger sender) { DebugPrinter.Print("NotInterestedMessage: " + sender.peer.Address + "\n"); }

            public void PortMessage() { DebugPrinter.Print("PortMessage\n"); }

        }

    }
}
