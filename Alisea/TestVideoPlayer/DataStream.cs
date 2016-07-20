using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace TestVideoPlayer
{
    class DataStream : IRandomAccessStream
    {

        Byte[] filmdata = null;

        ulong wantedPosition = 0;
        ulong offsetPosition = 0;

        IRandomAccessStream stram = null;
        public DataStream()
        {
            try
            {
                StorageFolder storageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                Task<StorageFile> sampleFileTask = Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\video.mp4").AsTask();
                sampleFileTask.Wait();

                StorageFile sampleFile = sampleFileTask.Result;

                Task<Stream> streamT = sampleFile.OpenStreamForReadAsync();
                streamT.Wait();
                Stream stream = streamT.Result;
                using (var memoryStream = new MemoryStream())
                {

                    stream.CopyTo(memoryStream);
                    filmdata = memoryStream.ToArray();

                    Debug.Write("\nFile APERTO\n");
                }

            }
            catch (Exception e)
            {
                Debug.Write(e);
            }
        }

        public bool CanRead { get { return true; } }
        public bool CanWrite { get { return false; } }
        public ulong Position { get { return wantedPosition; } }

        public ulong Size
        {
            get
            {
                return (ulong)filmdata.Length;
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

            try
            {


                return AsyncInfo.Run<IBuffer, uint>(async (cancellationToken, progress) =>
                {
                    progress.Report(0);
                    try
                    {
                        Debug.Write("\nDati Richiesti\n");

                        Debug.Write("\nOltre\n");

                        await Task.Delay(TimeSpan.FromMilliseconds(100));

                        IRandomAccessStream s = filmdata.AsBuffer().AsStream().AsRandomAccessStream();
                        s.Seek(wantedPosition + offsetPosition);
                        offsetPosition += count;
                        return await s.ReadAsync(buffer, count, options);
                    }
                    catch(Exception e)
                    {
                        Debug.Write(e);
                    }
                    return null;
                });
                
            }
            catch(Exception e)
            {
                Debug.Write(e);
            }

            return null;
        }



        public void Seek(ulong position)
        {
            this.wantedPosition = position;
            this.offsetPosition = 0;
        }

        public IAsyncOperationWithProgress<uint, uint> WriteAsync(IBuffer buffer)
        {
            throw new NotImplementedException();
        }
    }
}
