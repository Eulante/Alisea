using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Bencode
{
    class BencodeList : BencodeItem
    {
        List<BencodeItem> items;
        public List<BencodeItem> Items { get { return items; } }


        public BencodeList(List<BencodeItem> items)
        {
            this.items = items;
        }


        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("[ ");

            foreach (BencodeItem item in items)
            {
                builder.Append(item);

                if (item != items.Last())
                    builder.Append(", ");
            }

            builder.Append(" ]");

            return builder.ToString();
        }

    }
}
