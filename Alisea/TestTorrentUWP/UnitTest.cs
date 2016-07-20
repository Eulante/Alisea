using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using AliseaTorrent;
using System.Diagnostics;
using AliseaTorrent.Metadata;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.Security.Cryptography.Core;
using AliseaTorrent.Util;
using AliseaTorrent.Tracking;
using AliseaTorrent.Tracking.Data;
using AliseaTorrent.Data;
using AliseaTorrent.Data.Concrete;
using Windows.Networking.Sockets;
using System.IO;
using Windows.Networking;
using System.Threading.Tasks;
using AliseaTorrent.Core;
using Windows.Storage;

namespace TestTorrentUWP
{
    [TestClass]
    public class UnitTest1
    {


        StreamSocketListener socketListener1, socketListener2;
        StreamWriter writer;
        StreamSocket socket;

        [TestMethod]
        public void TestMethod1()
        {
            /*      
                  Byte[] br = { 0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf1, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0x12, 0x34, 0x56, 0x78, 0x9a };
                  Debug.Write(BitConverter.ToString(br).Replace('-', '%'));

                  Byte[]bys = SHA1.GenerateHash(br);

                  //var strHashBase64 = CryptographicBuffer.EncodeToBase64String(bys);
                  //Debug.Write("SHA1: " + strHashBase64 + "\n");


                  string a = Encoding.ASCII.GetString(bys);
                  Debug.Write("A length: " + a.Length);

                  string d = "\x12\x34\x56\x78\x9A\xBC\xDE\xF1\x23\x45\x67\x89\xAB\xCD\xEF\x12\x34\x56\x78\x9A";
                  Debug.Write("\nESCAPED!!!! : " + Uri.EscapeUriString(d)  +  "\n");
                  Debug.Write(a + "\n");
                  string ab = Uri.EscapeDataString("test(brackets)");


                  Debug.WriteLine("String: " + ab + "\tString byte length: " + bys.Length);


      */
            //testTracker();
            //testDataStore();
            //testSocket();

            testCoreTorrent();


        }


        #region CORE TORRENT TEST

        async void testCoreTorrent()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile sampleFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("interstellar.torrent");
            IBuffer buffer = await FileIO.ReadBufferAsync(sampleFile);
            DataReader dataReader = DataReader.FromBuffer(buffer);
            byte[] fileContent = new byte[dataReader.UnconsumedBufferLength];

            dataReader.ReadBytes(fileContent);

            AliseaCoreTorrent core = new AliseaCoreTorrent(fileContent);
            core.StartCarro();
        }

        #endregion

        
        #region SOCKET TEST

        public async void testSocket()
        {
            creaSocket1();
            creaSocket2();
            creaSocketClient();
            await connetti1();
            //await connetti2();
            scrivi1();
        }


        public async void creaSocket1()
        {
            socketListener1 = new StreamSocketListener();
            socketListener1.ConnectionReceived += SocketListener_ConnectionReceived1;

            await socketListener1.BindServiceNameAsync("5252");
        }

        public async void creaSocket2()
        {
            socketListener2 = new StreamSocketListener();
            socketListener2.ConnectionReceived += SocketListener_ConnectionReceived2;

            await socketListener2.BindServiceNameAsync("2525");
        }



        async void SocketListener_ConnectionReceived1(StreamSocketListener s, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.Write("\nConnessione a sock 1\n");

            Stream outStream = args.Socket.OutputStream.AsStreamForWrite();
            writer = new StreamWriter(outStream);

            Stream inStream = args.Socket.InputStream.AsStreamForRead();
            StreamReader reader = new StreamReader(inStream);
            string request = await reader.ReadLineAsync();

            Debug.Write(request);

        }

        async void SocketListener_ConnectionReceived2(StreamSocketListener s, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.Write("Connessione a sock 2");

            Stream inStream = args.Socket.InputStream.AsStreamForRead();
            StreamReader reader = new StreamReader(inStream);
            string request = await reader.ReadLineAsync();

            Debug.Write(request);

        }


        public void creaSocketClient()
        {
            socket = new StreamSocket();

        }

        public async Task<int> connetti1()
        {
            await socket.ConnectAsync(new HostName("localhost"), "5252");

            Stream streamOut = socket.OutputStream.AsStreamForWrite();
            writer = new StreamWriter(streamOut);

            return 1;
        }

        public async Task<int> connetti2()
        {
            await socket.ConnectAsync(new HostName("localhost"), "2525");

            return 1;
        }

        public async void scrivi1()
        {
            await writer.WriteLineAsync("CIAONE CAMPIONE");
            await writer.FlushAsync();
        }




        #endregion


        #region OTHER TEST

        async void testDataStore()
        {
            byte[] dati1 = Encoding.ASCII.GetBytes("ciaotest12");
            byte[] dati2 = Encoding.ASCII.GetBytes("comevamolt");
            byte[] dati3 = Encoding.ASCII.GetBytes("obeneciao1");

            byte[] sha = new byte[60];
            Array.Copy(SHA1.GenerateHash(dati1), 0, sha, 0, 20);
            Array.Copy(SHA1.GenerateHash(dati2), 0, sha, 20, 20);
            Array.Copy(SHA1.GenerateHash(dati3), 0, sha, 40, 20);

            TorrentMetaData metadata = new TorrentMetaData();
            metadata.MetaInfo = new TorrentFilesInfo();
            metadata.MetaInfo.SingleFileMode = true;
            metadata.MetaInfo.PieceLength = 10;
            metadata.MetaInfo.Length = 30;
            metadata.MetaInfo.Pieces = sha;
            
            IDataStore store = new VolatileDataStore();

            store.InitializeDataStore(metadata);
            store.SetEventListener(new DataStoreListener());

            var get1 = store.GetData(0, 4);
            var get2 = store.GetData(0, 30);
            var get3 = store.GetData(20, 4);

            DataTransferUnit dt1 = new DataTransferUnit();
            dt1.pieceId = 0;
            dt1.inPieceOffset = 0;
            dt1.data =  Encoding.ASCII.GetBytes("ciaotest12");

            DataTransferUnit dt2 = new DataTransferUnit();
            dt2.pieceId = 1;
            dt2.inPieceOffset = 0;
            dt2.data = Encoding.ASCII.GetBytes("comevamolt");

            DataTransferUnit dt3 = new DataTransferUnit();
            dt3.pieceId = 2;
            dt3.inPieceOffset = 0;
            dt3.data = Encoding.ASCII.GetBytes("obeneciao1");

            store.PutData(dt1);
            store.PutData(dt2);
            store.PutData(dt3);

            byte[] d1 = await get1;
            byte[] d2 = await get2;
            byte[] d3 = await get3;

            Debug.Write("\n" + ASCIIEncoding.UTF8.GetString(d1) + "\n" + ASCIIEncoding.UTF8.GetString(d2) + "\n");
            Debug.Write("\n" + ASCIIEncoding.UTF8.GetString(d3) + "\n");
        }
        

        async void testTracker()
        {
            TorrentMetaData torrent = new TorrentMetaBuilder().GetTorrentMetaData(@"moon.torrent");
            Debug.Write(torrent);

            string trackeruri = "udp://tracker.coppersurfer.tk:6969/announce";
            AbstractTracker tracker = new TrackerFactory().CreateAbstractTracker(trackeruri);

            AnnounceResponse response = await tracker.RequestAnnounceAsync(
                new AnnounceRequest()
                {
                    InfoHash = SHA1.GenerateHash(torrent.InfoBytes),
                    Port = 4562,
                    PeerId = SHA1.GenerateHash(Encoding.ASCII.GetBytes("ALISEA CIAONE"))
                }
                );

            Debug.Write("\n\nRisposta Annuncio :\n");
            Debug.Write("Complete: " + response.Complete + "\n");
            Debug.Write("Peer number: " + response.Peers.Count + "\n");

            Debug.Write("Peer List:\n");
            for (int i = 0; i < response.Peers.Count; ++i)
            {
                Debug.Write("Peer #" + i + ":\t" + response.Peers[i].Address + " " + response.Peers[i].Port + "\n");
            }
        }


        private class DataStoreListener : IDataStoreEventListener
        {
            public void OnMissingDataRequested(ulong pieceNumber)
            {
                Debug.Write("\nMissing data: " + pieceNumber);
            }

            public void OnMissingDataRequested(ulong dataOffset, uint size)
            {
                Debug.Write("\nMissing data: " + dataOffset);
            }

            public void OnPieceCompleted(int pieceNumber)
            {
                Debug.Write("\nCompleted piece: " + pieceNumber);
            }

            public void OnPieceError(int pieceNumber)
            {
                Debug.Write("\nPiece Error: " + pieceNumber);
            }
        }

        #endregion
    }
}
