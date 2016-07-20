using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Bencode
{
    class BencodeByteString : BencodeItem
    {
        Byte[] value;
        public Byte[] ByteStringValue { get { return value; } }


        public BencodeByteString(Byte[] value)
        {
            this.value = value;
        }


        public override string ToString()
        {
            return ("\"" + Encoding.UTF8.GetString(value) + "\"");
        }

    }
}
