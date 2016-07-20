using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Util
{
    class BufferCloner
    {

        public static T[] Copy<T>(T[] buffer, int start, int length)
        {
            T[] result = new T[length];
            Array.Copy(buffer, start, result, 0, length);
            return result;
        }
    }
}
