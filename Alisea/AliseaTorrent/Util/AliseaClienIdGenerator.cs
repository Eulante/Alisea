using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Util
{
    public class AliseaClienIdGenerator
    {

        public static byte[] Generate()
        {
            byte[] id = new byte[20];

            // Generating a random number which, composed with the string "alisea", will be converted
            // in byte giving the peerId.
            int randomNumber = new Random(DateTime.Now.Millisecond).Next(2000, 5000);
            id = SHA1.GenerateHash(Encoding.ASCII.GetBytes(randomNumber.ToString()));
            Array.Copy(Encoding.ASCII.GetBytes("AL0001"), id, 6);

            return id;
        }
    }
}
