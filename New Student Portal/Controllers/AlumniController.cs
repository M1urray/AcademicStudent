using New_Student_Portal.CustomSecurity;
using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.Controllers
{
    [CustomeAuthentication]
    [CustomAuthorization(Role = "ALLUMINAE")]
    public class AlumniController : Controller
    {
        // GET: Allumni
        public ActionResult Dashboard()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                string RegNo = Session["Username"].ToString();

                string page = "CustomerList?$filter=No eq '" + RegNo + "'&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);

                StudentDetailView Details = new StudentDetailView();

                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        Details.No = (string)config["No"];
                        Details.Name = (string)config["Name"];
                        Details.ID_No = (string)config["ID_No"];
                        Details.Gender = (string)config["Gender"];
                        Session["STDGender"] = (string)config["Gender"];
                        Details.Date_Of_Birth = (string)config["Date_Of_Birth"];
                        Details.Phone_No = (string)config["Phone_No"];
                        Details.Address = (string)config["Address"];
                        Details.E_Mail = (string)config["E_Mail"];
                        Details.Balance = (decimal)config["Balance_LCY"];
                        Details.Debit_Amount = (decimal)config["Debit_Amount"];
                        Details.Credit_Amount = (decimal)config["Credit_Amount"];
                        Details.ProfilePic = CommonClass.ProfilePicture(RegNo);
                    }
                }
                return View(Details);
            }
        }
        public ActionResult OnlineApplicationInstructions()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }
        public ActionResult ProgrammApplicationList()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }
        public ActionResult NewProgrammeApplication()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }
        public ActionResult StepOneView()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                return PartialView("~/Views/Allumni/Steps/Step1.cshtml");
            }
        }
        [HttpPost]
        public ActionResult SaveStepOneData()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult StepTwoView()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                return PartialView("~/Views/Allumni/Steps/Step2.cshtml");
            }
        }
        [HttpPost]
        public JsonResult SaveStepTwoData()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult StepThreeView()
        {
            return PartialView("~/Views/Allumni/Steps/Step3.cshtml");
        }
        [HttpPost]
        public JsonResult SaveStepThreeData()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult StepFourView()
        {
            return PartialView("~/Views/Allumni/Steps/Step4.cshtml");
        }
        [HttpPost]
        public JsonResult SaveStepFourData()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult StepFiveView()
        {
            return PartialView("~/Views/Allumni/Steps/Step5.cshtml");
        }
        [HttpPost]
        public JsonResult SaveStepFiveData()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}