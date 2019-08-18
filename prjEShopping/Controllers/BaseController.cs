using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using prjEShopping.Models;

namespace prjEShopping.Controllers
{
    
    public class BaseController : Controller
    {
        public BaseController()
        {
            SetCategory();
        }

        protected tEmployeeEntities db = new tEmployeeEntities();
        // GET: Home
        void SetCategory()
        {
            ViewBag.Category = db.tCategory.ToList();
        }
    }
}