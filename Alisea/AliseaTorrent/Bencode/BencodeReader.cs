using AliseaTorrent.Bencode.BencodeException;
using AliseaTorrent.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliseaTorrent.Bencode
{
    class BencodeReader
    {
        
        Byte[] message;
        int byteIdx;

        public BencodeReader(Byte[] message)
        {
            this.message = message;
            this.byteIdx = 0;
        }



        public static BencodeItem FindItemByKey(BencodeDictionary dict, string key)
        {
            BencodeItem item = null;

            foreach(BencodeElement element in dict.Elements)
            {
                if (element.Key.Equals(key))
                    return element.Value;

                if (element.Value is BencodeDictionary)
                {
                    BencodeItem rItem = FindItemByKey((BencodeDictionary)element.Value, key);
                    if (rItem != null)
                        return rItem;
                }
                    
            }
            return item;
        }


        public bool HasMoreItem()
        {
            return (byteIdx < message.Length);
        }


        public BencodeItem ReadNextItem()
        {
            char start = (char)message[byteIdx];

            switch (start)
            {
                case 'd':
                    return ReadDictionaryItem();
                case 'l':
                    return ReadListItem();
                case 'i':
                    return ReadIntItem();
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ReadStringItem();
                //throw new BadBencodingException("Bad Bencoding Exception: Unexpected character found\n");
            }
            return ReadStringItem();

        }

        private BencodeElement ReadNextElement()
        {           
            string key = ReadNextKey();
            BencodeItem item = ReadNextItem();
            return  new BencodeElement(key, item);

        }



        private string ReadNextKey()
        {
            BencodeByteString stringItem = ReadStringItem();
            return Encoding.UTF8.GetString(stringItem.ByteStringValue);
        }

        private BencodeLong ReadIntItem()
        {
            ++byteIdx;
            int endCharIdx = findNextChar(message, 'e', byteIdx);

            Byte[] strInt = BufferCloner.Copy(message, byteIdx, endCharIdx - byteIdx);
            long value = Convert.ToInt64(Encoding.UTF8.GetString(strInt));

#if DEBUG
            Debug.Write("Bencode int: " + value + "\n");
#endif
            byteIdx = endCharIdx + 1;

            return new BencodeLong(value);

        }

        private BencodeByteString ReadStringItem()
        {
           int nidx = 0;

            while (message[byteIdx+nidx] >= '0' && message[byteIdx+nidx] <= '9')
                ++nidx;

            if (nidx > 0)
            {
                Byte[] strLenght = BufferCloner.Copy(message, byteIdx, nidx);
                int length = Convert.ToInt32(Encoding.UTF8.GetString(strLenght));

                byteIdx = byteIdx + nidx + 1;

                Byte[] bencodedByteString = BufferCloner.Copy(message, byteIdx, length);

                byteIdx += length;
                
#if DEBUG
                Debug.Write("Bencode string (Length " + length + "): " + Encoding.UTF8.GetString(bencodedByteString) + "\n");
#endif
                return new BencodeByteString(bencodedByteString);
            }

            byteIdx = message.Length;
            return null;
        }

        private BencodeList ReadListItem()
        {

            List<BencodeItem> items = new List<BencodeItem>();

            ++byteIdx;

            while (message[byteIdx] != 'e')
                items.Add(ReadNextItem());

            ++byteIdx;

            return new BencodeList(items);
        }

        private BencodeDictionary ReadDictionaryItem()
        {
            List<BencodeElement> elements = new List<BencodeElement>();

            int dictStartIndex = byteIdx;
            ++byteIdx;

            while (message[byteIdx] != 'e')
                elements.Add(ReadNextElement());

            int length = byteIdx - dictStartIndex + 1;
            Byte[] dictByte = BufferCloner.Copy(message, dictStartIndex, length);

            ++byteIdx;

            return new BencodeDictionary(elements, dictByte);

        }





        private int findNextChar(Byte[] data, char c, int index)
        {
            for (int i = index; i < data.Length; ++i)
                if ((char)data[i] == c)
                    return i;
            return -1;
        }



    }

}
