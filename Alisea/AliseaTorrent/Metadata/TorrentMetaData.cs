using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Metadata
{
    public class TorrentMetaData
    {
        
        public TorrentFilesInfo MetaInfo { get; set; }

        public string Announce { get; set; }

        public List<string> AnnounceList { get; set; }

        public string Comment { get; set; }

        public string CreatedBy { get; set; }

        public int CreationDate { get; set; }

        public string Encoding { get; set; }

        public Byte[] InfoBytes { get; set; }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append("Announce: " + Announce);
            str.Append("\n\n");

            str.Append("Announce List:\n");
            foreach (string ann in AnnounceList)
                str.Append(ann + "\n");
            str.Append("\n");

            str.Append("Comment: " + Comment);
            str.Append("\n\n");

            str.Append("Created by: " + CreatedBy);
            str.Append("\n\n");

            str.Append("Creation Date: " + CreationDate);
            str.Append("\n\n");

            str.Append("Encoding: " + Encoding);
            str.Append("\n\n");

            str.Append("Files info: " + MetaInfo);
            str.Append("\n");

            return str.ToString();
        }

    }
}
