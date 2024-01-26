using Microsoft.Ajax.Utilities;
using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.Controllers
{
    public class RetakeController : Controller
    {
        public RetakeController()
        {
        }

        public ActionResult CourseRegistrationDetails()
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
                    string str5 = string.Concat(new string[] { "CourseReg?$filter=StudentNo eq '", str, "' and Programme eq '", registeredProgramme.CurrentProg, "' and Reversed eq false and Registerfor eq ''&format=json" });
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

        public ActionResult Retake()
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
    }
}