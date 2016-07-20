using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Metadata
{
    public class TorrentFilesDetails
    {
        public int Length { get; set; }

        public List<string> Path { get; set; }

        public string md5Sum { get; set; }


        public TorrentFilesDetails()
        {
            Path = new List<string>();
        }


        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append("Length: " + Length);
            str.Append("\n");
            
            str.Append("md5sum: " + md5Sum);
            str.Append("\n");

            str.Append("Path: ");

            foreach (string sp in Path)
                str.Append("\\" + sp);

            str.Append("\n");

            return str.ToString();
        }

    }
}
