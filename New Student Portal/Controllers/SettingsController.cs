using New_Student_Portal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.Controllers
{
    public class SettingsController : Controller
    {
        // GET: Settings
        public ActionResult ChangePassword()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                return View();
            }
        }
        public JsonResult ChangePassord(string newpass)
        {
            try
            {
                string message = "";
                bool success = false;
                if (Session["domainUser"] == null)
                {
                    return Json(new { message = "/Login/Login", success = false, redirect = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                   // string StdNo = Session["Username"].ToString();
                    string StdNo = Session["domainUser"].ToString();

                    string  ok = CommonClass.ResetPassword(StdNo, newpass);

                    if (ok!="")
                    {
                        message = "Password Changed Successfully";
                        success = true;
                        //return Json(new { , , redirect = false }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        message = "Problem Encountered while changing your password. Try later";
                        success = false;
                        //return Json(new { , , redirect = false }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { message = message, success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false, redirect = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}