using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Alisea.Classes.Model
{
    /// <summary>
    /// This class is reflecting the one used on Entity Framework, being able to create db tables from code.
    /// It contains all the fields related to a film.
    /// </summary>
    class Film
    {
        public int ID { set; get; }
        public string Title { set; get; }
        public string Director { set; get; }
        public short Length { set; get; }
        public DateTime ReleaseDate { set; get; }
        public DateTime InsertDate { set; get; }
        public string Actors { set; get; }
        public string Category { set; get; }
        public string FilmDescription { set; get; }
        public string ImagePath { set; get; }
        public string ThemePath { set; get; }
        public string TorrentLink { set; get; }
        public int Thumbs { set; get; }
        public int WeeklyThumbs { set; get; }
        public int Visualizations { set; get; }

        public static implicit operator Film(string v)
        {
            throw new NotImplementedException();
        }

#if DEBUG
         public void PrintObject()
        {
            Debug.WriteLine(this);
        }
#endif

    }
}
