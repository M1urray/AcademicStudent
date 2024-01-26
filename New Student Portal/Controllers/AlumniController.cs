using New_Student_Portal.CustomSecurity;
using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.Controllers
{
    [CustomAuthorization(Role = "ALLUMINAE")]
    [CustomeAuthentication]
    public class AlumniController : Controller
    {
        public AlumniController()
        {
        }

        public ActionResult Dashboard()
        {
            ActionResult action;
            if (base.Session["Username"] != null)
            {
                string str = base.Session["Username"].ToString();
                string str1 = string.Concat("CustomerList?$filter=No eq '", str, "'&format=json");
                HttpWebResponse odataData = Credentials.GetOdataData(str1);
                StudentDetailView studentDetailView = new StudentDetailView();
                using (StreamReader streamReader = new StreamReader(odataData.GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        studentDetailView.No = (string)item["No"];
                        studentDetailView.Name = (string)item["Name"];
                        studentDetailView.ID_No = (string)item["ID_No"];
                        studentDetailView.Gender = (string)item["Gender"];
                        base.Session["STDGender"] = (string)item["Gender"];
                        studentDetailView.Date_Of_Birth = (string)item["Date_Of_Birth"];
                        studentDetailView.Phone_No = (string)item["Phone_No"];
                        studentDetailView.Address = (string)item["Address"];
                        studentDetailView.E_Mail = (string)item["E_Mail"];
                        studentDetailView.Campus = (string)item["Global_Dimension_1_Code"];
                        studentDetailView.Balance = (decimal)item["Balance_LCY"];
                        studentDetailView.Debit_Amount = (decimal)item["Debit_Amount"];
                        studentDetailView.Credit_Amount = (decimal)item["Credit_Amount"];
                        studentDetailView.ProfilePic = CommonClass.ProfilePicture(str);
                    }
                }
                action = base.View(studentDetailView);
            }
            else
            {
                action = base.RedirectToAction("Login", "Login");
            }
            return action;
        }

        public ActionResult EducHistList()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    List<QualHist> qualHists = new List<QualHist>();
                    string str1 = string.Concat("StdEduHistory?$filter=Student_No eq '", str, "'&$format=json");
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            QualHist qualHist = new QualHist()
                            {
                                From = (string)item["From"],
                                To = (string)item["To"],
                                Institution = (string)item["Organisation"],
                                Award = (string)item["Job_Title"]
                            };
                            qualHists.Add(qualHist);
                        }
                    }
                    action = this.PartialView("~/Views/Alumni/Partial Views/QualificationHistory.cshtml", qualHists);
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                action = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return action;
        }

        public ActionResult EmplHistList()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    List<EmpHist> empHists = new List<EmpHist>();
                    string str1 = string.Concat("StdEmpHist?$filter=Student_No eq '", str, "'&$format=json");
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            EmpHist empHist = new EmpHist()
                            {
                                From = (string)item["From"],
                                To = (string)item["To"],
                                Company = (string)item["Organisation"],
                                Title = (string)item["Job_Title"]
                            };
                            empHists.Add(empHist);
                        }
                    }
                    action = this.PartialView("~/Views/Alumni/Partial Views/EmploymentHistory.cshtml", empHists);
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                action = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return action;
        }

        public ActionResult NewEmplymentHist()
        {
            return base.PartialView("~/Views/Alumni/Partial Views/EmpHistForm.cshtml");
        }

        public ActionResult NewProgrammeApplication()
        {
            ActionResult action;
            if (base.Session["Username"] != null)
            {
                action = base.View();
            }
            else
            {
                action = base.RedirectToAction("Login", "Login");
            }
            return action;
        }

        public ActionResult NewQualifHist()
        {
            return base.PartialView("~/Views/Alumni/Partial Views/QualHistForm.cshtml");
        }

        public ActionResult OnlineApplicationInstructions()
        {
            ActionResult action;
            if (base.Session["Username"] != null)
            {
                action = base.View();
            }
            else
            {
                action = base.RedirectToAction("Login", "Login");
            }
            return action;
        }

        public ActionResult ProgrammApplicationList()
        {
            ActionResult action;
            if (base.Session["Username"] != null)
            {
                action = base.View();
            }
            else
            {
                action = base.RedirectToAction("Login", "Login");
            }
            return action;
        }

        public ActionResult ProgrammeEnrollment()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    List<ProgEnrol> progEnrols = new List<ProgEnrol>();
                    string str1 = string.Concat("StudentEnrolment?$filter=Student_No eq '", str, "'&$format=json");
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            ProgEnrol progEnrol = new ProgEnrol()
                            {
                                Prog = (string)item["Programme"],
                                ProgName = CommonClass.GetProgrammeName((string)item["Programme"])
                            };
                            progEnrols.Add(progEnrol);
                        }
                    }
                    action = this.PartialView("~/Views/Alumni/Partial Views/ProgrammeEnrolment.cshtml", progEnrols);
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                action = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return action;
        }

        [HttpPost]
        public JsonResult RemoveEmpHistLine(string LineNo)
        {
            JsonResult jsonResult;
            try
            {
                jsonResult = base.Json(new { message = "Line deleted Successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message.Replace("'", ""), success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveStepFiveData()
        {
            JsonResult jsonResult = base.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveStepFourData()
        {
            JsonResult jsonResult = base.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [HttpPost]
        public ActionResult SaveStepOneData()
        {
            ActionResult action;
            if (base.Session["Username"] != null)
            {
                action = base.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                action = base.RedirectToAction("Login", "Login");
            }
            return action;
        }

        [HttpPost]
        public JsonResult SaveStepThreeData()
        {
            JsonResult jsonResult = base.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveStepTwoData()
        {
            JsonResult jsonResult = base.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public PartialViewResult StepFiveView()
        {
            return base.PartialView("~/Views/Alumni/Steps/Step5.cshtml");
        }

        public PartialViewResult StepFourView()
        {
            return base.PartialView("~/Views/Alumni/Steps/Step4.cshtml");
        }

        public ActionResult StepOneView()
        {
            ActionResult action;
            if (base.Session["Username"] != null)
            {
                action = base.PartialView("~/Views/Alumni/Steps/Step1.cshtml");
            }
            else
            {
                action = base.RedirectToAction("Login", "Login");
            }
            return action;
        }

        public PartialViewResult StepThreeView()
        {
            return base.PartialView("~/Views/Alumni/Steps/Step3.cshtml");
        }

        public ActionResult StepTwoView()
        {
            ActionResult action;
            if (base.Session["Username"] != null)
            {
                action = base.PartialView("~/Views/Alumni/Steps/Step2.cshtml");
            }
            else
            {
                action = base.RedirectToAction("Login", "Login");
            }
            return action;
        }

        [HttpPost]
        public JsonResult SubmitEmpHistLine(EmpHist empHist)
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                DateTime.ParseExact(empHist.From.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime.ParseExact(empHist.To.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                jsonResult = base.Json(new { message = "Line deleted Successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message.Replace("'", ""), success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SubmitQualHistLine(QualHist qual)
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                DateTime.ParseExact(qual.From.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime.ParseExact(qual.To.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                jsonResult = base.Json(new { message = "Line deleted Successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message.Replace("'", ""), success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }
    }
}