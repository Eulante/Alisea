using System;
using static AliseaTorrent.Standard.ProtocolNames.Tracking;

namespace AliseaTorrent.Tracking.Data
{
    public class AnnounceRequest
    {
        // 20 byte (need urlencoding to be used) hash of the info value in the metadata file
        public Byte[] InfoHash { get; set; } = null; 

        // 20 byte (need urlencoding to be used) unique id of this client
        public Byte[] PeerId { get; set; } = null; 

        // port number that this client is listening on
        public int Port { get; set; } 

        // total amount of byte uploaded
        public long Uploaded { get; set; } 

        // total  amount of byte downloaded
        public long Downloaded { get; set; }

        // number of byte this client still has to download
        public long Left { get; set; }

        // indicate if this client accept compact response
        public bool Compact { get; set; } = false; 

        // indicate if tracker can omit peer id in the reesponse
        public bool NoPeerId { get; set; } = true; 

        // status of the communication (client side)
        private string rEvent = Request.EventStarted;

        public string Event {
            get { return rEvent; }
            set {
                switch (value)
                {
                    case Request.EventStarted: // lo setto quando inizio
                    case Request.EventStopped: // lo setto quando faccio stoptracking
                    case Request.EventCompleted: // lo mando solo una volta quando completo il film.
                    case Request.EventEmpty: // 
                        rEvent = value;
                        break;
                    default:
                        rEvent = Request.EventEmpty;
                        break;
                }
            }
        }

        // (optional) client ip address (dotted quad format)
        public string Ip { get; set; } = null;

        // (optional) number of peer this client would like to receive
        public int Numwant { get; set; } = -1;

        // (optional) identification not sahred with other peer
        public string Key { get; set; } = null;

        // (optional) tracker Id from previus announce
        public string TrackerId { get; set; } = null;

    }
}
