using New_Student_Portal.CustomSecurity;
using New_Student_Portal.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using New_Student_Portal.ViewModel;

namespace New_Student_Portal.Controllers
{
    [CustomeAuthentication]
    [CustomAuthorization(Role = "STUD")]
    public class ViewDocumentsController : Controller
    {
        // GET: ViewDocuments
        public ActionResult ViewDocuments()
        {
            return View();
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
                    str = Credentials.ObjNav.GenerateStudentFeeStatement(str1);
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
                    str = Credentials.ObjNav.GenerateReceipt(ReceiptNo);
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
            try
            {
                string message = "";
                bool success = false;
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                else
                {
                    string StudentNo = Session["Username"].ToString();

                    string Sem = "";
                    if (Session["CurrentSem"] == null || Convert.ToString(Session["CurrentSem"]) == "")
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(StudentNo);
                    }

                    Sem = Session["CurrentSem"].ToString();

                    string filename = StudentNo.Replace("/", "") + "-SEM-PROFORMA";
                    Credentials.ObjNav.GenerateStudentSemesterInvoice(StudentNo, Sem, filename + ".pdf");
                    filename = filename + ".pdf";
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    CommonClass.MoveFile(filename, DestinationPath);

                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = @"/Downloads/" + filename;
                    }
                    else
                    {
                        success = false;
                        message = "File Not Found";
                    }

                }
                return Json(new { message = message, success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult ProformaInvoice()
        {
            try
            {
                string message = "";
                bool success = false;
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                else
                {
                    string studentNo = Session["Username"].ToString();
                    string sem = "";
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(studentNo);
                    }

                    sem = Session["CurrentSem"].ToString();

                    string filename = studentNo.Replace("/", "");
                    message = Credentials.ObjNav.fnGenerateStudentProfomaInvoice(studentNo, sem);
                    success = true;
                    if (message == "")
                    {
                        success = false;
                        message = "File Not Found";
                    }
                }
                return Json(new { message, success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ExamCard()
        {
            try
            {
                string message = "";
                bool success = false;
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    string RegNo = Session["Username"].ToString();
                    string Sem = "";
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                    }
                    Sem = Session["CurrentSem"].ToString();

                    if (EvaluatedAllUnits(RegNo, Sem))
                    {
                        string filename = Session["Username"].ToString().Replace("/", "");
                        Credentials.ObjNav.GenerateStudentExamCard(RegNo, Sem, "EXAMCARD-" + filename + ".pdf");
                        filename = "EXAMCARD-" + filename + ".pdf";
                        string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                        CommonClass.MoveFile(filename, DestinationPath);

                        System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                        if (file.Exists)
                        {
                            success = true;
                            message = @"/Downloads/" + filename;
                        }
                        else
                        {
                            success = false;
                            message = "File Not Found";
                        }
                    }
                    else
                    {
                        success = false;
                        message = "You need to Evaluate all the Units before printing exam card";
                    }
                }
                return Json(new { message = message, success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        protected bool EvaluatedAllUnits(string StdNo, string Sem)
        {
            bool s = true;
            try
            {
                string page = "StudentUnits?$filter=Student_No eq '" + StdNo + "' and Semester eq '" + Sem + "' and Evaluated eq false and (Teaching_Type eq 'Lecture' or Teaching_Type eq 'Teaching Practice')&format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    if (details["value"].Count() > 0)
                    {
                        s = false;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
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
                    str = Credentials.ObjNav.GenerateStudentReportCard(str1, string.Concat("PROVISIONAL RESULTS-", str2, ".pdf"));
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
                    Prog = CommonClass.GetStudentRegisteredProgramme(str1);
                    if (Prog == null)
                    {
                        Prog = "";
                    }
                    str = Credentials.ObjNav.GenerateStudentAudit(str1, Prog);
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
            try
            {
                string STDNo = Session["Username"].ToString();
                #region Programme List
                List<DropdownList> ProgList = new List<DropdownList>();
                string page = "StudentEnrollment?$select=Programme&$filter=Student_No eq '" + STDNo + "'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);


                    foreach (JObject config in details["value"])
                    {
                        DropdownList p = new DropdownList();
                        p.Value = (string)config["Programme"];
                        p.Text = (string)config["Programme"];
                        ProgList.Add(p);
                    }
                }
                #endregion
                DropdownListValues STDProg = new DropdownListValues
                {
                    ListOfValues = ProgList.Select(x =>
                                    new SelectListItem()
                                    {
                                        Text = x.Text,
                                        Value = x.Value
                                    }).ToList()
                };
                return Json(STDProg, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult AdmissionLetter(string AppNo)
        {
            bool success = false;
            try
            {
                string message = "";

                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                else
                {
                    string StudentNo = Session["Username"].ToString();
                    string filename = StudentNo.Replace("/", "");

                    Credentials.ObjNav.GenerateNextLevelAdmissionLetter(AppNo, "ADMLETTER-" + filename + ".pdf", "");
                    filename = "ADMLETTER-" + filename + ".pdf";
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    CommonClass.MoveFile(filename, DestinationPath);

                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = @"/Downloads/" + filename;
                    }
                    else
                    {
                        success = false;
                        message = "File Not Found";
                    }
                }
                return Json(new { message = message, success = success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult SupplimentaryExamCard(string DocNo)
        {
            bool success = false;
            try
            {
                string message = "";

                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    string Sem = "";
                    string RegNo = Session["Username"].ToString();
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                    }
                    Sem = Session["CurrentSem"].ToString();
                    string filename = Session["Username"].ToString().Replace("/", "");
                    Credentials.ObjNav.PrintSupplimentaryExamCard(RegNo, Sem, "SUPP_EXAMCARD-" + filename + ".pdf");
                    filename = "SUPP_EXAMCARD-" + filename + ".pdf";
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    CommonClass.MoveFile(filename, DestinationPath);
                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = @"/Downloads/" + filename;
                    }
                    else
                    {
                        success = false;
                        message = "File Not Found";
                    }
                }
                return Json(new { message = message, success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult SpecialExamCard(string DocNo)
        {
            bool success = false;
            try
            {
                string message = "";

                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    string RegNo = Session["Username"].ToString();

                    string filename = Session["Username"].ToString().Replace("/", "");
                    Credentials.ObjNav.PrintSpecialExamCard(DocNo, "SPC_EXAMCARD-" + filename + ".pdf");
                    filename = "SPC_EXAMCARD-" + filename + ".pdf";
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    CommonClass.MoveFile(filename, DestinationPath);
                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = @"/Downloads/" + filename;
                    }
                    else
                    {
                        success = false;
                        message = "File Not Found";
                    }
                }
                return Json(new { message = message, success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}