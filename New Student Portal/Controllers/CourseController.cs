using Microsoft.Ajax.Utilities;
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
using New_Student_Portal.CustomSecurity;
using System.Web.UI.WebControls;

namespace New_Student_Portal.Controllers
{
    [CustomeAuthentication]
    [CustomAuthorization(Role = "STUD")]
    public class CourseController : Controller
    {
        // GET: Course
        public ActionResult CourseRegistration()
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
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester("");
                    }
                    if (Session["StudentDetails"] == null || Session["CurrentSem"].ToString() == "")
                    {
                        Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(RegNo);
                    }
                    string Prog = CommonClass.GetStudentRegisteredProgramme(RegNo);
                    string[] s = (string[])Session["StudentDetails"];
                    string sem = Session["CurrentSem"].ToString();
                    bool NotInTimeTable = false;
                    string Campus = CommonClass.GetStudentCampus(RegNo);
                    string pageReg = "StudentUnits?$select=Unit,Unit_Description,Unit_Class_Code&$filter=Student_No eq '" + RegNo + "' and Semester eq '" + sem + "' and In_Timetable eq false&$format=json";
                    List<TimeTableView> timeTable = new List<TimeTableView>();
                    HttpWebResponse httpResponse = Credentials.GetOdataData(pageReg);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        if (details["value"].Count() > 0)
                        {
                            foreach (JObject config in details["value"])
                            {
                                string pageTimetable = "Timetable?$filter=Unit eq '" + (string)config["Unit"] + "' and Semester eq '" + sem + "'&$format=json";

                                HttpWebResponse httpResponseTimeTable = Credentials.GetOdataData(pageTimetable);
                                using (var streamReaderTimeTable = new StreamReader(httpResponseTimeTable.GetResponseStream()))
                                {
                                    var resultTimeTable = streamReaderTimeTable.ReadToEnd();

                                    var detailsTimeTable = JObject.Parse(resultTimeTable);

                                    if (detailsTimeTable["value"].Count() > 0)
                                    {
                                        foreach (JObject config1 in detailsTimeTable["value"])
                                        {
                                            if (((string)config1["Campus_Code"] == Campus) || ((bool)config1["Multi_Campus"] == true))
                                            {
                                                TimeTableView tmTable = new TimeTableView();
                                                tmTable.Unit = (string)config1["Unit"];
                                                tmTable.Period = (string)config1["Period"];
                                                tmTable.Semester = (string)config1["Semester"];
                                                tmTable.Day_of_Week = (string)config1["DayofWeek"];
                                                tmTable.Lecture_Room = (string)config1["Lecture_Room"];
                                                tmTable.Lecturer = (string)config1["Lecturer_Name"];
                                                tmTable.Campus = (string)config1["Campus_Code"];
                                                tmTable.Section = (string)config1["Unit_Class"];
                                                timeTable.Add(tmTable);
                                                NotInTimeTable = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (NotInTimeTable)
                    {
                        return View("~/Views/Course/UnitsNotInTimeTable.cshtml", timeTable);
                    }
                    else
                    {
                        StudentCourseDetails stdDetail = new StudentCourseDetails();
                        return View(stdDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return View("~/Views/Common/ErrorMessage.cshtml", error);
            }
        }
        public ActionResult CourseRegistrationDetails()
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
                    if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester("");
                    }

                    string sem = Session["CurrentSem"].ToString();
                    //string Prog = CommonClass.GetStudentRegisteredProgramme(RegNo);

                    #region Registered programmes
                    RegisteredProgrammes newR = new RegisteredProgrammes();
                    string pageReg = "CustomerList?$filter=No eq '" + RegNo + "'&$format=json";

                    HttpWebResponse httpResponseReg = Credentials.GetOdataData(pageReg);
                    using (var streamReader = new StreamReader(httpResponseReg.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            newR.CurrentProg = (string)config["Current_Programme"];
                            newR.CurrentProgDesc = (string)config["Programme_Name"];
                            newR.OtherProg = (string)config["Other_Programme"];
                            newR.OtherProgDesc = (string)config["Major_Description"];
                            newR.SecondConc = (string)config["Concentration2"];
                            newR.SecondConcDesc = (string)config["Second_Conc_Description"];
                            newR.Minor = (string)config["Minor_Concentration"];
                            newR.MinorDesc = (string)config["Minor_Description"];
                        }
                    }
                    #endregion

                    #region Stage List
                    List<DropdownList> stageList = new List<DropdownList>();
                    string pageResC = "ProgrammeStages?$select=Code&$filter=Programme_Code eq '" + newR.CurrentProg + "'&$format=json";

                    HttpWebResponse httpResponseC = Credentials.GetOdataData(pageResC);
                    using (var streamReader = new StreamReader(httpResponseC.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        var details = JObject.Parse(result);
                        foreach (JObject config in details["value"])
                        {
                            DropdownList d = new DropdownList();
                            d.Value = (string)config["Code"];
                            d.Text = (string)config["Code"];
                            stageList.Add(d);
                        }
                    }
                    #endregion

                    #region Unit Category
                    //List<DropdownList> unitCList = new List<DropdownList>();
                    //List<DropdownList> unitCList1 = new List<DropdownList>();
                    //List<DropdownList> unitCList2 = new List<DropdownList>();
                    //List<DropdownList> unitCList3 = new List<DropdownList>();

                    //#region Current Prog
                    //string pageResC = "UnitSubject?$select=Unit_Category&$filter=ProgrammeCode eq '" + newR.CurrentProg + "' and Unit_Category ne ''&$format=json";

                    //HttpWebResponse httpResponseC = Credentials.GetOdataData(pageResC);
                    //using (var streamReader = new StreamReader(httpResponseC.GetResponseStream()))
                    //{
                    //    var result = streamReader.ReadToEnd();
                    //    var details = JObject.Parse(result);
                    //    foreach (JObject config in details["value"])
                    //    {
                    //        DropdownList d = new DropdownList();
                    //        d.Value = (string)config["Unit_Category"];
                    //        d.Text = (string)config["Unit_Category"];
                    //        unitCList.Add(d);
                    //    }
                    //}
                    //#endregion
                    //if (newR.OtherProg != "")
                    //{
                    //    #region Other Prog
                    //    string pageOtherProg = "MajorsConcentrationUnits?$select=Unit_Category&$filter=Concentration_Code eq '" + newR.OtherProg + "' and Unit_Category ne ''&$format=json";

                    //    HttpWebResponse httpResponseOtherProg = Credentials.GetOdataData(pageOtherProg);
                    //    using (var streamReader = new StreamReader(httpResponseOtherProg.GetResponseStream()))
                    //    {
                    //        var result = streamReader.ReadToEnd();
                    //        var details = JObject.Parse(result);
                    //        foreach (JObject config in details["value"])
                    //        {
                    //            DropdownList d = new DropdownList();
                    //            d.Value = (string)config["Unit_Category"];
                    //            d.Text = (string)config["Unit_Category"];
                    //            unitCList1.Add(d);
                    //        }
                    //    }
                    //    #endregion
                    //}
                    //if (newR.SecondConc != "")
                    //{
                    //    #region Second Conc Prog
                    //    string pageSecondConc = "MajorsConcentrationUnits?$select=Unit_Category&$filter=Concentration_Code eq '" + newR.SecondConc + "' and Unit_Category ne ''&$format=json";

                    //    HttpWebResponse httpResponseSecondConc = Credentials.GetOdataData(pageSecondConc);
                    //    using (var streamReader = new StreamReader(httpResponseSecondConc.GetResponseStream()))
                    //    {
                    //        var result = streamReader.ReadToEnd();
                    //        var details = JObject.Parse(result);
                    //        foreach (JObject config in details["value"])
                    //        {
                    //            DropdownList d = new DropdownList();
                    //            d.Value = (string)config["Unit_Category"];
                    //            d.Text = (string)config["Unit_Category"];
                    //            unitCList2.Add(d);
                    //        }
                    //    }
                    //    #endregion
                    //}
                    //if (newR.Minor != "")
                    //{
                    //    #region Minor Prog
                    //    string pageMinor = "MajorsConcentrationUnits?$select=Unit_Category&$filter=Concentration_Code eq '" + newR.Minor + "' and Unit_Category ne ''&$format=json";

                    //    HttpWebResponse httpResponseMinor = Credentials.GetOdataData(pageMinor);
                    //    using (var streamReader = new StreamReader(httpResponseMinor.GetResponseStream()))
                    //    {
                    //        var result = streamReader.ReadToEnd();
                    //        var details = JObject.Parse(result);
                    //        foreach (JObject config in details["value"])
                    //        {
                    //            DropdownList d = new DropdownList();
                    //            d.Value = (string)config["Unit_Category"];
                    //            d.Text = (string)config["Unit_Category"];
                    //            unitCList3.Add(d);
                    //        }
                    //    }
                    //    #endregion
                    //}
                    #endregion
                    #region Course Reg
                    List<CourseReg> CReg = new List<CourseReg>();
                    string page = "CourseReg?$filter=StudentNo eq '" + RegNo + "' and Programme eq '" + newR.CurrentProg + "' and Reversed eq false&format=json";

                    HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            CourseReg CR = new CourseReg();
                            CR.RegID = (string)config["RegTransactonID"];
                            CR.Programme = newR.CurrentProg;
                            CR.Stage = (string)config["Stage"];
                            CR.Semester = (string)config["Semester"];
                            CR.Register_for = (string)config["Registerfor"];
                            CR.Settlement_Type = (string)config["SettlementType"];
                            CR.Registration_Date = (string)config["RegDate"];
                            CR.UnitsTaken = (string)config["UnitsTaken"];
                            CR.UnpostedCharges = (string)config["UnPosted_Charges"];
                            CR.TotalBilled = (string)config["TotalBilled"];
                            CReg.Add(CR);
                        }
                    }
                    #endregion
                    #region Basket Units
                    List<StudentUnits> BasketUnits = new List<StudentUnits>();
                    string pageBasket = "BasketUnits?$filter=Student_No eq '" + RegNo + "' and Semester eq '" + sem + "' and Submitted eq false and Unit ne ''&$format=json";

                    HttpWebResponse httpResponseBasket = Credentials.GetOdataData(pageBasket);
                    using (var streamReader = new StreamReader(httpResponseBasket.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            StudentUnits CR = new StudentUnits();
                            CR.Programme = (string)config["Programme"];
                            CR.Unit = (string)config["Unit"];
                            CR.Unit_Name = (string)config["Unit_Name"];
                            CR.Stage = (string)config["Stage"];
                            CR.Semester = (string)config["Semester"];
                            BasketUnits.Add(CR);
                        }
                    }
                    #endregion

                    if (Session["StudentDetails"] == null || Session["CurrentSem"].ToString() == "")
                    {
                        Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(RegNo);
                    }
                    string[] s = (string[])Session["StudentDetails"];
                    string Hostel = CommonClass.GetHostelFromCourseReg(RegNo, sem);
                    StudentCourseRegistration StdCourseReg = new StudentCourseRegistration
                    {
                        ListOfCourseRegistration = CReg.OrderBy(x => x.Stage).ThenBy(n => n.Registration_Date),
                        ListOfStudentBasketUnits = BasketUnits,
                        RegProg = newR,
                        HostelCode = Hostel,
                        Campus = s[3],
                        ListOfStages = stageList.Select(x =>
                                               new SelectListItem()
                                               {
                                                   Text = x.Text,
                                                   Value = x.Value
                                               }).DistinctBy(x => x.Value).OrderBy(x => x.Value).ToList()
                    };

                    return PartialView("~/Views/Course/CourseRegistrationDetails.cshtml", StdCourseReg);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetProgrammeOptions(string Prog, string Stage)
        {
            try
            {
                DropdownListValues OptionList = new DropdownListValues();
                #region
                string page = "ProgrammeStages?$filter=Programme_Code eq '" + Prog + "' and Code eq '" + Stage + "' and Allow_Programme_Options eq true&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        #region Option List
                        List<DropdownList> OptionlList = new List<DropdownList>();
                        string pageOpt = "ProgrammeOption?$filter=Programme_Code eq '" + Prog + "'&$format=json";

                        HttpWebResponse httpResponseOpt = Credentials.GetOdataData(pageOpt);
                        using (var streamReaderOpt = new StreamReader(httpResponseOpt.GetResponseStream()))
                        {
                            var resultOpt = streamReaderOpt.ReadToEnd();

                            var detailsOpt = JObject.Parse(resultOpt);

                            foreach (JObject config in detailsOpt["value"])
                            {
                                DropdownList ddl = new DropdownList();
                                ddl.Value = (string)config["Code"];
                                ddl.Text = (string)config["Code"];
                                OptionlList.Add(ddl);
                            }
                        }
                        #endregion
                        OptionList = new DropdownListValues
                        {
                            ListOfValues = OptionlList.Select(x =>
                                            new SelectListItem()
                                            {
                                                Text = x.Text,
                                                Value = x.Value
                                            }).ToList()
                        };
                    }
                }
                #endregion               
                return Json(new { message = OptionList, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ViewStudentRegisteredUnits(StudentUnits Filters)
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
                    #region Reg Units
                    List<StudentUnits> regUnits = new List<StudentUnits>();
                    //string page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Reg_Transacton_ID eq '" + Filters.RegID + "' and Semester eq '" + Filters.Semester + "'&format=json";
                    string page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Register_for ne 'Supplementary' and Semester eq '" + Filters.Semester + "'&format=json";

                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            StudentUnits CR = new StudentUnits();
                            CR.Programme = (string)config["Programme"];
                            CR.Unit = (string)config["Unit"];
                            CR.Unit_Name = (string)config["Unit_Description"];
                            CR.Stage = (string)config["Stage"];
                            CR.Semester = (string)config["Semester"];
                            CR.RegFor = (string)config["Register_for"];
                            CR.Section = (string)config["Unit_Class_Code"];
                            CR.CF = (string)config["No__Of_Units"];
                            regUnits.Add(CR);
                        }
                    }
                    #endregion
                    string Sem = "";
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester("");
                    }

                    Sem = Session["CurrentSem"].ToString();
                    GetRegisteredUnit regUnitsList = new GetRegisteredUnit
                    {
                        RegDeadlineDate = CommonClass.RegistrationDeadline(Filters.Semester),
                        ListOfRegUnit = regUnits.DistinctBy(x => x.Unit).ToList()
                    };
                    return PartialView("~/Views/Course/ViewStudentRegisteredUnits.cshtml", regUnitsList);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult GetProgramUnits(string RegType, string Prog, string Stage, string Option)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "L.ogin");
                }
                else
                {
                    string RegNo = Session["Username"].ToString();

                    string PartialViewPath = "";
                    UnitSubject UnitSub = new UnitSubject();
                    List<CoreUnitSubject> CoreUnitSub = new List<CoreUnitSubject>();
                    Error errormsg = new Error();
                    bool showClass = false;
                    bool succ = false;

                    if (Session["Username"] == null)
                    {
                        Response.Redirect(Url.Action("Login", "Login"));
                    }

                    string Sem = "";
                    if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester("");
                    }

                    Sem = Session["CurrentSem"].ToString();

                    //Credentials.ObjNav.TestRegistrationStartDate(Sem);
                    string[] s = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                    if (s[0] == null || s[1] == null)
                    {
                        succ = Credentials.ObjNav.StudentSelfPromotion(RegNo, Prog,0);
                        s = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                    }

                    if (s[0] == null || s[1] == null)
                    {
                        errormsg.Message = "You have not been registered in the current semester";
                        PartialViewPath = "~/Views/Shared/Partial Views/ErroMessangeView.cshtml";
                        succ = false;
                    }
                    else
                    {
                        int maxUnits = 0, sUnits = 0;

                        #region Programme Units    

                        string Opt = "";
                        if (Option != null)
                        {
                            Opt = Option;
                        }
                        string page = "UnitsSubjects?$filter=Programme_Code eq '" + Prog + "' and Stage_Code eq '" + Stage + "' and (Programme_Option eq '' or Programme_Option eq '" + Opt + "') and Old_Unit eq false&$format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            if (details["value"].Count() > 0)
                            {
                                foreach (JObject config in details["value"])
                                {
                                    if (RegType == "3")
                                    {
                                        if (!StudentExistInStudentUnitBakset(RegNo, (string)config["Code"], Sem))
                                        {
                                            CoreUnitSubject NewUnit = new CoreUnitSubject();
                                            NewUnit.Code = (string)config["Code"];
                                            NewUnit.Desription = (string)config["Desription"];
                                            CoreUnitSub.Add(NewUnit);
                                        }
                                    }
                                    else
                                    {
                                        if (!StudentRegisteredUnitExists(RegNo, (string)config["Programme_Code"], (string)config["Code"], Sem))
                                        {
                                            CoreUnitSubject NewUnit = new CoreUnitSubject();
                                            NewUnit.Code = (string)config["Code"];
                                            NewUnit.Desription = (string)config["Desription"];
                                            CoreUnitSub.Add(NewUnit);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region Maximum Courses
                        string pageMax = "ProgrammeStages?$select=Maximum_Allowed_CF&$filter=Programme_Code eq '" + s[0] + "' and Code eq '" + s[1] + "'&format=json";

                        HttpWebResponse httpResponseMax = Credentials.GetOdataData(pageMax);
                        using (var streamReader = new StreamReader(httpResponseMax.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            if (details["value"].Count() > 0)
                            {
                                foreach (JObject config in details["value"])
                                {
                                    maxUnits = (int)config["Maximum_Allowed_CF"];
                                }
                            }
                        }
                        #endregion
                        #region Total Selected Courses
                        string pageSelected = "BasketUnits?$select=No_Of_Units&$filter=Student_No eq '" + RegNo + "' and Semester eq '" + Sem + "'&$format=json";

                        HttpWebResponse httpResponseSelected = Credentials.GetOdataData(pageSelected);
                        using (var streamReader = new StreamReader(httpResponseSelected.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            if (details["value"].Count() > 0)
                            {
                                foreach (JObject config in details["value"])
                                {
                                    sUnits = sUnits + (int)config["No_Of_Units"];
                                }
                            }
                        }
                        #endregion
                        UnitSub = new UnitSubject
                        {
                            MaximumCourses = maxUnits,
                            SelectedCourses = sUnits,
                            RegT = RegType,
                            ListOfCoreUnitsSubjects = CoreUnitSub.DistinctBy(x => new { x.Code, x.Day, x.Period, x.Class }).OrderBy(x => x.Code).ToList()
                        };
                        PartialViewPath = "~/Views/Course/GetProgramUnits.cshtml";
                        succ = true;
                    }
                    if (succ)
                    {
                        return PartialView(PartialViewPath, UnitSub);
                    }
                    else
                    {
                        return PartialView(PartialViewPath, errormsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Error errormsg = new Error();
                errormsg.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", errormsg);
            }
        }
        protected bool GetClassCode(string RegNo)
        {
            bool HasClass = false;
            try
            {
                string page = "CustomerList?$select=Class_Code&$filter=No eq '" + RegNo + "' and Class_Code ne ''&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);

                StudentDetailView Details = new StudentDetailView();

                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    if (details["value"].Count() > 0)
                    {
                        HasClass = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return HasClass;
        }
        protected bool UnitInTimeTable(string stdNo, string Prog, string Unit, string Semester)
        {
            bool s = false;
            try
            {
                string page = "Timetable?$filter=Unit eq '" + Unit + "' and Semester eq '" + Semester + "'&format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            bool r = PrerequisiteUnit(stdNo, Prog, Unit, Semester);
                            if (!r)
                            {
                                s = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        protected bool PrerequisiteUnit(string StdNo, string Prog, string Unit, string Sem)
        {
            bool s = false;
            try
            {
                string page = "UnitPrerequisite?$select=Prerequisite_Unit&$filter=Unit eq '" + Unit + "' and Prerequisite_Unit ne '" + Unit + "' and Prerequisite_Unit ne ''&format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            //has Prerequisite been done
                            s = HasPrerequisiteUnitBeenDone(StdNo, Prog, (string)config["Prerequisite"], Sem);
                            if (!s)
                            {
                                s = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        [HttpPost]
        public JsonResult GetPrerequisiteUnits(string Unit)
        {
            try
            {

                string UnitCodesMessage = "Prerequisite Unit(s)=", PrelUnitList = "";
                bool UnitFound = false;
                string RegNo = Session["Username"].ToString();
                string page = "UnitPrerequisite?$select=Prerequisite_Unit&$filter=Unit eq '" + Unit + "' and Prerequisite_Unit ne '" + Unit + "' and Prerequisite_Unit ne ''&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        int i = 1;
                        foreach (JObject config in details["value"])
                        {
                            string pageAudit = "StudentUnitsAudit?$select=Unit&$filter=StudentNo eq '" + RegNo + "' and Unit eq '" + (string)config["Prerequisite_Unit"] + "' and Progress_Status eq 'Future'&$format=json";
                            HttpWebResponse httpResponseAudit = Credentials.GetOdataData(pageAudit);
                            using (var streamReaderAudit = new StreamReader(httpResponseAudit.GetResponseStream()))
                            {
                                var resultAudit = streamReaderAudit.ReadToEnd();

                                var detailsAudit = JObject.Parse(resultAudit);

                                if (detailsAudit["value"].Count() > 0)
                                {
                                    if (i == 1)
                                    {
                                        PrelUnitList = (string)config["Prerequisite_Unit"];
                                    }
                                    else
                                    {
                                        PrelUnitList = PrelUnitList + "," + (string)config["Prerequisite_Unit"];
                                    }
                                    UnitFound = true;
                                }
                            }
                        }
                        UnitCodesMessage = UnitCodesMessage + PrelUnitList;
                    }
                }
                return Json(new { message = UnitCodesMessage, success = true, Found = UnitFound }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult TestStudentTimeTable(string Unit, string Campus, string ClassCode, string day, string period)
        {
            try
            {
                string RegNo = Session["Username"].ToString();
                string Sem = "";
                if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester("");
                }

                Sem = Session["CurrentSem"].ToString();
                Credentials.ObjNav.TestStudentTableConflict(RegNo, Unit, Sem, Campus, ClassCode, day, period);
                return Json(new { message = "", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        protected bool AllowOption(string Prog, string Stage)
        {
            bool s = false;
            try
            {
                string page = "ProgrammeStages?$filter=Programme_Code eq '" + Prog + "' and Code eq '" + Stage + "' and Allow_Programme_Options eq true&format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        s = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        public PartialViewResult GetProgramUnitsWithOptions(string Option, string Type)
        {
            try
            {
                UnitSubject UnitSub = new UnitSubject();
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                else
                {
                    string RegNo = Session["Username"].ToString();
                    string Sem = "";
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester("");
                    }

                    Sem = Session["CurrentSem"].ToString();

                    if (Session["CurrentProgDetails"] == null)
                    {
                        Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                    }
                    string[] s = (string[])Session["CurrentProgDetails"];


                    List<CoreUnitSubject> CoreUnitSub = new List<CoreUnitSubject>();
                    List<ElectiveUnitSubject> ElectiveUnitSub = new List<ElectiveUnitSubject>();

                    #region Programme Option Units
                    string page = "UnitsSubjects?$filter=Programme_Code eq '" + s[0] + "' and Stage_Code eq '" + s[1] + "' and (Programme_Option eq '' or Programme_Option eq '" + Option + "') and Old_Unit eq false&format=json";

                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            CoreUnitSubject NewUnit = new CoreUnitSubject();
                            ElectiveUnitSubject ElectiveNewUnit = new ElectiveUnitSubject();
                            if (!StudentRegisteredUnitExists(RegNo, (string)config["Programme_Code"], (string)config["Code"], Sem))
                            {
                                if ((string)config["Unit_Type"] == "Elective")
                                {
                                    ElectiveNewUnit.Code = (string)config["Code"];
                                    ElectiveNewUnit.Desription = (string)config["Desription"];
                                    ElectiveUnitSub.Add(ElectiveNewUnit);
                                }
                                else
                                {
                                    NewUnit.Code = (string)config["Code"];
                                    NewUnit.Desription = (string)config["Desription"];
                                    CoreUnitSub.Add(NewUnit);
                                }
                            }
                        }
                    }
                    #endregion

                    UnitSub = new UnitSubject
                    {
                        ListOfCoreUnitsSubjects = CoreUnitSub.DistinctBy(x => x.Code).ToList(),
                        ListOfElectiveUnitsSubjects = ElectiveUnitSub.DistinctBy(x => x.Code).ToList()

                    };
                }
                return PartialView("~/Views/Course/GetProgramUnits.cshtml", UnitSub);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        protected bool StudentRegisteredUnitExists(string stdNo, string prog, string Unit, string sem)
        {
            bool exist = false;
            try
            {
                exist = StudentExistInStudentUnits(stdNo, Unit);

                if (!exist)
                {
                    exist = StudentExistInStudentUnitBakset(stdNo, Unit, sem);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return exist;
        }
        protected bool StudentExistInStudentUnits(string stdNo, string Unit)
        {
            bool s = false;
            try
            {
                string page = "StudentUnits?$filter=Student_No eq '" + stdNo + "' and Unit eq '" + Unit + "'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        s = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        protected bool HasPrerequisiteUnitBeenDone(string stdNo, string Prog, string Unit, string Sem)
        {
            bool s = false;
            try
            {
                string page = "StudentUnits?$filter=Student_No eq '" + stdNo + "' and Programme eq '" + Prog + "' and Unit eq '" + Unit + "'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    int c = details["value"].Count();
                    if (details["value"].Count() > 0)
                    {
                        s = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        protected bool StudentExistInStudentUnitBakset(string stdNo, string Unit, string sem)
        {
            bool s = false;
            try
            {
                string pageBasket = "BasketUnits?$filter=Student_No eq '" + stdNo + "' and Unit eq '" + Unit + "' and Semester eq '" + sem + "'&$format=json";

                HttpWebResponse httpResponsebasket = Credentials.GetOdataData(pageBasket);
                using (var streamReader = new StreamReader(httpResponsebasket.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        s = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        [HttpPost]
        public JsonResult SaveSelectedUnits(List<UnitRegistration> UnitReg, string RegT)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                string Sem = "";
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester("");
                }

                Sem = Session["CurrentSem"].ToString();

                if (Session["CurrentProgDetails"] == null)
                {
                    Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                }
                string[] s = (string[])Session["CurrentProgDetails"];
                int i = 0;
                foreach (var c in UnitReg)
                {
                    string ClassCode = "", Campus = "", Day = "", Period = "";
                    string d = c.UnitCode.Trim();
                    if (c.ClassCode != null)
                    {
                        ClassCode = c.ClassCode.Trim();
                    }
                    if (c.Campus != null)
                    {
                        Campus = c.Campus.Trim();
                    }
                    if (c.Period != null)
                    {
                        Period = c.Period.Trim();
                    }
                    if (c.Day != null)
                    {
                        Day = c.Day.Trim();
                    }
                    Credentials.ObjNav.RegisterStudentUnitBasket(RegNo, Sem, "", "", d, Convert.ToInt32(RegT), ClassCode, Campus, Day, Period);
                    i++;
                }
                return Json(new { message = i.ToString() + " Units Selected successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DropBasketUnit(UnitFilters Filters)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                Credentials.ObjNav.DropUnitBasket(RegNo, Filters.Unit, Filters.sem);
                return Json(new { message = "Unit " + Filters.Unit + " dropped successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult RegisterSelectedUnits()
        {
            try
            {
                string msg = "";
                bool Redirect = false;

                if (Session["Username"] == null)
                {
                    msg = "/Login/Login";
                    Redirect = true;
                }
                else
                {
                    string RegNo = Session["Username"].ToString();
                    string Sem = "";
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester("");
                    }

                    Sem = Session["CurrentSem"].ToString();

                    if (Session["CurrentProgDetails"] == null)
                    {
                        Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                    }
                    string[] s = (string[])Session["CurrentProgDetails"];

                    bool paymntPlan = Credentials.ObjNav.RegisterStudentUnits(RegNo, Sem, "", s[0], 0, false);


                    if (paymntPlan)
                    {
                        Session["SuccessMsg"] = "Units Registered and payment plan created successfully";
                        msg = "/Financial/PaymentPlan";
                        Redirect = true;
                    }
                    else
                    {
                        msg = "Units Registered successfully";
                    }
                }
                return Json(new { message = msg, success = true, Redirect = Redirect }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        private bool InsertPaymentPlan(string STDNo, string Sem, decimal StdBal)
        {
            bool s = false;
            try
            {
                string page = "StudentPaymentPlanSetup?$select=Class_Code&$filter=Semester eq '" + Sem + "'&$orderby=Installment_No asc&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);

                StudentDetailView Details = new StudentDetailView();

                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    if (details["value"].Count() > 0)
                    {
                        decimal previousPerc = 0, ActualPerc = 0, AmountDue = 0;
                        foreach (JObject config in details["value"])
                        {
                            string installNo = (string)config["Installment_No"];
                            string byDate = (string)config["Due_Date"];
                            string perc = (string)config["Installment_Percentage"];

                            if (byDate != "" && perc != "")
                            {
                                ActualPerc = Convert.ToDecimal(perc) - previousPerc;
                                AmountDue = StdBal * (ActualPerc / 100);
                                DateTime Dby = DateTime.ParseExact(byDate.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                Credentials.ObjNav.InsertStudentPaymentPlan(STDNo, Dby, Sem, installNo, Convert.ToDecimal(perc), AmountDue);
                                previousPerc = Convert.ToDecimal(perc);
                                installNo = "";
                                byDate = "";
                                perc = "";
                            }
                        }
                        s = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        public JsonResult DropRegisteredUnit(UnitFilters Filters)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                string Prog = "";
                if (Filters.prog != null)
                {
                    Prog = Filters.prog;
                }
                Credentials.ObjNav.DropStudentUnits(RegNo.Trim(), Filters.sem.Trim(), "", Prog, Filters.Unit.Trim(), false);
                return Json(new { message = "Unit " + Filters.UnitName + " dropped successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult StudentTimeTable()
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
                    Credentials.ObjNav.RefreshStudentAudit(RegNo);
                    List<TimeTableView> timeTableR = new List<TimeTableView>();
                    List<TimeTableView> timeTableF = new List<TimeTableView>();


                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester("");
                    }
                    if (Session["StudentDetails"] == null)
                    {
                        Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(RegNo);
                    }
                    string[] s = (string[])Session["StudentDetails"];
                    string sem = Session["CurrentSem"].ToString();
                    string Campus = CommonClass.GetStudentCampus(RegNo);

                    string pageReg = "StudentUnits?$select=Unit,Unit_Description,Unit_Class_Code,Campus&$filter=Student_No eq '" + RegNo + "' and Semester eq '" + sem + "'&$format=json";

                    HttpWebResponse httpResponse = Credentials.GetOdataData(pageReg);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        if (details["value"].Count() > 0)
                        {
                            foreach (JObject config in details["value"])
                            {
                                string pageTimetable = "Timetable?$filter=Unit eq '" + (string)config["Unit"] + "' and Semester eq '" + sem + "'&$format=json";
                                //string pageTimetable = "Timetable?$filter=Unit eq '" + (string)config["Unit"] + "' and Semester eq '" + sem + "' and Campus_Code eq '" + Campus + "'&$format=json";

                                HttpWebResponse httpResponseTimeTable = Credentials.GetOdataData(pageTimetable);
                                using (var streamReaderTimeTable = new StreamReader(httpResponseTimeTable.GetResponseStream()))
                                {
                                    var resultTimeTable = streamReaderTimeTable.ReadToEnd();

                                    var detailsTimeTable = JObject.Parse(resultTimeTable);

                                    if (detailsTimeTable["value"].Count() > 0)
                                    {
                                        foreach (JObject config1 in detailsTimeTable["value"])
                                        {
                                            //if (((string)config1["Campus_Code"] == Campus) || ((bool)config1["Multi_Campus"] == true))
                                            //{
                                            TimeTableView tmTable = new TimeTableView();
                                            tmTable.Unit = (string)config1["Unit"];
                                            tmTable.Period = (string)config1["Period"];
                                            tmTable.Semester = (string)config1["Semester"];
                                            tmTable.Day_of_Week = (string)config1["Day_of_Week"];
                                            tmTable.Lecture_Room = (string)config1["Lecture_Room"];
                                            tmTable.Lecturer = CommonClass.GetEmployeeName((string)config1["Lecturer"]);
                                            tmTable.Campus = (string)config1["AuxiliaryIndex3"];
                                            tmTable.Section = (string)config1["AuxiliaryIndex1"];
                                            tmTable.Registered = "Registered";
                                            timeTableR.Add(tmTable);
                                            //}
                                        }
                                    }
                                }
                            }
                        }
                    }
                    string page = "StudentUnitsAudit?$select=Unit,Description,Progress_Status&$filter=StudentNo eq '" + RegNo + "' and Progress_Status eq 'Future'&$format=json";

                    HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        if (details["value"].Count() > 0)
                        {
                            foreach (JObject config in details["value"])
                            {
                                string pageTimetable = "Timetable?$filter=Unit eq '" + (string)config["Unit"] + "' and Semester eq '" + sem + "'&$format=json";

                                HttpWebResponse httpResponseTimeTable = Credentials.GetOdataData(pageTimetable);
                                using (var streamReaderTimeTable = new StreamReader(httpResponseTimeTable.GetResponseStream()))
                                {
                                    var resultTimeTable = streamReaderTimeTable.ReadToEnd();

                                    var detailsTimeTable = JObject.Parse(resultTimeTable);

                                    if (detailsTimeTable["value"].Count() > 0)
                                    {
                                        foreach (JObject config1 in detailsTimeTable["value"])
                                        {
                                            //if (((string)config1["Campus_Code"] == Campus) || ((bool)config1["Multi_Campus"] == true))
                                            //{
                                            TimeTableView tmTable = new TimeTableView();
                                            tmTable.Unit = (string)config1["Unit"];
                                            tmTable.Period = (string)config1["Period"];
                                            tmTable.Semester = (string)config1["Semester"];
                                            tmTable.Day_of_Week = (string)config1["Day_of_Week"];
                                            tmTable.Lecture_Room = (string)config1["Lecture_Room"];
                                            tmTable.Lecturer = CommonClass.GetEmployeeName((string)config1["Lecturer"]);
                                            tmTable.Campus = (string)config1["AuxiliaryIndex3"];
                                            tmTable.Section = (string)config1["AuxiliaryIndex1"];
                                            tmTable.Registered = "Future";
                                            timeTableF.Add(tmTable);
                                            //}
                                        }
                                    }
                                }
                            }
                        }
                    }

                    TimeTableSummeryView newSummery = new TimeTableSummeryView
                    {
                        RegisteredUnitsTimeT = timeTableR,
                        FutureUnitsTimeT = timeTableF
                    };
                    return View(newSummery);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return View("~/Views/Common/ErrorMessage.cshtml", error);
            }
        }
        public ActionResult LecturerEvaluation()
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
        public PartialViewResult CurrentRegistredUnits()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                string Sem = "";
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester("");
                }
                Sem = Session["CurrentSem"].ToString();

                #region Reg Units
                List<StudentUnits> regUnits = new List<StudentUnits>();
                string page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Semester eq '" + Sem + "' and Evaluated eq false&format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        StudentUnits CR = new StudentUnits();
                        CR.Unit = (string)config["Unit"];
                        CR.Unit_Name = (string)config["Unit_Description"];
                        CR.Section = (string)config["Unit_Class_Code"];
                        regUnits.Add(CR);
                    }
                }
                #endregion
                LecturerEvaluationUnits unitsToEvaluate = new LecturerEvaluationUnits
                {
                    ListOfRegisteredUnits = regUnits
                };

                return PartialView("~/Views/Course/CurrentRegisteredUnits.cshtml", unitsToEvaluate);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public PartialViewResult LecturerEvaluationForm(string Unit, string Sec)
        {
            try
            {
                string RegNo = Session["Username"].ToString();
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester("");
                }
                string sem = Session["CurrentSem"].ToString();
                string[] s = CommonClass.CurrentCourseRegistration(RegNo, sem);
                Lecturer lec = new Lecturer();
                Error error = new Error();
                //string page = "LectAllocatedUnits?$filter=Code eq '" + s[0] + "' and Stage eq '" + s[1] + "' and Semester eq '" + sem + "' and Unit eq '" + Unit + "'&$format=json";
                // string page = "Timetable?$filter=Semester eq '" + sem + "' and Unit eq '" + Unit + "' and Unit_Class eq '" + Sec + "'&$format=json";
                string page = "Timetable?$filter=Semester eq '" + sem + "' and Unit eq '" + Unit + "'&$format=json";
                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            lec.LecNo = (string)config["Lecturer"];
                            lec.LecName = (string)config["Lecturer_Name"];
                            lec.Unit = (string)config["Unit"];
                            lec.UnitName = (string)config["Unit_Description"];
                        }
                        return PartialView("~/Views/Course/LecturerEvaluationForm.cshtml", lec);
                    }
                    else
                    {
                        error.Message = "No lecturer assigned this unit";
                        return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester("");
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
                            stdreq.ApprovalCount = (int)config["Approval_Count"];
                            stdreq.LinesCounter = GetRegLinesCounter((string)config["Code"]).ToString();
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

                return PartialView("~/Views/Course/NewAcademicRequisition.cshtml");
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveEvalueatedUnit(Lecturer Lec, List<LecEvaluationQuiz> lecQuiz)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                string Sem = "";
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester("");
                }
                Sem = Session["CurrentSem"].ToString();

                if (Session["CurrentProgDetails"] == null)
                {
                    Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                }
                string[] s = (string[])Session["CurrentProgDetails"];

                Credentials.ObjNav.LecturerEvaluationHeader(RegNo, Lec.Unit, Sem, s[1], Lec.LecNo,
                    s[0], Lec.LecName, Lec.Comments);
                foreach (var c in lecQuiz)
                {
                    string[] que = c.Quiz.Trim().Split('.');
                    int quizC = Convert.ToInt32(c.QuizCategory.Trim());
                    string quiz = que[1].Trim();
                    decimal score = Convert.ToDecimal(c.Score.Trim());
                    Credentials.ObjNav.LecturerEvaluationCreate(RegNo, Lec.Unit, Sem, Lec.LecNo, Lec.LecName, quiz, "",
                        s[0], score, quizC, "");
                }
                return Json(new { message = "Unit " + Lec.UnitName + " Evaluated successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetCampusList()
        {
            try
            {
                #region Campus List
                List<DimensionValues> Campuses = new List<DimensionValues>();
                string pageCampus = "DimensionValues?$filter=Global_Dimension_No_ eq 1&$format=json";

                HttpWebResponse httpResponseCampus = Credentials.GetOdataData(pageCampus);
                using (var streamReader = new StreamReader(httpResponseCampus.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);


                    foreach (JObject config in details["value"])
                    {
                        DimensionValues CmpList = new DimensionValues();
                        CmpList.Code = (string)config["Code"];
                        CmpList.Name = (string)config["Name"];
                        Campuses.Add(CmpList);
                    }
                }
                #endregion
                CampusList CampusData = new CampusList
                {
                    ListOfCampus = Campuses.Select(x =>
                                       new SelectListItem()
                                       {
                                           Text = x.Name,
                                           Value = x.Code
                                       }).ToList()
                };
                return Json(CampusData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GetCurrentRegisteredUnits(string RegType)
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
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester("");
                    }
                    string sem = Session["CurrentSem"].ToString();

                    string Prog = CommonClass.GetStudentRegisteredProgramme(RegNo);

                    List<StudentUnits> regUnits = new List<StudentUnits>();
                    if (RegType == "8")
                    {
                        #region Reg Units
                        string page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Grade eq 'X'" +
                           " and Programme eq '" + Prog + "'&$format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            foreach (JObject config in details["value"])
                            {
                                string pageTimetable = "Timetable?$filter=Unit eq '" + (string)config["Unit"] + "' and Semester eq '" + sem + "'&$format=json";

                                HttpWebResponse httpResponseTmT = Credentials.GetOdataData(pageTimetable);
                                using (var streamReaderTmT = new StreamReader(httpResponseTmT.GetResponseStream()))
                                {
                                    var resultTmT = streamReaderTmT.ReadToEnd();

                                    var detailsTmT = JObject.Parse(resultTmT);

                                    if (detailsTmT["value"].Count() > 0)
                                    {
                                        foreach (JObject config1 in detailsTmT["value"])
                                        {
                                            StudentUnits CR = new StudentUnits();
                                            CR.Programme = (string)config["Programme"];
                                            CR.Unit = (string)config["Unit"];
                                            CR.Unit_Name = (string)config["Unit_Description"];
                                            CR.Stage = (string)config["Stage"];
                                            CR.Semester = (string)config["Semester"];
                                            CR.RegFor = (string)config["Register_for"];
                                            regUnits.Add(CR);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    if (RegType == "11" || RegType == "20")
                    {
                        #region Reg Units
                        string page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Semester eq '" + sem + "'" +
                           " and Programme eq '" + Prog + "'&$format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            foreach (JObject config in details["value"])
                            {
                                StudentUnits CR = new StudentUnits();
                                CR.Programme = (string)config["Programme"];
                                CR.Unit = (string)config["Unit"];
                                CR.Unit_Name = (string)config["Unit_Description"];
                                CR.Stage = (string)config["Stage"];
                                CR.Semester = (string)config["Semester"];
                                CR.RegFor = (string)config["Register_for"];
                                regUnits.Add(CR);
                            }
                        }
                        #endregion
                    }

                    GetRegisteredUnit regUnitsList = new GetRegisteredUnit
                    {
                        ListOfRegUnit = regUnits.DistinctBy(x => x.Unit).ToList()
                    };
                    return PartialView("~/Views/Course/CourseRequisitionLines.cshtml", regUnitsList);
                }
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
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitStudentProgrammeChangeRequest(StudentReqs regData, string base64Upload, string fileName, string Extn)
        {
            try
            {
                string msg = "";
                bool sucV = false;
                if (base64Upload != "" && Extn != "")
                {
                    string ext = Path.GetExtension(fileName);

                    if (ext.ToLower() == ".pdf" || ext.ToLower() == ".docx" || ext.ToLower() == ".doc" || ext.ToLower() == ".xlsx" ||
                        ext.ToLower() == ".jpeg" || ext.ToLower() == ".jpg" || ext.ToLower() == ".png")
                    {
                        string RegNo = "", Sem = "", Remarks = "", prog = "";
                        int r = 0;
                        bool SendFApp = true;
                        if (regData.StdNo != null)
                        {
                            RegNo = regData.StdNo;
                        }
                        if (regData.Semester != null)
                        {
                            Sem = regData.Semester;
                        }
                        if (regData.Requisition_Type != null)
                        {
                            r = Convert.ToInt32(regData.Requisition_Type.Trim());
                        }
                        if (regData.Remarks != null)
                        {
                            Remarks = regData.Remarks.Trim();
                        }
                        if (regData.Prog != null)
                        {
                            prog = regData.Prog.Trim();
                        }

                        string DocNo = "";// Credentials.ObjNav.StudentRequisitionCreate(RegNo, r, Remarks, prog, "", Sem, "", 0, "", SendFApp, 0);

                        string filePath = Server.MapPath("~/Uploads/" + fileName);
                        string s = Credentials.UploadDocumentAttachment(DocNo, base64Upload, filePath, 70134894);
                        if (s == "SUCCESS")
                        {
                            msg = "Requisition document No : " + DocNo + " Submited successfully";
                            sucV = true;
                        }
                        else
                        {
                            msg = s;
                            sucV = false;
                        }
                    }
                    else
                    {
                        msg = "Only files with extensions(.pdf, .docx, .doc, .xlsx, .jpeg, .jpg, .png) can be uploaded";
                        sucV = false;
                    }
                }
                else
                {
                    msg = "Attach results slip";
                    sucV = false;
                }
                return Json(new { message = msg, success = sucV }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveStudentRequisition(StudentReqs regData)
        {
            try
            {
                string RegNo = "", Sem = "", Remarks = "", prog = "", Concentration = "", Campus = "";
                int r = 0, NoOfCopies = 0;
                bool SendFApp = false;

                if (regData.StdNo != null)
                {
                    RegNo = regData.StdNo;
                }
                if (regData.Semester != null)
                {
                    Sem = regData.Semester;
                }
                if (regData.Requisition_Type != null)
                {
                    r = Convert.ToInt32(regData.Requisition_Type.Trim());
                }
                if (regData.NoOfCopies != null && regData.NoOfCopies != "")
                {
                    NoOfCopies = Convert.ToInt32(regData.NoOfCopies);
                }
                if (regData.Remarks != null)
                {
                    Remarks = regData.Remarks.Trim();
                }
                if (regData.Prog != null)
                {
                    prog = regData.Prog.Trim();
                }
                if (regData.Campus != null)
                {
                    Campus = regData.Campus.Trim();
                }
                if (regData.Concentration != null)
                {
                    Concentration = regData.Concentration.Trim();
                }
                if (r == 2 || r == 12 || r == 13 || r == 14)
                {
                    SendFApp = true;
                }
                string DocNo = "";// Credentials.ObjNav.StudentRequisitionCreate(RegNo, r, Remarks, prog, "", Sem, Campus, 0, Concentration, SendFApp, NoOfCopies);

                return Json(new { message = "Requisition document No : " + DocNo + " Submited successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveStudentRequisitionWithLines(StudentReqs regData, List<StudentReqLines> StdRegLines)
        {
            try
            {
                string RegNo = "", Sem = "", Remarks = "", prog = "";
                int r = 0;
                bool SendFApp = false;
                if (regData.StdNo != null)
                {
                    RegNo = regData.StdNo;
                }
                if (regData.Semester != null)
                {
                    Sem = regData.Semester;
                }
                if (regData.Requisition_Type != null)
                {
                    r = Convert.ToInt32(regData.Requisition_Type.Trim());
                }
                if (regData.Remarks != null)
                {
                    Remarks = regData.Remarks.Trim();
                }
                if (regData.Prog != null)
                {
                    prog = regData.Prog.Trim();
                }
                if (r == 8 || r == 12 || r == 13 || r == 14)
                {
                    SendFApp = true;
                }
                if (r == 8 || r == 18)
                {
                    foreach (var c in StdRegLines)
                    {
                        string Unit = c.Unit.Trim();
                        string UnitName = c.UnitName.Trim();
                        string Reason = Remarks;

                        //Credentials.ObjNav.StudentRequisitionLinesInsertCreateDoc(RegNo, Sem, Unit, UnitName, Reason, r, "");
                    }
                    //Credentials.ObjNav.SendStudentRegBacthApproval(RegNo, Sem, r);
                }
                else
                {
                    string DocNo = "";// Credentials.ObjNav.StudentRequisitionCreate(RegNo, r, Remarks, prog, "", Sem, "", 0, "", SendFApp, 0);
                    foreach (var c in StdRegLines)
                    {
                        string Unit = c.Unit.Trim();
                        string UnitName = c.UnitName.Trim();
                        string Reason = "";
                        string EquiUnit = "";
                        if (c.EquivalentUnit != null)
                        {
                            EquiUnit = c.EquivalentUnit;
                        }
                        Credentials.ObjNav.StudentRequisitionLinesCreate(DocNo, Unit, UnitName, Reason, EquiUnit);
                    }
                }
                return Json(new { message = "Requisition Submited successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitStudentExemptionRequest(StudentReqs regData, List<StudentReqLines> StdRegLines, string base64Upload, string fileName, string Extn)
        {
            try
            {
                string msg = "";
                bool sucV = false;
                if (base64Upload != "" && Extn != "")
                {
                    string ext = Path.GetExtension(fileName);

                    if (ext.ToLower() == ".pdf" || ext.ToLower() == ".docx" || ext.ToLower() == ".doc" || ext.ToLower() == ".xlsx" ||
                        ext.ToLower() == ".jpeg" || ext.ToLower() == ".jpg" || ext.ToLower() == ".png")
                    {
                        string RegNo = "", Sem = "", Remarks = "", prog = "";
                        int r = 0;
                        if (regData.StdNo != null)
                        {
                            RegNo = regData.StdNo;
                        }
                        if (regData.Semester != null)
                        {
                            Sem = regData.Semester;
                        }
                        if (regData.Requisition_Type != null)
                        {
                            r = Convert.ToInt32(regData.Requisition_Type.Trim());
                        }
                        if (regData.Remarks != null)
                        {
                            Remarks = regData.Remarks.Trim();
                        }
                        if (regData.Prog != null)
                        {
                            prog = regData.Prog.Trim();
                        }
                        foreach (var c in StdRegLines)
                        {
                            string Unit = c.Unit.Trim();
                            string UnitName = c.UnitName.Trim();
                            string Reason = Remarks;
                            string EquiUnit = "";
                            if (c.EquivalentUnit != null)
                            {
                                EquiUnit = c.EquivalentUnit;
                            }
                            //Credentials.ObjNav.StudentRequisitionLinesInsertCreateDoc(RegNo, Sem, Unit, UnitName, Reason, r, EquiUnit);
                        }

                        string filePath = Server.MapPath("~/Uploads/" + fileName);

                        string page = "StudentRequisitions?$select=Code&$filter=Student_No eq '" + RegNo + "' and Requisition_Type eq 'Exemption' and Semester eq '" + Sem
                            + "' and Status eq 'Open'&$format=json";
                        HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            if (details["value"].Count() > 0)
                            {
                                foreach (JObject config in details["value"])
                                {
                                    Credentials.UploadDocumentAttachment((string)config["Code"], base64Upload, filePath, 70134894);
                                }
                                msg = "Requisition Submited successfully";
                                sucV = true;
                            }
                            else
                            {
                                msg = "You have already requested for the Units";
                                sucV = false;
                            }
                        }
                        //Credentials.ObjNav.SendStudentRegBacthApproval(RegNo, Sem, r);

                        //string DocNo = Credentials.ObjNav.StudentRequisitionCreate(RegNo, r, Remarks, prog, "", Sem, "", 0, "", SendFApp);
                        //foreach (var c in StdRegLines)
                        //{
                        //    string Unit = c.Unit.Trim();
                        //    string UnitName = c.UnitName.Trim();
                        //    string Reason = "";
                        //    string EquiUnit = "";
                        //    if (c.EquivalentUnit != null)
                        //    {
                        //        EquiUnit = c.EquivalentUnit;
                        //    }
                        //    Credentials.ObjNav.StudentRequisitionLinesCreate(DocNo, Unit, UnitName, Reason, EquiUnit);
                        //}

                        //string filePath = Server.MapPath("~/Uploads/" + fileName);
                        //string s = Credentials.UploadDocumentAttachment(DocNo, base64Upload, filePath, 70134894);
                        //if (s == "SUCCESS")
                        //{
                        //    msg = "Requisition document No : " + DocNo + " Submited successfully";
                        //    sucV = true;
                        //}
                        //else
                        //{
                        //    msg = s;
                        //    sucV = false;
                        //}
                    }
                    else
                    {
                        msg = "Only files with extensions(.pdf, .docx, .doc, .xlsx, .jpeg, .jpg, .png) can be uploaded";
                        sucV = false;
                    }
                }
                else
                {
                    msg = "Attach results slip";
                    sucV = false;
                }
                return Json(new { message = msg, success = sucV }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitStudentExamChallengeRequest(StudentReqs regData, List<StudentReqLines> StdRegLines, string base64Upload, string fileName, string Extn)
        {
            try
            {
                string msg = "";
                bool sucV = false;
                if (base64Upload != "" && Extn != "")
                {
                    string ext = Path.GetExtension(fileName);

                    if (ext.ToLower() == ".pdf" || ext.ToLower() == ".docx" || ext.ToLower() == ".doc" || ext.ToLower() == ".xlsx" ||
                        ext.ToLower() == ".jpeg" || ext.ToLower() == ".jpg" || ext.ToLower() == ".png")
                    {
                        string RegNo = "", Sem = "", Remarks = "", prog = "";
                        int r = 0;
                        if (regData.StdNo != null)
                        {
                            RegNo = regData.StdNo;
                        }
                        if (regData.Semester != null)
                        {
                            Sem = regData.Semester;
                        }
                        if (regData.Requisition_Type != null)
                        {
                            r = Convert.ToInt32(regData.Requisition_Type.Trim());
                        }
                        if (regData.Remarks != null)
                        {
                            Remarks = regData.Remarks.Trim();
                        }
                        if (regData.Prog != null)
                        {
                            prog = regData.Prog.Trim();
                        }

                        foreach (var c in StdRegLines)
                        {
                            string Unit = c.Unit.Trim();
                            string UnitName = c.UnitName.Trim();
                            string Reason = Remarks;
                            string EquiUnit = "";
                            if (c.EquivalentUnit != null)
                            {
                                EquiUnit = c.EquivalentUnit;
                            }
                            //Credentials.ObjNav.StudentRequisitionLinesInsertCreateDoc(RegNo, Sem, Unit, UnitName, Reason, r, EquiUnit);
                        }

                        string filePath = Server.MapPath("~/Uploads/" + fileName);

                        string page = "StudentRequisitions?$select=Code&$filter=Student_No eq '" + RegNo + "' and Requisition_Type eq 'Exam Challenge' and Semester eq '" + Sem
                            + "' and Status eq 'Open'&$format=json";
                        HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            if (details["value"].Count() > 0)
                            {
                                foreach (JObject config in details["value"])
                                {
                                    Credentials.UploadDocumentAttachment((string)config["Code"], base64Upload, filePath, 70134894);
                                }
                                msg = "Requisition Submited successfully";
                                sucV = true;
                            }
                            else
                            {
                                msg = "You have already requested for the Units";
                                sucV = false;
                            }
                        }

                        //Credentials.ObjNav.SendStudentRegBacthApproval(RegNo, Sem, r);
                    }
                    else
                    {
                        msg = "Only files with extensions(.pdf, .docx, .doc, .xlsx, .jpeg, .jpg, .png) can be uploaded";
                        sucV = false;
                    }
                }
                else
                {
                    msg = "Attach results slip";
                    sucV = false;
                }
                return Json(new { message = msg, success = sucV }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult GraduationRequest()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    bool GradAllowed = CommonClass.AllowGraduationApplication();
                    if (GradAllowed)
                    {
                        string RegNo = Session["Username"].ToString();
                        bool AllowApplication = CommonClass.AllowGradClearanceApplication(RegNo);
                        if (!AllowApplication)
                        {
                            Error errormsg = new Error();
                            errormsg.Message = "You are not in the graduating List";
                            return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                        }
                        else
                        {
                            StudentDetailView Details = new StudentDetailView();
                            #region Graduation Header
                            string pageGradReq = "GraduationRequest?$filter=StudentNo eq '" + RegNo + "'&format=json";

                            HttpWebResponse httpResponseGradReq = Credentials.GetOdataData(pageGradReq);
                            using (var streamReaderGradReq = new StreamReader(httpResponseGradReq.GetResponseStream()))
                            {
                                var resultGradReq = streamReaderGradReq.ReadToEnd();

                                var detailsGradReq = JObject.Parse(resultGradReq);
                                string ProgCode = "";
                                if (detailsGradReq["value"].Count() > 0)
                                {
                                    foreach (JObject config in detailsGradReq["value"])
                                    {
                                        Details.DocNo = (string)config["Code"];
                                        Details.No = (string)config["StudentNo"];
                                        Details.Name = (string)config["Names"];
                                        Details.ID_No = (string)config["IDNumber"];
                                        Details.Phone_No = (string)config["Telephone"];
                                        Details.Address = (string)config["Address"];
                                        Details.E_Mail = (string)config["Email"];
                                        Details.Balance = CommonClass.GetStudentBalance(RegNo);
                                        ProgCode = (string)config["Programme"];
                                        if (ProgCode == "")
                                        {
                                            ProgCode = CommonClass.GetStudentRegisteredProgramme(RegNo);
                                        }
                                        Details.Prog = ProgCode;
                                        Details.ProgName = CommonClass.GetProgrammeName(ProgCode);
                                        Details.PersonalMail = (string)config["PersonalEmail"];
                                        Details.Profession = (string)config["Currentprofession"];
                                        Details.Company = (string)config["CurrentInstitustionCompany"];
                                        Details.CurrentPhoneNo = (string)config["Current_Phone_No"];
                                        Details.Gown = (string)config["Gown_Required"];
                                        Details.CollectionPoint = (string)config["Gown_Collection_Campus"];
                                        Details.MadeRequest = true;
                                    }
                                }
                                else
                                {
                                    #region Graduation Request Header
                                    string page = "CustomerList?$filter=No eq '" + RegNo + "'&format=json";

                                    HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                                    using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                                    {
                                        var result = streamReader.ReadToEnd();

                                        var details = JObject.Parse(result);
                                        foreach (JObject config in details["value"])
                                        {
                                            Details.No = (string)config["No"];
                                            Details.Name = (string)config["Name"];
                                            Details.ID_No = (string)config["ID_No"];
                                            Details.Phone_No = (string)config["Phone_No"];
                                            Details.Address = (string)config["Address"];
                                            Details.E_Mail = (string)config["E_Mail"];
                                            ProgCode = (string)config["Student_Programme"];
                                            if (ProgCode == "")
                                            {
                                                ProgCode = (string)config["Current_Programme"];
                                            }
                                            if (ProgCode == "")
                                            {
                                                ProgCode = (string)config["Current_Program"];
                                            }
                                            if (ProgCode == "")
                                            {
                                                ProgCode = CommonClass.GetStudentRegisteredProgramme(RegNo);
                                            }
                                            Details.Prog = ProgCode;
                                            Details.ProgName = CommonClass.GetProgrammeName(ProgCode);
                                            Details.MadeRequest = false;
                                        }
                                    }
                                    #endregion
                                }
                            }
                            #endregion
                            return View(Details);
                        }
                    }
                    else
                    {
                        Error errormsg = new Error();
                        errormsg.Message = "Graduation request not active at the moment";
                        return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Error errormsg = new Error();
                errormsg.Message = ex.Message;
                return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
            }
        }
        public ActionResult GraduationRequestInfo(string DocNo, decimal Bal)
        {
            try
            {
                GradRequestInfomation inf = new GradRequestInfomation();
                inf.ReqNo = DocNo;
                inf.Balance = Bal;
                return PartialView("~/Views/Shared/Partial Views/GraduationReqInfo.cshtml", inf);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitGraduationRequest(GradRequest regData)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();

                string PEmail = "", currentProf = "", Company = "", PhoneNo = "", CollectionPoint = ""; ;
                bool gown = false;

                if (regData.PersoanlEmail != null)
                {
                    PEmail = regData.PersoanlEmail.Trim();
                }
                if (regData.CrrProf != null)
                {
                    currentProf = regData.CrrProf.Trim();
                }
                if (regData.Company != null)
                {
                    Company = regData.Company.Trim();
                }
                if (regData.CrrPhoneNo != null)
                {
                    PhoneNo = regData.CrrPhoneNo.Trim();
                }
                if (regData.Gown)
                {
                    gown = regData.Gown;
                    CollectionPoint = regData.CollectionPoint;
                }
                decimal Bal = 0;
                string DocNo = "";// Credentials.ObjNav.fnSaveGraduation(RegNo, PEmail, currentProf, Company, PhoneNo, gown, CollectionPoint);
                Bal = CommonClass.GetStudentBalance(RegNo);
                return Json(new { message = "Graduation Request document No : " + DocNo + " Submited successfully", Doc = DocNo, Balance = Bal, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ClearanceRequest()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    bool ClearanceAllowed = CommonClass.AllowClearanceApplication();
                    if (ClearanceAllowed)
                    {
                        string RegNo = Session["Username"].ToString();
                        bool AllowApplication = CommonClass.AllowGradClearanceApplication(RegNo);
                        if (!AllowApplication)
                        {
                            Error errormsg = new Error();
                            errormsg.Message = "You do not qualify to apply for clearance";
                            return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                        }
                        else
                        {
                            StudentDetailView Details = new StudentDetailView();
                            if (CommonClass.StudentHasRaisedRequest(RegNo, "Clearance"))
                            {
                                Details.MadeRequest = true;
                            }
                            else
                            {
                                #region Graduation Header
                                string page = "CustomerList?$filter=No eq '" + RegNo + "'&format=json";

                                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);

                                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                                {
                                    var result = streamReader.ReadToEnd();

                                    var details = JObject.Parse(result);

                                    string ProgCode = "";
                                    foreach (JObject config in details["value"])
                                    {
                                        Details.No = (string)config["No"];
                                        Details.Name = (string)config["Name"];
                                        Details.ID_No = (string)config["ID_No"];
                                        Details.Gender = (string)config["Gender"];
                                        Details.Phone_No = (string)config["Phone_No"];
                                        Details.Address = (string)config["Address"];
                                        Details.E_Mail = (string)config["E_Mail"];
                                        ProgCode = (string)config["Student_Programme"];
                                        if (ProgCode == "")
                                        {
                                            ProgCode = (string)config["Current_Programme"];
                                        }
                                        if (ProgCode == "")
                                        {
                                            ProgCode = (string)config["Current_Program"];
                                        }
                                        if (ProgCode == "")
                                        {
                                            ProgCode = CommonClass.GetStudentRegisteredProgramme(RegNo);
                                        }
                                        Details.Prog = ProgCode;
                                        Details.ProgName = CommonClass.GetProgrammeName(ProgCode);
                                        Details.MadeRequest = false;
                                    }
                                }
                                #endregion
                            }
                            return View(Details);
                        }
                    }
                    else
                    {
                        Error errormsg = new Error();
                        errormsg.Message = "Clearance request not active at the moment";
                        return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Error errormsg = new Error();
                errormsg.Message = ex.Message;
                return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitClearanceRequest(StudentReqs regData)
        {
            try
            {
                string RegNo = "", Sem = "", Remarks = "", prog = "", Campus = "";
                int r = 0;

                if (regData.StdNo != null)
                {
                    RegNo = regData.StdNo;
                }
                if (regData.Requisition_Type != null)
                {
                    r = Convert.ToInt32(regData.Requisition_Type.Trim());
                }
                if (regData.Prog != null)
                {
                    prog = regData.Prog.Trim();
                }
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester("");
                }
                Sem = Session["CurrentSem"].ToString();
                string DocNo = "";// Credentials.ObjNav.StudentRequisitionCreate(RegNo, r, Remarks, prog, "", Sem, Campus, 0, "", false, 0);

                return Json(new { message = "Clearance Request document No : " + DocNo + " Submited successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult GetRequistionLines(string DocNo, string Status, string RegT)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }

                List<StudentReqLines> ReqLines = new List<StudentReqLines>();
                string pageLine = "StudentRequisitionLines?$filter=Application_No eq '" + DocNo + "'&$format=json";
                HttpWebResponse httpResponse = Credentials.GetOdataData(pageLine);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    foreach (JObject config in details["value"])
                    {
                        StudentReqLines ReqLine = new StudentReqLines();
                        ReqLine.AppNo = (string)config["Application_No"];
                        ReqLine.Unit = (string)config["Unit_Code"];
                        ReqLine.UnitName = (string)config["Unit_Title"];
                        ReqLine.Lecturer = (string)config["Lecture_Name"];
                        ReqLine.Section = (string)config["Section"];
                        ReqLines.Add(ReqLine);
                    }
                }

                RequisitionLines LnList = new RequisitionLines
                {
                    ListLines = ReqLines,
                    RegT = RegT,
                    Status = Status
                };
                return PartialView("~/Views/Course/RequisitionLines.cshtml", LnList);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public PartialViewResult StudentGradClearanceRequisitionLine(string RegType)
        {
            try
            {
                StudentReqs RegLine = new StudentReqs();
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                string page = "StudentRequisition?$filter=StudentNo eq '" + RegNo + "' and RequisitionType eq '" + RegType + "'&format=json";
                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            RegLine.Code = (string)config["Code"];
                            RegLine.Date = (string)config["Date"];
                            RegLine.Requisition_Type = (string)config["RequisitionType"];
                            RegLine.Semester = (string)config["Semester"];
                            RegLine.Status = (string)config["Status"];
                            string[] s = null;// CommonClass.RequisitionApprovalLevel((string)config["Code"]);
                            string AppLevel = s[0];
                            string comment = s[1];
                            if (AppLevel == null || AppLevel == "")
                            {
                                RegLine.ApprovalLevel = "Fully Approved";
                                RegLine.CommentFound = false;
                            }
                            else
                            {
                                RegLine.ApprovalLevel = AppLevel;
                            }
                            if (AppLevel != null && AppLevel != "" && comment != null && comment != "")
                            {
                                RegLine.CommentFound = true;
                                RegLine.Comment = comment;
                            }
                            else
                            {
                                RegLine.CommentFound = false;
                                RegLine.Comment = "";
                            }
                        }
                    }
                }
                return PartialView("~/Views/Course/GradClearanceLine.cshtml", RegLine);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult PrintClearanceForm()
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

                    //Credentials.ObjNav.GenerateClearanceReport(StudentNo, "CLEARANCEREPORT-" + filename + ".pdf");
                    filename = "CLEARANCEREPORT-" + filename + ".pdf";
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
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult CertificateCollectionDate(string Date)
        {
            try
            {
                string stdNo = Session["Username"].ToString();
                DateTime CollectionDate = DateTime.ParseExact(Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //Credentials.ObjNav.CollectionDateForCertificate(stdNo, CollectionDate);

                return Json(new { message = "Your certificate collection date submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult ProgrammeListViewMinorMajor(string type)
        {
            try
            {
                List<DropdownList> ConcList = new List<DropdownList>();
                string ConcType = "", name = "";
                if (type == "12")
                {
                    ConcType = "MINOR";
                    name = "Minors";
                }
                else if (type == "13")
                {
                    ConcType = "MAJOR";
                    name = "Majors";
                }
                else if (type == "14")
                {
                    ConcType = "CONCEN";
                    name = "Concentrations";
                }
                else if (type == "2")
                {
                    ConcType = "PROG";
                    name = "List";
                }
                else
                {
                    ConcType = "";
                    name = "";
                }
                if (ConcType != "")
                {
                    string Prog = CommonClass.GetStudentRegisteredProgramme(Session["Username"].ToString());
                    if (type == "2")
                    {
                        ConcList = GetListOfProgrammes(Prog);
                    }
                    else
                    {
                        ConcList = GetProgrammeConcentration(Prog, ConcType);
                    }
                    ProgrammeConcentration ProgConc = new ProgrammeConcentration
                    {
                        Conc = name,
                        Code = "",
                        RegType = type,
                        ListOfConcentration = ConcList.Select(x =>
                                                  new SelectListItem()
                                                  {
                                                      Text = x.Text,
                                                      Value = x.Value
                                                  }).ToList()
                    };
                    return PartialView("~/Views/Course/Partial Views/ProgrammeMajorMinor.cshtml", ProgConc);
                }
                else
                {
                    Error error = new Error();
                    error.Message = "Challenge encountered";
                    return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }

        }
        protected List<DropdownList> GetProgrammeConcentration(string Prog, string Type)
        {
            List<DropdownList> ConcList = new List<DropdownList>();
            try
            {
                #region Programme Concentration               
                string page = "ProgrammeConcentration?$filter=Programme_Code eq '" + Prog + "' and Type eq '" + Type + "' and Blocked eq false&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        DropdownList CR = new DropdownList();
                        CR.Text = (string)config["Description"];
                        CR.Value = (string)config["Concentration_Code"];
                        ConcList.Add(CR);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return ConcList;
        }
        protected List<DropdownList> GetListOfProgrammes(string Prog)
        {
            List<DropdownList> ConcList = new List<DropdownList>();
            try
            {
                string Categ = CommonClass.GetProgrammeCategory(Prog);
                #region Programme Concentration               
                string page = "ProgrammeList?$select=Code,Description&$filter=Category eq '" + Categ + "' and OldCarriculum eq false and Description ne ''&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        DropdownList CR = new DropdownList();
                        CR.Value = (string)config["Code"];
                        CR.Text = (string)config["Description"];
                        ConcList.Add(CR);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return ConcList;
        }
        protected List<DropdownList> GetProgrammeUnits(string Prog)
        {
            List<DropdownList> ProgUnitList = new List<DropdownList>();
            try
            {
                #region Programme Concentration               
                string page = "UnitSubject?$filter=ProgrammeCode eq '" + Prog + "' and Unit_Category ne 'FREE ELECTIVES' and OldUnit eq false&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        DropdownList unit = new DropdownList();
                        unit.Text = (string)config["Code"] + " - " + (string)config["Desription"];
                        unit.Value = (string)config["Code"];
                        ProgUnitList.Add(unit);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return ProgUnitList;
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
        public ActionResult UnitsNotInTimeTable()
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
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester("");
                    }
                    if (Session["StudentDetails"] == null || Session["CurrentSem"].ToString() == "")
                    {
                        Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(RegNo);
                    }
                    string Prog = CommonClass.GetStudentRegisteredProgramme(RegNo);
                    string[] s = (string[])Session["StudentDetails"];
                    string sem = Session["CurrentSem"].ToString();
                    if (CommonClass.UnitsNotInTimeTable(RegNo, sem))
                    {
                        return View("~/View/Course/UitsNotInTimeTable.cshtml");
                    }
                    else
                    {
                        StudentCourseDetails stdDetail = new StudentCourseDetails
                        {
                            StdNo = RegNo,
                            Name = s[0],
                            CurrentSem = sem,
                            Prog = Prog,
                            Email = s[2]
                        };
                        return View(stdDetail);
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return View("~/Views/Common/ErrorMessage.cshtml", error);
            }
        }
        [HttpPost]
        public JsonResult UpdateRegisteredUnits(List<UnitRegistration> UnitReg)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                string Sem = "";
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester("");
                }

                Sem = Session["CurrentSem"].ToString();

                if (Session["CurrentProgDetails"] == null)
                {
                    Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                }
                string[] s = (string[])Session["CurrentProgDetails"];
                int i = 0;
                foreach (var c in UnitReg)
                {
                    string ClassCode = "", Campus = "", Day = "", Period = "";
                    string d = c.UnitCode.Trim();
                    if (c.ClassCode != null)
                    {
                        ClassCode = c.ClassCode.Trim();
                    }
                    if (c.Campus != null)
                    {
                        Campus = c.Campus.Trim();
                    }
                    if (c.Period != null)
                    {
                        Period = c.Period.Trim();
                    }
                    if (c.Day != null)
                    {
                        Day = c.Day.Trim();
                    }
                    Credentials.ObjNav.UpdateRegisterdUnits(RegNo, Sem, d, ClassCode, Campus);
                    i++;
                }
                return Json(new { message = i.ToString() + " Units Updated successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult StudentEnquiryList()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    #region dim2
                    List<DimensionValues> Dim2List = new List<DimensionValues>();
                    string pageDivision = "DimensionValues?$filter=Global_Dimension_No_ eq 2&$format=json";

                    HttpWebResponse httpResponseDivision = Credentials.GetOdataData(pageDivision);
                    using (var streamReader = new StreamReader(httpResponseDivision.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);


                        foreach (JObject config in details["value"])
                        {
                            DimensionValues DList = new DimensionValues();
                            DList.Code = (string)config["Code"];
                            DList.Name = (string)config["Name"];
                            Dim2List.Add(DList);
                        }
                    }
                    #endregion
                    DropdownListValues newDOC = new DropdownListValues
                    {
                        Code = "",
                        ListOfValues = Dim2List.Select(x =>
                                           new SelectListItem()
                                           {
                                               Text = x.Name,
                                               Value = x.Code
                                           }).ToList()
                    };
                    return View(newDOC);
                }
            }
            catch (Exception ex)
            {
                Error errormsg = new Error();
                errormsg.Message = ex.Message;
                return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
            }
        }
        public PartialViewResult StudentEnquiryListView()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();

                #region Enquiry List
                List<Enquiry> EnquiryList = new List<Enquiry>();
                string page = "StudentEnquiry?$filter=Student_No eq '" + RegNo + "'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        Enquiry CR = new Enquiry();
                        CR.EnqNo = (string)config["Enquiry_No"];
                        CR.OpenTo = (string)config["Department"];
                        CR.EnquireFor = (string)config["Enquiry"];
                        CR.DateRequested = ((DateTime)config["Date_Raised"]).ToString("dd/MM/yyyy");
                        if ((string)config["Response"] != "")
                        {
                            CR.Response = (string)config["Response"];
                        }
                        else
                        {
                            CR.Response = "No Response Yet";
                        }
                        EnquiryList.Add(CR);
                    }
                }
                #endregion
                return PartialView("~/Views/Course/EnquiryListView.cshtml", EnquiryList);
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitEnquiry(string Dep, string Enquiry)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                else
                {
                    string StudentNo = Session["Username"].ToString();

                    Credentials.ObjNav.SaveStudentEnquiry(StudentNo, Enquiry, 0, Dep);
                }
                return Json(new { message = "Enquiry send successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #region student Requisitions
        public ActionResult GetUnitsWithMissingGrades()
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

                    string Prog = CommonClass.GetStudentRegisteredProgramme(RegNo);

                    #region Reg Units
                    List<StudentUnits> regUnits = new List<StudentUnits>();
                    string page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Programme eq '" + Prog + "' and (Grade eq 'X' or Grade eq 'F' or Grade eq 'E') and &format=json";

                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            StudentUnits CR = new StudentUnits();
                            CR.Unit = (string)config["Unit"];
                            CR.Unit_Name = (string)config["Unit_Description"];
                            regUnits.Add(CR);
                        }
                    }
                    #endregion

                    GetRegisteredUnit regUnitsList = new GetRegisteredUnit
                    {
                        ListOfRegUnit = regUnits.DistinctBy(x => x.Unit).ToList()
                    };
                    return PartialView("~/Views/Course/CourseRequisitionLines.cshtml", regUnitsList);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult GetExamChallengeUnits()
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

                    List<DropdownList> ProgUnitList = new List<DropdownList>();

                    string Prog = CommonClass.GetStudentRegisteredProgramme(RegNo);
                    List<StudentUnits> regUnits = new List<StudentUnits>();

                    string Sem = "";
                    if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester("");
                    }
                    Sem = Session["CurrentSem"].ToString();

                    Credentials.ObjNav.RefreshStudentAudit(RegNo);
                    #region Programme Units                    
                    string page = "StudentUnitsAudit?$select=Programme,Unit,Description,UnitType&$filter=StudentNo eq '" + RegNo + "' and Concentration eq '" + Prog + "' and Progress_Status eq 'Future'&$format=json";

                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        if (details["value"].Count() > 0)
                        {
                            foreach (JObject config in details["value"])
                            {
                                if (!StudentRegisteredUnitExists(RegNo, (string)config["Programme"], (string)config["Unit"], Sem))
                                {
                                    string pageTimetable = "Timetable?$filter=Unit eq '" + (string)config["Unit"] + "' and Semester eq '" + Sem + "'&$format=json";

                                    HttpWebResponse httpResponseTmT = Credentials.GetOdataData(pageTimetable);
                                    using (var streamReaderTmT = new StreamReader(httpResponseTmT.GetResponseStream()))
                                    {
                                        var resultTmT = streamReaderTmT.ReadToEnd();

                                        var detailsTmT = JObject.Parse(resultTmT);

                                        if (detailsTmT["value"].Count() > 0)
                                        {
                                            foreach (JObject config1 in detailsTmT["value"])
                                            {
                                                DropdownList unit = new DropdownList();
                                                unit.Text = (string)config1["Unit"] + " - " + (string)config1["Unit_Description"];
                                                unit.Value = (string)config1["Unit"];
                                                ProgUnitList.Add(unit);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    ProgrammeConcentration ProgConc = new ProgrammeConcentration
                    {
                        Code = "",
                        ListOfConcentration = ProgUnitList.Select(x =>
                                                  new SelectListItem()
                                                  {
                                                      Text = x.Text,
                                                      Value = x.Value
                                                  }).DistinctBy(x => x.Value).ToList()
                    };
                    return PartialView("~/Views/Course/Partial Views/ExcemptionRequest.cshtml", ProgConc);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult GetUnitsForExemptions()
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

                    List<DropdownList> ProgUnitList = new List<DropdownList>();
                    string Prog = CommonClass.GetStudentRegisteredProgramme(Session["Username"].ToString());
                    ProgUnitList = GetProgrammeUnits(Prog);

                    ProgrammeConcentration ProgConc = new ProgrammeConcentration
                    {
                        Code = "",
                        ListOfConcentration = ProgUnitList.Select(x =>
                                                  new SelectListItem()
                                                  {
                                                      Text = x.Text,
                                                      Value = x.Value
                                                  }).DistinctBy(x => x.Value).ToList()
                    };
                    return PartialView("~/Views/Course/Partial Views/ExcemptionRequest.cshtml", ProgConc);
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
                return PartialView("~/Views/Course/Partial Views/ProgrammeTransit.cshtml");
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        #endregion
    }
}
