using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Data
{
    public class DataTransferUnit
    {
        public Int32 pieceId;
        public Int32 inPieceOffset;
        public Byte[] data;
    }
}
