using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using prjEShopping.Models;

namespace prjEShopping.Controllers
{
    
    public class MemberController : BaseController
    {
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(tMember NewMember)
        {
            string Uid = NewMember.fUId;
            var member = db.tMember.Where(m => m.fUId == Uid).FirstOrDefault();
            if (member != null)
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

        [Authorize]
        // GET: Member
        public ActionResult MemberEdit()
        {
            string uid = User.Identity.Name;

            var member = db.tMember.Where(m => m.fUId == uid).FirstOrDefault();
            member.fPwd = string.Empty;
            return View(member);
        }
        [Authorize]
        [HttpPost]
        public ActionResult MemberEdit(tMember vMember)
        {
            string uid = User.Identity.Name;
            var member = db.tMember.Where(m => m.fUId == uid).FirstOrDefault();
            member.fName = vMember.fName;
            //member.fPwd = vMember.fPwd;
            string Password = vMember.fPwd;
            string salt = member.salt;
            string dbPassword = member.fPwd;
            byte[] passwordAndSaltBytes = System.Text.Encoding.UTF8.GetBytes(Password + salt);
            byte[] hashBytes = new System.Security.Cryptography.SHA256Managed().ComputeHash(passwordAndSaltBytes);
            string hashString = Convert.ToBase64String(hashBytes);
            member.fPwd = hashString;
            member.fEmail = vMember.fEmail;
            db.SaveChanges();
            ViewBag.Msg = "會員基本資訊修改完成！";
            return View();
        }
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }
        [Authorize]
        public ActionResult ProductList(int fCategoryId)
        {
            var ProductList = db.tProduct.Where(m => m.fCategoryId == fCategoryId).ToList();
            return View(ProductList);
        }
        [Authorize]
        public ActionResult Product(int fCategoryId)
        {
            var product = db.tProduct.Where(m => m.fCategoryId == fCategoryId).ToList();
            return View(product);
        }
        [Authorize]
        public ActionResult ShoppingCar(string fPid)
        {
            string uid = User.Identity.Name;
            var ShoppingCar = db.tShoppingCar.Where(m=>m.fUId==uid &&m.fPId==fPid).FirstOrDefault();
            if(ShoppingCar != null)
            {
                ShoppingCar.fQty += 1;
            }
            else
            {
                var product = db.tProduct.Where(m => m.fPId == fPid).FirstOrDefault();
                tShoppingCar newCar = new tShoppingCar();
                newCar.fUId = uid;
                newCar.fPId = fPid;
                newCar.fPName = product.fPName;
                newCar.fPrice = product.fPrice;
                newCar.fQty = 1;
                db.tShoppingCar.Add(newCar);
            }
            db.SaveChanges();

            return RedirectToAction("ShoppingCarList");
        }
        [Authorize]
        public ActionResult ShoppingCarList()
        {
            string uid = User.Identity.Name;
            var shoppingCar = db.tShoppingCar.Where(m => m.fUId == uid).ToList();
            return View(shoppingCar);
        }
        [Authorize]
        public ActionResult ShoppingCarAddQty(int fId)
        {
            string uid = User.Identity.Name;
            var ShoppingCar = db.tShoppingCar.Where(m => m.fId==fId).FirstOrDefault();
            ShoppingCar.fQty += 1;
            db.SaveChanges();
            return RedirectToAction("ShoppingCarList");
        }


        [Authorize]
        public ActionResult ShoppingCarSubQty(int fId)
        {
            string uid = User.Identity.Name;
            var ShoppingCar = db.tShoppingCar.Where(m => m.fId == fId).FirstOrDefault();
            ShoppingCar.fQty -= 1;
            if(ShoppingCar.fQty==0)
            {
                db.tShoppingCar.Remove(ShoppingCar);
            }
            db.SaveChanges();
            return RedirectToAction("ShoppingCarList");
        }
        [Authorize]
        public ActionResult ShoppingCarDelete(int fId)
        {
            string uid = User.Identity.Name;
            var ShoppingCar = db.tShoppingCar.Where(m => m.fId == fId).FirstOrDefault();

            db.tShoppingCar.Remove(ShoppingCar);
            
            db.SaveChanges();
            return RedirectToAction("ShoppingCarList");
        }

        [Authorize]
        [HttpPost]
        public ActionResult Order(tOrder vOrder)
        {
            string uid = User.Identity.Name;

            var order = new tOrder
            {
                fReceiverAddress = vOrder.fReceiverAddress,
                fOrderDate = DateTime.Now,
                fOrderState = "未出貨",
                fReceiver = vOrder.fReceiver,
                fReceiverPhone = vOrder.fReceiverPhone,
                fUId = uid
            };

            db.tOrder.Add(order);
            db.SaveChanges();

            var shoppings = db.tShoppingCar.Where(e => e.fUId == uid).ToList();

            var details = shoppings.Select(e =>
              new tOrderDetails
              {
                  fOrderId = order.fOrderId,
                  fPId = e.fPId,
                  fPName = e.fPName,
                  fPrice = e.fPrice,
                  fQty = e.fQty
              }).ToList();

            db.tOrderDetails.AddRange(details);
            db.tShoppingCar.RemoveRange(shoppings);

            db.SaveChanges();

            return RedirectToAction("OrderList");

        }
        [Authorize]
        public ActionResult OrderList()
        {
            string uid = User.Identity.Name;
            var order = db.tOrder.Where(m => m.fUId == uid).OrderByDescending(m => m.fOrderId).ToList();
            return View(order);

        }

        [Authorize]
        public ActionResult OrderDetails(int fOrderId)
        {
            var order = db.tOrderDetails.Where(m => m.fOrderId == fOrderId).ToList();
            return View(order);
        }
    }

}