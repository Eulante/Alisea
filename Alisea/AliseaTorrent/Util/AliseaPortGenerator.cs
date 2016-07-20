using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Util
{
    public class AliseaPortGenerator
    {
        public static int GeneratePort()
        {
            int port = new Random(DateTime.Now.Millisecond).Next(5000,15000);
            return port;
        }
    }
}
