using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace AliseaTorrent.Util
{
    public class SHA1
    {
        public static Byte[] GenerateHash(Byte[] data)
        {
            Byte[] result = null;

            IBuffer buffer = CryptographicBuffer.CreateFromByteArray(data);
            HashAlgorithmProvider hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            IBuffer hashBuffer = hashAlgorithm.HashData(buffer);
            CryptographicBuffer.CopyToByteArray(hashBuffer, out result);

            return result;
        }
    }
}
