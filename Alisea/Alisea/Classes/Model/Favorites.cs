using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alisea.Classes.Model
{
    /// <summary>
    /// This class is used to create a localdb, using SQLite, to store all the favorite films from the user.
    /// </summary>
    class Favorites
    {
        /// <summary>
        /// filmID is the only column of the database, which is also a primary key, which will contain the id
        /// of the films prefered from the user.
        /// </summary>
        [PrimaryKey]
        public int filmID { get; set; }
    }
}
