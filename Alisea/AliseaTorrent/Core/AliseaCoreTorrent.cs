using AliseaTorrent.Data;
using AliseaTorrent.Data.Concrete;
using AliseaTorrent.Metadata;
using AliseaTorrent.Peering;
using AliseaTorrent.Tracking;
using AliseaTorrent.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Core
{
    public class AliseaCoreTorrent
    {
        // handle communication with the trackers
        protected ITrackingManager trackingManager = null;

        // handle communication with peers
        protected IPeerManager peerManager = null;
        
        // store multimedia data received from peers
        protected IDataStore dataStore = null;
        


        public AliseaCoreTorrent(Byte[] byteTorrent)
        {
            TorrentMetaData torrent = new TorrentMetaBuilder().GetTorrentMetaData(byteTorrent);

            int peeringPort = AliseaPortGenerator.GeneratePort();
            byte[] clientId = AliseaClienIdGenerator.Generate();

            dataStore = new VolatileDataStore();
            dataStore.InitializeDataStore(torrent);

            trackingManager = new TrackingManager(torrent, peeringPort, clientId);
                                 
            peerManager = new ConcretePeerManager(torrent, peeringPort, clientId);
            
         
            dataStore.SetEventListener(new DataStoreCallbackHandler(this));
            peerManager.SetResultListener(new PeeringResultHandler(this));
            trackingManager.SetResultListener(new TrackingResultHandler(this));
        }


        public AliseaCoreTorrent(TorrentMetaData torrent)
        {
            int peeringPort = AliseaPortGenerator.GeneratePort();
            byte[] clientId = AliseaClienIdGenerator.Generate();

            dataStore = new VolatileDataStore();
            dataStore.InitializeDataStore(torrent);

            trackingManager = new TrackingManager(torrent, peeringPort, clientId);

            peerManager = new ConcretePeerManager(torrent, peeringPort, clientId);


            dataStore.SetEventListener(new DataStoreCallbackHandler(this));
            peerManager.SetResultListener(new PeeringResultHandler(this));
            trackingManager.SetResultListener(new TrackingResultHandler(this));
        }


        ~AliseaCoreTorrent()
        {
            ReleaseResources();
        }

        
        public void StartCarro()
        {
            peerManager.StartPeerCommunication();
            trackingManager.StartTrackingRoutine();
            
        }

        public void StopCarro()
        {
            trackingManager.Reset();
            peerManager.Reset();
            dataStore.Close();

        }


        /**
         *  Used from destructor. Calls release methods of principal class attributes
         */
        protected void ReleaseResources()
        {
            trackingManager.Reset();
            peerManager.Reset();
            dataStore.Close();
        }



        public IDataStore RetrieveIDataStore()
        {
            return dataStore;
        }


        #region RESULTS HANDLERS

        /**
         *  Internal class that handles result callback from the TrackingManager.
         *  Intermediary from TrackingManager and PeerManager (in this way)
         */
        private class TrackingResultHandler : ITrackingResultListener
        {
            AliseaCoreTorrent core;

            public TrackingResultHandler(AliseaCoreTorrent core) { this.core = core; }

            public void OnTrackingResult(List<Peer> peerList)
            {
                core.peerManager.AddPeers(peerList);
            }
        }




        /**
         *  Internal class that handles result callback from the PeeringManager.
         *  Intermediary from PeerManager and DataStore (in this way)
         */
        private class PeeringResultHandler : IPeeringResultListener
        {
            AliseaCoreTorrent core;

            public PeeringResultHandler(AliseaCoreTorrent core) { this.core = core; }

            public DataTransferUnit GiveMeData(int pieceId, int offset, int size)
            {
                // Empty
                return null;
            }

            public void OnPeeringResult(DataTransferUnit data)
            {
                core.dataStore.PutData(data);
            }
        }




        /**
         *  Internal class that handles callbacks from the DataStore.
         *  Intermediary from DataStore and PeerManager (in this way)
         */
        private class DataStoreCallbackHandler : IDataStoreEventListener
        {
            AliseaCoreTorrent core;

            public DataStoreCallbackHandler(AliseaCoreTorrent core) { this.core = core; }


            public void OnPieceError(int pieceNumber)
            {
                DebugPrinter.Print("PEZZO ERRATO: " + pieceNumber + "\n");
                //throw new NotImplementedException();
            }

            public void OnMissingDataRequested(UInt64 dataOffset, UInt32 size)
            {
                //core.peerManager.RequestData(dataOffset, size);
            }

            public void OnMissingDataRequested(ulong pieceNumber)
            {
                core.peerManager.RequestData((int)pieceNumber);
            }

            public void OnPieceCompleted(int pieceNumber)
            {

                DebugPrinter.Print("PEZZO COMPLETO: " + pieceNumber + "\n");
                core.peerManager.PieceCompleted(pieceNumber);
            }
        }

        #endregion


    }
}
