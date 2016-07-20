using Alisea.Classes.Model;
using Alisea.Classes.Stream;
using AliseaTorrent.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using AliseaTorrent.Util;
using AliseaTorrent.Metadata;

// Il modello di elemento Pagina vuota è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=234238

namespace Alisea.Pages
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class AliseaPlayerPage : Page
    {
        private string metadataUrl;
        private AliseaCoreTorrent coreTorrent;

        public AliseaPlayerPage()
        {
            this.InitializeComponent();
        }



        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            MultimediaPlayedFileInfo executionInfo = (MultimediaPlayedFileInfo)e.Parameter;

            TorrentMetaData metadata = executionInfo.metadata;

            coreTorrent = new AliseaCoreTorrent(metadata);
            
            Player.SetSource(new VideoRandomAccessStream(coreTorrent.RetrieveIDataStore(), executionInfo.filenumber), executionInfo.mimetype);

            coreTorrent.StartCarro();
        }



        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            coreTorrent.StopCarro();

            if (Frame.CanGoBack)
                Frame.GoBack();
        }




    }
}
