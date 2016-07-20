using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Bencode
{
    class BencodeDictionary : BencodeItem
    {
        public List<BencodeElement> Elements;
        public Byte[] ByteData;


        public BencodeDictionary(List<BencodeElement> elements, Byte[] byteData)
        {
            this.Elements = elements;
            this.ByteData = byteData;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("{ ");

            foreach(BencodeElement el in Elements)
            {
                builder.Append("\"" + el.Key.Replace(" ", string.Empty) + "\" : ");
                builder.Append(el.Value);

                if (el != Elements.Last())
                    builder.Append(", ");
            }

            builder.Append(" }");

            return builder.ToString();
        }
    }
}
