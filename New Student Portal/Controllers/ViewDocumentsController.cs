using Microsoft.Ajax.Utilities;
using New_Student_Portal.CustomSecurity;
using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json;
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
    [CustomAuthorization(Role = "STUD")]
    public class ViewDocumentsController : Controller
    {
        // GET: ViewDocuments
        public ActionResult ViewDocuments()
        {
            return View();
        }
        public ActionResult DocumentViewResults()
        {
            string RegNo = Session["Username"].ToString();
            StudentProgrammes ListPrograms = new StudentProgrammes();
            #region Years
            List<CourseReg> CReg = new List<CourseReg>();

            string page = "CourseReg?$filter=StudentNo eq '" + RegNo + "' and Reversed eq false&format=json";

            HttpWebResponse httpResponse = Credentials.GetOdataData(page);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                foreach (JObject config in details["value"])
                {
                    CourseReg Prgrammes = new CourseReg();
                    Prgrammes.Programme = (string)config["Programme"];
                    Prgrammes.ProgrammeName = (string)config["Programme_Name"];
                    CReg.Add(Prgrammes);
                }
            }
            #endregion            
            ListPrograms = new StudentProgrammes
            {
                ListOfProgrammes = CReg.Select(x =>
                                     new SelectListItem()
                                     {
                                         Text = x.ProgrammeName,
                                         Value = x.Programme
                                     }).DistinctBy(x => x.Value).ToList()
            };
            return View(ListPrograms);

        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetSemesters(string Programme)
        {
            try
            {
                string RegNo = Session["Username"].ToString();
                StudentSemesters ListSemesters = new StudentSemesters();
                #region Years
                List<CourseReg> CReg = new List<CourseReg>();

                string page = "CourseReg?$filter=StudentNo eq '" + RegNo + "' and Reversed eq false and Programme eq '"+Programme+"'&format=json";

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
                ListSemesters = new StudentSemesters
                {
                    ListOfSemesters = CReg.Select(x =>
                                         new SelectListItem()
                                         {
                                             Text = x.Semester,
                                             Value = x.Semester
                                         }).DistinctBy(x => x.Value).ToList()
                };
                return Json(ListSemesters, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetTranscriptReport(string Programme, string Semester)
        {
            try 
            {
                string message = "";
                bool success = false;
                
                    string RegNo = Session["Username"].ToString();

                    string filename = Session["Username"].ToString().Replace("/", "");
                    Credentials.ObjNav.GenerateSemesterTranscript(RegNo, "PROVISIONAL RESULTS-" + filename + ".pdf", Semester, Programme);
                    filename = "PROVISIONAL RESULTS-" + filename + ".pdf";
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    CommonClass.MoveFile(filename, DestinationPath);

                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = Credentials.fileDownLoads + filename;
                    }
                    else
                    {
                        success = false;
                        message = "File Not Found";
                    }
                
                return Json(new { message = message, success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }

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
                        message = Credentials.fileDownLoads + filename;
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
        public JsonResult FeeStructure()
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
                    string Prog = Session["CurrProg"].ToString();
                    string SettlementType = Session["SettlementType"].ToString();
                    string filename = StudentNo.Replace("/", "");

                    Credentials.ObjNav.GenerateFeeStructureReport(Prog, SettlementType,StudentNo, "FEESTRUCTURE-" + filename + ".pdf");
                    filename = "FEESTRUCTURE-" + filename + ".pdf";
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    CommonClass.MoveFile(filename, DestinationPath);

                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = Credentials.fileDownLoads + filename;
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
                    Credentials.ObjNav.GenerateReceipt(ReceiptNo, "RCP-" + filename + ".pdf");
                    filename = "RCP-" + filename + ".pdf";
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    CommonClass.MoveFile(filename, DestinationPath);

                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = Credentials.fileDownLoads + filename;
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
                        Session["CurrentSem"] = CommonClass.CurrentSemester();
                    }
                    Sem = Session["CurrentSem"].ToString();

                    //if (EvaluatedAllUnits(RegNo, Sem))
                    //{
                    string filename = Session["Username"].ToString().Replace("/", "");
                    Credentials.ObjNav.GenerateStudentExamCard(RegNo, Sem, "EXAMCARD-" + filename + ".pdf");
                    filename = "EXAMCARD-" + filename + ".pdf";
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    CommonClass.MoveFile(filename, DestinationPath);

                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = Credentials.fileDownLoads + filename;
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
        protected bool EvaluatedAllUnits(string StdNo, string Sem)
        {
            bool s = true;
            try
            {
                string page = "StudentUnits?$filter=Student_No eq '" + StdNo + "' and Semester eq '" + Sem + "' and Evaluated eq false&format=json";

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
                        message = Credentials.fileDownLoads + filename;
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
                        Session["CurrentSem"] = CommonClass.CurrentSemester();
                    }

                    Sem = Session["CurrentSem"].ToString();

                    string filename = StudentNo.Replace("/", "");
                    Credentials.ObjNav.GenerateStudentProformaInvoice(StudentNo, Sem, "PROFORMA-" + filename + ".pdf");
                    filename = "PROFORMA-" + filename + ".pdf";
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    CommonClass.MoveFile(filename, DestinationPath);

                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = Credentials.fileDownLoads + filename;
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
                        Session["CurrentSem"] = CommonClass.CurrentSemester();
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
                        message = Credentials.fileDownLoads + filename;
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
        public JsonResult GenerateStudentAudit()
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

                    Credentials.ObjNav.GenerateStudentAudit(StudentNo, "STDAUDIT-" + filename + ".pdf");
                    filename = "STDAUDIT-" + filename + ".pdf";
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    CommonClass.MoveFile(filename, DestinationPath);

                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = Credentials.fileDownLoads + filename;
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
    }
}