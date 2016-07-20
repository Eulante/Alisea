using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web.Http.Description;
using AliseaMiddleService.Models;
using System.Web.Http;
using System.Data.SqlClient;
using System.Configuration;

namespace AliseaMiddleService.Controllers
{
    public class FilmsController : ApiController
    {

        private AliseaMiddleServiceContext db = new AliseaMiddleServiceContext();


        private string connectionString;


        public FilmsController()
        {
            this.connectionString = ConfigurationManager.ConnectionStrings["AliseaMiddleServiceContext"].ConnectionString;

        }
    
      
        // GET: api/Films/NewFilms
        [System.Web.Http.Route("api/Films/Get/FilmsFromCategory/{category}")]
        public IEnumerable<Film> GetFIlmsFromCategory(string category)
        {
            var query = from f in db.Films
                        join c in db.Categories on f.Category equals c.Name
                        where c.Name == category
                        orderby f.ReleaseDate descending
                        select f;

            return query;

        }

        // UPDATE: api/Films/ResetWeeklyThumbs
        // REMEMBER TO CALL IT EVERY MONDAY.
        [AcceptVerbs("GET", "POST")]
        [Route("api/Films/ResetWeeklyThumbs")]
        public void ResetWeeklyThumbs()
        {
            // Creating a SQL connection with the connection string retrieved from
            // Web.config file.
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Calling out the stored procedure needed inside the function.
                using (SqlCommand cmd = new SqlCommand("SP_ResetAllWeeklyThumbs", con))
                {
                    // Defining the type of SqlCommand we want to execute
                    cmd.CommandType = CommandType.StoredProcedure;
                    //Opening the connection and executing the query.
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // UPDATE: api/Films/ThumbsUp/{idFilm}
        [AcceptVerbs("GET", "POST")]
        [Route("api/Films/ThumbsUp/{idFilm}")]
        public void ThumbsUpFilm(int idFilm)
        {
            // Creating a SQL connection with the connection string retrieved from
            // Web.config file.
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Calling out the stored procedure needed inside the function.
                using (SqlCommand cmd = new SqlCommand("SP_FilmThumbUp", con))
                {
                    // Defining the type of SqlCommand we want to execute
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Binding the stored procedure variabile with the one passed
                    // to the function.
                    cmd.Parameters.Add("@filmID", SqlDbType.Int).Value = idFilm;
                    //Opening the connection and executing the query.
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // UPDATE: api/Films/ThumbsUp/{idFilm}
        [AcceptVerbs("GET", "POST")]
        [Route("api/Films/ThumbsDown/{idFilm}")]
        public void ThumbsDownFilm(int idFilm)
        {
            // Creating a SQL connection with the connection string retrieved from
            // Web.config file.
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Calling out the stored procedure needed inside the function.
                using (SqlCommand cmd = new SqlCommand("SP_FilmThumbDown", con))
                {
                    // Defining the type of SqlCommand we want to execute
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Binding the stored procedure variabile with the one passed
                    // to the function.
                    cmd.Parameters.Add("@filmID", SqlDbType.Int).Value = idFilm;
                    //Opening the connection and executing the query.
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // UPDATE: api/Films/AddFilmVisualization/{idFilm}
        [AcceptVerbs("GET", "POST")]
        [Route("api/Films/AddFilmVisualization/{idFilm}")]
        public void AddFilmVisualization(int idFilm)
        {
            // Creating a SQL connection with the connection string retrieved from
            // Web.config file.
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Calling out the stored procedure needed inside the function.
                using (SqlCommand cmd = new SqlCommand("SP_NewVisualization", con))
                {
                    // Defining the type of SqlCommand we want to execute
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Binding the stored procedure variabile with the one passed
                    // to the function.
                    cmd.Parameters.Add("@filmID", SqlDbType.Int).Value = idFilm;
                    //Opening the connection and executing the query.
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // GET: api/Films/NewFilms
        [System.Web.Http.Route("api/Films/NewFilms")]
        public IEnumerable<Film> GetNewFilms()
        {
            var query = from f in db.Films
                        orderby f.ReleaseDate ascending
                        select f;

            return query;

        }

        // GET: api/Films/TrendingFilms
        [System.Web.Http.Route("api/Films/TrendingFilms")]
        public IQueryable<Film> GetTrendingFilms()
        {
            var query = from f in db.Films
                        orderby f.WeeklyThumbs descending
                        select f;

            return query;

        }

        // GET: api/Films/MostViewedFilms
        [System.Web.Http.Route("api/Films/MostViewedFilms")]
        public IEnumerable<Film> GetMostViewedFilms()
        {
            var query = from f in db.Films
                        orderby f.Visualizations descending
                        select f;

            return query;

        }
        // GET: api/Films
        public IQueryable<Film> GetFilms()
        {
            return db.Films;
        }

        // GET: api/Films/NomeFIlm
        [ResponseType(typeof(Film))]
        [System.Web.Http.Route("api/Films/Search/{title}")]
        public IEnumerable<Film> GetFilm(string Title)
        {
            var query = from f in db.Films
                        where f.Title.Contains(Title)
                        select f;

            return query;
        }

        // GET: api/Films/id
        [ResponseType(typeof(Film))]
        [System.Web.Http.Route("api/Films/{filmID}")]
        public IEnumerable<Film> GetFilm(int filmID)
        {
            var query = from f in db.Films
                        where f.ID == filmID
                        select f;

            return query;
        }


       

        
    }
}