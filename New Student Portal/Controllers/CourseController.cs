using Microsoft.Ajax.Utilities;
using New_Student_Portal.CustomSecurity;
using New_Student_Portal.Models;
using New_Student_Portal.NAVWS;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.Controllers
{
    [CustomAuthorization(Role = "STUD")]
    [CustomeAuthentication]
    public class CourseController : Controller
    {
        public CourseController()
        {
        }

        protected bool AllowOption(string Prog, string Stage)
        {
            bool flag = false;
            try
            {
                string str = string.Concat(new string[] { "ProgrammeStages?$filter=Programme_Code eq '", Prog, "' and Code eq '", Stage, "' and Allow_Programme_Options eq true&format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() > 0)
                    {
                        flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult CertificateCollectionDate(string Date)
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                DateTime.ParseExact(Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                jsonResult = base.Json(new { message = "Your certificate collection date submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public ActionResult ClearanceRequest()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    action = base.View();
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception)
            {
                Error error = new Error()
                {
                    Message = exception.Message
                };
                action = base.View("~/Views/Shared/ErrorMessange.cshtml", error);
            }
            return action;
        }

        public ActionResult CourseRegistration(string T)
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    StudentCourseDetails studentCourseDetail = new StudentCourseDetails()
                    {
                        RegT = T
                    };
                    action = base.View(studentCourseDetail);
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
                action = base.View("~/Views/Common/ErrorMessage.cshtml", error);
            }
            return action;
        }

        public ActionResult CourseRegistrationDetails(string T)
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    if ((base.Session["CurrentSem"] == null ? true : base.Session["CurrentSem"].ToString() == ""))
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    string str1 = base.Session["CurrentSem"].ToString();
                    RegisteredProgrammes registeredProgramme = new RegisteredProgrammes();
                    string str2 = string.Concat("CustomerList?$filter=No eq '", str, "'&$format=json");
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str2).GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            registeredProgramme.CurrentProg = (string)item["Current_Programme"];
                            registeredProgramme.CurrentProgDesc = (string)item["Programme_Name"];
                        }
                    }
                    List<DropdownList> dropdownLists = new List<DropdownList>();
                    string str3 = string.Concat("UnitSubject?$select=Unit_Category&$filter=ProgrammeCode eq '", registeredProgramme.CurrentProg, "' and Unit_Category ne ''&$format=json");
                    using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str3).GetResponseStream()))
                    {
                        foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader1.ReadToEnd())["value"])
                        {
                            DropdownList dropdownList = new DropdownList()
                            {
                                Value = (string)jObjects["Unit_Category"],
                                Text = (string)jObjects["Unit_Category"]
                            };
                            dropdownLists.Add(dropdownList);
                        }
                    }
                    List<DropdownList> dropdownLists1 = new List<DropdownList>();
                    string str4 = string.Concat(new string[] { "CourseReg?$select=Stage&$filter=StudentNo eq '", str, "' and Programme eq '", registeredProgramme.CurrentProg, "' and Reversed eq false&format=json" });
                    using (StreamReader streamReader2 = new StreamReader(Credentials.GetOdataData(str4).GetResponseStream()))
                    {
                        foreach (JObject item1 in (IEnumerable<JToken>)JObject.Parse(streamReader2.ReadToEnd())["value"])
                        {
                            DropdownList dropdownList1 = new DropdownList()
                            {
                                Value = (string)item1["Stage"],
                                Text = (string)item1["Stage"]
                            };
                            dropdownLists1.Add(dropdownList1);
                        }
                    }
                    List<CourseReg> courseRegs = new List<CourseReg>();
                    string str5 = "";
                    if (T == "N")
                    {
                        str5 = string.Concat(new string[] { "CourseReg?$filter=StudentNo eq '", str, "' and Programme eq '", registeredProgramme.CurrentProg, "' and Reversed eq false and (Registerfor eq 'Stage' or Registerfor eq 'Unit/Subject')&$format=json" });
                    }
                    if (T == "S")
                    {
                        str5 = string.Concat(new string[] { "CourseReg?$filter=StudentNo eq '", str, "' and Programme eq '", registeredProgramme.CurrentProg, "' and Reversed eq false and Registerfor eq 'Supplementary'&$format=json" });
                    }
                    if (T == "R")
                    {
                        str5 = string.Concat(new string[] { "CourseReg?$filter=StudentNo eq '", str, "' and Programme eq '", registeredProgramme.CurrentProg, "' and Reversed eq false and Registerfor eq 'Retake'&$format=json" });
                    }
                    using (StreamReader streamReader3 = new StreamReader(Credentials.GetOdataData(str5).GetResponseStream()))
                    {
                        foreach (JObject jObjects1 in (IEnumerable<JToken>)JObject.Parse(streamReader3.ReadToEnd())["value"])
                        {
                            CourseReg courseReg = new CourseReg()
                            {
                                RegID = (string)jObjects1["RegTransactonID"],
                                Programme = registeredProgramme.CurrentProg,
                                Stage = (string)jObjects1["Stage"],
                                Semester = (string)jObjects1["Semester"],
                                Register_for = (string)jObjects1["Registerfor"],
                                Settlement_Type = (string)jObjects1["SettlementType"],
                                Registration_Date = (string)jObjects1["RegDate"],
                                UnitsTaken = (string)jObjects1["UnitsTaken"],
                                TotalBilled = (string)jObjects1["TotalBilled"]
                            };
                            courseRegs.Add(courseReg);
                        }
                    }
                    List<StudentUnits> studentUnits = new List<StudentUnits>();
                    string str6 = string.Concat(new string[] { "BasketUnits?$filter=Student_No eq '", str, "' and Semester eq '", str1, "' and Submitted eq false and Unit ne ''&$format=json" });
                    using (StreamReader streamReader4 = new StreamReader(Credentials.GetOdataData(str6).GetResponseStream()))
                    {
                        foreach (JObject item2 in (IEnumerable<JToken>)JObject.Parse(streamReader4.ReadToEnd())["value"])
                        {
                            StudentUnits studentUnit = new StudentUnits()
                            {
                                Programme = (string)item2["Programme"],
                                Unit = (string)item2["Unit"],
                                Unit_Name = (string)item2["Unit_Name"],
                                Stage = (string)item2["Stage"],
                                Semester = (string)item2["Semester"]
                            };
                            studentUnits.Add(studentUnit);
                        }
                    }
                    if ((base.Session["StudentDetails"] == null ? true : base.Session["CurrentSem"].ToString() == ""))
                    {
                        base.Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(str);
                    }
                    string[] strArrays = (string[])base.Session["StudentDetails"];
                    string hostelFromCourseReg = CommonClass.GetHostelFromCourseReg(str, str1);
                    action = this.PartialView("~/Views/Course/CourseRegistrationDetails.cshtml", new StudentCourseRegistration()
                    {
                        ListOfCourseRegistration =
                            from x in courseRegs
                            orderby x.Stage, x.Registration_Date
                            select x,
                        ListOfStudentBasketUnits = studentUnits,
                        RegProg = registeredProgramme,
                        HostelCode = hostelFromCourseReg,
                        Campus = strArrays[3],
                        RegT = T,
                        ListOfUnitCategory = (
                            from x in (
                                from x in dropdownLists
                                select new SelectListItem()
                                {
                                    Text = x.Text,
                                    Value = x.Value
                                }).DistinctBy<SelectListItem, string>((SelectListItem x) => x.Value)
                            orderby x.Value
                            select x).ToList<SelectListItem>(),
                        ListOfStages = (
                            from x in (
                                from x in dropdownLists1
                                select new SelectListItem()
                                {
                                    Text = x.Text,
                                    Value = x.Value
                                }).DistinctBy<SelectListItem, string>((SelectListItem x) => x.Value)
                            orderby x.Value
                            select x).ToList<SelectListItem>()
                    });
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

        public PartialViewResult CurrentRegistredUnits()
        {
            PartialViewResult partialViewResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                string str1 = "";
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                str1 = base.Session["CurrentSem"].ToString();
                List<StudentUnits> studentUnits = new List<StudentUnits>();
                string str2 = string.Concat(new string[] { "StudentUnits?$filter=Student_No eq '", str, "' and Semester eq '", str1, "' and Evaluated eq false&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str2).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        StudentUnits studentUnit = new StudentUnits()
                        {
                            Unit = (string)item["Unit"],
                            Unit_Name = (string)item["Unit_Description"],
                            Section = (string)item["Unit_Class_Code"]
                        };
                        studentUnits.Add(studentUnit);
                    }
                }
                partialViewResult = this.PartialView("~/Views/Course/CurrentRegisteredUnits.cshtml", new LecturerEvaluationUnits()
                {
                    ListOfRegisteredUnits = studentUnits
                });
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        public JsonResult DropBasketUnit(UnitFilters Filters)
        {
            JsonResult jsonResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                Credentials.ObjNav.DropUnitBasket(str, Filters.Unit, Filters.sem);
                jsonResult = base.Json(new { message = string.Concat("Unit ", Filters.Unit, " dropped successfully"), success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public JsonResult DropRegisteredUnit(UnitFilters Filters)
        {
            JsonResult jsonResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                string filters = "";
                if (Filters.prog != null)
                {
                    filters = Filters.prog;
                }
                Credentials.ObjNav.DropStudentUnits(str.Trim(), Filters.sem.Trim(), "", filters, Filters.Unit.Trim(), false);
                jsonResult = base.Json(new { message = string.Concat("Unit ", Filters.UnitName, " dropped successfully"), success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetCampusList()
        {
            JsonResult jsonResult;
            try
            {
                List<DimensionValues> dimensionValues = new List<DimensionValues>();
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData("DimensionValues?$filter=Global_Dimension_No_ eq 3&$format=json").GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        DimensionValues dimensionValue = new DimensionValues()
                        {
                            Code = (string)item["Code"],
                            Name = (string)item["Name"]
                        };
                        dimensionValues.Add(dimensionValue);
                    }
                }
                jsonResult = base.Json(new CampusList()
                {
                    ListOfCampus = (
                        from x in dimensionValues
                        select new SelectListItem()
                        {
                            Text = x.Name,
                            Value = x.Code
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

        protected bool GetClassCode(string RegNo)
        {
            bool flag = false;
            try
            {
                string str = string.Concat("CustomerList?$select=Class_Code&$filter=No eq '", RegNo, "' and Class_Code ne ''&format=json");
                HttpWebResponse odataData = Credentials.GetOdataData(str);
                StudentDetailView studentDetailView = new StudentDetailView();
                using (StreamReader streamReader = new StreamReader(odataData.GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() > 0)
                    {
                        flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public ActionResult GetCurrentRegisteredUnits(string RegType)
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    string str1 = base.Session["CurrentSem"].ToString();
                    string studentRegisteredProgramme = CommonClass.GetStudentRegisteredProgramme(str);
                    string[] studentCampus = CommonClass.GetStudentCampus(str);
                    string str2 = studentCampus[0];
                    string str3 = studentCampus[1];
                    List<StudentUnits> studentUnits = new List<StudentUnits>();
                    if (RegType == "8")
                    {
                        //string str4 = string.Concat(new string[] { "StudentUnits?$filter=Student_No eq '", str, "' and Grade eq 'X' and Programme eq '", studentRegisteredProgramme, "'&$format=json" });
                        string str4 = string.Concat(new string[] { "StudentUnits?$filter=Student_No eq '", str, "' and Released eq false and Programme eq '", studentRegisteredProgramme, "' and Semester ne '" + str1 + "'&$format=json" });
                        using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str4).GetResponseStream()))
                        {
                            foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                            {
                                string str5 = string.Concat(new string[] { "Timetable?$filter=Unit eq '", (string)item["Unit"], "' and Semester eq '", str1, "' and Mode_of_Study eq '", str3, "'&$format=json" });
                                using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str5).GetResponseStream()))
                                {
                                    JObject jObjects = JObject.Parse(streamReader1.ReadToEnd());
                                    if (jObjects["value"].Count<JToken>() > 0)
                                    {
                                        foreach (JObject item1 in (IEnumerable<JToken>)jObjects["value"])
                                        {
                                            StudentUnits studentUnit = new StudentUnits()
                                            {
                                                Programme = (string)item["Programme"],
                                                Unit = (string)item["Unit"],
                                                Unit_Name = (string)item["Unit_Description"],
                                                Stage = (string)item["Stage"],
                                                Semester = (string)item["Semester"],
                                                RegFor = (string)item["Register_for"]
                                            };
                                            studentUnits.Add(studentUnit);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if ((RegType == "11" ? true : RegType == "20"))
                    {
                        string str6 = string.Concat(new string[] { "StudentUnits?$filter=Student_No eq '", str, "' and Semester eq '", str1, "' and Programme eq '", studentRegisteredProgramme, "'&$format=json" });
                        using (StreamReader streamReader2 = new StreamReader(Credentials.GetOdataData(str6).GetResponseStream()))
                        {
                            foreach (JObject jObjects1 in (IEnumerable<JToken>)JObject.Parse(streamReader2.ReadToEnd())["value"])
                            {
                                StudentUnits studentUnit1 = new StudentUnits()
                                {
                                    Programme = (string)jObjects1["Programme"],
                                    Unit = (string)jObjects1["Unit"],
                                    Unit_Name = (string)jObjects1["Unit_Description"],
                                    Stage = (string)jObjects1["Stage"],
                                    Semester = (string)jObjects1["Semester"],
                                    RegFor = (string)jObjects1["Register_for"]
                                };
                                studentUnits.Add(studentUnit1);
                            }
                        }
                    }
                    action = this.PartialView("~/Views/Course/CourseRequisitionLines.cshtml", new GetRegisteredUnit()
                    {
                        ListOfRegUnit = studentUnits.DistinctBy<StudentUnits, string>((StudentUnits x) => x.Unit).ToList<StudentUnits>()
                    });
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

        public ActionResult GetExamChallengeUnits()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    List<DropdownList> dropdownLists = new List<DropdownList>();
                    string studentRegisteredProgramme = CommonClass.GetStudentRegisteredProgramme(str);
                    List<StudentUnits> studentUnits = new List<StudentUnits>();
                    string str1 = "";
                    if ((base.Session["CurrentSem"] == null ? true : base.Session["CurrentSem"].ToString() == ""))
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    str1 = base.Session["CurrentSem"].ToString();
                    Credentials.ObjNav.RefreshStudentAudit(str);
                    string str2 = string.Concat(new string[] { "StudentUnitsAudit?$select=Programme,Unit,Description,UnitType&$filter=StudentNo eq '", str, "' and Concentration eq '", studentRegisteredProgramme, "' and Progress_Status eq 'Future'&$format=json" });
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str2).GetResponseStream()))
                    {
                        JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                        if (jObjects["value"].Count<JToken>() > 0)
                        {
                            foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                            {
                                if (!this.StudentRegisteredUnitExists(str, (string)item["Programme"], (string)item["Unit"], str1))
                                {
                                    string str3 = string.Concat(new string[] { "Timetable?$filter=Unit eq '", (string)item["Unit"], "' and Semester eq '", str1, "'&$format=json" });
                                    using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str3).GetResponseStream()))
                                    {
                                        JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                                        if (jObjects1["value"].Count<JToken>() > 0)
                                        {
                                            foreach (JObject item1 in (IEnumerable<JToken>)jObjects1["value"])
                                            {
                                                DropdownList dropdownList = new DropdownList()
                                                {
                                                    Text = string.Concat((string)item1["Unit"], " - ", (string)item1["Unit_Description"]),
                                                    Value = (string)item1["Unit"]
                                                };
                                                dropdownLists.Add(dropdownList);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    action = this.PartialView("~/Views/Course/Partial Views/ExcemptionRequest.cshtml", new ProgrammeConcentration()
                    {
                        Code = "",
                        ListOfConcentration = (
                            from x in dropdownLists
                            select new SelectListItem()
                            {
                                Text = x.Text,
                                Value = x.Value
                            }).DistinctBy<SelectListItem, string>((SelectListItem x) => x.Value).ToList<SelectListItem>()
                    });
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

        protected List<DropdownList> GetListOfProgrammes(string Prog)
        {
            List<DropdownList> dropdownLists = new List<DropdownList>();
            try
            {
                string programmeCategory = CommonClass.GetProgrammeCategory(Prog);
                string str = string.Concat("ProgrammeList?$select=Code,Description&$filter=Category eq '", programmeCategory, "' and OldCarriculum eq false and Description ne ''&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        DropdownList dropdownList = new DropdownList()
                        {
                            Value = (string)item["Code"],
                            Text = (string)item["Description"]
                        };
                        dropdownLists.Add(dropdownList);
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return dropdownLists;
        }

        [HttpPost]
        public JsonResult GetPrerequisiteUnits(string Unit)
        {
            JsonResult jsonResult;
            try
            {
                string str = "Prerequisite Unit(s)=";
                string str1 = "";
                bool flag = false;
                string str2 = base.Session["Username"].ToString();
                string str3 = string.Concat(new string[] { "UnitPrerequisite?$select=Prerequisite_Unit&$filter=Unit eq '", Unit, "' and Prerequisite_Unit ne '", Unit, "' and Prerequisite_Unit ne ''&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str3).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        int num = 1;
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            string str4 = string.Concat(new string[] { "StudentUnitsAudit?$select=Unit&$filter=StudentNo eq '", str2, "' and Unit eq '", (string)item["Prerequisite_Unit"], "' and Progress_Status eq 'Future'&$format=json" });
                            using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str4).GetResponseStream()))
                            {
                                if (JObject.Parse(streamReader1.ReadToEnd())["value"].Count<JToken>() > 0)
                                {
                                    str1 = (num != 1 ? string.Concat(str1, ",", (string)item["Prerequisite_Unit"]) : (string)item["Prerequisite_Unit"]);
                                    flag = true;
                                }
                            }
                        }
                        str = string.Concat(str, str1);
                    }
                }
                jsonResult = base.Json(new { message = str, success = true, Found = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        protected List<DropdownList> GetProgrammeConcentration(string Prog, string Type)
        {
            List<DropdownList> dropdownLists = new List<DropdownList>();
            try
            {
                string str = string.Concat(new string[] { "ProgrammeConcentration?$filter=Programme_Code eq '", Prog, "' and Type eq '", Type, "' and Blocked eq false&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        DropdownList dropdownList = new DropdownList()
                        {
                            Text = (string)item["Description"],
                            Value = (string)item["Concentration_Code"]
                        };
                        dropdownLists.Add(dropdownList);
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return dropdownLists;
        }

        public JsonResult GetProgrammeList(string Categ)
        {
            JsonResult jsonResult;
            try
            {
                List<Programmes> programmes = new List<Programmes>();
                string str = string.Concat("ProgrammeList?$filter=Category eq '", Categ, "' and OldCarriculum eq false and Description ne ''&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        Programmes programme = new Programmes()
                        {
                            Code = (string)item["Code"],
                            Description = (string)item["Description"]
                        };
                        programmes.Add(programme);
                    }
                }
                jsonResult = base.Json(new
                {
                    programmeList = new ProgrammeList()
                    {
                        ListOfProgrammes = (
                        from x in programmes
                        select new SelectListItem()
                        {
                            Text = x.Description,
                            Value = x.Code
                        }).ToList<SelectListItem>()
                    },
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public ActionResult GetProgrammeTransit()
        {
            ActionResult actionResult;
            try
            {
                actionResult = base.PartialView("~/Views/Course/Partial Views/ProgrammeTransit.cshtml");
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                actionResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return actionResult;
        }

        protected List<DropdownList> GetProgrammeUnits(string Prog)
        {
            List<DropdownList> dropdownLists = new List<DropdownList>();
            try
            {
                string str = string.Concat("UnitSubject?$filter=ProgrammeCode eq '", Prog, "' and Unit_Category ne 'FREE ELECTIVES' and OldUnit eq false&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        DropdownList dropdownList = new DropdownList()
                        {
                            Text = string.Concat((string)item["Code"], " - ", (string)item["Desription"]),
                            Value = (string)item["Code"]
                        };
                        dropdownLists.Add(dropdownList);
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return dropdownLists;
        }

        public ActionResult GetProgramUnits(string RegT, string Prog, string Stage)
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    string str1 = "";
                    UnitSubject unitSubject = new UnitSubject();
                    List<CoreUnitSubject> coreUnitSubjects = new List<CoreUnitSubject>();
                    Error error = new Error();
                    bool flag = false;
                    bool flag1 = false;
                    if (base.Session["Username"] == null)
                    {
                        base.Response.Redirect(base.Url.Action("Login", "Login"));
                    }
                    string str2 = "";
                    if ((base.Session["CurrentSem"] == null ? true : base.Session["CurrentSem"].ToString() == ""))
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    str2 = base.Session["CurrentSem"].ToString();
                    string[] strArrays = CommonClass.CurrentCourseRegistration(str, str2, RegT);
                    Credentials.ObjNav.RefreshStudentAudit(str);
                    if ((strArrays[0] == null ? true : strArrays[1] == null))
                    {
                        if ((RegT == "0" ? true : RegT == "1"))
                        {
                            flag1 = Credentials.ObjNav.StudentSelfPromotion(str, Prog);
                        }
                        if ((RegT == "2" ? true : RegT == "3"))
                        {
                            flag1 = Credentials.ObjNav.StudentCourseRegistration(str, Stage, str2, Convert.ToInt32(RegT));
                        }
                        strArrays = CommonClass.CurrentCourseRegistration(str, str2, RegT);
                    }
                    //Credentials.ObjNav.TestRegistrationStartDate(str, str2);
                    if ((strArrays[0] == null ? false : strArrays[1] != null))
                    {
                        int item = 0;
                        int num = 0;
                        int num1 = 0;
                        string[] studentCampus = CommonClass.GetStudentCampus(str);
                        string str3 = studentCampus[0];
                        string str4 = studentCampus[1];
                        //if ((RegT == "2" && RegT != "3"))
                        if (RegT == "2")
                        {
                            string str7 = string.Concat(new string[] { "StudentUnits?$filter=Student_No eq '", str, "' and Released eq true and Programme eq '", Prog, "' and Stage eq '", Stage, "' and Failed eq true&$format=json" });
                            using (StreamReader streamReader2 = new StreamReader(Credentials.GetOdataData(str7).GetResponseStream()))
                            {
                                JObject jObjects2 = JObject.Parse(streamReader2.ReadToEnd());
                                if (jObjects2["value"].Count<JToken>() > 0)
                                {
                                    foreach (JObject item3 in (IEnumerable<JToken>)jObjects2["value"])
                                    {
                                        if (!this.StudentExistInStudentUnitBakset(str, (string)item3["Code"], str2))
                                        {
                                            string str6 = string.Concat(new string[] { "Timetable?$filter=Unit eq '", (string)item3["Unit"], "' and Semester eq '", str2, "' and Mode_of_Study eq '", str4, "'&$format=json" });
                                            using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str6).GetResponseStream()))
                                            {
                                                JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                                                if (jObjects1["value"].Count<JToken>() > 0)
                                                {
                                                    foreach (JObject item2 in (IEnumerable<JToken>)jObjects1["value"])
                                                    {
                                                        CoreUnitSubject coreUnitSubject = new CoreUnitSubject();
                                                        coreUnitSubject.Code = (string)item3["Unit"];
                                                        coreUnitSubject.Desription = (string)item3["Unit_Description"];
                                                        coreUnitSubject.Day = (string)item2["Day_of_Week"];
                                                        coreUnitSubject.Period = (string)item2["Period"];
                                                        coreUnitSubject.Class = (string)item2["Unit_Class"];
                                                        coreUnitSubject.Lec = (string)item2["Lecturer_Name"];
                                                        coreUnitSubject.CF = "1";
                                                        coreUnitSubject.Campus = (string)item2["Campus_Code"];
                                                        coreUnitSubject.Room = (string)item2["Lecture_Room"];
                                                        coreUnitSubjects.Add(coreUnitSubject);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            string str5 = string.Concat("UnitSubject?$select=ProgrammeCode,Code,Desription&$filter=ProgrammeCode eq '", strArrays[0], "' and OldUnit eq false&$format=json");
                            using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str5).GetResponseStream()))
                            {
                                JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                                if (jObjects["value"].Count<JToken>() > 0)
                                {
                                    foreach (JObject item1 in (IEnumerable<JToken>)jObjects["value"])
                                    {
                                        if (!this.StudentRegisteredUnitExists(str, (string)item1["ProgrammeCode"], (string)item1["Code"], str2))
                                        {
                                            string str6 = string.Concat(new string[] { "Timetable?$filter=Unit eq '", (string)item1["Code"], "' and Semester eq '", str2, "' and Mode_of_Study eq '", str4, "'&$format=json" });
                                            using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str6).GetResponseStream()))
                                            {
                                                JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                                                if (jObjects1["value"].Count<JToken>() > 0)
                                                {
                                                    foreach (JObject item2 in (IEnumerable<JToken>)jObjects1["value"])
                                                    {
                                                        CoreUnitSubject coreUnitSubject = new CoreUnitSubject();
                                                        bool flag2 = false;
                                                        flag2 = CommonClass.UnitHasPreliquisites(str, (string)item1["Unit"]);
                                                        coreUnitSubject.Code = (string)item1["Code"];
                                                        coreUnitSubject.Desription = (string)item1["Desription"];
                                                        coreUnitSubject.Day = (string)item2["Day_of_Week"];
                                                        coreUnitSubject.Period = (string)item2["Period"];
                                                        coreUnitSubject.Class = (string)item2["Unit_Class"];
                                                        coreUnitSubject.Lec = (string)item2["Lecturer_Name"];
                                                        coreUnitSubject.CF = "1";
                                                        coreUnitSubject.Campus = (string)item2["Campus_Code"];
                                                        coreUnitSubject.Room = (string)item2["Lecture_Room"];
                                                        coreUnitSubjects.Add(coreUnitSubject);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        string str8 = string.Concat(new string[] { "Programme_Study_Modes?$select=Minimum_Units,Maximum_Units&$filter=Code eq '", strArrays[0], "' and Student_Type eq '", strArrays[5], "'&$format=json" });
                        using (StreamReader streamReader3 = new StreamReader(Credentials.GetOdataData(str8).GetResponseStream()))
                        {
                            JObject jObjects3 = JObject.Parse(streamReader3.ReadToEnd());
                            if (jObjects3["value"].Count<JToken>() > 0)
                            {
                                foreach (JObject item4 in (IEnumerable<JToken>)jObjects3["value"])
                                {
                                    item = (int)item4["Minimum_Units"];
                                    num = (int)item4["Maximum_Units"];
                                }
                            }
                        }
                        string str9 = string.Concat(new string[] { "BasketUnits?$select=Unit&$filter=Student_No eq '", str, "' and Semester eq '", str2, "'&$format=json" });
                        using (StreamReader streamReader4 = new StreamReader(Credentials.GetOdataData(str9).GetResponseStream()))
                        {
                            JObject jObjects4 = JObject.Parse(streamReader4.ReadToEnd());
                            if (jObjects4["value"].Count<JToken>() > 0)
                            {
                                num1 += jObjects4["value"].Count<JToken>();
                            }
                        }
                        unitSubject = new UnitSubject()
                        {
                            ShowClass = flag,
                            MinimumCourses = item,
                            MaximumCourses = num,
                            SelectedCourses = num1,
                            RegT = RegT,
                            Stage = strArrays[1],
                            ListOfCoreUnitsSubjects = (
                                from x in coreUnitSubjects.DistinctBy((CoreUnitSubject x) => new { Code = x.Code, Day = x.Day, Period = x.Period, Class = x.Class })
                                orderby x.Code
                                select x).ToList<CoreUnitSubject>()
                        };
                        str1 = "~/Views/Course/GetProgramUnits.cshtml";
                        flag1 = true;
                    }
                    else
                    {
                        error.Message = "You have not been registered in the current semester";
                        str1 = "~/Views/Shared/Partial Views/ErroMessangeView.cshtml";
                        flag1 = false;
                    }
                    action = (!flag1 ? this.PartialView(str1, error) : this.PartialView(str1, unitSubject));
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error1 = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                action = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error1);
            }
            return action;
        }

        public PartialViewResult GetProgramUnitsWithOptions(string Option, string Type)
        {
            PartialViewResult partialViewResult;
            try
            {
                UnitSubject unitSubject = new UnitSubject();
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    string str1 = "";
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    str1 = base.Session["CurrentSem"].ToString();
                    if (base.Session["CurrentProgDetails"] == null)
                    {
                        base.Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(str, str1, "0");
                    }
                    string[] item = (string[])base.Session["CurrentProgDetails"];
                    List<CoreUnitSubject> coreUnitSubjects = new List<CoreUnitSubject>();
                    List<ElectiveUnitSubject> electiveUnitSubjects = new List<ElectiveUnitSubject>();
                    string str2 = string.Concat(new string[] { "UnitsSubjects?$filter=Programme_Code eq '", item[0], "' and Stage_Code eq '", item[1], "' and (Programme_Option eq '' or Programme_Option eq '", Option, "') and Old_Unit eq false&format=json" });
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str2).GetResponseStream()))
                    {
                        foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            CoreUnitSubject coreUnitSubject = new CoreUnitSubject();
                            ElectiveUnitSubject electiveUnitSubject = new ElectiveUnitSubject();
                            if (!this.StudentRegisteredUnitExists(str, (string)jObjects["Programme_Code"], (string)jObjects["Code"], str1))
                            {
                                if ((string)jObjects["Unit_Type"] != "Elective")
                                {
                                    coreUnitSubject.Code = (string)jObjects["Code"];
                                    coreUnitSubject.Desription = (string)jObjects["Desription"];
                                    coreUnitSubjects.Add(coreUnitSubject);
                                }
                                else
                                {
                                    electiveUnitSubject.Code = (string)jObjects["Code"];
                                    electiveUnitSubject.Desription = (string)jObjects["Desription"];
                                    electiveUnitSubjects.Add(electiveUnitSubject);
                                }
                            }
                        }
                    }
                    unitSubject = new UnitSubject()
                    {
                        ListOfCoreUnitsSubjects = coreUnitSubjects.DistinctBy<CoreUnitSubject, string>((CoreUnitSubject x) => x.Code).ToList<CoreUnitSubject>(),
                        ListOfElectiveUnitsSubjects = electiveUnitSubjects.DistinctBy<ElectiveUnitSubject, string>((ElectiveUnitSubject x) => x.Code).ToList<ElectiveUnitSubject>()
                    };
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                partialViewResult = this.PartialView("~/Views/Course/GetProgramUnits.cshtml", unitSubject);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        public int GetRegLinesCounter(string DocNo)
        {
            int item = 0;
            try
            {
                string str = string.Concat("StudentRequisitionLines?$select=Line_No&$count=true&$filter=Application_No eq '", DocNo, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    item = (int)jObjects["@odata.count"];
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public PartialViewResult GetRequistionLines(string DocNo, string Status, string RegT)
        {
            PartialViewResult partialViewResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                List<StudentReqLines> studentReqLines = new List<StudentReqLines>();
                string str = "";
                if (Status == "Approved")
                {
                    str = string.Concat("StudentRequisitionLines?$filter=Application_No eq '", DocNo, "' and Approved eq true&$format=json");
                }
                str = (Status != "Rejected" ? string.Concat("StudentRequisitionLines?$filter=Application_No eq '", DocNo, "'&$format=json") : string.Concat("StudentRequisitionLines?$filter=Application_No eq '", DocNo, "' and Approved eq false&$format=json"));
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        StudentReqLines studentReqLine = new StudentReqLines()
                        {
                            AppNo = (string)item["Application_No"],
                            Unit = (string)item["Unit_Code"],
                            UnitName = (string)item["Unit_Title"],
                            Lecturer = (string)item["Lecture_Name"],
                            Section = (string)item["Section"]
                        };
                        studentReqLines.Add(studentReqLine);
                    }
                }
                partialViewResult = this.PartialView("~/Views/Course/RequisitionLines.cshtml", new RequisitionLines()
                {
                    ListLines = studentReqLines,
                    RegT = RegT,
                    Status = Status
                });
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        public ActionResult GetUnitsForExemptions()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    base.Session["Username"].ToString();
                    List<DropdownList> dropdownLists = new List<DropdownList>();
                    string studentRegisteredProgramme = CommonClass.GetStudentRegisteredProgramme(base.Session["Username"].ToString());
                    dropdownLists = this.GetProgrammeUnits(studentRegisteredProgramme);
                    action = this.PartialView("~/Views/Course/Partial Views/ExcemptionRequest.cshtml", new ProgrammeConcentration()
                    {
                        Code = "",
                        ListOfConcentration = (
                            from x in dropdownLists
                            select new SelectListItem()
                            {
                                Text = x.Text,
                                Value = x.Value
                            }).DistinctBy<SelectListItem, string>((SelectListItem x) => x.Value).ToList<SelectListItem>()
                    });
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

        public ActionResult GetUnitsWithMissingGrades()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    string studentRegisteredProgramme = CommonClass.GetStudentRegisteredProgramme(str);
                    List<StudentUnits> studentUnits = new List<StudentUnits>();
                    string str1 = string.Concat(new string[] { "StudentUnits?$filter=Student_No eq '", str, "' and Programme eq '", studentRegisteredProgramme, "' and (Grade eq 'X' or Grade eq 'F' or Grade eq 'E') and &format=json" });
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            StudentUnits studentUnit = new StudentUnits()
                            {
                                Unit = (string)item["Unit"],
                                Unit_Name = (string)item["Unit_Description"]
                            };
                            studentUnits.Add(studentUnit);
                        }
                    }
                    action = this.PartialView("~/Views/Course/CourseRequisitionLines.cshtml", new GetRegisteredUnit()
                    {
                        ListOfRegUnit = studentUnits.DistinctBy<StudentUnits, string>((StudentUnits x) => x.Unit).ToList<StudentUnits>()
                    });
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

        public ActionResult GraduationRequest()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] == null)
                {
                    action = base.RedirectToAction("Login", "Login");
                }
                else if (!CommonClass.AllowGraduationApplication())
                {
                    Error error = new Error()
                    {
                        Message = "Graduation request not active at the moment"
                    };
                    action = base.View("~/Views/Shared/ErrorMessange.cshtml", error);
                }
                else
                {
                    string str = base.Session["Username"].ToString();
                    if (CommonClass.AllowGradClearanceApplication(str))
                    {
                        StudentDetailView studentDetailView = new StudentDetailView();
                        string str1 = string.Concat("GraduationRequest?$filter=StudentNo eq '", str, "'&format=json");
                        using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                        {
                            JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                            string item = "";
                            if (jObjects["value"].Count<JToken>() <= 0)
                            {
                                string str2 = string.Concat("CustomerList?$filter=No eq '", str, "'&format=json");
                                using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str2).GetResponseStream()))
                                {
                                    foreach (JObject item1 in (IEnumerable<JToken>)JObject.Parse(streamReader1.ReadToEnd())["value"])
                                    {
                                        studentDetailView.No = (string)item1["No"];
                                        studentDetailView.Name = (string)item1["Name"];
                                        studentDetailView.ID_No = (string)item1["ID_No"];
                                        studentDetailView.Phone_No = (string)item1["Phone_No"];
                                        studentDetailView.Address = (string)item1["Address"];
                                        studentDetailView.E_Mail = (string)item1["E_Mail"];
                                        studentDetailView.Prog = (string)item1["Current_Programme"];
                                        studentDetailView.ProgName = (string)item1["Programme_Name"];
                                        studentDetailView.MadeRequest = false;
                                    }
                                }
                            }
                            else
                            {
                                foreach (JObject jObjects1 in (IEnumerable<JToken>)jObjects["value"])
                                {
                                    studentDetailView.DocNo = (string)jObjects1["Code"];
                                    studentDetailView.No = (string)jObjects1["StudentNo"];
                                    studentDetailView.Name = (string)jObjects1["Names"];
                                    studentDetailView.ID_No = (string)jObjects1["IDNumber"];
                                    studentDetailView.Phone_No = (string)jObjects1["Telephone"];
                                    studentDetailView.Address = (string)jObjects1["Address"];
                                    studentDetailView.E_Mail = (string)jObjects1["Email"];
                                    studentDetailView.Balance = CommonClass.GetStudentBalance(str);
                                    item = (string)jObjects1["Programme"];
                                    if (item == "")
                                    {
                                        item = CommonClass.GetStudentRegisteredProgramme(str);
                                    }
                                    studentDetailView.Prog = item;
                                    studentDetailView.ProgName = CommonClass.GetProgrammeName(item);
                                    studentDetailView.PersonalMail = (string)jObjects1["PersonalEmail"];
                                    studentDetailView.Profession = (string)jObjects1["Currentprofession"];
                                    studentDetailView.Company = (string)jObjects1["CurrentInstitustionCompany"];
                                    studentDetailView.CurrentPhoneNo = (string)jObjects1["Current_Phone_No"];
                                    studentDetailView.Gown = (string)jObjects1["Gown_Required"];
                                    studentDetailView.CollectionPoint = (string)jObjects1["Gown_Collection_Campus"];
                                    studentDetailView.MadeRequest = true;
                                }
                            }
                        }
                        action = base.View(studentDetailView);
                    }
                    else
                    {
                        Error error1 = new Error()
                        {
                            Message = "You are not in the graduating List"
                        };
                        action = base.View("~/Views/Shared/ErrorMessange.cshtml", error1);
                    }
                }
            }
            catch (Exception exception)
            {
                Error error2 = new Error()
                {
                    Message = exception.Message
                };
                action = base.View("~/Views/Shared/ErrorMessange.cshtml", error2);
            }
            return action;
        }

        public ActionResult GraduationRequestInfo(string DocNo, decimal Bal)
        {
            ActionResult actionResult;
            try
            {
                GradRequestInfomation gradRequestInfomation = new GradRequestInfomation()
                {
                    ReqNo = DocNo,
                    Balance = Bal
                };
                actionResult = this.PartialView("~/Views/Shared/Partial Views/GraduationReqInfo.cshtml", gradRequestInfomation);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                actionResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return actionResult;
        }

        protected bool HasPrerequisiteUnitBeenDone(string stdNo, string Prog, string Unit, string Sem)
        {
            bool flag = false;
            try
            {
                string str = string.Concat(new string[] { "StudentUnits?$filter=Student_No eq '", stdNo, "' and Programme eq '", Prog, "' and Unit eq '", Unit, "'&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    jObjects["value"].Count<JToken>();
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        private bool InsertPaymentPlan(string STDNo, string Sem, decimal StdBal)
        {
            bool flag = false;
            try
            {
                string str = string.Concat("StudentPaymentPlanSetup?$select=Class_Code&$filter=Semester eq '", Sem, "'&$orderby=Installment_No asc&format=json");
                HttpWebResponse odataData = Credentials.GetOdataData(str);
                StudentDetailView studentDetailView = new StudentDetailView();
                using (StreamReader streamReader = new StreamReader(odataData.GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        decimal num = new decimal();
                        decimal num1 = new decimal();
                        decimal stdBal = new decimal();
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            string item1 = (string)item["Installment_No"];
                            string str1 = (string)item["Due_Date"];
                            string item2 = (string)item["Installment_Percentage"];
                            if ((str1 == "" ? false : item2 != ""))
                            {
                                num1 = Convert.ToDecimal(item2) - num;
                                stdBal = StdBal * (num1 / new decimal(100));
                                DateTime dateTime = DateTime.ParseExact(str1.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                Credentials.ObjNav.InsertStudentPaymentPlan(STDNo, dateTime, Sem, item1, Convert.ToDecimal(item2), stdBal);
                                num = Convert.ToDecimal(item2);
                                item1 = "";
                                str1 = "";
                                item2 = "";
                            }
                        }
                        flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public ActionResult LecturerEvaluation()
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

        public PartialViewResult LecturerEvaluationForm(string Unit, string Sec)
        {
            PartialViewResult partialViewResult;
            try
            {
                string str = base.Session["Username"].ToString();
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                string str1 = base.Session["CurrentSem"].ToString();
                Lecturer lecturer = new Lecturer();
                List<Eval_Form> evalForms = new List<Eval_Form>();
                Error error = new Error();
                string[] studentCampus = CommonClass.GetStudentCampus(str);
                string str2 = studentCampus[0];
                string str3 = studentCampus[1];
                string str4 = string.Concat(new string[] { "Timetable?$filter=Semester eq '", str1, "' and Unit eq '", Unit, "' and Lecturer ne ''&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str4).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() <= 0)
                    {
                        error.Message = "No lecturer assigned this unit";
                        partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
                    }
                    else
                    {
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            lecturer.LecNo = (string)item["Lecturer"];
                            lecturer.LecName = (string)item["Lecturer_Name"];
                            lecturer.Unit = (string)item["Unit"];
                            lecturer.UnitName = (string)item["Unit_Description"];
                        }
                        using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData("LecEvaluationSections?$format=json").GetResponseStream()))
                        {
                            JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                            if (jObjects1["value"].Count<JToken>() > 0)
                            {
                                foreach (JObject item1 in (IEnumerable<JToken>)jObjects1["value"])
                                {
                                    Eval_Form evalForm = new Eval_Form()
                                    {
                                        Category = (string)item1["Code"],
                                        order = (int)item1["Order"]
                                    };
                                    List<LecEvaluationQuiz> lecEvaluationQuizzes = new List<LecEvaluationQuiz>();
                                    string str5 = string.Concat("EvaluationQuiz?$filter=Section eq '", evalForm.Category, "'&$format=json");
                                    using (StreamReader streamReader2 = new StreamReader(Credentials.GetOdataData(str5).GetResponseStream()))
                                    {
                                        JObject jObjects2 = JObject.Parse(streamReader2.ReadToEnd());
                                        if (jObjects2["value"].Count<JToken>() > 0)
                                        {
                                            foreach (JObject item2 in (IEnumerable<JToken>)jObjects2["value"])
                                            {
                                                LecEvaluationQuiz lecEvaluationQuiz = new LecEvaluationQuiz()
                                                {
                                                    Quiz = (string)item2["Description"],
                                                    Index = (int)item2["Question_Order"]
                                                };
                                                lecEvaluationQuizzes.Add(lecEvaluationQuiz);
                                            }
                                        }
                                    }
                                    evalForm.Eval_Quiz = (
                                        from x in lecEvaluationQuizzes
                                        orderby x.Index
                                        select x).ToList<LecEvaluationQuiz>();
                                    evalForms.Add(evalForm);
                                }
                            }
                        }
                        partialViewResult = this.PartialView("~/Views/Course/LecturerEvaluationForm.cshtml", new Evaluation_Form()
                        {
                            LecDet = lecturer,
                            Eval_Form_Quiz = (
                                from x in evalForms
                                orderby x.order
                                select x).ToList<Eval_Form>()
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                Error error1 = new Error()
                {
                    Message = exception.Message
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error1);
            }
            return partialViewResult;
        }

        public ActionResult NewClearanceRequest()
        {
            ActionResult actionResult;
            try
            {
                string str = base.Session["Username"].ToString();
                StudentDetailView studentDetailView = new StudentDetailView();
                IsClearanceRequest isClearanceRequest = CommonClass.StudentHasRaisedRequest(str, "Clearance");
                if (isClearanceRequest.Requested)
                {
                    studentDetailView.MadeRequest = true;
                    List<ApprovalCodes> approvalCodes = new List<ApprovalCodes>();
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData("ClearanceApprovalCodes?$filter=Approval_Type eq 'Student'&$format=json").GetResponseStream()))
                    {
                        JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                        if (jObjects["value"].Count<JToken>() > 0)
                        {
                            foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                            {
                                ApprovalCodes approvalCode = new ApprovalCodes()
                                {
                                    ApprovalCode = (string)item["Clearance_Level_Code"],
                                    Sequence = (string)item["Sequence"],
                                    Status = CommonClass.GetClearanceApprovalStatus(isClearanceRequest.ReqNo, (int)item["Sequence"])
                                };
                                string docRejectionComment = CommonClass.GetDocRejectionComment(isClearanceRequest.ReqNo, (int)item["Sequence"]);
                                if (docRejectionComment != "")
                                {
                                    approvalCode.CommentExist = true;
                                    approvalCode.Rejection_Comment = docRejectionComment;
                                }
                                approvalCodes.Add(approvalCode);
                            }
                        }
                    }
                    actionResult = base.View("~/Views/Course/ClearanceApprovalEntries.cshtml", new Clearance_Codes()
                    {
                        ApprovalCode = (
                            from x in approvalCodes
                            orderby Convert.ToInt32(x.Sequence)
                            select x).ToList<ApprovalCodes>(),
                        DocNo = isClearanceRequest.ReqNo,
                        print = isClearanceRequest.print
                    });
                }
                else if (!CommonClass.AllowClearanceApplication())
                {
                    Error error = new Error()
                    {
                        Message = "Clearance request not active at the moment"
                    };
                    actionResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
                }
                else if (CommonClass.AllowGradClearanceApplication(str))
                {
                    string str1 = string.Concat("CustomerList?$filter=No eq '", str, "'&format=json");
                    using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                    {
                        JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                        foreach (JObject item1 in (IEnumerable<JToken>)jObjects1["value"])
                        {
                            studentDetailView.No = (string)item1["No"];
                            studentDetailView.Name = (string)item1["Name"];
                            studentDetailView.ID_No = (string)item1["ID_No"];
                            studentDetailView.Gender = (string)item1["Gender"];
                            studentDetailView.Phone_No = (string)item1["Phone_No"];
                            studentDetailView.Address = (string)item1["Address"];
                            studentDetailView.E_Mail = (string)item1["E_Mail"];
                            studentDetailView.Prog = (string)item1["Current_Programme"];
                            studentDetailView.ProgName = (string)item1["Programme_Name"];
                            studentDetailView.MadeRequest = false;
                        }
                    }
                    actionResult = base.View("~/Views/Course/NewClearanceRequestForm.cshtml", studentDetailView);
                }
                else
                {
                    Error error1 = new Error()
                    {
                        Message = "You do not qualify to apply for clearance"
                    };
                    actionResult = base.View("~/Views/Shared/ErrorMessange.cshtml", error1);
                }
            }
            catch (Exception exception)
            {
                Error error2 = new Error()
                {
                    Message = exception.Message
                };
                actionResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error2);
            }
            return actionResult;
        }

        public PartialViewResult NewRequistion()
        {
            PartialViewResult partialViewResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                partialViewResult = base.PartialView("~/Views/Course/NewAcademicRequisition.cshtml");
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        public ActionResult OfficialTranscript()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
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
                                Text = CommonClass.GetProgrammeName((string)item["Programme"])
                            };
                            dropdownLists.Add(dropdownList);
                        }
                    }
                    action = this.PartialView("~/Views/Course/Partial Views/OfficalTranscript.cshtml", new DropdownListValues()
                    {
                        ListOfValues = (
                            from x in dropdownLists
                            select new SelectListItem()
                            {
                                Text = x.Text,
                                Value = x.Value
                            }).ToList<SelectListItem>()
                    });
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

        protected bool PrerequisiteUnit(string StdNo, string Prog, string Unit, string Sem)
        {
            bool flag = false;
            try
            {
                string str = string.Concat(new string[] { "UnitPrerequisite?$select=Prerequisite_Unit&$filter=Unit eq '", Unit, "' and Prerequisite_Unit ne '", Unit, "' and Prerequisite_Unit ne ''&format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            flag = this.HasPrerequisiteUnitBeenDone(StdNo, Prog, (string)item["Prerequisite"], Sem);
                            if (!flag)
                            {
                                flag = true;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult PrintClearanceForm()
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
                    str2 = string.Concat("CLEARANCEREPORT-", str2, ".pdf");
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
                jsonResult = base.Json(new { message = exception.Message, success = flag }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public PartialViewResult ProgrammeListViewMinorMajor(string type)
        {
            PartialViewResult partialViewResult;
            try
            {
                List<DropdownList> dropdownLists = new List<DropdownList>();
                string str = "";
                string str1 = "";
                if (type == "12")
                {
                    str = "MINOR";
                    str1 = "Minors";
                }
                else if (type == "13")
                {
                    str = "MAJOR";
                    str1 = "Majors";
                }
                else if (type == "14")
                {
                    str = "CONCEN";
                    str1 = "Concentrations";
                }
                else if (type != "2")
                {
                    str = "";
                    str1 = "";
                }
                else
                {
                    str = "PROG";
                    str1 = "List";
                }
                if (str == "")
                {
                    Error error = new Error()
                    {
                        Message = "Challenge encountered"
                    };
                    partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
                }
                else
                {
                    string studentRegisteredProgramme = CommonClass.GetStudentRegisteredProgramme(base.Session["Username"].ToString());
                    dropdownLists = (type != "2" ? this.GetProgrammeConcentration(studentRegisteredProgramme, str) : this.GetListOfProgrammes(studentRegisteredProgramme));
                    partialViewResult = this.PartialView("~/Views/Course/Partial Views/ProgrammeMajorMinor.cshtml", new ProgrammeConcentration()
                    {
                        Conc = str1,
                        Code = "",
                        RegType = type,
                        ListOfConcentration = (
                            from x in dropdownLists
                            select new SelectListItem()
                            {
                                Text = x.Text,
                                Value = x.Value
                            }).ToList<SelectListItem>()
                    });
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error1 = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error1);
            }
            return partialViewResult;
        }

        [HttpPost]
        public JsonResult RegisterSelectedUnits()
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                bool flag1 = false;
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    string str2 = "";
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    str2 = base.Session["CurrentSem"].ToString();
                    if (base.Session["CurrentProgDetails"] == null)
                    {
                        base.Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(str1, str2, "0");
                    }
                    string[] item = (string[])base.Session["CurrentProgDetails"];
                    if ((item[0] == null ? false : item[1] != null))
                    {
                        int num = 0;
                        int num1 = 0;
                        string str3 = string.Concat("Programme_Study_Modes?$select=Minimum_Units&$filter=Code eq '", item[0], "'&$format=json");
                        using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str3).GetResponseStream()))
                        {
                            JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                            if (jObjects["value"].Count<JToken>() > 0)
                            {
                                foreach (JObject item1 in (IEnumerable<JToken>)jObjects["value"])
                                {
                                    num = (int)item1["Minimum_Units"];
                                }
                            }
                        }
                        string str4 = string.Concat(new string[] { "BasketUnits?$select=Unit&$filter=Student_No eq '", str1, "' and Semester eq '", str2, "'&$format=json" });
                        using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str4).GetResponseStream()))
                        {
                            JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                            if (jObjects1["value"].Count<JToken>() > 0)
                            {
                                num1 += jObjects1["value"].Count<JToken>();
                            }
                        }
                        if (num1 >= num)
                        {
                            bool flag2 = Credentials.ObjNav.RegisterStudentUnits(str1, str2, "", item[0], 0, false);
                            flag = true;
                            if (!flag2)
                            {
                                str = "Units Registered successfully";
                            }
                            else
                            {
                                base.Session["SuccessMsg"] = "Units Registered and payment plan created successfully";
                                str = "/Financial/PaymentPlan";
                                flag1 = true;
                            }
                        }
                        else
                        {
                            str = string.Concat("Minimum Allowed No. of units is ", num.ToString());
                            flag = false;
                        }
                    }
                    else
                    {
                        str = "You have not selected any units to submit";
                        flag = false;
                    }
                }
                else
                {
                    str = "/Login/Login";
                    flag1 = true;
                }
                jsonResult = base.Json(new { message = str, success = flag, Redirect = flag1 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveEvalueatedUnit(Lecturer Lec, List<LecEvaluationQuiz> lecQuiz)
        {
            JsonResult jsonResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                string str1 = "";
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                str1 = base.Session["CurrentSem"].ToString();
                if (base.Session["CurrentProgDetails"] == null)
                {
                    base.Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(str, str1, "0");
                }
                string[] item = (string[])base.Session["CurrentProgDetails"];
                foreach (LecEvaluationQuiz lecEvaluationQuiz in lecQuiz)
                {
                    lecEvaluationQuiz.Quiz.Trim().Split(new char[] { '.' })[1].Trim();
                    Convert.ToDecimal(lecEvaluationQuiz.Score.Trim());
                }
                jsonResult = base.Json(new { message = string.Concat("Unit ", Lec.UnitName, " Evaluated successfully"), success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveSelectedUnits(List<UnitRegistration> UnitReg, string RegT, string Stage)
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str1 = base.Session["Username"].ToString();
                string str2 = "";
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                str2 = base.Session["CurrentSem"].ToString();
                if (base.Session["CurrentProgDetails"] == null)
                {
                    base.Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(str1, str2, RegT);
                }
                string[] item = (string[])base.Session["CurrentProgDetails"];
                if ((item[0] == null ? false : item[1] != null))
                {
                    int num = 0;
                    foreach (UnitRegistration unitReg in UnitReg)
                    {
                        string str3 = "";
                        string str4 = "";
                        string str5 = "";
                        string str6 = "";
                        string str7 = unitReg.UnitCode.Trim();
                        if (unitReg.ClassCode != null)
                        {
                            str3 = unitReg.ClassCode.Trim();
                        }
                        if (unitReg.Campus != null)
                        {
                            str4 = unitReg.Campus.Trim();
                        }
                        if (unitReg.Period != null)
                        {
                            str6 = unitReg.Period.Trim();
                        }
                        if (unitReg.Day != null)
                        {
                            str5 = unitReg.Day.Trim();
                        }
                        if ((RegT == "2" ? false : RegT != "3"))
                        {
                            Credentials.ObjNav.RegisterStudentUnitBasket(str1, str2, "", "", str7, 1, str3, str4, str5, str6);
                        }
                        else
                        {
                            Credentials.ObjNav.SubmitRetakeResitUnits(str1, str7, item[1], str2, Convert.ToInt32(RegT), "");
                        }
                        num++;
                    }
                    str = string.Concat(num.ToString(), " Units Selected successfully");
                    flag = true;
                }
                else
                {
                    str = "You have not been registered in the current semester";
                    flag = false;
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

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveStudentRequisition(StudentReqs regData)
        {
            JsonResult jsonResult;
            try
            {
                string stdNo = "";
                string semester = "";
                string str = "";
                string str1 = "";
                string str2 = "";
                string str3 = "";
                int num = 0;
                int num1 = 0;
                bool flag = false;
                if (regData.StdNo != null)
                {
                    stdNo = regData.StdNo;
                }
                if (regData.Semester != null)
                {
                    semester = regData.Semester;
                }
                if (regData.Requisition_Type != null)
                {
                    num = Convert.ToInt32(regData.Requisition_Type.Trim());
                }
                if ((regData.NoOfCopies == null ? false : regData.NoOfCopies != ""))
                {
                    num1 = Convert.ToInt32(regData.NoOfCopies);
                }
                if (regData.Remarks != null)
                {
                    str = regData.Remarks.Trim();
                }
                if (regData.Prog != null)
                {
                    str1 = regData.Prog.Trim();
                }
                if (regData.Campus != null)
                {
                    str3 = regData.Campus.Trim();
                }
                if (regData.Concentration != null)
                {
                    str2 = regData.Concentration.Trim();
                }
                if ((num == 2 || num == 12 || num == 13 ? true : num == 14))
                {
                    flag = true;
                }
                string str4 = Credentials.ObjNav.StudentRequisitionCreate(stdNo, num, str, str1, "", semester, str3, decimal.Zero, str2, flag, num1);
                jsonResult = base.Json(new { message = string.Concat("Requisition document No : ", str4, " Submited successfully"), success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveStudentRequisitionWithLines(StudentReqs regData, List<StudentReqLines> StdRegLines)
        {
            JsonResult jsonResult;
            try
            {
                string stdNo = "";
                string semester = "";
                string str = "";
                string str1 = "";
                int num = 0;
                bool flag = false;
                if (regData.StdNo != null)
                {
                    stdNo = regData.StdNo;
                }
                if (regData.Semester != null)
                {
                    semester = regData.Semester;
                }
                if (regData.Requisition_Type != null)
                {
                    num = Convert.ToInt32(regData.Requisition_Type.Trim());
                }
                if (regData.Remarks != null)
                {
                    str = regData.Remarks.Trim();
                }
                if (regData.Prog != null)
                {
                    str1 = regData.Prog.Trim();
                }
                if ((num == 8 || num == 12 || num == 13 ? true : num == 14))
                {
                    flag = true;
                }
                if ((num == 8 ? false : num != 18))
                {
                    string str2 = Credentials.ObjNav.StudentRequisitionCreate(stdNo, num, str, str1, "", semester, "", decimal.Zero, "", flag, 0);
                    foreach (StudentReqLines stdRegLine in StdRegLines)
                    {
                        string str3 = stdRegLine.Unit.Trim();
                        string str4 = stdRegLine.UnitName.Trim();
                        string str5 = "";
                        string equivalentUnit = "";
                        if (stdRegLine.EquivalentUnit != null)
                        {
                            equivalentUnit = stdRegLine.EquivalentUnit;
                        }
                        Credentials.ObjNav.StudentRequisitionLinesCreate(str2, str3, str4, str5, equivalentUnit);
                    }
                }
                else
                {
                    foreach (StudentReqLines studentReqLine in StdRegLines)
                    {
                        string str6 = studentReqLine.Unit.Trim();
                        string str7 = studentReqLine.UnitName.Trim();
                        string str8 = str;
                        Credentials.ObjNav.StudentRequisitionLinesInsertCreateDoc(stdNo, semester, str6, str7, str8, num, "");
                    }
                    Credentials.ObjNav.SendStudentRegBacthApproval(stdNo, semester, num);
                }
                jsonResult = base.Json(new { message = "Requisition Submited successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public ActionResult StudentEnquiryList()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    List<DimensionValues> dimensionValues = new List<DimensionValues>();
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData("DimensionValues?$filter=Global_Dimension_No_ eq 4&$format=json").GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            DimensionValues dimensionValue = new DimensionValues()
                            {
                                Code = (string)item["Code"],
                                Name = (string)item["Name"]
                            };
                            dimensionValues.Add(dimensionValue);
                        }
                    }
                    action = base.View(new DropdownListValues()
                    {
                        Code = "",
                        ListOfValues = (
                            from x in dimensionValues
                            select new SelectListItem()
                            {
                                Text = x.Name,
                                Value = x.Code
                            }).ToList<SelectListItem>()
                    });
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception)
            {
                Error error = new Error()
                {
                    Message = exception.Message
                };
                action = base.View("~/Views/Shared/ErrorMessange.cshtml", error);
            }
            return action;
        }

        public PartialViewResult StudentEnquiryListView()
        {
            PartialViewResult partialViewResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                List<Enquiry> enquiries = new List<Enquiry>();
                string str1 = string.Concat("StudentEnquiry?$filter=Student_No eq '", str, "'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        Enquiry enquiry = new Enquiry()
                        {
                            EnqNo = (string)item["Enquiry_No"],
                            OpenTo = (string)item["Department"],
                            EnquireFor = (string)item["Enquiry"]
                        };
                        DateTime dateTime = (DateTime)item["Date_Raised"];
                        enquiry.DateRequested = dateTime.ToString("dd/MM/yyyy");
                        if ((string)item["Response"] == "")
                        {
                            enquiry.Response = "No Response Yet";
                        }
                        else
                        {
                            enquiry.Response = (string)item["Response"];
                        }
                        enquiries.Add(enquiry);
                    }
                }
                partialViewResult = this.PartialView("~/Views/Course/EnquiryListView.cshtml", enquiries);
            }
            catch (Exception exception)
            {
                Error error = new Error()
                {
                    Message = exception.Message
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        protected bool StudentExistInStudentUnitBakset(string stdNo, string Unit, string sem)
        {
            bool flag = false;
            try
            {
                string str = string.Concat(new string[] { "BasketUnits?$filter=Student_No eq '", stdNo, "' and Unit eq '", Unit, "' and Semester eq '", sem, "'&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() > 0)
                    {
                        flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        protected bool StudentExistInStudentUnits(string stdNo, string Unit)
        {
            bool flag = false;
            try
            {
                string str = string.Concat(new string[] { "StudentUnits?$filter=Student_No eq '", stdNo, "' and Unit eq '", Unit, "' and Grade ne 'F' and Grade ne 'E' and Grade ne 'X' and Grade ne 'Z' and Grade ne 'W'&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() > 0)
                    {
                        flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public PartialViewResult StudentGradClearanceRequisitionLine(string RegType)
        {
            PartialViewResult partialViewResult;
            try
            {
                StudentReqs studentReq = new StudentReqs();
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                string str1 = string.Concat(new string[] { "StudentRequisition?$filter=StudentNo eq '", str, "' and RequisitionType eq '", RegType, "'&format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            studentReq.Code = (string)item["Code"];
                            studentReq.Date = (string)item["Date"];
                            studentReq.Requisition_Type = (string)item["RequisitionType"];
                            studentReq.Semester = (string)item["Semester"];
                            studentReq.Status = (string)item["Status"];
                            string[] strArrays = null;
                            string str2 = strArrays[0];
                            string str3 = strArrays[1];
                            if ((str2 == null ? false : str2 != ""))
                            {
                                studentReq.ApprovalLevel = str2;
                            }
                            else
                            {
                                studentReq.ApprovalLevel = "Fully Approved";
                                studentReq.CommentFound = false;
                            }
                            if ((str2 == null || !(str2 != "") || str3 == null ? true : str3 == ""))
                            {
                                studentReq.CommentFound = false;
                                studentReq.Comment = "";
                            }
                            else
                            {
                                studentReq.CommentFound = true;
                                studentReq.Comment = str3;
                            }
                        }
                    }
                }
                partialViewResult = this.PartialView("~/Views/Course/GradClearanceLine.cshtml", studentReq);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        protected bool StudentRegisteredUnitExists(string stdNo, string prog, string Unit, string sem)
        {
            bool flag = false;
            try
            {
                flag = this.StudentExistInStudentUnits(stdNo, Unit);
                if (!flag)
                {
                    flag = this.StudentExistInStudentUnitBakset(stdNo, Unit, sem);
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public PartialViewResult StudentRequisitionList()
        {
            PartialViewResult partialViewResult;
            try
            {
                List<StudentReqs> studentReqs = new List<StudentReqs>();
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                string str1 = string.Concat("StudentRequisitions?$filter=Student_No eq '", str, "' and Requisition_Type ne 'Clearance'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            StudentReqs studentReq = new StudentReqs()
                            {
                                Code = (string)item["Code"],
                                Date = (string)item["Date"],
                                Requisition_Type = (string)item["Requisition_Type"],
                                Semester = (string)item["Semester"],
                                Status = (string)item["Status"],
                                ApprovalCount = (int)item["Approval_Count"],
                                //LinesCounter = (string)item["Lines_Count"]
                            };
                            studentReq.ApprovalCount = (int)item["Approved_Lines_Count"];
                            studentReq.RejectedCount = (int)item["Rejected_Lines_Count"];
                            studentReqs.Add(studentReq);
                        }
                    }
                }
                partialViewResult = this.PartialView("~/Views/Course/StudentRequisitionList.cshtml", (
                    from x in studentReqs
                    orderby x.Code descending
                    select x).ToList<StudentReqs>());
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        public ActionResult StudentRequisitions()
        {
            ActionResult actionResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                if (base.Session["StudentDetails"] == null)
                {
                    base.Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(str);
                }
                string[] item = (string[])base.Session["StudentDetails"];
                string str1 = base.Session["CurrentSem"].ToString();
                actionResult = base.View(new StudentCourseDetails()
                {
                    StdNo = str,
                    Name = item[0],
                    CurrentSem = str1,
                    Prog = item[1],
                    Email = item[2]
                });
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                actionResult = base.View("~/Views/Common/ErrorMessage.cshtml", error);
            }
            return actionResult;
        }

        public ActionResult StudentTimeTable()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    Credentials.ObjNav.RefreshStudentAudit(str);
                    List<TimeTableView> timeTableViews = new List<TimeTableView>();
                    List<TimeTableView> timeTableViews1 = new List<TimeTableView>();
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    if (base.Session["StudentDetails"] == null)
                    {
                        base.Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(str);
                    }
                    string[] item = (string[])base.Session["StudentDetails"];
                    string str1 = base.Session["CurrentSem"].ToString();
                    string[] studentCampus = CommonClass.GetStudentCampus(str);
                    string str2 = studentCampus[0];
                    string str3 = studentCampus[1];
                    string str4 = string.Concat(new string[] { "StudentUnits?$select=Programme,Stage,Unit,Unit_Description,Unit_Class_Code,Campus&$filter=Student_No eq '", str, "' and Semester eq '", str1, "'&$format=json" });
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str4).GetResponseStream()))
                    {
                        JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                        if (jObjects["value"].Count<JToken>() > 0)
                        {
                            foreach (JObject item1 in (IEnumerable<JToken>)jObjects["value"])
                            {
                                string str5 = string.Concat(new string[] { "Timetable?$filter=Unit eq '", (string)item1["Unit"], "' and Semester eq '", str1, "' and Mode_of_Study eq '", str3, "'&$format=json" });
                                using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str5).GetResponseStream()))
                                {
                                    JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                                    if (jObjects1["value"].Count<JToken>() > 0)
                                    {
                                        foreach (JObject item2 in (IEnumerable<JToken>)jObjects1["value"])
                                        {
                                            if (((string)item2["Campus_Code"] == str2 ? true : (bool)item2["Multi_Campus"]))
                                            {
                                                TimeTableView timeTableView = new TimeTableView()
                                                {
                                                    Unit = (string)item2["Unit"],
                                                    Period = (string)item2["Period"],
                                                    Semester = (string)item2["Semester"],
                                                    Day_of_Week = (string)item2["Day_of_Week"],
                                                    Lecture_Room = (string)item2["Lecture_Room"],
                                                    Lecturer = (string)item2["Lecturer_Name"],
                                                    Campus = (string)item2["Campus_Code"],
                                                    Section = (string)item2["Unit_Class"],
                                                    Registered = "Registered"
                                                };
                                                timeTableViews.Add(timeTableView);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    string str6 = string.Concat("StudentUnitsAudit?$select=Unit,Description,Progress_Status&$filter=StudentNo eq '", str, "' and Progress_Status eq 'Future'&$format=json");
                    using (StreamReader streamReader2 = new StreamReader(Credentials.GetOdataData(str6).GetResponseStream()))
                    {
                        JObject jObjects2 = JObject.Parse(streamReader2.ReadToEnd());
                        if (jObjects2["value"].Count<JToken>() > 0)
                        {
                            foreach (JObject item3 in (IEnumerable<JToken>)jObjects2["value"])
                            {
                                string str7 = string.Concat(new string[] { "Timetable?$filter=Unit eq '", (string)item3["Unit"], "' and Semester eq '", str1, "' and Mode_of_Study eq '", str3, "'&$format=json" });
                                using (StreamReader streamReader3 = new StreamReader(Credentials.GetOdataData(str7).GetResponseStream()))
                                {
                                    JObject jObjects3 = JObject.Parse(streamReader3.ReadToEnd());
                                    if (jObjects3["value"].Count<JToken>() > 0)
                                    {
                                        foreach (JObject item4 in (IEnumerable<JToken>)jObjects3["value"])
                                        {
                                            if (((string)item4["Campus_Code"] == str2 ? true : (bool)item4["Multi_Campus"]))
                                            {
                                                TimeTableView timeTableView1 = new TimeTableView()
                                                {
                                                    Unit = (string)item4["Unit"],
                                                    Period = (string)item4["Period"],
                                                    Semester = (string)item4["Semester"],
                                                    Day_of_Week = (string)item4["Day_of_Week"],
                                                    Lecture_Room = (string)item4["Lecture_Room"],
                                                    Lecturer = (string)item4["Lecturer_Name"],
                                                    Campus = (string)item4["Campus_Code"],
                                                    Section = (string)item4["Unit_Class"],
                                                    Registered = "Future"
                                                };
                                                timeTableViews1.Add(timeTableView1);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    action = base.View(new TimeTableSummeryView()
                    {
                        RegisteredUnitsTimeT = timeTableViews,
                        FutureUnitsTimeT = timeTableViews1
                    });
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
                action = base.View("~/Views/Common/ErrorMessage.cshtml", error);
            }
            return action;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitClearanceRequest(StudentReqs regData)
        {
            JsonResult jsonResult;
            try
            {
                string stdNo = "";
                string str = "";
                string str1 = "";
                string str2 = "";
                string str3 = "";
                int num = 0;
                if (regData.StdNo != null)
                {
                    stdNo = regData.StdNo;
                }
                if (regData.Requisition_Type != null)
                {
                    num = Convert.ToInt32(regData.Requisition_Type.Trim());
                }
                if (regData.Prog != null)
                {
                    str2 = regData.Prog.Trim();
                }
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester("");
                }
                base.Session["CurrentSem"].ToString();
                string str4 = Credentials.ObjNav.StudentRequisitionCreate(stdNo, num, str1, str2, "", str, str3, decimal.Zero, "", true, 0);
                jsonResult = base.Json(new { message = string.Concat("Clearance Request document No : ", str4, " Submited successfully"), success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitEnquiry(string Dep, string Enquiry)
        {
            JsonResult jsonResult;
            try
            {
                if (base.Session["Username"] != null)
                {
                    base.Session["Username"].ToString();
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = "Enquiry send successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitGraduationRequest(GradRequest regData)
        {
            JsonResult jsonResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                string str1 = "";
                string str2 = "";
                string str3 = "";
                string str4 = "";
                string str5 = "";
                bool flag = false;
                if (regData.PersoanlEmail != null)
                {
                    str1 = regData.PersoanlEmail.Trim();
                }
                if (regData.CrrProf != null)
                {
                    str2 = regData.CrrProf.Trim();
                }
                if (regData.Company != null)
                {
                    str3 = regData.Company.Trim();
                }
                if (regData.CrrPhoneNo != null)
                {
                    str4 = regData.CrrPhoneNo.Trim();
                }
                decimal studentBalance = new decimal();
                string str6 = Credentials.ObjNav.fnSaveGraduation(str, str1, str2, str3, str4, flag, str5, "");
                studentBalance = CommonClass.GetStudentBalance(str);
                base.Session["SuccessMsg"] = string.Concat("Graduation Request document No : ", str6, " Submited successfully. Proceed and raise your clearance request. The link is on the sidebar");
                jsonResult = base.Json(new { Doc = str6, Balance = studentBalance, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitStudentExamChallengeRequest(StudentReqs regData, List<StudentReqLines> StdRegLines, string base64Upload, string fileName, string Extn)
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                if ((base64Upload == "" ? true : Extn == ""))
                {
                    str = "Attach results slip";
                    flag = false;
                }
                else
                {
                    string extension = Path.GetExtension(fileName);
                    if ((extension.ToLower() == ".pdf" || extension.ToLower() == ".docx" || extension.ToLower() == ".doc" || extension.ToLower() == ".xlsx" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".jpg" ? false : extension.ToLower() != ".png"))
                    {
                        str = "Only files with extensions(.pdf, .docx, .doc, .xlsx, .jpeg, .jpg, .png) can be uploaded";
                        flag = false;
                    }
                    else
                    {
                        string stdNo = "";
                        string semester = "";
                        string str1 = "";
                        string str2 = "";
                        int num = 0;
                        if (regData.StdNo != null)
                        {
                            stdNo = regData.StdNo;
                        }
                        if (regData.Semester != null)
                        {
                            semester = regData.Semester;
                        }
                        if (regData.Requisition_Type != null)
                        {
                            num = Convert.ToInt32(regData.Requisition_Type.Trim());
                        }
                        if (regData.Remarks != null)
                        {
                            str1 = regData.Remarks.Trim();
                        }
                        if (regData.Prog != null)
                        {
                            str2 = regData.Prog.Trim();
                        }
                        foreach (StudentReqLines stdRegLine in StdRegLines)
                        {
                            string str3 = stdRegLine.Unit.Trim();
                            string str4 = stdRegLine.UnitName.Trim();
                            string str5 = str1;
                            string equivalentUnit = "";
                            if (stdRegLine.EquivalentUnit != null)
                            {
                                equivalentUnit = stdRegLine.EquivalentUnit;
                            }
                            Credentials.ObjNav.StudentRequisitionLinesInsertCreateDoc(stdNo, semester, str3, str4, str5, num, equivalentUnit);
                        }
                        string str6 = base.Server.MapPath(string.Concat("~/Uploads/", fileName));
                        string str7 = string.Concat(new string[] { "StudentRequisitions?$select=Code&$filter=Student_No eq '", stdNo, "' and Requisition_Type eq 'Exam Challenge' and Semester eq '", semester, "' and Status eq 'Open'&$format=json" });
                        using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str7).GetResponseStream()))
                        {
                            JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                            if (jObjects["value"].Count<JToken>() <= 0)
                            {
                                str = "You have already requested for the Units";
                                flag = false;
                            }
                            else
                            {
                                foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                                {
                                    Credentials.UploadDocumentAttachment((string)item["Code"], base64Upload, str6, 70134894);
                                }
                                str = "Requisition Submited successfully";
                                flag = true;
                            }
                        }
                        Credentials.ObjNav.SendStudentRegBacthApproval(stdNo, semester, num);
                    }
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

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitStudentExemptionRequest(StudentReqs regData, List<StudentReqLines> StdRegLines, string base64Upload, string fileName, string Extn)
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                if ((base64Upload == "" ? true : Extn == ""))
                {
                    str = "Attach results slip";
                    flag = false;
                }
                else
                {
                    string extension = Path.GetExtension(fileName);
                    if ((extension.ToLower() == ".pdf" || extension.ToLower() == ".docx" || extension.ToLower() == ".doc" || extension.ToLower() == ".xlsx" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".jpg" ? false : extension.ToLower() != ".png"))
                    {
                        str = "Only files with extensions(.pdf, .docx, .doc, .xlsx, .jpeg, .jpg, .png) can be uploaded";
                        flag = false;
                    }
                    else
                    {
                        string stdNo = "";
                        string semester = "";
                        string str1 = "";
                        string str2 = "";
                        int num = 0;
                        if (regData.StdNo != null)
                        {
                            stdNo = regData.StdNo;
                        }
                        if (regData.Semester != null)
                        {
                            semester = regData.Semester;
                        }
                        if (regData.Requisition_Type != null)
                        {
                            num = Convert.ToInt32(regData.Requisition_Type.Trim());
                        }
                        if (regData.Remarks != null)
                        {
                            str1 = regData.Remarks.Trim();
                        }
                        if (regData.Prog != null)
                        {
                            str2 = regData.Prog.Trim();
                        }
                        foreach (StudentReqLines stdRegLine in StdRegLines)
                        {
                            string str3 = stdRegLine.Unit.Trim();
                            string str4 = stdRegLine.UnitName.Trim();
                            string str5 = str1;
                            string equivalentUnit = "";
                            if (stdRegLine.EquivalentUnit != null)
                            {
                                equivalentUnit = stdRegLine.EquivalentUnit;
                            }
                            Credentials.ObjNav.StudentRequisitionLinesInsertCreateDoc(stdNo, semester, str3, str4, str5, num, equivalentUnit);
                        }
                        string str6 = base.Server.MapPath(string.Concat("~/Uploads/", fileName));
                        string str7 = string.Concat(new string[] { "StudentRequisitions?$select=Code&$filter=Student_No eq '", stdNo, "' and Requisition_Type eq 'Exemption' and Semester eq '", semester, "' and Status eq 'Open'&$format=json" });
                        using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str7).GetResponseStream()))
                        {
                            JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                            if (jObjects["value"].Count<JToken>() <= 0)
                            {
                                str = "You have already requested for the Units";
                                flag = false;
                            }
                            else
                            {
                                foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                                {
                                    Credentials.UploadDocumentAttachment((string)item["Code"], base64Upload, str6, 70134894);
                                }
                                str = "Requisition Submited successfully";
                                flag = true;
                            }
                        }
                        Credentials.ObjNav.SendStudentRegBacthApproval(stdNo, semester, num);
                    }
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

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SubmitStudentProgrammeChangeRequest(StudentReqs regData, string base64Upload, string fileName, string Extn)
        {
            JsonResult jsonResult;
            try
            {
                string str = "";
                bool flag = false;
                if ((base64Upload == "" ? true : Extn == ""))
                {
                    str = "Attach results slip";
                    flag = false;
                }
                else
                {
                    string extension = Path.GetExtension(fileName);
                    if ((extension.ToLower() == ".pdf" || extension.ToLower() == ".docx" || extension.ToLower() == ".doc" || extension.ToLower() == ".xlsx" || extension.ToLower() == ".jpeg" || extension.ToLower() == ".jpg" ? false : extension.ToLower() != ".png"))
                    {
                        str = "Only files with extensions(.pdf, .docx, .doc, .xlsx, .jpeg, .jpg, .png) can be uploaded";
                        flag = false;
                    }
                    else
                    {
                        string stdNo = "";
                        string semester = "";
                        string str1 = "";
                        string str2 = "";
                        int num = 0;
                        bool flag1 = true;
                        if (regData.StdNo != null)
                        {
                            stdNo = regData.StdNo;
                        }
                        if (regData.Semester != null)
                        {
                            semester = regData.Semester;
                        }
                        if (regData.Requisition_Type != null)
                        {
                            num = Convert.ToInt32(regData.Requisition_Type.Trim());
                        }
                        if (regData.Remarks != null)
                        {
                            str1 = regData.Remarks.Trim();
                        }
                        if (regData.Prog != null)
                        {
                            str2 = regData.Prog.Trim();
                        }
                        string str3 = Credentials.ObjNav.StudentRequisitionCreate(stdNo, num, str1, str2, "", semester, "", decimal.Zero, "", flag1, 0);
                        string str4 = base.Server.MapPath(string.Concat("~/Uploads/", fileName));
                        string str5 = Credentials.UploadDocumentAttachment(str3, base64Upload, str4, 70134894);
                        if (str5 != "SUCCESS")
                        {
                            str = str5;
                            flag = false;
                        }
                        else
                        {
                            str = string.Concat("Requisition document No : ", str3, " Submited successfully");
                            flag = true;
                        }
                    }
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

        [HttpPost]
        public JsonResult TestStudentTimeTable(string Unit, string Campus, string ClassCode, string day, string period)
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                string str = "";
                if ((base.Session["CurrentSem"] == null ? true : base.Session["CurrentSem"].ToString() == ""))
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                str = base.Session["CurrentSem"].ToString();
                jsonResult = base.Json(new { message = "", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        protected bool UnitInTimeTable(string stdNo, string Prog, string Unit, string Semester)
        {
            bool flag = false;
            try
            {
                string[] studentCampus = CommonClass.GetStudentCampus(stdNo);
                string str = studentCampus[0];
                string str1 = studentCampus[1];
                string str2 = string.Concat(new string[] { "Timetable?$filter=Unit eq '", Unit, "' and Semester eq '", Semester, "' and Mode_of_Study eq '", str1, "'&format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str2).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            if (!this.PrerequisiteUnit(stdNo, Prog, Unit, Semester))
                            {
                                flag = true;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public ActionResult UnitsNotInTimeTable()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    if ((base.Session["StudentDetails"] == null ? true : base.Session["CurrentSem"].ToString() == ""))
                    {
                        base.Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(str);
                    }
                    string studentRegisteredProgramme = CommonClass.GetStudentRegisteredProgramme(str);
                    string[] item = (string[])base.Session["StudentDetails"];
                    string str1 = base.Session["CurrentSem"].ToString();
                    action = (!CommonClass.UnitsNotInTimeTable(str, str1) ? base.View(new StudentCourseDetails()
                    {
                        StdNo = str,
                        Name = item[0],
                        CurrentSem = str1,
                        Prog = studentRegisteredProgramme,
                        Email = item[2]
                    }) : base.View("~/View/Course/UitsNotInTimeTable.cshtml"));
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
                action = base.View("~/Views/Common/ErrorMessage.cshtml", error);
            }
            return action;
        }

        [HttpPost]
        public JsonResult UpdateRegisteredUnits(List<UnitRegistration> UnitReg)
        {
            JsonResult jsonResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                string str1 = "";
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                str1 = base.Session["CurrentSem"].ToString();
                if (base.Session["CurrentProgDetails"] == null)
                {
                    base.Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(str, str1, "0");
                }
                string[] item = (string[])base.Session["CurrentProgDetails"];
                int num = 0;
                foreach (UnitRegistration unitReg in UnitReg)
                {
                    string str2 = "";
                    string str3 = "";
                    string str4 = "";
                    string str5 = "";
                    unitReg.UnitCode.Trim();
                    if (unitReg.ClassCode != null)
                    {
                        str2 = unitReg.ClassCode.Trim();
                    }
                    if (unitReg.Campus != null)
                    {
                        str3 = unitReg.Campus.Trim();
                    }
                    if (unitReg.Period != null)
                    {
                        str5 = unitReg.Period.Trim();
                    }
                    if (unitReg.Day != null)
                    {
                        str4 = unitReg.Day.Trim();
                    }
                    num++;
                }
                jsonResult = base.Json(new { message = string.Concat(num.ToString(), " Units Updated successfully"), success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public ActionResult ViewStudentRegisteredUnits(StudentUnits Filters)
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    List<StudentUnits> studentUnits = new List<StudentUnits>();
                    string str1 = string.Concat(new string[] { "StudentUnits?$filter=Student_No eq '", str, "' and Reg_Transacton_ID eq '", Filters.RegID, "'&format=json" });
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            StudentUnits studentUnit = new StudentUnits()
                            {
                                Programme = (string)item["Programme"],
                                Unit = (string)item["Unit"],
                                Unit_Name = (string)item["Unit_Description"],
                                Stage = (string)item["Stage"],
                                Semester = (string)item["Semester"],
                                RegFor = (string)item["Register_for"],
                                Section = (string)item["Unit_Class_Code"],
                                CF = (string)item["No__Of_Units"]
                            };
                            studentUnits.Add(studentUnit);
                        }
                    }
                    string str2 = "";
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    str2 = base.Session["CurrentSem"].ToString();
                    action = this.PartialView("~/Views/Course/ViewStudentRegisteredUnits.cshtml", new GetRegisteredUnit()
                    {
                        RegDeadlineDate = CommonClass.RegistrationDeadline(str2),
                        ListOfRegUnit = studentUnits.DistinctBy<StudentUnits, string>((StudentUnits x) => x.Unit).ToList<StudentUnits>()
                    });
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
    }
}