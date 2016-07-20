using AliseaTorrent.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Peering
{
    public interface IPeeringResultListener
    {
        void OnPeeringResult(DataTransferUnit data);

        DataTransferUnit GiveMeData(int pieceId, int offset, int size);
    }
}
