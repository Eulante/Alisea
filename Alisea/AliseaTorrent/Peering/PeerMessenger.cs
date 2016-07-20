using AliseaTorrent.Data;
using AliseaTorrent.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace AliseaTorrent.Peering
{
    public class PeerMessenger : IPeerMessenger
    {


        public Peer peer { get; private set; }

        private StreamSocket socket = null;

        private IPeerMessengerCallback callbackListener = null;

        private bool continueListeningFromSocketInputStream = true;

        private byte[] bitfield = null;

        private bool valid = true;




        public PeerMessenger(Peer peer, StreamSocket socket)
        {
            this.peer = peer;
            this.socket = socket;
        }


        public PeerMessenger(Peer peer, StreamSocket socket, IPeerMessengerCallback callbackListener)
        {
            this.peer = peer;
            this.socket = socket;
            this.callbackListener = callbackListener;
        }




        public async void StartListening()
        {
            await ListenHandshake();
            Task listenTask = Task.Run(() => StreamListenTask(socket));
        }



        public bool IsValid()
        {
            return valid;
        }
    

        public bool OwnPiece(int pid)
        {
            if (pid > bitfield.Count() * 8)
                return false;

            int bpos = pid / 8;
            int bid = pid % 8;

            int p = 1 << (7 - bid);

            int test = bitfield[bpos] & p;

            if (test != 0)
                return true;

            return false;
        }



        private async Task<int> StreamRead(Stream inStream, byte[] btd, int size)
        {
            int read = 0;

            read += inStream.Read(btd, read, size - read);

            while (read < size)
            {
                await Task.Delay(50);
                read += inStream.Read(btd, read, size - read);
            }

            return size;
        }



        async void StreamListenTask(StreamSocket socket)
        {
            try
            {
                int pid, siz, star;

                while (continueListeningFromSocketInputStream)
                {
                    DataReader reader = new DataReader(socket.InputStream);
                    reader.ByteOrder = ByteOrder.BigEndian;

                    await reader.LoadAsync(4);
                    int payloadLength = reader.ReadInt32();

                    if (payloadLength == 0)
                    {
                        callbackListener.KeepAliveMessage(this);
                        continue;
                    }

                    await reader.LoadAsync(1);
                    byte messageid = reader.ReadByte();
                    

                    switch (messageid)
                    {
                        case 0:
                            peer.peer_choking = true;
                            callbackListener.ChokeMessage(this);
                            break;
                        case 1:
                            peer.peer_choking = false;
                            callbackListener.UnchokeMessage(this);
                            break;
                        case 2:
                            peer.peer_interested = true;
                            callbackListener.InterestedMessage(this);
                            break;
                        case 3:
                            peer.peer_interested = false;
                            callbackListener.NotInterestedMessage(this);
                            break;
                        case 4:
                            try
                            {
                                await reader.LoadAsync(4);
                                pid = reader.ReadInt32();
                                callbackListener.HaveMessage(this, pid);
                            }
                            catch (Exception e)
                            {
                                valid = false;
                                await socket.CancelIOAsync();
                            }
                            break;
                        case 5:
                            try
                            {
                                int bitfieldsize = payloadLength - 1;
                                byte[] bts = new byte[bitfieldsize];
                                await reader.LoadAsync((uint)bitfieldsize);
                                reader.ReadBytes(bts);

                                this.bitfield = bts;

                                callbackListener.BitfieldMessage(this, bts);
                            }
                            catch (Exception e)
                            {
                                valid = false;
                                await socket.CancelIOAsync();
                            }
                            break;
                        case 6:
                            try
                            {
                                await reader.LoadAsync(4);
                                pid = reader.ReadInt32();
                                await reader.LoadAsync(4);
                                star = reader.ReadInt32();
                                await reader.LoadAsync(4);
                                siz = reader.ReadInt32();
                                callbackListener.RequestMessage(this, pid, star, siz);
                            }
                            catch (Exception e)
                            {
                                valid = false;
                                await socket.CancelIOAsync();
                            }
                            break;
                        case 7:
                            try
                            {
                                await reader.LoadAsync(4);
                                pid = reader.ReadInt32();
                                await reader.LoadAsync(4);
                                star = reader.ReadInt32();

                                int lng = payloadLength - 9;
                                byte[] block = new byte[lng];

                                await reader.LoadAsync((uint)lng);
                                reader.ReadBytes(block);

                                DataTransferUnit dtu = new DataTransferUnit();
                                dtu.data = block;
                                dtu.inPieceOffset = star;
                                dtu.pieceId = pid;

                                callbackListener.PieceMessage(this, dtu);
                            }
                            catch (Exception e)
                            {
                                valid = false;
                                await socket.CancelIOAsync();
                            }
                            break;
                        case 8:
                            try
                            {
                                await reader.LoadAsync(4);
                                pid = reader.ReadInt32();
                                await reader.LoadAsync(4);
                                star = reader.ReadInt32();
                                await reader.LoadAsync(4);
                                siz = reader.ReadInt32();
                                callbackListener.CancelMessage(this, pid, star, siz);
                            }
                            catch (Exception e)
                            {
                                valid = false;
                                await socket.CancelIOAsync();
                            }
                            break;
                        case 9:
                            try
                            {
                                await reader.LoadAsync(2);
                                Int16 port = reader.ReadInt16();
                            }catch(Exception e)
                            {
                                valid = false;
                                await socket.CancelIOAsync();
                            }                          
                            break;
                    }

                    reader.DetachStream();
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
                valid = false;
            }
        }



        public async Task<int> ListenHandshake()
        {
            try
            {
                DataReader reader = new DataReader(socket.InputStream);

                reader.InputStreamOptions = InputStreamOptions.None;
                await reader.LoadAsync(1);

                byte firstByte = reader.ReadByte();

                await reader.LoadAsync(firstByte);
                byte[] protocol = new byte[firstByte];
                reader.ReadBytes(protocol);

                await reader.LoadAsync(8);
                byte[] reserved = new byte[8];
                reader.ReadBytes(reserved);

                await reader.LoadAsync(20);
                byte[] trSha = new byte[20];
                reader.ReadBytes(trSha);

                await reader.LoadAsync(20);
                byte[] peerId = new byte[20];
                reader.ReadBytes(peerId);

                reader.DetachStream();
            }
            catch (Exception e) {  }

            return 0;
        }



        // First message sent
        public async void HandshakeMessage(byte[] torrentSha, byte[] clientId)
        {
            DataWriter writer = new DataWriter(socket.OutputStream);     

            byte[] handshake = new byte[68];
            handshake[0] = 19;
            Array.Copy(Encoding.UTF8.GetBytes("BitTorrent protocol"), 0, handshake, 1, 19);
            Array.Copy(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, 0, handshake, 20, 8);
            Array.Copy(torrentSha, 0, handshake, 28, 20);
            Array.Copy(clientId, 0, handshake, 48, 20);

            writer.WriteBytes(handshake);
            await WriterStoreAndFlush(writer);

            writer.DetachStream();
        }


        // Mantain the connection alive
        public async void KeepAliveMessage()
        {        
            DataWriter writer = new DataWriter(socket.OutputStream);
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteInt32(0);
            await WriterStoreAndFlush(writer);
            writer.DetachStream();
        }

        // Choke the destination peer
        public async void ChokeMessage()
        {
            DataWriter writer = new DataWriter(socket.OutputStream);
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteInt32(1);
            writer.WriteByte(0);
            await WriterStoreAndFlush(writer);
            writer.DetachStream();
        }

        // Unchoke the destination peer
        public async void UnchokeMessage()
        {

            DataWriter writer = new DataWriter(socket.OutputStream);
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteInt32(1);
            writer.WriteByte(1);
            await WriterStoreAndFlush(writer);
            writer.DetachStream();
        }

        // Set interest to the destination peer
        public async void InterestedMessage()
        {

            DataWriter writer = new DataWriter(socket.OutputStream);
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteInt32(1);
            writer.WriteByte(2);
            await WriterStoreAndFlush(writer);
            writer.DetachStream();
        }

        // Remove interest in the destination peer
        public async void NotInterestedMessage()
        {

            DataWriter writer = new DataWriter(socket.OutputStream);
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteInt32(1);
            writer.WriteByte(3);
            await WriterStoreAndFlush(writer);
            writer.DetachStream();
        }

        // Notify to destination an owned piece id
        public async void HaveMessage(int pieceIndex)
        {

            DataWriter writer = new DataWriter(socket.OutputStream);
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteInt32(5);
            writer.WriteByte(4);
            writer.WriteInt32(pieceIndex);
            await WriterStoreAndFlush(writer);
            writer.DetachStream();
        }

        // Include all the owned pieces
        public void BitfieldMessage()
        {
            
        }

        // Request a piece's block 
        public async void RequestMessage(int pieceIndex, int begin, int size)
        {

            DataWriter writer = new DataWriter(socket.OutputStream);
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteInt32(13);
            writer.WriteByte(6);
            writer.WriteInt32(pieceIndex);
            writer.WriteInt32(begin);
            writer.WriteInt32(size);
            await WriterStoreAndFlush(writer);
            writer.DetachStream();
        }

        // Send a piece's block (after a request)
        public async void PieceMessage(DataTransferUnit dataUnit)
        {

            DataWriter writer = new DataWriter(socket.OutputStream);
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteInt32(9 + dataUnit.data.Count());
            writer.WriteByte(7);
            writer.WriteInt32(dataUnit.pieceId);
            writer.WriteInt32(dataUnit.inPieceOffset);
            writer.WriteBytes(dataUnit.data);
            await WriterStoreAndFlush(writer);
            writer.DetachStream();
        }

        // Cancel a piece request
        public async void CancelMessage(int pieceIndex, int begin, int size)
        {

            DataWriter writer = new DataWriter(socket.OutputStream);
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.WriteInt32(13);
            writer.WriteByte(8);
            writer.WriteInt32(pieceIndex);
            writer.WriteInt32(begin);
            writer.WriteInt32(size);
            await WriterStoreAndFlush(writer);
            writer.DetachStream();
        }


        public void PortMessage()
        {
            
        }


        public bool IsChocking()
        {
            return peer.peer_choking;
        }





        private async Task WriterStoreAndFlush(IDataWriter writer)
        {
            try
            {
                await writer.StoreAsync();
                await writer.FlushAsync();
            }
            catch (Exception e)
            {
                continueListeningFromSocketInputStream = false;
                socket.Dispose();
            }
        }
    }
}
