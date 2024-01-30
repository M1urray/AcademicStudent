using New_Student_Portal.CustomSecurity;
using New_Student_Portal.Models;
using New_Student_Portal.NAVWS;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;

namespace New_Student_Portal.Controllers
{
    [CustomAuthorization(Role = "STUD")]
    [CustomeAuthentication]
    public class ViewDocumentsController : Controller
    {
        public ViewDocumentsController()
        {
        }

        public JsonResult AdmissionLetter(string AppNo)
        {
            JsonResult jsonResult;
            bool flag = false;
            try
            {
                string str = "";
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    string str2 = str1.Replace("/", "");
                    str2 = string.Concat("ADMLETTER-", str2, ".pdf");
                    string str3 = base.Server.MapPath(string.Concat("~/Downloads/", str2));
                    CommonClass.MoveFile(str2, str3);
                    if (!(new FileInfo(str3)).Exists)
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                    else
                    {
                        flag = true;
                        str = string.Concat("/Downloads/", str2);
                    }
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        protected bool EvaluatedAllUnits(string StdNo, string Sem)
        {
            bool flag = true;
            try
            {
                string str = string.Concat(new string[] { "StudentUnits?$filter=Student_No eq '", StdNo, "' and Semester eq '", Sem, "' and Evaluated eq false and (Teaching_Type eq 'Lecture' or Teaching_Type eq 'Teaching Practice')&format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() > 0)
                    {
                        flag = false;
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetSemesters()
        {
            try
            {
                string regNo = Session["Username"].ToString();
                string programme = Session["CurrentProgram"].ToString();

                #region Years
                List<CourseReg> CReg = new List<CourseReg>();

                string page = "CourseReg?$filter=StudentNo eq '" + regNo + "' and Reversed eq false and Programme eq '" + programme + "'&format=json";


                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    foreach (JObject config in details["value"])
                    {
                        CourseReg Prgrammes = new CourseReg();
                        Prgrammes.Semester = (string)config["Semester"];
                        CReg.Add(Prgrammes);
                    }
                }
                #endregion
                var listSemesters = new StudentSemesters
                {
                    ListOfSemesters = CReg.Select(x =>
                        new SelectListItem()
                        {
                            Text = x.Semester,
                            Value = x.Semester
                        }).DistinctBy(x => x.Value).ToList()
                };
                return Json(listSemesters, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ExamCard(string semester)
        {
            ActionResult action;
            try
            {
                string str = "";
                bool flag = false;
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    if (!this.EvaluatedAllUnits(str1, semester))
                    {
                        flag = false;
                        str = "You need to Evaluate all the Units before printing exam card";
                    }
                    else
                    {
                        base.Session["Username"].ToString().Replace("/", "");
                        str = Credentials.ObjNav.GenerateStudentExamCards(str1, semester);
                        flag = true;
                        if (str == "")
                        {
                            flag = false;
                            str = "File Not Found";
                        }
                    }
                    action = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                action = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return action;
        }
        public JsonResult FeeStatement()
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    str1.Replace("/", "");
                    str = Credentials.ObjNav.GenerateStudentStatement(str1);
                    flag = true;
                    if (str == "")
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public JsonResult GenerateStudentAudit(string Prog)
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    string str2 = str1.Replace("/", "");
                    Prog = Session["CurrentProgram"].ToString();
                    if (Prog == null)
                    {
                        Prog = "";
                    }
                    //str = Credentials.ObjNav.GenerateStudentAudit(str1, Prog, string.Concat("STDAUDIT-", str2, ".pdf"));
                    flag = true;
                    if (str == "")
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetEnrolledProgrammes()
        {
            JsonResult jsonResult;
            try
            {
                string str = base.Session["Username"].ToString();
                List<DropdownList> dropdownLists = new List<DropdownList>();
                string str1 = string.Concat("StudentEnrolment?$select=Programme&$filter=Student_No eq '", str, "'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        DropdownList dropdownList = new DropdownList()
                        {
                            Value = (string)item["Programme"],
                            Text = (string)item["Programme"]
                        };
                        dropdownLists.Add(dropdownList);
                    }
                }
                jsonResult = base.Json(new DropdownListValues()
                {
                    ListOfValues = (
                        from x in dropdownLists
                        select new SelectListItem()
                        {
                            Text = x.Text,
                            Value = x.Value
                        }).ToList<SelectListItem>()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public JsonResult PrintCourseStatement()
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    string str2 = "";
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    str2 = base.Session["CurrentSem"].ToString();
                    string str3 = str1.Replace("/", "");
                    Credentials.ObjNav.PrintCourseStatement(str1, str2, string.Concat("COURSESTATEMENT-", str3, ".pdf"));
                    str3 = string.Concat("COURSESTATEMENT-", str3, ".pdf");
                    string str4 = base.Server.MapPath(string.Concat("~/Downloads/", str3));
                    CommonClass.MoveFile(str3, str4);
                    if (!(new FileInfo(str4)).Exists)
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                    else
                    {
                        flag = true;
                        str = string.Concat("/Downloads/", str3);
                    }
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public JsonResult ProformaInvoice()
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    string str2 = "";
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    str2 = base.Session["CurrentSem"].ToString();
                    string str3 = str1.Replace("/", "");
                    str = Credentials.ObjNav.GenerateStudentProformaInvoices(str1, str2, string.Concat("PROFORMA-", str3, ".pdf"));
                    flag = true;
                    if (str == "")
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public ActionResult ProvisionalResults()
        {
            ActionResult action;
            try
            {
                string str = "";
                bool flag = false;
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    string str2 = base.Session["Username"].ToString().Replace("/", "");
                    //str = Credentials.ObjNav.GenerateStudentReportCard(str1, string.Concat("PROVISIONAL RESULTS-", str2, ".pdf"));
                    flag = true;
                    if (str == "")
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                    action = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                action = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return action;
        }

        public JsonResult Receipts(string ReceiptNo)
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    str1.Replace("/", "");
                    str = Credentials.ObjNav.GenerateReceipts(ReceiptNo);
                    flag = true;
                    if (str == "")
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public JsonResult SemesterProformaInvoice()
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    string str2 = "";
                    if ((base.Session["CurrentSem"] == null ? true : base.Session["CurrentSem"].ToString() == ""))
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    str2 = base.Session["CurrentSem"].ToString();
                    string str3 = string.Concat(str1.Replace("/", ""), "-SEM-PROFORMA");
                    Credentials.ObjNav.GenerateStudentSemesterInvoice(str1, str2, string.Concat(str3, ".pdf"));
                    str3 = string.Concat(str3, ".pdf");
                    string str4 = base.Server.MapPath(string.Concat("~/Downloads/", str3));
                    CommonClass.MoveFile(str3, str4);
                    if (!(new FileInfo(str4)).Exists)
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                    else
                    {
                        flag = true;
                        str = string.Concat("/Downloads/", str3);
                    }
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public ActionResult SpecialExamCard(string DocNo)
        {
            ActionResult action;
            bool flag = false;
            try
            {
                string str = "";
                if (base.Session["Username"] != null)
                {
                    base.Session["Username"].ToString();
                    string str1 = base.Session["Username"].ToString().Replace("/", "");
                    str1 = string.Concat("SPC_EXAMCARD-", str1, ".pdf");
                    string str2 = base.Server.MapPath(string.Concat("~/Downloads/", str1));
                    CommonClass.MoveFile(str1, str2);
                    if (!(new FileInfo(str2)).Exists)
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                    else
                    {
                        flag = true;
                        str = string.Concat("/Downloads/", str1);
                    }
                    action = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                action = base.Json(new { message = exception.Message, success = flag }, JsonRequestBehavior.AllowGet);
            }
            return action;
        }

        public ActionResult ViewDocuments()
        {
            return base.View();
        }
    }
}