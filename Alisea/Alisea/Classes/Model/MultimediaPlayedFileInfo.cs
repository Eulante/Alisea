using AliseaTorrent.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alisea.Classes.Model
{
    public class MultimediaPlayedFileInfo
    {
        public string filename;
        public string mimetype;
        public int filenumber;
        public TorrentMetaData metadata;
    }
}
