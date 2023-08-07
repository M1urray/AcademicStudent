using Microsoft.Ajax.Utilities;
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
                        Session["CurrentSem"] = CommonClass.CurrentSemester();
                    }
                    if (Session["StudentDetails"] == null || Session["CurrentSem"].ToString() == "")
                    {
                        Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(RegNo);
                    }
                    string[] s = (string[])Session["StudentDetails"];
                    string sem = Session["CurrentSem"].ToString();
                    if (sem == "")
                    {
                        sem = "Not Set";
                    }

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
            }
            catch (Exception ex)
            {
                Error errormsg = new Error();
                errormsg.Message = ex.Message;
                return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
            }
        }
        public PartialViewResult CourseRegistrationDetails()
        {
            try
            {
                string RegNo = Session["Username"].ToString();
                if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
                }

                string sem = Session["CurrentSem"].ToString();

                #region Course Reg
                List<CourseReg> CReg = new List<CourseReg>();
                string page = "CourseReg?$filter=StudentNo eq '" + RegNo + "' and Reversed eq false&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        CourseReg CR = new CourseReg();
                        CR.RegID = (string)config["RegTransactonID"];
                        CR.Programme = (string)config["Programme"];
                        CR.Stage = (string)config["Stage"];
                        CR.Semester = (string)config["Semester"];
                        CR.Register_for = (string)config["Registerfor"];
                        CR.Settlement_Type = (string)config["SettlementType"];
                        CR.Registration_Date = (string)config["RegDate"];
                        CR.UnitsTaken = (string)config["UnitsTaken"];
                        CR.TotalBilled = (string)config["TotalBilled"];
                        CReg.Add(CR);
                    }
                }
                #endregion

                #region Basket Units
                List<StudentUnits> BasketUnits = new List<StudentUnits>();
                string pageBasket = "BasketUnits?$filter=Student_No eq '" + RegNo + "' and Semester eq '" + sem + "' and Submitted eq false&format=json";

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

                StudentCourseRegistration StdCourseReg = new StudentCourseRegistration
                {
                    ListOfCourseRegistration = CReg.OrderBy(x => x.Stage).ThenBy(n => n.Registration_Date),
                    ListOfStudentBasketUnits = BasketUnits
                };

                return PartialView("~/Views/Course/CourseRegistrationDetails.cshtml", StdCourseReg);
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
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
                    string page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Semester eq '" + Filters.Semester + "'" +
                       " and Programme eq '" + Filters.Programme + "' and Register_for eq '" + Filters.RegFor + "' and Reg_Transacton_ID eq '" + Filters.RegID + "'&format=json";
                    //string page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Semester eq '" + Filters.Semester + "'" +
                    //   " and Programme eq '" + Filters.Programme + "'&format=json";

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

                    GetRegisteredUnit regUnitsList = new GetRegisteredUnit
                    {
                        RegDeadlineDate = CommonClass.RegistrationDeadline(),
                        ListOfRegUnit = regUnits.DistinctBy(x => x.Unit).ToList()
                    };
                    return PartialView("~/Views/Course/ViewStudentRegisteredUnits.cshtml", regUnitsList);
                }
            }
            catch (Exception ex)
            {
                Error errormsg = new Error();
                errormsg.Message = ex.Message;
                return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
            }
        }
        public PartialViewResult GetProgramUnits(string Type)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                string Sem = "";
                if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
                }

                Sem = Session["CurrentSem"].ToString();

                if (Session["CurrentProgDetails"] == null)
                {
                    Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                }
                string[] s = (string[])Session["CurrentProgDetails"];

                if ((s[0] == "" || s[0] == null || s[1] == "" || s[1] == null) && Type != "3")
                {
                    Error errormsg = new Error();
                    errormsg.Message = "You have not been registered in the current semester";
                    return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", errormsg);
                }
                else
                {
                    if (Type == "2")
                    {
                        #region Stages
                        List<DropdownList> ListOfStages = new List<DropdownList>();
                        string page = "CourseReg?$select=Stage&$filter=StudentNo eq '" + RegNo + "' and Registerfor eq 'Stage' and Reversed eq false&$format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            foreach (JObject config in details["value"])
                            {
                                DropdownList ddl = new DropdownList();
                                ddl.Value = (string)config["Stage"];
                                ddl.Text = (string)config["Stage"];
                                ListOfStages.Add(ddl);
                            }
                        }
                        #endregion

                        DropdownListValues ddlList = new DropdownListValues
                        {
                            Code = Type,
                            ListOfValues = ListOfStages.Select(x =>
                                         new SelectListItem()
                                         {
                                             Text = x.Text,
                                             Value = x.Value
                                         }).DistinctBy(x => x.Value).OrderBy(x => x.Value).ToList()
                        };
                        return PartialView("~/Views/Course/Resit_Retake.cshtml", ddlList);
                    }
                    else if (Type == "3")
                    {
                        #region Academic Year
                        List<DropdownList> ListOfAcademicYear = new List<DropdownList>();
                        string page = "CourseReg?$select=AcademicYear&$filter=StudentNo eq '" + RegNo + "' and Registerfor eq 'Stage' and Reversed eq false&$format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            foreach (JObject config in details["value"])
                            {
                                DropdownList ddl = new DropdownList();
                                ddl.Value = (string)config["AcademicYear"];
                                ddl.Text = (string)config["AcademicYear"];
                                ListOfAcademicYear.Add(ddl);
                            }
                        }
                        #endregion

                        DropdownListValues ddlList = new DropdownListValues
                        {
                            Code = Type,
                            ListOfValues = ListOfAcademicYear.Select(x =>
                                         new SelectListItem()
                                         {
                                             Text = x.Text,
                                             Value = x.Value
                                         }).DistinctBy(x => x.Value).OrderBy(x => x.Value).ToList()
                        };
                        return PartialView("~/Views/Course/Resit_Retake.cshtml", ddlList);
                    }
                    else if (Type == "1")
                    {
                        #region Programme Units
                        List<CoreUnitSubject> ElectiveUnitSub = new List<CoreUnitSubject>();
                        //string page = "UnitsSubjects?$filter=Programme_Code eq '" + s[0] + "' and Stage_Code eq '" + s[1] + "' and Old_Unit eq false and Unit_Type eq 'Core'&format=json";

                        string page = "UnitsSubjects?$filter=Programme_Code eq '" + s[0] + "' and Stage_Code eq '" + s[1] + "' and Unit_Type eq 'Elective' and Old_Unit eq false and Time_Table eq true &$format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            foreach (JObject config in details["value"])
                            {
                                CoreUnitSubject NewUnit = new CoreUnitSubject();
                                if (!StudentRegisteredUnitExists(RegNo, (string)config["Programme_Code"], (string)config["Code"], Sem))
                                {
                                    NewUnit.Code = (string)config["Code"];
                                    NewUnit.Desription = (string)config["Desription"];
                                    ElectiveUnitSub.Add(NewUnit);
                                }
                            }
                        }
                        #endregion

                        UnitSubject ElectiveUnits = new UnitSubject
                        {
                            ListOfCoreUnitsSubjects = ElectiveUnitSub.DistinctBy(x => x.Code).ToList()
                        };
                        return PartialView("~/Views/Course/GetBonusCourse.cshtml", ElectiveUnits);
                    }
                    else
                    {
                        bool OptionAllowed = AllowOption(s[0], s[1]);

                        if (OptionAllowed)
                        {
                            ProgrammeOptionsList ProgOptionList = new ProgrammeOptionsList();

                            #region Programme Options
                            List<ProgrammeOptions> ProgOption = new List<ProgrammeOptions>();
                            string page = "ProgrammeOption?$filter=Programme_Code eq '" + s[0] + "'&format=json";

                            HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var result = streamReader.ReadToEnd();

                                var details = JObject.Parse(result);

                                foreach (JObject config in details["value"])
                                {
                                    ProgrammeOptions POption = new ProgrammeOptions();
                                    POption.Code = (string)config["Code"];
                                    POption.Desription = (string)config["Code"];
                                    ProgOption.Add(POption);
                                }
                            }
                            #endregion

                            ProgOptionList = new ProgrammeOptionsList
                            {
                                ListOfProgOption = ProgOption.Select(x =>
                                             new SelectListItem()
                                             {
                                                 Text = x.Desription,
                                                 Value = x.Code
                                             }).ToList()
                            };

                            return PartialView("~/Views/Course/ProgrammeOptions.cshtml", ProgOptionList);
                        }
                        else
                        {
                            #region Programme Units
                            List<CoreUnitSubject> CoreUnitSub = new List<CoreUnitSubject>();
                            //string page = "UnitsSubjects?$filter=Programme_Code eq '" + s[0] + "' and Stage_Code eq '" + s[1] + "' and Old_Unit eq false and Unit_Type eq 'Core'&format=json";

                            string page = "UnitsSubjects?$filter=Programme_Code eq '" + s[0] + "' and Stage_Code eq '" + s[1] + "' and Unit_Type eq 'Core' and Old_Unit eq false and Time_Table eq true &$format=json";

                            HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var result = streamReader.ReadToEnd();

                                var details = JObject.Parse(result);

                                foreach (JObject config in details["value"])
                                {
                                    CoreUnitSubject NewUnit = new CoreUnitSubject();
                                    if (!StudentRegisteredUnitExists(RegNo, (string)config["Programme_Code"], (string)config["Code"], Sem))
                                    {
                                        NewUnit.Code = (string)config["Code"];
                                        NewUnit.Desription = (string)config["Desription"];
                                        CoreUnitSub.Add(NewUnit);
                                    }
                                }
                            }
                            #endregion

                            UnitSubject UnitSub = new UnitSubject
                            {
                                ListOfCoreUnitsSubjects = CoreUnitSub.DistinctBy(x => x.Code).ToList()
                            };

                            return PartialView("~/Views/Course/GetProgramUnits.cshtml", UnitSub);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
            }
        }
        public PartialViewResult GetNonResidencyForm()
        {
            try
            {
                return PartialView("~/Views/Course/NonResidenceForm.cshtml");
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
            }
        }
        public PartialViewResult SaveResidenctInformation()
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
                }

                Sem = Session["CurrentSem"].ToString();

                if (Session["CurrentProgDetails"] == null)
                {
                    Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                }
                string[] s = (string[])Session["CurrentProgDetails"];

                if (s[0] == "" || s[1] == "")
                {
                    Error errormsg = new Error();
                    errormsg.Message = "You have not been registered in the current semester";
                    return PartialView("~/Views/Shared/Parial Views/ErroMessangeView.cshtml", errormsg);
                }
                else
                {
                    Credentials.ObjNav.InserStudentResidency(RegNo, Sem, 1);

                    bool OptionAllowed = AllowOption(s[0], s[1]);

                    if (OptionAllowed)
                    {
                        ProgrammeOptionsList ProgOptionList = new ProgrammeOptionsList();

                        #region Programme Options
                        List<ProgrammeOptions> ProgOption = new List<ProgrammeOptions>();
                        string page = "ProgrammeOption?$filter=Programme_Code eq '" + s[0] + "'&format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            foreach (JObject config in details["value"])
                            {
                                ProgrammeOptions POption = new ProgrammeOptions();
                                POption.Code = (string)config["Code"];
                                POption.Desription = (string)config["Code"];
                                ProgOption.Add(POption);
                            }
                        }
                        #endregion

                        ProgOptionList = new ProgrammeOptionsList
                        {
                            ListOfProgOption = ProgOption.Select(x =>
                                         new SelectListItem()
                                         {
                                             Text = x.Desription,
                                             Value = x.Code
                                         }).ToList()
                        };

                        return PartialView("~/Views/Course/ProgrammeOptions.cshtml", ProgOptionList);
                    }
                    else
                    {
                        #region Programme Units
                        List<CoreUnitSubject> CoreUnitSub = new List<CoreUnitSubject>();
                        string page = "UnitsSubjects?$filter=Programme_Code eq '" + s[0] + "' and Stage_Code eq '" + s[1] + "' and Old_Unit eq false and Unit_Type eq 'Core'&format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            foreach (JObject config in details["value"])
                            {
                                CoreUnitSubject NewUnit = new CoreUnitSubject();
                                if (!StudentRegisteredUnitExists(RegNo, (string)config["Programme_Code"], (string)config["Code"], Sem))
                                {
                                    NewUnit.Code = (string)config["Code"];
                                    NewUnit.Desription = (string)config["Desription"];
                                    CoreUnitSub.Add(NewUnit);
                                }
                            }
                        }
                        #endregion

                        UnitSubject UnitSub = new UnitSubject
                        {
                            ListOfCoreUnitsSubjects = CoreUnitSub.DistinctBy(x => x.Code).ToList()
                        };

                        return PartialView("~/Views/Course/GetProgramUnits.cshtml", UnitSub);
                    }
                }
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
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
                        Session["CurrentSem"] = CommonClass.CurrentSemester();
                    }

                    Sem = Session["CurrentSem"].ToString();

                    if (Session["CurrentProgDetails"] == null)
                    {
                        Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                    }
                    string[] s = (string[])Session["CurrentProgDetails"];


                    List<CoreUnitSubject> CoreUnitSub = new List<CoreUnitSubject>();
                    List<ElectiveUnitSubject> ElectiveUnitSub = new List<ElectiveUnitSubject>();
                    Option = HttpUtility.UrlEncode(Option);
                    #region Programme Option Units
                    string page = "UnitsSubjects?$filter=Programme_Code eq '" + s[0] + "' and Stage_Code eq '" + s[1] + "' and (Programme_Option eq '' or Programme_Option eq '" + Option + "') and Old_Unit eq false and Time_Table eq true&format=json";

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
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
            }
        }
        protected bool StudentRegisteredUnitExists(string stdNo, string prog, string Unit, string sem)
        {
            bool exist = false;
            try
            {
                string page = "StudentUnits?$filter=Student_No eq '" + stdNo + "' and Programme eq '" + prog + "' and Unit eq '" + Unit + "'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        exist = true;
                    }
                }

                if (!exist)
                {
                    string pageBasket = "StudentBasketUnits?$filter=Student_No eq '" + stdNo + "' and Programme eq '" + prog + "' and Unit eq '" + Unit + "' and Semester eq '" + sem + "'&$format=json";

                    HttpWebResponse httpResponsebasket = Credentials.GetOdataData(pageBasket);
                    using (var streamReader = new StreamReader(httpResponsebasket.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        if (details["value"].Count() > 0)
                        {
                            exist = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return exist;
        }
        public JsonResult SaveSelectedUnits(List<UnitRegistration> UnitReg)
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
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
                    string d = c.UnitCode.Trim();
                    Credentials.ObjNav.RegisterStudentUnitBasket(RegNo, Sem, s[1], s[0], d, 0, "", "", "", "");
                    i++;
                }
                return Json(new { message = i.ToString() + " Units Selected successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult SaveNonResidencedata(NonResidenceData ResData)
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
                }

                Sem = Session["CurrentSem"].ToString();

                if (Session["CurrentProgDetails"] == null)
                {
                    Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                }
                string[] s = (string[])Session["CurrentProgDetails"];
                if (s[0] == "" || s[1] == "")
                {
                    Error errormsg = new Error();
                    errormsg.Message = "You have not been registered in the current semester";
                    return PartialView("~/Views/Shared/Parial Views/ErroMessangeView.cshtml", errormsg);
                }
                else
                {
                    string witness = "";
                    if (ResData.Witness != null)
                    {
                        witness = ResData.Witness;
                    }
                    Credentials.ObjNav.InsertNonResidenceInformation(RegNo, Sem, ResData.Premise, ResData.RoomNo,
                    ResData.LandLoard, ResData.Caretaker, witness, ResData.AreaName);

                    Credentials.ObjNav.InserStudentResidency(RegNo, Sem, 2);

                    bool OptionAllowed = AllowOption(s[0], s[1]);

                    if (OptionAllowed)
                    {
                        ProgrammeOptionsList ProgOptionList = new ProgrammeOptionsList();

                        #region Programme Options
                        List<ProgrammeOptions> ProgOption = new List<ProgrammeOptions>();
                        string page = "ProgrammeOption?$filter=Programme_Code eq '" + s[0] + "'&format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            foreach (JObject config in details["value"])
                            {
                                ProgrammeOptions POption = new ProgrammeOptions();
                                POption.Code = (string)config["Code"];
                                POption.Desription = (string)config["Code"];
                                ProgOption.Add(POption);
                            }
                        }
                        #endregion

                        ProgOptionList = new ProgrammeOptionsList
                        {
                            ListOfProgOption = ProgOption.Select(x =>
                                         new SelectListItem()
                                         {
                                             Text = x.Desription,
                                             Value = x.Code
                                         }).ToList()
                        };

                        return PartialView("~/Views/Course/ProgrammeOptions.cshtml", ProgOptionList);
                    }
                    else
                    {
                        #region Programme Units
                        List<CoreUnitSubject> CoreUnitSub = new List<CoreUnitSubject>();
                        string page = "UnitsSubjects?$filter=Programme_Code eq '" + s[0] + "' and Stage_Code eq '" + s[1] + "' and Old_Unit eq false and Unit_Type eq 'Core'&format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            foreach (JObject config in details["value"])
                            {
                                CoreUnitSubject NewUnit = new CoreUnitSubject();
                                if (!StudentRegisteredUnitExists(RegNo, (string)config["Programme_Code"], (string)config["Code"], Sem))
                                {
                                    NewUnit.Code = (string)config["Code"];
                                    NewUnit.Desription = (string)config["Desription"];
                                    CoreUnitSub.Add(NewUnit);
                                }
                            }
                        }
                        #endregion

                        UnitSubject UnitSub = new UnitSubject
                        {
                            ListOfCoreUnitsSubjects = CoreUnitSub.DistinctBy(x => x.Code).ToList()
                        };

                        return PartialView("~/Views/Course/GetProgramUnits.cshtml", UnitSub);
                    }
                }
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
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
        public JsonResult RegisterSelectedUnits()
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
                }

                Sem = Session["CurrentSem"].ToString();

                if (Session["CurrentProgDetails"] == null)
                {
                    Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                }
                string[] s = (string[])Session["CurrentProgDetails"];

                Credentials.ObjNav.RegisterStudentUnits(RegNo, Sem, s[1], s[0], 0, false);


                return Json(new { message = "Units Registered successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
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
                Credentials.ObjNav.DropStudentUnits(RegNo, Filters.sem, Filters.stage, Filters.prog, Filters.Unit, false);
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
                List<TimeTableView> timeTable = new List<TimeTableView>();

                string RegNo = Session["Username"].ToString();
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
                }
                if (Session["StudentDetails"] == null)
                {
                    Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(RegNo);
                }
                string[] s = (string[])Session["StudentDetails"];
                string sem = Session["CurrentSem"].ToString();

                string page = "CourseReg?$filter=StudentNo eq '" + RegNo + "' and Semester eq '" + sem + "'&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            //string pageTimetable = "Timetable?$filter=Programme eq '" + (string)config["Programme"] + "' and Stage eq '" + (string)config["Stage"] + "' and Semester eq '" + (string)config["Semester"] + "'&format=json";
                            string pageTimetable = "Timetable?$filter=Programme eq '" + (string)config["Programme"] + "' and Semester eq '" + (string)config["Semester"] + "'&format=json";

                            HttpWebResponse httpResponseTimeTable = Credentials.GetOdataData(pageTimetable);
                            using (var streamReaderTimeTable = new StreamReader(httpResponseTimeTable.GetResponseStream()))
                            {
                                var resultTimeTable = streamReaderTimeTable.ReadToEnd();

                                var detailsTimeTable = JObject.Parse(resultTimeTable);

                                if (detailsTimeTable["value"].Count() > 0)
                                {
                                    foreach (JObject config1 in detailsTimeTable["value"])
                                    {
                                        TimeTableView tmTable = new TimeTableView();
                                        tmTable.Unit = (string)config1["Unit"];
                                        tmTable.Period = (string)config1["Period"];
                                        tmTable.Semester = (string)config1["Semester"];
                                        tmTable.Day_of_Week = (string)config1["DayofWeek"];
                                        tmTable.Lecture_Room = (string)config1["Lecture_Room"];
                                        tmTable.Lecturer = (string)config1["Lecturer_Name"];
                                        timeTable.Add(tmTable);
                                    }
                                }
                            }
                        }
                    }
                }
                return View(timeTable);
            }
            catch (Exception ex)
            {
                Error errormsg = new Error();
                errormsg.Message = ex.Message;
                return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
            }
        }
        public ActionResult LecturerEvaluation()
        {
            try
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
            catch (Exception ex)
            {
                Error errormsg = new Error();
                errormsg.Message = ex.Message;
                return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
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
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
            }
        }
        public PartialViewResult LecturerEvaluationForm(string Unit)
        {
            try
            {
                string RegNo = Session["Username"].ToString();
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
                }
                string sem = Session["CurrentSem"].ToString();
                string[] s = CommonClass.CurrentCourseRegistration(RegNo, sem);
                Lecturer lec = new Lecturer();
                Error error = new Error();
                string page = "LectAllocatedUnits?$filter=Code eq '" + s[0] + "' and Stage eq '" + s[1] + "' and Semester eq '" + sem + "' and Unit eq '" + Unit + "'&format=json";

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
                            lec.LecName = CommonClass.GetEmployeeName((string)config["Lecturer"]);
                            lec.Unit = (string)config["Unit"];
                            lec.UnitName = (string)config["Unit_Name"];
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
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
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
                Error errormsg = new Error();
                errormsg.Message = ex.Message;
                return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
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
                string page = "StudentRequisition?$filter=StudentNo eq '" + RegNo + "' and RequisitionType ne 'Clearance'&format=json";
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
                            stdreq.Requisition_Type = (string)config["RequisitionType"];
                            stdreq.Semester = (string)config["Semester"];
                            stdreq.Status = (string)config["Status"];
                            stdreq.LinesCounter = GetRegLinesCounter((string)config["Code"]).ToString();
                            RegList.Add(stdreq);
                        }
                    }
                }

                return PartialView("~/Views/Course/StudentRequisitionList.cshtml", RegList);
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
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
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
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
                return Json(new { message = "Units Registered successfully", success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetRetake_ResitUnits()
        {
            try
            {
                #region Units List
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
        public ActionResult GetCurrentRegisteredUnits()
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
                        Session["CurrentSem"] = CommonClass.CurrentSemester();
                    }
                    string sem = Session["CurrentSem"].ToString();

                    string[] s = CommonClass.CurrentCourseRegistration(RegNo, sem);

                    #region Reg Units
                    List<StudentUnits> regUnits = new List<StudentUnits>();
                    string page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Semester eq '" + sem + "'" +
                       " and Programme eq '" + s[0] + "'&format=json";

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

                    GetRegisteredUnit regUnitsList = new GetRegisteredUnit
                    {
                        ListOfRegUnit = regUnits.DistinctBy(x => x.Unit).ToList()
                    };
                    return PartialView("~/Views/Course/CourseRequisitionLines.cshtml", regUnitsList);
                }
            }
            catch (Exception ex)
            {
                Error errormsg = new Error();
                errormsg.Message = ex.Message;
                return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
            }
        }
        public ActionResult GetRetakeResitUnits(string Type, string Value)
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
                    List<StudentUnits> regUnits = new List<StudentUnits>();
                    if (Type == "2")
                    {
                        #region Reg Units Resit
                        string page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Stage eq '" + Value + "'" +
                           " and Failed eq true and Supp_Taken eq false&$format=json";

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
                    if (Type == "3")
                    {
                        #region Reg Units Retake
                        string page = "CourseReg?$filter=StudentNo eq '" + RegNo + "' and AcademicYear eq '" + Value + "' and Reversed eq false&format=json";

                        HttpWebResponse httpResponseRetake = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponseRetake.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            foreach (JObject config in details["value"])
                            {
                                string pageRetake = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Stage eq '" + (string)config["Stage"] + "'" +
                                                  " and Programme eq '" + (string)config["Programme"] + "' and Semester eq '" + (string)config["Semester"] +
                                                  "' and Register_for eq 'Stage' and Failed eq true and Re_Taken eq false&$format=json";

                                HttpWebResponse httpResponseRetakeUnits = Credentials.GetOdataData(pageRetake);
                                using (var streamReaderReatke = new StreamReader(httpResponseRetakeUnits.GetResponseStream()))
                                {
                                    var resultRetake = streamReaderReatke.ReadToEnd();

                                    var detailsRetake = JObject.Parse(resultRetake);

                                    foreach (JObject config1 in detailsRetake["value"])
                                    {
                                        StudentUnits CR = new StudentUnits();
                                        CR.Programme = (string)config1["Programme"];
                                        CR.Unit = (string)config1["Unit"];
                                        CR.Unit_Name = (string)config1["Unit_Description"];
                                        CR.Stage = (string)config1["Stage"];
                                        CR.Semester = (string)config1["Semester"];
                                        CR.RegFor = (string)config1["Register_for"];
                                        regUnits.Add(CR);
                                    }
                                }
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
                Error errormsg = new Error();
                errormsg.Message = ex.Message;
                return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveStudentRequisition(StudentReqs regData)
        {
            try
            {
                string RegNo = "", Sem = "", Remarks = "", prog = "", Campus = "", Code = "", date = "";
                int r = 0;
                bool sendForApproval = false;

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
                if (regData.Campus != null)
                {
                    Campus = regData.Campus.Trim();
                }
                if (r == 3)
                {
                    sendForApproval = true;
                }
                string DocNo = Credentials.ObjNav.StudentRequisitionCreate(RegNo, r, Remarks, prog, "", Sem, Campus, 0, "");

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
                string RegNo = "", Sem = "", Remarks = "", prog = "", Campus = "", Code = "", date = "";
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
                string DocNo = Credentials.ObjNav.StudentRequisitionCreate(RegNo, r, Remarks, prog, "", Sem, "", 0, "");
                foreach (var c in StdRegLines)
                {
                    string Unit = c.Unit.Trim();
                    string UnitName = c.UnitName.Trim();
                    string Reason = "";
                    Credentials.ObjNav.StudentRequisitionLinesCreate(DocNo, Unit, UnitName, Reason);
                }
                return Json(new { message = "Requisition document No : " + DocNo + " Submited successfully", success = true }, JsonRequestBehavior.AllowGet);
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
            GradRequestInfomation inf = new GradRequestInfomation();
            inf.ReqNo = DocNo;
            inf.Balance = Bal;
            return PartialView("~/Views/Shared/Partial Views/GraduationReqInfo.cshtml", inf);
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
                string DocNo = Credentials.ObjNav.fnSaveGraduation(RegNo, PEmail, currentProf, Company, PhoneNo, gown, CollectionPoint);
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
                }
                Sem = Session["CurrentSem"].ToString();
                string DocNo = Credentials.ObjNav.StudentRequisitionCreate(RegNo, r, Remarks, prog, "", Sem, Campus, 0, "");

                return Json(new { message = "Clearance Request document No : " + DocNo + " Submited successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult GetRequistionLines(string DocNo)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }

                List<StudentReqLines> ReqLines = new List<StudentReqLines>();
                string pageLine = "StudentRequisitionLines?$filter=Application_No eq '" + DocNo + "'&format=json";
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
                        ReqLines.Add(ReqLine);
                    }
                }
                return PartialView("~/Views/Course/RequisitionLines.cshtml", ReqLines);
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
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
                            string[] s = CommonClass.RequisitionApprovalLevel((string)config["Code"]);
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
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
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

                    Credentials.ObjNav.GenerateClearanceReport(StudentNo, "CLEARANCEREPORT-" + filename + ".pdf");
                    filename = "CLEARANCEREPORT-" + filename + ".pdf";
                    //CommonClass.MoveFile(filename);
                    string DestinationPath = "";// Credentials.fileDestinationPath + filename;
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
                Credentials.ObjNav.CollectionDateForCertificate(stdNo, CollectionDate);

                return Json(new { message = "Your certificate collection date submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
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
                    return View();
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
                        CR.OpenTo = (string)config["Raised_To"];
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

                    Credentials.ObjNav.SaveStudentEnquiry(StudentNo, Enquiry, Convert.ToInt32(Dep));
                }
                return Json(new { message = "Enquiry send successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
