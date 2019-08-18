using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using prjEShopping.Models;

namespace prjEShopping.Controllers
{
    
    public class HomeController : BaseController
    {

        public ActionResult Index()
        {
            var ProductList = db.tProduct.OrderByDescending(m => m.fId).Take(8).ToList();
            return View(ProductList);
        }

        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(tMember MemberLogin)
        {
            string Uid = MemberLogin.fUId;
            var member = db.tMember.Where(m => m.fUId == Uid).FirstOrDefault();
            if (member == null)
            {
                ViewBag.Msg_Fail = "帳號或密碼錯誤請重新輸入!!";
            }
            else
            {
                string Password = MemberLogin.fPwd;
                string salt = member.salt;
                string dbPassword = member.fPwd;
                byte[] passwordAndSaltBytes = System.Text.Encoding.UTF8.GetBytes(Password + salt);
                byte[] hashBytes = new System.Security.Cryptography.SHA256Managed().ComputeHash(passwordAndSaltBytes);
                string hashString = Convert.ToBase64String(hashBytes);

                if (dbPassword == hashString)
                {
                    FormsAuthentication.RedirectFromLoginPage
                      (member.fUId, true);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Msg_Fail = "帳號或密碼錯誤請重新輸入!!";
                }
                
            }
            return View();
        }


    }
}