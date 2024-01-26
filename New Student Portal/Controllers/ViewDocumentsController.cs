using New_Student_Portal.CustomSecurity;
using New_Student_Portal.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
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
                    string filename = StudentNo.Replace("/", "");

                    Credentials.ObjNav.GenerateStudentStatement(StudentNo, "FEESTATEMENT-" + filename + ".pdf");
                    filename = "FEESTATEMENT-" + filename + ".pdf";
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
        public JsonResult Receipts(string ReceiptNo)
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
                    string filename = StudentNo.Replace("/", "");
                    Credentials.ObjNav.GenerateReceipts(ReceiptNo, "RCP-" + filename + ".pdf");
                    filename = "RCP-" + filename + ".pdf";
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
                    if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(Session["CurrentProgram"].ToString());
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
                        Session["CurrentSem"] = CommonClass.CurrentSemester(Session["CurrentProgram"].ToString());
                    }
                    Sem = Session["CurrentSem"].ToString();

                    if (EvaluatedAllUnits(RegNo, Sem))
                    {
                        string filename = Session["Username"].ToString().Replace("/", "");
                        Credentials.ObjNav.GenerateStudentExamCards(RegNo, Sem, "EXAMCARD-" + filename + ".pdf");
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

                    string filename = Session["Username"].ToString().Replace("/", "");
                    Credentials.ObjNav.GenerateStudentReportCard(RegNo, "PROVISIONAL RESULTS-" + filename + ".pdf");
                    filename = "PROVISIONAL RESULTS-" + filename + ".pdf";
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
                    string StudentNo = Session["Username"].ToString();
                    string Sem = "";
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(Session["CurrentProgram"].ToString());
                    }

                    Sem = Session["CurrentSem"].ToString();

                    string filename = StudentNo.Replace("/", "");
                    Credentials.ObjNav.GenerateStudentProformaInvoices(StudentNo, Sem, "PROFORMA-" + filename + ".pdf");
                    filename = "PROFORMA-" + filename + ".pdf";
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
        public JsonResult PrintCourseStatement()
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
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(Session["CurrentProgram"].ToString());
                    }

                    Sem = Session["CurrentSem"].ToString();

                    string filename = StudentNo.Replace("/", "");
                    Credentials.ObjNav.PrintCourseStatement(StudentNo, Sem, "COURSESTATEMENT-" + filename + ".pdf");
                    filename = "COURSESTATEMENT-" + filename + ".pdf";
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
        public JsonResult GenerateStudentAudit(string Prog)
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
                    string filename = StudentNo.Replace("/", "");

                    if (Prog == null)
                    {
                        Prog = "";
                    }
                    Credentials.ObjNav.GenerateStudentAudit(StudentNo, Prog, "STDAUDIT-" + filename + ".pdf");
                    filename = "STDAUDIT-" + filename + ".pdf";
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
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetEnrolledProgrammes()
        {
            try
            {
                string STDNo = Session["Username"].ToString();
                #region Programme List
                List<DropdownList> ProgList = new List<DropdownList>();
                string page = "StudentEnrolment?$select=Programme&$filter=Student_No eq '" + STDNo + "'&$format=json";

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

                    //Credentials.ObjNav.GenerateNextLevelAdmissionLetter(AppNo, "ADMLETTER-" + filename + ".pdf", "");
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
                    //Credentials.ObjNav.PrintSpecialExamCard(DocNo, "SPC_EXAMCARD-" + filename + ".pdf");
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