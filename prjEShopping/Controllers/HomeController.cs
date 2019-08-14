using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using prjEShopping.Models;

namespace prjEShopping.Controllers
{
    
    public class HomeController : Controller
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
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(tMember NewMember)
        {
            string Uid = NewMember.fUId;
            var member = db.tMember.Where(m => m.fUId == Uid).FirstOrDefault();
            if (member !=null)
            {
                ViewBag.Msg_Fail = "帳號已有人使用，請輸入另一個!";
            }
            else
            {
                string Password = NewMember.fPwd;
                string salt = Guid.NewGuid().ToString();
                byte[] passwordAndSaltBytes = System.Text.Encoding.UTF8.GetBytes(Password + salt);
                byte[] hashBytes = new System.Security.Cryptography.SHA256Managed().ComputeHash(passwordAndSaltBytes);
                string hashString = Convert.ToBase64String(hashBytes);

                NewMember.fPwd = hashString;
                NewMember.salt = salt;
                db.tMember.Add(NewMember);
                db.SaveChanges();
                ViewBag.Msg_OK = "註冊成功，請登入系統購物";
            }
            return View();
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
        [Authorize]
        // GET: Member
        public ActionResult MemberEdit()
        {
            
            string uid = User.Identity.Name;

            var member = db.tMember.Where(m => m.fUId == uid).FirstOrDefault();
            return View(member);
        }
        [Authorize]
        [HttpPost]
        public ActionResult MemberEdit(tMember vMember)
        {

            string uid = User.Identity.Name;
            var member = db.tMember.Where(m => m.fUId == uid).FirstOrDefault();
            member.fName = vMember.fName;
            member.fPwd = vMember.fPwd;
            member.fEmail = vMember.fEmail;
            db.SaveChanges();
            ViewBag.Msg = "會員基本資訊修改完成！";
            return View();
        }

    }
}