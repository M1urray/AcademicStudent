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
                if (Session["Username"] == null)
                {
                    return Json(new { message = "/Login/Login", success = false, redirect = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string StdNo = Session["Username"].ToString();

                    bool ok = CommonClass.ChangeStudentPassword(StdNo, newpass);

                    if (ok)
                    {
                        return Json(new { message = "Password Changed Successfully", success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { message = "Problem Encountered while changing your password. Try later", success = false, redirect = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false, redirect = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}