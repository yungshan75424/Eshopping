using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using prjEShopping.Models;

namespace prjEShopping.Controllers
{
    
    public class MemberController : Controller
    {

        tEmployeeEntities db = new tEmployeeEntities();
        // GET: Home
        
        public ActionResult Index()
        {
            string uid = User.Identity.Name;
            if (uid != null)
            {
                ViewBag.memberislogin = 1;
            }
            else
            {
                ViewBag.memberislogin = 0;
            }
            var ProductList = db.tProduct.OrderByDescending(m => m.fId).Take(8).ToList();
            return View(ProductList);
        }
        
    }
}