using AliseaTorrent.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Data
{
    public interface IDataStore
    {
        void SetEventListener(IDataStoreEventListener eventListener);

        void InitializeDataStore(TorrentMetaData torrent);

        void InitializeDataStore(TorrentMetaData torrent, Int32 targetFile);

        void PutData(DataTransferUnit data);

        ulong GetDataSize();

        Task<Byte[]> GetData(Int32 offset, Int32 size);

        Task<Byte[]> GetData(Int32 offset, Int32 size, Int32 targetFile);

        void Close();
    }
}
