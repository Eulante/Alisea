using AliseaTorrent.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Alisea.Classes.Stream
{
    class VideoRandomAccessStream : IRandomAccessStream
    {
        private IDataStore dataStore;
        private ulong currentPosition;
        private ulong offset;
        private int selectedFile = 0;

        public VideoRandomAccessStream(IDataStore dataStore)
        {
            this.dataStore = dataStore;

            currentPosition = 0;
            offset = 0;
        }

        public VideoRandomAccessStream(IDataStore dataStore, int selectedFile)
        {
            this.dataStore = dataStore;
            this.selectedFile = selectedFile;

            currentPosition = 0;
            offset = 0;
        }

        public bool CanRead
        {
            get
            {
                return true;
            }
        }

        public bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public ulong Position
        {
            get
            {
                return currentPosition;
            }
        }

        public ulong Size
        {
            get
            {
                return dataStore.GetDataSize();
            }

            set
            {
            }
        }

        public IRandomAccessStream CloneStream()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IAsyncOperation<bool> FlushAsync()
        {
            throw new NotImplementedException();
        }

        public IInputStream GetInputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }

        public IOutputStream GetOutputStreamAt(ulong position)
        {
            throw new NotImplementedException();
        }

        public IAsyncOperationWithProgress<IBuffer, uint> ReadAsync(IBuffer buffer, uint count, InputStreamOptions options)
        {

            return AsyncInfo.Run<IBuffer, uint>(async (cancellationToken, progress) =>
            {
                progress.Report(0);

                byte[] data = await dataStore.GetData((int)currentPosition + (int)offset, (int)count, selectedFile);
                offset += count;

                return data.AsBuffer();

            });
        }

        public void Seek(ulong position)
        {
            currentPosition = position;
            offset = 0;
        }

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}
