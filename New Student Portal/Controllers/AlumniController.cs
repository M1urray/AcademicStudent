using New_Student_Portal.CustomSecurity;
using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                        Details.Campus = (string)config["Global_Dimension_1_Code"];
                        Details.Balance = (decimal)config["Balance_LCY"];
                        Details.Debit_Amount = (decimal)config["Debit_Amount"];
                        Details.Credit_Amount = (decimal)config["Credit_Amount"];
                        Details.ProfilePic = CommonClass.ProfilePicture(RegNo);
                    }
                }
                return View(Details);
            }
        }
        public ActionResult ProgrammeEnrollment()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    string RegNo = Session["Username"].ToString();

                    #region Registered programmes
                    List<ProgEnrol> ProgEnrList = new List<ProgEnrol>();
                    string pageReg = "StudentEnrolment?$filter=Student_No eq '" + RegNo + "'&$format=json";

                    HttpWebResponse httpResponseReg = Credentials.GetOdataData(pageReg);
                    using (var streamReader = new StreamReader(httpResponseReg.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            ProgEnrol e = new ProgEnrol();
                            e.Prog = (string)config["Programme"];
                            e.ProgName = CommonClass.GetProgrammeName((string)config["Programme"]);
                            ProgEnrList.Add(e);
                        }
                    }
                    #endregion                 

                    return PartialView("~/Views/Alumni/Partial Views/ProgrammeEnrolment.cshtml", ProgEnrList);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult EmplHistList()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    string RegNo = Session["Username"].ToString();

                    #region Registered programmes
                    List<EmpHist> EmpList = new List<EmpHist>();
                    string pageReg = "StdEmpHist?$filter=Student_No eq '" + RegNo + "'&$format=json";

                    HttpWebResponse httpResponseReg = Credentials.GetOdataData(pageReg);
                    using (var streamReader = new StreamReader(httpResponseReg.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            EmpHist e = new EmpHist();
                            e.From = (string)config["From"];
                            e.To = (string)config["To"];
                            e.Company = (string)config["Organisation"];
                            e.Title = (string)config["Job_Title"];
                            EmpList.Add(e);
                        }
                    }
                    #endregion                 

                    return PartialView("~/Views/Alumni/Partial Views/EmploymentHistory.cshtml", EmpList);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult EducHistList()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    string RegNo = Session["Username"].ToString();

                    #region Educ Hist
                    List<QualHist> EmpList = new List<QualHist>();
                    string pageReg = "StdEduHistory?$filter=Student_No eq '" + RegNo + "'&$format=json";

                    HttpWebResponse httpResponseReg = Credentials.GetOdataData(pageReg);
                    using (var streamReader = new StreamReader(httpResponseReg.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            QualHist e = new QualHist();
                            e.From = (string)config["From"];
                            e.To = (string)config["To"];
                            e.Institution = (string)config["Organisation"];
                            e.Award = (string)config["Job_Title"];
                            EmpList.Add(e);
                        }
                    }
                    #endregion

                    return PartialView("~/Views/Alumni/Partial Views/QualificationHistory.cshtml", EmpList);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult NewEmplymentHist()
        {
            return PartialView("~/Views/Alumni/Partial Views/EmpHistForm.cshtml");
        }
        public ActionResult NewQualifHist()
        {
            return PartialView("~/Views/Alumni/Partial Views/QualHistForm.cshtml");
        }
        [HttpPost]
        public JsonResult SubmitEmpHistLine(EmpHist empHist)
        {
            try
            {
                string RegNo = Session["Username"].ToString();

                DateTime startDate = DateTime.ParseExact(empHist.From.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(empHist.To.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                Credentials.ObjNav.StudentEmploymentHistory(RegNo, empHist.Title, empHist.Company, startDate, endDate, "");

                return Json(new { message = "Line deleted Successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message.Replace("'", ""), success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult SubmitQualHistLine(QualHist qual)
        {
            try
            {
                string RegNo = Session["Username"].ToString();

                DateTime startDate = DateTime.ParseExact(qual.From.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(qual.To.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                Credentials.ObjNav.StudentEducationHistory(RegNo, "", qual.Institution, qual.Award, startDate, endDate, "");

                return Json(new { message = "Line deleted Successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message.Replace("'", ""), success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult RemoveEmpHistLine(string LineNo)
        {
            try
            {
                Credentials.ObjNav.DeleteStudentEmploymentHistory(Convert.ToInt32(LineNo));

                return Json(new { message = "Line deleted Successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message.Replace("'", ""), success = false }, JsonRequestBehavior.AllowGet);
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
                return PartialView("~/Views/Alumni/Steps/Step1.cshtml");
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
                return PartialView("~/Views/Alumni/Steps/Step2.cshtml");
            }
        }
        [HttpPost]
        public JsonResult SaveStepTwoData()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult StepThreeView()
        {
            return PartialView("~/Views/Alumni/Steps/Step3.cshtml");
        }
        [HttpPost]
        public JsonResult SaveStepThreeData()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult StepFourView()
        {
            return PartialView("~/Views/Alumni/Steps/Step4.cshtml");
        }
        [HttpPost]
        public JsonResult SaveStepFourData()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult StepFiveView()
        {
            return PartialView("~/Views/Alumni/Steps/Step5.cshtml");
        }
        [HttpPost]
        public JsonResult SaveStepFiveData()
        {
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult StudentRequisitions()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                }
                if (Session["StudentDetails"] == null)
                {
                    Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(RegNo);
                }
                string[] s = (string[])Session["StudentDetails"];
                string sem = Session["CurrentSem"].ToString();

                StudentCourseDetails stdDetail = new StudentCourseDetails
                {
                    StdNo = RegNo,
                    Name = s[0],
                    CurrentSem = sem,
                    Prog = s[1],
                    Email = s[2]
                };
                return View(stdDetail);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return View("~/Views/Common/ErrorMessage.cshtml", error);
            }
        }
        public PartialViewResult StudentRequisitionList()
        {
            try
            {
                List<StudentReqs> RegList = new List<StudentReqs>();
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                string page = "StudentRequisitions?$filter=Student_No eq '" + RegNo + "' and Requisition_Type ne 'Clearance'&$format=json";
                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            StudentReqs stdreq = new StudentReqs();
                            stdreq.Code = (string)config["Code"];
                            stdreq.Date = (string)config["Date"];
                            stdreq.Requisition_Type = (string)config["Requisition_Type"];
                            stdreq.Semester = (string)config["Semester"];
                            stdreq.Status = (string)config["Status"];
                            stdreq.ApprovalCount = (int)config["Approval_Count"];
                            stdreq.LinesCounter = (string)config["Lines_Count"];// GetRegLinesCounter((string)config["Code"]).ToString();
                            stdreq.ApprovalCount = (int)config["Approved_Lines_Count"];
                            stdreq.RejectedCount = (int)config["Rejected_Lines_Count"];
                            RegList.Add(stdreq);
                        }
                    }
                }

                return PartialView("~/Views/Course/StudentRequisitionList.cshtml", RegList.OrderByDescending(x => x.Code).ToList());
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public int GetRegLinesCounter(string DocNo)
        {
            int counter = 0;
            try
            {
                string pageLine = "StudentRequisitionLines?$select=Line_No&$count=true&$filter=Application_No eq '" + DocNo + "'&format=json";
                HttpWebResponse httpResponse = Credentials.GetOdataData(pageLine);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    counter = (int)details["@odata.count"];
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return counter;
        }
        public PartialViewResult NewRequistion()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }

                return PartialView("~/Views/Alumni/Partial Views/NewAcademicRequisition.cshtml");
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult OfficialTranscript()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
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
                            p.Text = CommonClass.GetProgrammeName((string)config["Programme"]);
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
                    return PartialView("~/Views/Course/Partial Views/OfficalTranscript.cshtml", STDProg);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult GetProgrammeTransit()
        {
            try
            {
                return PartialView("~/Views/Alumni/Partial Views/ProgrammeTransit.cshtml");
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public JsonResult GetProgrammeList(string Categ)
        {
            try
            {
                #region Programme List
                List<Programmes> ProgList = new List<Programmes>();
                string page = "ProgrammeList?$filter=Category eq '" + Categ + "' and OldCarriculum eq false and Description ne ''&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);


                    foreach (JObject config in details["value"])
                    {
                        Programmes PList = new Programmes();
                        PList.Code = (string)config["Code"];
                        PList.Description = (string)config["Description"];
                        ProgList.Add(PList);
                    }
                }
                #endregion
                ProgrammeList programmeList = new ProgrammeList
                {
                    ListOfProgrammes = ProgList.Select(x =>
                                     new SelectListItem()
                                     {
                                         Text = x.Description,
                                         Value = x.Code
                                     }).ToList()
                };
                return Json(new { programmeList, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}