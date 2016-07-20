using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Metadata
{
    public class TorrentFilesInfo
    {
        // Common Info
        public int PieceLength { get; set; }

        private Byte[] pieces;
        public Byte[] Pieces {
            get { return pieces; }
            set
            {
                this.pieces = value;

                Sha1 = new List<Byte[]>();

                int nP = pieces.Count() / 20;
                for(int i=0; i<nP; ++i)
                {
                    Byte[] byt = new Byte[20];
                    Array.Copy(pieces, i * 20, byt, 0, 20);
                    Sha1.Add(byt);
                }
            }
        }

        public List<Byte[]> Sha1;

        public int Private { get; set; }

        public bool SingleFileMode;

        
        public string Name { get; set; }    //SingleFile: filename   MultiFiles: file folder name

        //Single File Mode Property
        private int lng;
        public int Length { get { return lng; } set { SingleFileMode = true; lng = value; } }

        public string md5Sum { get; set; }

        //Multi File Mode Property
        private List<TorrentFilesDetails> fi;
        public List<TorrentFilesDetails> FilesDetails { get { return fi; } set { SingleFileMode = false; fi = value; } }



        public TorrentFilesInfo()
        {
            fi = new List<TorrentFilesDetails>();
        }


        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.Append("Piece length: " + PieceLength);
            str.Append("\n\n");

            str.Append("Pieces (Byte String): ");
            foreach (Byte b in Pieces)
                str.Append(b + " ");
            str.Append("\n\n");

            str.Append("Private: " + Private);
            str.Append("\n\n");

            str.Append("Is single File Mode: " + SingleFileMode);
            str.Append("\n\n");

            str.Append("Name: " + Name);
            str.Append("\n\n");

            str.Append("Single File Details\n");

            str.Append("Length: " + Length);
            str.Append("\n");
            str.Append("md5sum: " + md5Sum);
            str.Append("\n");

            str.Append("Multi File Details:\n");
            foreach (TorrentFilesDetails fs in FilesDetails)
                str.Append(fs);

            return str.ToString();
        }

    }
}
