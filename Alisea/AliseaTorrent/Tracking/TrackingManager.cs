using AliseaTorrent.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AliseaTorrent.Metadata;
using AliseaTorrent.Util;
using AliseaTorrent.Tracking.Data;
using AliseaTorrent.Core;
using static AliseaTorrent.Standard.ProtocolNames.Tracking;
using AliseaTorrent.Peering;
using System.Diagnostics;


namespace AliseaTorrent
{
    /// <summary>
    /// NAME: TrackingManager
    /// DESCRIPTION: the main purpose of this class is to retrieve the trackers list from TrackerFactory class,
    /// being able to interact with them in order to retrieve the list of peers. Each peers list is stored inside
    //  peersList, which can be retrieved with the address of the tracker.In addition to this, every request to
    /// the trackers will also return a timer which determines about how much time TrackingManager should wait before
    /// retrieving the peers list again.
    /// </summary>
    /// 
    class TrackingManager : ITrackingManager
    {
        
        /// <summary>
        /// Object which contains the information about the torrent choosen.
        /// </summary>
        private TorrentMetaData torrentMetaData;    

        private ITrackingResultListener trackingListener;

        /// <summary>
        /// The list of trackers.
        /// </summary>
        private List<AbstractTracker> trackingList;

        /// <summary>
        ///  Object containing the information about the peer torrent communication.
        /// </summary>
        private AnnounceRequest announceRequest;

        /// <summary>
        /// A Dictionary which contains the tracker link followed by the list of peers associated.
        /// </summary>
        private Dictionary<string, Task<List<Peer>>> peersList;


        private bool continueTrackingRoutine;


        public TrackingManager(TorrentMetaData metaData, int port, byte[] peerId)
        {
            // Setting up the torrent metadata.
            this.torrentMetaData = metaData;

            // Creating singleton object containing the informations about 
            TransferCounterSingleton counter = TransferCounterSingleton.Instance();

            // Setting up the announce request.
            announceRequest = new AnnounceRequest
            {
                InfoHash = SHA1.GenerateHash(torrentMetaData.InfoBytes),
                Port = port,
                PeerId = peerId,
                Downloaded = counter.downloaded,
                Uploaded = counter.uploaded,
                Left = counter.left,
                Event = Request.EventEmpty
            };

            // Setting up the tracking list.
            ITrackerFactory trackerFactory = new TrackerFactory();
            trackingList = trackerFactory.createTrackersList(metaData.AnnounceList);

        }
         
       
                   
        public void Reset()
        {
            this.trackingListener = null;

            foreach (AbstractTracker a in this.trackingList)
                StopTrackingRoutine(a);
        }

        
        public void SetResultListener(ITrackingResultListener resultListener)
        {
           this.trackingListener = resultListener;
        }

        
        public void SetTrackers(List<AbstractTracker> trackers)
        {
            return;
        }

        
        public void StartTrackingRoutine()
        {
            continueTrackingRoutine = true;

            // Since there will be a communication, the event type is set to start
            // in order to respect the BitTorrent communication standard.
            this.announceRequest.Event = Request.EventEmpty;
            foreach(AbstractTracker a in this.trackingList)
            {
                try
                {
                    Task ad = Task.Run(()=>TrackerAnnounceRoutine(a));
                }
                catch (Exception)
                {
                    Debug.Write("An error occured during request.");
                }
               
            }
        
        }

        
        public void StopTrackingRoutine(AbstractTracker a)
        {
            continueTrackingRoutine = false;

            this.announceRequest.Event = Request.EventStopped;
            try
            {
                a.RequestAnnounceAsync(this.announceRequest);
            }
            catch (Exception)
            {
                Debug.Write("An error occured during request.");
            }
        }


        private async void TrackerAnnounceRoutine(AbstractTracker a)
        {
            try
            {

                AnnounceResponse ar = await a.RequestAnnounceAsync(this.announceRequest);
                trackingListener.OnTrackingResult(ar.Peers);

                while(continueTrackingRoutine)
                {
                    await Task.Delay(TimeSpan.FromSeconds(ar.MinInterval));


                        if(continueTrackingRoutine)
                        {
                            ar = await a.RequestAnnounceAsync(this.announceRequest);
                            trackingListener.OnTrackingResult(ar.Peers);
                        }             
                    }
                }
            catch (Exception)
            {
                Debug.Write("An error occured during request.\n");
            }
        }

        /// <summary>
        /// Unusued because the torrentMetaData is passed into the constructor of the class.
        /// </summary>
        /// <param name="metadata"></param>
        public void InitializeTrackingManager(TorrentMetaData metadata)
        {
            throw new NotImplementedException();
        }


    }
}

