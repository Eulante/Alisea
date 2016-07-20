using Alisea.Classes.Database;
using Alisea.Classes.Model;
using AliseaTorrent.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Il modello di elemento Pagina vuota è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=234238

namespace Alisea.Pages
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class FilmDetailPage : Page
    {

        
        private Film selectedFilm;

        private IDatabaseManager db;

        private TorrentMetaData torrent;

        private bool favorite = false;

        public FilmDetailPage()
        {
            this.InitializeComponent();
            this.db = new DatabaseManager();

            selectedFilm = null;

        }


       
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            selectedFilm = (Film)e.Parameter;

            int result = db.GetLikeOrDislike(selectedFilm.ID);
            if (result == 1) likeButton.IsEnabled = false;
            if (result == 2) dislikeButton.IsEnabled = false;

            if (db.GetFavorite(selectedFilm.ID) == true)
            {
                favorite = true;
                favoriteButton.Visibility = Visibility.Collapsed;
                unfavoriteButton.Visibility = Visibility.Visible;
            }
            else
            {
                favorite = false;
                favoriteButton.Visibility = Visibility.Visible;
                unfavoriteButton.Visibility = Visibility.Collapsed;
            }

            db.AddVisualization(selectedFilm.ID);

            byte[] torrentByte = await GetMetadataByte(selectedFilm.TorrentLink);
            torrent = new TorrentMetaBuilder().GetTorrentMetaData(torrentByte);
            AddPlayButton();
            

        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }



        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TextBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }

        private void likeButton_Click(object sender, RoutedEventArgs e)
        {
            db.AddThumbUp(selectedFilm.ID);
            db.addLike(selectedFilm.ID);

            if (likeButton.IsEnabled == true) likeButton.IsEnabled = false;
            if (dislikeButton.IsEnabled == false) dislikeButton.IsEnabled = true;
        }

        private void dislikeButton_Click(object sender, RoutedEventArgs e)
        {
            db.AddThumbDown(selectedFilm.ID);
            db.addDislike(selectedFilm.ID);

            if (likeButton.IsEnabled == false) likeButton.IsEnabled = true;
            if (dislikeButton.IsEnabled == true) dislikeButton.IsEnabled = false;

        }

        private void favoriteButton_Click(object sender, RoutedEventArgs e)
        {

            db.AddFavorite(selectedFilm.ID);

            favoriteButton.Visibility = Visibility.Collapsed;
            unfavoriteButton.Visibility = Visibility.Visible;
           
        }

        private void unfavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            db.removeFavorite(selectedFilm.ID);
            favoriteButton.Visibility = Visibility.Visible;
            unfavoriteButton.Visibility = Visibility.Collapsed;
        }



        private void btnWatchGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(AliseaPlayerPage), (MultimediaPlayedFileInfo)e.ClickedItem);
        }



        private async Task<byte[]> GetMetadataByte(string metadataUrl)
        {

            byte[] fileContent = null;

            try
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage response = await client.GetAsync(metadataUrl);
                if (response.IsSuccessStatusCode)
                    fileContent = await response.Content.ReadAsByteArrayAsync();
                else
                    Debug.Write("Download non riuscito\n");
            }
            catch (Exception e)
            {
                Debug.Write("Download Error");
            }

            return fileContent;
        }





        private void  AddPlayButton()
        {
            TorrentFilesInfo filesInfo = torrent.MetaInfo;

            List<string> filesName = new List<string>();

            if(filesInfo.SingleFileMode)
            {
                filesName.Add(filesInfo.Name);
                
            }
            else
            {
                foreach (var file in filesInfo.FilesDetails)
                    filesName.Add(file.Path[file.Path.Count -1]);
            }

            List<MultimediaPlayedFileInfo> validFile = new List<MultimediaPlayedFileInfo>();

            int fn = 0;
            foreach(string name in filesName)
            {
                string mime = GetMimeType(name);
                if (mime != null)
                    validFile.Add(new MultimediaPlayedFileInfo() { filename = name, mimetype = mime, filenumber = fn, metadata = torrent });

                ++fn;
            }

            BtnWatchGrid.ItemsSource = validFile;
            Debug.Write("aGGIUNTA TASTI VISUALIZZAZIONE " + validFile.Count + " \n");

        }



        private string GetMimeType(string fileName)
        {
            if (fileName.EndsWith(".mkv"))
                return "video/x-matroska";

            if (fileName.EndsWith(".mp4"))
                return "video/mp4";

            if (fileName.EndsWith(".avi"))
                return "video/x-msvideo";

            if (fileName.EndsWith(".wmv"))
                return "video/x-ms-wmv";

            return null;
        }


    }
}

