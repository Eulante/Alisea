using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AliseaMiddleService.Models
{
    public class Category
    {
        [Key]
        public string Name{ get; set;}
    }
}