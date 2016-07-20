using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AliseaMiddleService.Models
{
    public class Film
    {
        public int ID { set; get; }
        public string Title { set; get; }
        public string Director { set; get; }
        public short Length { set; get; }
        public DateTime ReleaseDate { set; get; }
        public DateTime InsertDate { set; get; }
        public string Actors { set; get; }
        public string Category { set; get;}
        public string FilmDescription { set; get; }
        public string ImagePath { set; get; }
        public string ThemePath { set; get; }
        public string TorrentLink { set; get; }
        public short Thumbs { set; get; }
        public short WeeklyThumbs { set; get; }
        public short Visualizations { set; get; }
    }
}