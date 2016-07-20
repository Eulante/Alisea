using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Bencode
{
    class BencodeLong : BencodeItem
    {
        long value;
        public long LongValue { get { return value; } }


        public BencodeLong(long value)
        {
            this.value = value;
        }


        public override string ToString()
        {
            return value.ToString();
        }
    }
}
