using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AliseaTorrent.Metadata;
using System.Diagnostics;
using AliseaTorrent.Util;

namespace AliseaTorrent.Data.Concrete
{
    public class VolatileDataStore : IDataStore
    {
        protected IDataStoreEventListener callbackListener = null;

        private Int32 pieceNumber;
        private UInt32 pieceLength;

        private List<Byte[]> pieceSHA;
        private List<Piece> pieces;
        private Boolean[] completePieces;
        private Boolean[] requestedPieces;

        private UInt64 totalDataSize;
        private Int32 fileNumber;
        private UInt64[] fileSize;
        private UInt64[] fileOffset;


        //controllo
        private int selectedFile = -1;



        public void InitializeDataStore(TorrentMetaData torrent)
        {
            TorrentFilesInfo filesInfo = torrent.MetaInfo;

            pieceLength = (UInt32)filesInfo.PieceLength;
            pieceSHA = torrent.MetaInfo.Sha1;
            pieceNumber = pieceSHA.Count();

            pieces = new List<Piece>(pieceNumber);
            for (int i = 0; i < pieceNumber; ++i)
                pieces.Add(new Piece(pieceLength));


            requestedPieces = new bool[pieceNumber];
            completePieces = new bool[pieceNumber];
            

            if (filesInfo.SingleFileMode)
            {
                fileNumber = 1;
                totalDataSize = (UInt64)filesInfo.Length;   //TODO Metadata uses int size (otherwise it would be enough)

                fileSize = new UInt64[] { (UInt64)filesInfo.Length };
                fileOffset = new UInt64[] { 0 };
            }
            else
            {
                fileNumber = filesInfo.FilesDetails.Count;
                fileSize = new UInt64[fileNumber];
                fileOffset = new UInt64[fileNumber];

                totalDataSize = 0;

                for(int i=0; i<fileNumber; ++i)
                {
                    fileSize[i] = (ulong)filesInfo.FilesDetails[i].Length;
                    fileOffset[i] = totalDataSize;
                    totalDataSize += fileSize[i];
                }
            }
        }



        public void InitializeDataStore(TorrentMetaData torrent, int targetFile)
        {
            InitializeDataStore(torrent);
            selectedFile = targetFile;
        }




        public void Close()
        {

            pieceSHA.Clear();
            pieceSHA = null;
            pieces.Clear();
            pieces = null;
            completePieces = null;
            requestedPieces = null;
            
            fileSize = null;
            fileOffset = null;
    }




        public ulong GetDataSize()
        {
            return totalDataSize;
        }



        public async Task<byte[]> GetData(int offset, int size)
        {
            return await Task.Run( async () =>
            {
                Int64 pieceId = offset / pieceLength;

                Int32 inOffset = (Int32)(offset % pieceLength);

                Int32 neededPieces = 1;
                Int32 tmpsize = size - (Int32)(pieceLength - inOffset);
                while (tmpsize > 0)
                {
                    ++neededPieces;
                    tmpsize -= (Int32)pieceLength;
                }

                if (pieceId + neededPieces > pieceNumber)
                    --neededPieces;
                

                /* Controllo presenza dati */
                bool missingData = true;
                while(missingData)
                {
                    int c = 0;
                    for (int i = 0; i < neededPieces; ++i)
                    {
                        if (completePieces[pieceId + i])
                        {
                            ++c;
                        }                        
                        else if (!requestedPieces[pieceId + i])
                        {
                            if(callbackListener != null)
                            {
                                callbackListener.OnMissingDataRequested((UInt64)(pieceId+i));
                                requestedPieces[pieceId + i] = true;
                            }                               
                        }
                    }

                    if (c == neededPieces)
                        missingData = false;
                    else
                        await Task.Delay(TimeSpan.FromSeconds(2));      
                }

                /* caricamento dati */
                Byte[] buffer = new Byte[size];

                int nextCopy = 0;
                for (int i = (int)pieceId; i < neededPieces + pieceId; ++i)
                {
                    int copySize = 0;
                    if (inOffset + size > pieceLength)
                        copySize = (Int32)pieceLength - inOffset;
                    else
                        copySize = size;

                    Array.Copy(pieces[i].data, inOffset, buffer, nextCopy, copySize);

                    inOffset = 0;
                    nextCopy += copySize;
                    size -= copySize;
                }

                return buffer;
            });
            
        }




        public async Task<byte[]> GetData(int offset, int size, int targetFile)
        {
            if (targetFile >= fileNumber)
                throw new IndexOutOfRangeException();

            return await GetData(offset + (int)fileOffset[targetFile], size);
        }





        public void PutData(DataTransferUnit dataUnit)
        {
            int pid = dataUnit.pieceId;

            if (completePieces[pid])
                return;

            pieces[pid].InsertData(dataUnit);

            if (pieces[pid].IsComplete())
            {
                byte[] sha1 = SHA1.GenerateHash(pieces[pid].data);

                if(CompareSha1(sha1, pieceSHA[pid]))
                {
                    completePieces[pid] = true;

                    if (callbackListener != null)
                        callbackListener.OnPieceCompleted(dataUnit.pieceId);
                }
                else
                {
                    pieces[pid].Reset();

                    if (callbackListener != null)
                        callbackListener.OnPieceError(dataUnit.pieceId);
                }                

            }
                
        }





        public void SetEventListener(IDataStoreEventListener eventListener)
        {
            this.callbackListener = eventListener;
        }



        private bool CompareSha1(byte[] a1, byte[] a2)
        {
            for (int i = 0; i < 20; ++i)
                if (a1[i] != a2[i])
                    return false;
            return true;
        }




        private class Piece
        {
            public Byte[] data = null;

            private UInt32 size;
            private UInt32 used;
            private Boolean[] bitmask;
            private bool complete;


            public Piece(UInt32 pieceSize)
            {
                size = pieceSize;
                used = 0;
                complete = false;
            }


            public bool IsComplete()
            {
                if (complete)
                    return complete;

                for (int i = 0; i < size; ++i)
                    if (!bitmask[i])
                        return false;

                bitmask = null;
                complete =  true;

                return complete;
            }


            public void InsertData(DataTransferUnit dataUnit)
            {
                if (data == null)
                    data = new Byte[size];

                if(bitmask == null)
                    bitmask = new Boolean[size];

                Array.Copy(dataUnit.data, 0, data, dataUnit.inPieceOffset, dataUnit.data.Length);
                used += (UInt32)dataUnit.data.Length;
                int finish = dataUnit.inPieceOffset + dataUnit.data.Length;
                for (int i = dataUnit.inPieceOffset; i < finish; ++i)
                    bitmask[i] = true;
            }

            public void Reset()
            {
                used = 0;
                data = null;
            }
        }
    }
}
