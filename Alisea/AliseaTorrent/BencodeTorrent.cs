using AliseaTorrent.Bencode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;

namespace AliseaTorrent
{
    public class BencodeTorrent
    {
        public BencodeItem DictionaryInfo;



        public BencodeTorrent(string fileUrl)
        {
            var task = GetData(fileUrl);
            task.Wait();

        }



        public BencodeTorrent(Byte[] torrentBytes)
        {
            BencodeReader reader = new BencodeReader(torrentBytes);
            DictionaryInfo = reader.ReadNextItem();
        }




        private async Task GetData(string fileUrl)
        {

            Windows.Storage.StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile sampleFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(fileUrl);

            IBuffer buffer = await FileIO.ReadBufferAsync(sampleFile);

            DataReader dataReader = DataReader.FromBuffer(buffer);

            byte[] fileContent = new byte[dataReader.UnconsumedBufferLength];

            dataReader.ReadBytes(fileContent);


            BencodeReader reader = new BencodeReader(fileContent);
            DictionaryInfo = reader.ReadNextItem();
        }





    }
}
