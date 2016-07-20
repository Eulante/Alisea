using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Data
{
    public interface IDataStoreEventListener
    {
        void OnMissingDataRequested(UInt64 pieceNumber);

        void OnPieceCompleted(Int32 pieceNumber);

        void OnPieceError(Int32 pieceNumber);
    }
}
