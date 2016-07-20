using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AliseaMiddleService.Models;

namespace AliseaMiddleService.Controllers
{
    public class CategoriesController : ApiController
    {
        private AliseaMiddleServiceContext db = new AliseaMiddleServiceContext();

        // GET: api/Categories
        public IQueryable<Category> GetCategories()
        {
            return db.Categories;
        }
        
    }
}