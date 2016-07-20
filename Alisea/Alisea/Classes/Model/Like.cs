using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alisea.Classes.Model
{
    /// <summary>
    /// This class represent every film liked or disliked from the user. Everytime he presses the like or
    /// dislike button in a film detail page, a new record is created with the correct value of like or dislike,
    /// or instead there will be modified in order to reflect users actions. For each film, like and dislike button
    /// can't have the same value.
    /// </summary>
    class Like
    {
        [PrimaryKey]
        public int idFilm { get; set; }
        public bool like {get; set;}
        public bool dislike { get; set; }

        public Like()
        {
            this.like = false;
            this.dislike = false;
        }
    }
}
