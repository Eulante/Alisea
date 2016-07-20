using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Core
{
    public class TransferCounterSingleton
    {
        private static TransferCounterSingleton instance = null;

        public long uploaded;
        public long downloaded;
        public long left;

        private TransferCounterSingleton()
        {
            uploaded = 0;
            downloaded = 0;
            left = 0;
        }

        public static TransferCounterSingleton Instance()
        {
            if (instance == null)
                instance = new TransferCounterSingleton();

            return instance;
        }
    }
}
