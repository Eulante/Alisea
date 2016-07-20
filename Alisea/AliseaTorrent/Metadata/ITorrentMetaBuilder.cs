using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Metadata
{
    interface ITorrentMetaBuilder
    {
        TorrentMetaData GetTorrentMetaData(string metaFileUrl);
        TorrentMetaData GetTorrentMetaData(Byte[] metadataByteArray);
    }
}
