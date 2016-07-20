using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alisea.Classes.Model
{
    /// <summary>
    /// This class was created in order to reflect the one used in entity framework in order to create db tables
    /// from a class. It represent a film category, with only its name which is unique.
    /// </summary>
    class FilmCategory
    {
        public string Name { get; set; }

        public static implicit operator List<object>(FilmCategory v)
        {
            throw new NotImplementedException();
        }
    }
}
