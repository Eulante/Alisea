using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AliseaTorrent.Tracking.Data;
using Windows.Networking.Sockets;
using Windows.Foundation;
using Windows.Networking;
using Windows.Storage.Streams;
using AliseaTorrent.Tracking.Data.Udp;
using System.Runtime.InteropServices.WindowsRuntime;
using AliseaTorrent.Peering;
using System.Diagnostics;
using AliseaTorrent.Util;
using System.IO;

namespace AliseaTorrent.Tracking
{
    class UdpTracker : AbstractTracker
    {

        private string TrackerName = null;
        private string TrackerPort = null;

        private DatagramSocket socket = null;

        // identify the coomunication with the tracker. give from tracker it self
        // initially value set to 0x41727101980
        private Int64 connectionId;
        
        // enumerate the communication's messages
        private Int32 transactionId;

        private AnnounceResponse announceResponse = null;
        private bool responseOK = false;


        public UdpTracker(string announceUrl) : base(announceUrl)
        {
            try
            {
                string sbName = announceUrl.Substring(6);

                int startPortIndex = sbName.IndexOf(':') + 1;
                int endPortIndex = sbName.IndexOf('/') - 1;
                TrackerPort = sbName.Substring(startPortIndex, endPortIndex - startPortIndex + 1);

                string sbn1 = sbName.Substring(0, startPortIndex - 1);
                string sbn2 = sbName.Substring(endPortIndex + 1);
                TrackerName = sbn1;

                this.socket = new DatagramSocket();

                socket.MessageReceived += AnnounceResponseHandler;

                this.connectionId = 0x41727101980;
                this.transactionId = new Random().Next(0, 1000000);
            }
            catch (Exception e) { }

        }


         ~UdpTracker()
        {
            socket.Dispose();
        }




        public override async Task<AnnounceResponse> RequestAnnounceAsync(AnnounceRequest Request)
        {
            if (socket == null)
                return null;

            announceResponse = null;

            // need to connect to the tracker
            if (this.connectionId == 0x41727101980)
            {
                try
                {
                    HostName host = new HostName(TrackerName);

                    await socket.ConnectAsync(host, TrackerPort);

                    DataWriter writer = new DataWriter(socket.OutputStream);
                    WriteConnectMessage(writer);
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                    writer.DetachStream();

                    while (!responseOK) await Task.Delay(TimeSpan.FromMilliseconds(100));
                    responseOK = false;

                    writer = new DataWriter(socket.OutputStream);

                    WriteAnnounceMessage(writer, Request);
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                    writer.DetachStream();

                    while (!responseOK) await Task.Delay(TimeSpan.FromMilliseconds(100));
                    responseOK = false;

                }
                catch (Exception e)
                {
                    Debug.Write("Announce Request Error: " + e);
                }

               

            }
            // already connected to the tracker
            else
            {
                DataWriter writer = new DataWriter(socket.OutputStream);
                WriteAnnounceMessage(writer, Request);
                await writer.StoreAsync();
                await writer.FlushAsync();

                writer.DetachStream();

                while (!responseOK) await Task.Delay(TimeSpan.FromMilliseconds(100));
                responseOK = false;
            }

            return announceResponse;
        }




        public override void RequestScrape(ScrapeRequest Request)
        {
            throw new NotImplementedException();
        }


        

        private void AnnounceResponseHandler(DatagramSocket socket, DatagramSocketMessageReceivedEventArgs args)
        {
            try
            {
                IDataReader reader = args.GetDataReader();
                reader.ByteOrder = ByteOrder.BigEndian;

                int action = reader.ReadInt32();

                if (action == 0) //connect response
                {
                    HandleConnectReponse(reader);
                    responseOK = true;
                }
                else if (action == 1) //announce response
                {
                    announceResponse = AnnounceResponse.BuildFromUdpTrackerResponse(reader);
                    responseOK = true;
                }                                
            }
            catch(Exception e)
            {
                Debug.Write("Udp Tracker Receive Error\n" + e);
            }

        }


        private void WriteConnectMessage(DataWriter writer)
        {
            #region connect message info
            /*
            *   Offset Size            Name Value
            *   0       64 - bit integer connection_id   0x41727101980
            *   8       32 - bit integer action          0 // connect
            *   12      32 - bit integer transaction_id
            *   16
            */
            #endregion

            this.transactionId++;
            
            writer.WriteInt64(connectionId);
            writer.WriteInt32(0);
            writer.WriteInt32(transactionId);
        }


        private bool HandleConnectReponse(IDataReader reader)
        {
            #region connect response info
            /*
            *   Offset  Size            Name            Value
            *   0       32-bit integer  action          0 // connect
            *   4       32-bit integer  transaction_id
            *   8       64-bit integer  connection_id
            *   16
            */
            #endregion

            bool success = false;     

            if (reader.UnconsumedBufferLength >= 12)
            {
                this.transactionId = reader.ReadInt32();
                this.connectionId = reader.ReadInt64();
                success = true;
            }

            return success;
        }



        private void WriteAnnounceMessage(DataWriter writer, AnnounceRequest Request)
        {
            #region announce message info
            /*
            *   Offset  Size    Name    Value
            *   0       64-bit integer  connection_id
            *   8       32-bit integer  action          1 // announce
            *   12      32-bit integer  transaction_id
            *   16      20-byte string  info_hash
            *   36      20-byte string  peer_id
            *   56      64-bit integer  downloaded
            *   64      64-bit integer  left
            *   72      64-bit integer  uploaded
            *   80      32-bit integer  event           0 // 0: none; 1: completed; 2: started; 3: stopped
            *   84      32-bit integer  IP address      0 // default
            *   88      32-bit integer  key
            *   92      32-bit integer  num_want        -1 // default
            *   96      16-bit integer  port
            *   98
            */
            #endregion

            this.transactionId++;
            writer.WriteInt64(connectionId);
            writer.WriteInt32(1);
            writer.WriteInt32(transactionId);
            writer.WriteBytes(Request.InfoHash);
            writer.WriteBytes(Request.PeerId);
            writer.WriteInt64(Request.Downloaded);
            writer.WriteInt64(Request.Left);
            writer.WriteInt64(Request.Uploaded);
            writer.WriteInt32(EventValue(Request.Event));
            writer.WriteInt32(0);
            writer.WriteInt32(new Random().Next());
            writer.WriteInt32(100);
            writer.WriteInt16((Int16)Request.Port);
        }





        private int EventValue(string evn)
        {
            switch (evn)
            {
                case "none":
                    return 0;
                case "completed":
                    return 1;
                case "started":
                    return 2;
                case "stopped":
                    return 3;
            }
            return 0;
        }

        private string EventValue(int evn)
        {
            switch (evn)
            {
                case 0:
                    return "none";
                case 1:
                    return "completed";
                case 2:
                    return "started";
                case 3:
                    return "stopped";
            }
            return "none";
        }

    }
}
