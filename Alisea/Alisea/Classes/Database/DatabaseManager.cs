using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alisea.Classes.Model;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System.Diagnostics;

namespace Alisea.Classes.Database
{
   
    class DatabaseManager : IDatabaseManager
    {
        private SQLiteConnection conn;

        public DatabaseManager()
        {
            string path = Path.Combine(Windows.Storage.ApplicationData.
            Current.LocalFolder.Path, "dbo.alisea");

            this.conn = new SQLite.Net.SQLiteConnection(new
               SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), path);

        }
        public async Task<List<FilmCategory>> GetFilmCategories()
        {
            List<FilmCategory> FilmCategories = new List<FilmCategory>();

            using (var client = new HttpClient())
            {
                /*
                 * Setting up the address of the service and the type of the response waited for.
                 */
                client.BaseAddress = new Uri("http://aliseamiddleservice.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // The api we want to call.
                HttpResponseMessage response = await client.GetAsync("api/Categories");

                /*
                * If the response is right, the string given as response will be parsed from
                * JsonConvert object into FilmList.
                */
                if (response.IsSuccessStatusCode)
                {

                    string JSONResponse = await response.Content.ReadAsStringAsync();
                    FilmCategories = JsonConvert.DeserializeObject<List<FilmCategory>>(JSONResponse);
                }

                return FilmCategories;
            }
        }

        // FilmCategories changed into Film for debug purpopes. If everything goes right
        // I'll change it back to the original value
        public async Task<List<Film>> GetHotFilm()
        {
            List<Film> FilmList = new List<Film>();

            using (var client = new HttpClient())
            {
                /*
                 * Setting up the address of the service and the type of the response waited for.
                 */
                client.BaseAddress = new Uri("http://aliseamiddleservice.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // The api we want to call.
                HttpResponseMessage response = await client.GetAsync("api/Films/TrendingFilms");

                /*
                * If the response is right, the string given as response will be parsed from
                * JsonConvert object into FilmList.
                */
                if (response.IsSuccessStatusCode)
                {

                    string JSONResponse = await response.Content.ReadAsStringAsync();
                    FilmList = JsonConvert.DeserializeObject<List<Film>>(JSONResponse);

                }

                return FilmList;
            }

        }

        public async Task<List<Film>> GetNewFilm()
        {
            List<Film> FilmList = new List<Film>();

            using (var client = new HttpClient())
            {
                /*
                 * Setting up the address of the service and the type of the response waited for.
                 */

                client.BaseAddress = new Uri("http://aliseamiddleservice.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // The api we want to call.
                HttpResponseMessage response = await client.GetAsync("/api/Films/NewFilms");

                /*
                * If the response is right, the string given as response will be parsed from
                * JsonConvert object into FilmList.
                */
                if (response.IsSuccessStatusCode)
                {

                    string JSONResponse = await response.Content.ReadAsStringAsync();
                    FilmList = JsonConvert.DeserializeObject<List<Film>>(JSONResponse);

                }

                return FilmList;
            }
        }

        public async Task<List<Film>> GetTopFilm()
        {
            List<Film> FilmList = new List<Film>();

            using (var client = new HttpClient())
            {
                /*
                 * Setting up the address of the service and the type of the response waited for.
                 */
                client.BaseAddress = new Uri("http://aliseamiddleservice.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // The api we want to call.
                HttpResponseMessage response = await client.GetAsync("api/Films/TrendingFilms");

                /*
                * If the response is right, the string given as response will be parsed from
                * JsonConvert object into FilmList.
                */
                if (response.IsSuccessStatusCode)
                {

                    string JSONResponse = await response.Content.ReadAsStringAsync();
                    FilmList = JsonConvert.DeserializeObject<List<Film>>(JSONResponse);

                }

                return FilmList;
            }
        }

        public async Task<List<Film>> GetFilmsFromCategory(string categoryName)
        {
            List<Film> category = new List<Film>();

            using (var client = new HttpClient())
            {
                /*
                 * Setting up the address of the service and the type of the response waited for.
                 */
                client.BaseAddress = new Uri("http://aliseamiddleservice.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // The api we want to call.
                HttpResponseMessage response = await client.GetAsync("api/Films/Get/FilmsFromCategory/" + categoryName);

                /*
                * If the response is right, the string given as response will be parsed from
                * JsonConvert object into FilmList.
                */
                if (response.IsSuccessStatusCode)
                {

                    string JSONResponse = await response.Content.ReadAsStringAsync();
                    category = JsonConvert.DeserializeObject<List<Film>>(JSONResponse);

                }

                return category;

            }
        }

        public async Task<List<Film>> SearchFilm(string searchToken)
        {
            List<Film> FilmList = new List<Film>();

            using (var client = new HttpClient())
            {
               
                client.BaseAddress = new Uri("http://aliseamiddleservice.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // searchToken is the film title or one of its part we want to search.
                HttpResponseMessage response = await client.GetAsync("api/Films/Search/" + searchToken);

                if (response.IsSuccessStatusCode)
                {

                    string JSONResponse = await response.Content.ReadAsStringAsync();
                    FilmList = JsonConvert.DeserializeObject<List<Film>>(JSONResponse);

                }

                return FilmList;
            }
        }

        public async void AddThumbUp(int filmID)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri("http://aliseamiddleservice.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await client.GetAsync("api/Films/ThumbsUp/" + filmID);            

            }
        }

        public async void AddThumbDown(int filmID)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri("http://aliseamiddleservice.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await client.GetAsync("api/Films/ThumbsDown/" + filmID);

            }
        }

        public async void AddVisualization(int filmID)
        {
            using (var client = new HttpClient())
            {

                client.BaseAddress = new Uri("http://aliseamiddleservice.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();

                HttpResponseMessage response = await client.GetAsync("api/Films/AddFilmVisualization/" + filmID);

            }
        }

        public bool CreateLocalDatabase()
        {
            // Check if the db was already created.
            if (File.Exists("dbo.alisea"))
            {
                Debug.Write("The db is already created.");
                return false;
            }

            // Creating the tables from the class, which will be 
            // called just by inserting an object of their class.
            this.conn.CreateTable<Favorites>();
            this.conn.CreateTable<Like>();


            return true;
        }

        public bool AddFavorite(int id)
        {
            try
            {
                // Creating a Favorites object with the id passed as parameter. After that
                // the new object will be inserted as a row in their related table (dbo.Favorites)
                var s = new Favorites();
                s.filmID = id;
                conn.Insert(s);

            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


     

        public int GetLikeOrDislike(int filmID)
        {
            // Retrieving the films from the database of liked/disliked with the
            // id passed as parameter.
            var result = conn.Table<Like>().Where(x => x.idFilm == filmID);

            foreach(var item in result)
            {
                if (item.like == true) return 1;
                if (item.dislike == true) return 2;
               
            }

            return 0;
        }

        public bool addLike(int filmID)
        {

            // Retrieving the records from the database of liked/disliked film with the
            // id passed as parameter.
            var result = conn.Table<Like>().Where(x => x.idFilm == filmID).FirstOrDefault();

            // If the film wasn't previously liked or disliked, a new record will be creating
            // by inserting a new object of Like class. Otherwise, an update will be done, changing
            // only che like value.
            if (result == null)
            {
                result = new Like();

                result.idFilm = filmID;
                result.like = true;
                result.dislike = false;

                conn.Insert(result);
            }
            else
            {
                result.like = true;
                result.dislike = false;

                conn.Update(result);
            }
            return true;
        }

        // Same as addLike but the dislike field will be set to true instead of like.
        public bool addDislike(int filmID)
        {
            var result = conn.Table<Like>().Where(x => x.idFilm == filmID).FirstOrDefault();

            if (result == null)
            {
                result = new Like();

                result.idFilm = filmID;
                result.like = false;
                result.dislike = true;

                conn.Insert(result);
            }
            else
            {
                result.dislike = true;
                result.like = false;

                conn.Update(result);
            }

            

            return true;
        }

        public bool GetFavorite(int filmID)
        {
            var result = conn.Table<Favorites>().Where(x => x.filmID == filmID).FirstOrDefault();
            if (result != null) return true;
            else return false;
        }

        public bool removeFavorite(int filmID)
        {
            try
            {
                Favorites favorites = new Favorites();
                favorites.filmID = filmID;
                conn.Delete(favorites);
            }
            catch (Exception)
            {
                Debug.Write("An error occurred during favorite removing");
                return false;
            }

            return true;

        }

        public async Task<List<Film>> GetFavoriteFilms()
        {
            List<Film> favoriteFilms = new List<Film>();

            // Getting the id of the favorite films from db.
            var favorites = conn.Table<Favorites>();

            using (var client = new HttpClient())
            {
                // Preparing the API communication.
                client.BaseAddress = new Uri("http://aliseamiddleservice.azurewebsites.net");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // For each id retrieved from the local db, the application will obtain its informations
                // from the database.
                foreach (var f in favorites)
                {
                    HttpResponseMessage response = await client.GetAsync("api/Films/" + f.filmID);

                    if (response.IsSuccessStatusCode)
                    {
                        string JSONResponse = await response.Content.ReadAsStringAsync();

                        // Since JsonConvert always deserialize into a list of <T>, where T is
                        // a template, the workaround above will add into the list that will be returned
                        // the first value obtained from each conversion, that contains the object deserialized.
                        List<Film> tmp = new List<Film>();
                        tmp = JsonConvert.DeserializeObject<List<Film>>(JSONResponse);
                        favoriteFilms.Add(tmp[0]);
                    }
                }
            }

            return favoriteFilms;


        }
    }
}
