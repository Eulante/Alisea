using AliseaTorrent.Bencode;
using AliseaTorrent.Peering;
using AliseaTorrent.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace AliseaTorrent.Tracking.Data
{
    public class AnnounceResponse
    {
        // Readeable Error message
        public string FailureReason { get; set; } = null;

        // (optional) Similar to the error message
        public string WarninMessage { get; set; } = null;

        // Interval in seconds between regular request
        public int Interval { get; set; }

        // (optional) Minimun announce interval
        public int MinInterval { get; set; }

        // String the client should send back in the next announcements
        public string TrackerId { get; set; } = null;

        // Number of seeder
        public int Complete { get; set; }

        // Number of leechers
        public int Incomplete { get; set; }

        // list of peers send back by the tracker
        public List<Peer> Peers { get; private set; } = null;




        public static AnnounceResponse BuildFromHttpTrackerResponse(Byte[] response)
        {
            AnnounceResponse result = null;
            try
            {
                //Byte[] message = Encoding.UTF8.GetBytes(response);  // TODO problema  forse codifica in UTF8
                BencodeReader reader = new BencodeReader(response);
                BencodeItem bitem = reader.ReadNextItem();

                if (bitem != null)
                {
                    if (bitem is BencodeDictionary)
                    {

                        result = new AnnounceResponse();

                        BencodeDictionary dict = (BencodeDictionary)bitem;

                        foreach (BencodeElement element in dict.Elements)
                        {
                            switch (element.Key)
                            {
                                case "failure reason":
                                    BencodeByteString f = (BencodeByteString)element.Value;
                                    result.FailureReason = f.ToString();
                                    break;
                                case "warning message":
                                    BencodeByteString w = (BencodeByteString)element.Value;
                                    result.WarninMessage = w.ToString();
                                    break;
                                case "interval":
                                    BencodeLong i = (BencodeLong)element.Value;
                                    result.Interval = (int)i.LongValue;
                                    break;
                                case "min interval":
                                    BencodeLong mini = (BencodeLong)element.Value;
                                    result.MinInterval = (int)mini.LongValue;
                                    break;
                                case "tracker id":
                                    BencodeByteString tid = (BencodeByteString)element.Value;
                                    result.TrackerId = tid.ToString();
                                    break;
                                case "complete":
                                    BencodeLong complete = (BencodeLong)element.Value;
                                    result.Complete = (int)complete.LongValue;
                                    break;
                                case "incomplete":
                                    BencodeLong incomplete = (BencodeLong)element.Value;
                                    result.Incomplete = (int)incomplete.LongValue;
                                    break;
                                case "peers":
                                    BencodeItem item = element.Value;
                                    if (item is BencodeList)
                                        AddPeerInAnnounceResponse(result, (BencodeList)item);
                                    else if (item is BencodeByteString)
                                        AddPeerInAnnounceResponse(result, (BencodeByteString)item);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.Write(exception);
            }
            return result;
        }


        public static AnnounceResponse BuildFromUdpTrackerResponse(IDataReader reader)
        {
            #region struttura risposta
            /*
             * Offset      Size            Name            Value
                0           32-bit integer  action          1 // announce
                4           32-bit integer  transaction_id
                8           32-bit integer  interval
                12          32-bit integer  leechers
                16          32-bit integer  seeders
                20 + 6 * n  32-bit integer  IP address
                24 + 6 * n  16-bit integer  TCP port
                20 + 6 * N 
             */
            #endregion

            AnnounceResponse result = null;

            if (reader.UnconsumedBufferLength >= 16)
            {
                //int action = reader.ReadInt32();
                int transactionId = reader.ReadInt32();
                int interval = reader.ReadInt32();
                int leechers = reader.ReadInt32();
                int seeders = reader.ReadInt32();

                List<Peer> peers = new List<Peer>();

                while(reader.UnconsumedBufferLength  >=  6)   //there's another ip+port
                {
                    string strip = reader.ReadByte() + "." + reader.ReadByte() + "." + reader.ReadByte() + "." + reader.ReadByte();
                    int intport = reader.ReadInt16();

                    peers.Add(new Peer(strip, intport));
                }

                result = new AnnounceResponse();
                result.Interval = interval;
                result.Incomplete = leechers;
                result.Complete = seeders;
                result.Peers = peers;

            }

            return result;
        }





        private static void AddPeerInAnnounceResponse(AnnounceResponse response, BencodeList listofdict)
        {
            response.Peers = new List<Peer>();

            foreach (BencodeItem item in listofdict.Items)
            {
                string id = "NoPeerId";
                string address = "";
                int port = 0;

                BencodeDictionary dict = (BencodeDictionary)item;
                foreach (BencodeElement element in dict.Elements)
                {
                    switch (element.Key)
                    {
                        case "peer id":
                            id = ((BencodeByteString)element.Value).ToString();
                            break;
                        case "ip":
                            address = ((BencodeByteString)element.Value).ToString();
                            break;
                        case "port":
                            port = (int)((BencodeLong)element.Value).LongValue;
                            break;
                    }

                    response.Peers.Add(new Peer(id, address, port));
                }
            }
        }

        private static void AddPeerInAnnounceResponse(AnnounceResponse response, BencodeByteString str)
        {
            response.Peers = new List<Peer>();

            Byte[] peersData = str.ByteStringValue;
            int idx = 0;

            // Look for some more data
            while (idx <= peersData.Length - 6)
            {
                // Data in Big Endian Notation
                string ip = peersData[idx] + "." + peersData[idx + 1] + "." + peersData[idx + 2] + "." + peersData[idx + 3];
                idx += 4;

                int port = peersData[idx] * 256 + peersData[idx + 1];
                idx += 2;

                response.Peers.Add(new Peer(ip, port));
            }
        }


    }
}
