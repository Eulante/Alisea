using Alisea.Classes.Database;
using Alisea.Classes.Model;
using Alisea.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Il modello di elemento Pagina vuota è documentato all'indirizzo http://go.microsoft.com/fwlink/?LinkId=234238

namespace Alisea
{
    /// <summary>
    /// Pagina vuota che può essere usata autonomamente oppure per l'esplorazione all'interno di un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region ATTRIBUTES

        private IDatabaseManager database = null;

        private List<Film> film = null;

        private UInt16 last_film_request;
        
        // mappa delle liste per semi cache
        private IDictionary<string, List<Film>> list_of_film_list = null;

        #endregion



        public MainPage()
        {
            this.InitializeComponent();
            this.Initialize();
        }


        #region INITIALIZATION

        private void Initialize()
        {
            database = new DatabaseManager();
            bool result = database.CreateLocalDatabase();

            list_of_film_list = new Dictionary<string, List<Film>>();

            last_film_request = 0;

            LoadCategories();
            LoadFilm();

            FilmGrid.Items.VectorChanged += FilmGrid_SourceChanged;
        }

        #endregion



        #region INITIAL DATA LOADING

        /*
         *  Asinchronous Categories list request
         */
        private async void LoadCategories()
        {
            Task<List<FilmCategory>> CategoryTask = database.GetFilmCategories();
            CategoryGrid.ItemsSource = await CategoryTask;
        }


        /*
         *  Asinchronous Film list request
         */
        private async void LoadFilm()
        {
            Task<List<Film>> FilmTask = database.GetHotFilm();
            UInt16 film_list_request = ++last_film_request;

            film = await FilmTask;

            UpdateFilmGrid(film_list_request);
        }

        #endregion



        #region CLICK EVENT HANDLERS

        private void btnHamburger_Click(object sender, RoutedEventArgs e)
        {
            HamburgerMenu.IsPaneOpen = !HamburgerMenu.IsPaneOpen;
        }



        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                // User selected an item from the suggestion list, take an action on it here.
            }
            else
            {
                string search = args.QueryText;

                UInt16 film_list_request = ++last_film_request;

                VisualStateManager.GoToState(this, "Loading", false);

                film = await database.SearchFilm(search);

                UpdateFilmGrid(film_list_request);
            }
        }



        private async void MenuMainClass_Click(object sender, RoutedEventArgs e)
        {
            string className = (sender as Button).Name;
            UInt16 film_list_request = ++last_film_request;

            if(list_of_film_list.TryGetValue(className, out film))
            {
                FilmGrid.ItemsSource = film;
                return;
            }


            VisualStateManager.GoToState(this, "Loading", false);

            switch (className)
            {
                case "btnHotFilm":
                    film = await database.GetHotFilm();
                    UpdateFilmGrid(film_list_request);
                    list_of_film_list.Add(className, film);
                    break;
                case "btnNewFilm":
                    film = await database.GetHotFilm();
                    UpdateFilmGrid(film_list_request);
                    list_of_film_list.Add(className, film);
                    break;
                case "btnTopFilm":
                    film = await database.GetHotFilm();
                    UpdateFilmGrid(film_list_request);
                    list_of_film_list.Add(className, film);
                    break;
                case "btnFavoriteFilm":
                    film = await database.GetFavoriteFilms();
                    UpdateFilmGrid(film_list_request);
                    list_of_film_list.Add(className, film);
                    break;
                default:
                    VisualStateManager.GoToState(this, "Empty", false);
                    break;
            }


        }


        private async void MenuCategory_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = (sender as Button).Name;
            UInt16 film_list_request = ++last_film_request;

            if (list_of_film_list.TryGetValue(categoryName, out film))
            {
                FilmGrid.ItemsSource = film;
            }
            else
            {
                VisualStateManager.GoToState(this, "Loading", false);
                film =  await database.GetFilmsFromCategory(categoryName);
                list_of_film_list.Add(categoryName, film);
                UpdateFilmGrid(film_list_request);
            }
            
        }




        private void FilmGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            Film selectedFilm = (Film)e.ClickedItem;
            Frame.Navigate(typeof(FilmDetailPage), selectedFilm);
        }

        #endregion


        #region UI EVENT HANDLERS

        private void FilmGrid_SourceChanged<T>(IObservableVector<T> sender, IVectorChangedEventArgs e)
        {
            if(FilmGrid.Items.Count == 0)
                VisualStateManager.GoToState(this, "Empty", false);
            else
                VisualStateManager.GoToState(this, "NotEmpty", false);
        }

        #endregion



        #region UTIL

        private void UpdateFilmGrid(UInt16 request_number)
        {
            if (request_number == last_film_request)
            {
                FilmGrid.ItemsSource = film;
                FilmGrid_SourceChanged<int>(null, null); //Chiamato per evitare il bu della lista vuota (Progress Ring non va via)
            }
                
                
        }

        #endregion


    }
}
