using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Bencode
{
    class BencodeElement
    {
        string key;
        public string Key { get { return key; } }

        BencodeItem value;
        public BencodeItem Value { get { return value; } }


        public BencodeElement(string key, BencodeItem value)
        {
            this.key = key;
            this.value = value;
        }

    }
}
