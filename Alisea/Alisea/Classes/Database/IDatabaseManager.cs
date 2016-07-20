using Alisea.Classes.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alisea.Classes.Database
{
    interface IDatabaseManager
    {
        /// <summary>
        /// Retrieve a list of films from database ordered by the number of thumbs, descendent.
        /// </summary>
        /// <returns>The list of the films obtained.</returns>
        Task<List<Film>> GetTopFilm();

        /// <summary>
        /// Retrieve a list of films from database ordered by their launch date, descendent.
        /// </summary>
        /// <returns>The list of the films obtained.</returns>
        Task<List<Film>> GetNewFilm();

        /// <summary>
        /// Retrieve a list of films from database ordered by the number of weekly thumbs, descendent.
        /// </summary>
        /// <returns>The list of films obtained.</returns>
        Task<List<Film>> GetHotFilm();

        /// <summary>
        /// Retrieve a list of films containing the string passed as parameter.
        /// </summary>
        /// <param name="searchToken">A string containing the name of the film or a part of it.</param>
        /// <returns>The list of the films obtained.</returns>
        Task<List<Film>> SearchFilm(String searchToken);

        /// <summary>
        /// Retrieve a list containing all the film categories stored in the database.
        /// </summary>
        /// <returns></returns>
        Task<List<FilmCategory>> GetFilmCategories();

        /// <summary>
        /// Given the name of the category, this method will return all the films with that category.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        Task<List<Film>> GetFilmsFromCategory(string categoryName);

        /// <summary>
        /// Adds one thumb to the relative column of the film, specified with an id.
        /// </summary>
        /// <param name="filmID">The id of the film the user liked.</param>
        void AddThumbUp(int filmID);

        /// <summary>
        /// Subctract one thumb to the relative column of the film, specified with an id.
        /// </summary>
        /// <param name="filmID">The id of the film the user disliked.</param>
        void AddThumbDown(int filmID);

        /// <summary>
        /// Adds one visualization to the relative column of the film, specified with an id.
        /// </summary>
        /// <param name="filmID">The id of the film the user has viewed.</param>
        void AddVisualization(int FilmID);

        /// <summary>
        /// Check if database exists. If doesn't, then it will be created.
        /// </summary>
        /// <returns>true if the db and the tables are created, false otherwise.</returns>
        bool CreateLocalDatabase();

        /// <summary>
        /// Adding the film id into the favorites table.
        /// </summary>
        /// <returns></returns>
        bool AddFavorite(int id);

        /// <summary>
        /// Retrieve a favorite from an ID, if exists.
        /// </summary>
        /// <returns>true if the film is found from the local favorites, false otherwise.</returns>
        bool GetFavorite(int filmID);

        /// <summary>
        /// Delete a favorite film from local database, given an ID.
        /// </summary>
        /// <param name="filmID"></param>
        /// <returns>true if the favorite film was removed from the local db, false otherwise.</returns>
        bool removeFavorite(int filmID);

        /// <summary>
        /// Given the filmID, the method check if the film was previously liked or disliked in order to
        /// disable the related buttons on the page.
        /// </summary>
        /// <param name="filmID">the ID of the film.</param>
        /// <returns>1 if the film was previously liked, 2 if disliked, 0 if none of them.</returns>
        int GetLikeOrDislike(int filmID);

        /// <summary>
        /// Setting the like field of the film, identified with the ID, to true.
        /// </summary>
        /// <param name="filmID"></param>
        /// <returns>true if the operation terminates correctly, false otherwise.</returns>
        bool addLike(int filmID);

        /// <summary>
        /// Setting the dislike field of the film, identified with the ID, to true.
        /// </summary>
        /// <param name="filmID"></param>
        /// <returns>true if the operation terminates correctly, false otherwise.</returns>
        bool addDislike(int filmID);

        /// <summary>
        /// Obtaining the list of favorite films stored in the local db.
        /// </summary>
        /// <returns>The row field of the favorite films</returns>
        Task<List<Film>> GetFavoriteFilms();
    }
}

