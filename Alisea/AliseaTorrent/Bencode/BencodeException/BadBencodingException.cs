using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Bencode.BencodeException
{
    class BadBencodingException : Exception
    {
        public BadBencodingException(String exception) : base(exception) { }

    }
}
