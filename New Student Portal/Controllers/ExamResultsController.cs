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
    public class ExamResultsController : Controller
    {
        public ExamResultsController()
        {
        }

        public ActionResult ProvisionalResults()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    string studentRegisteredProgramme = CommonClass.GetStudentRegisteredProgramme(str);
                    List<ExamSemesters> examSemesters = new List<ExamSemesters>();
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData("SemesterList?$select=Code&$filter=Allow_Online_Results eq true&$format=json").GetResponseStream()))
                    {
                        JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                        if (jObjects["value"].Count<JToken>() > 0)
                        {
                            foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                            {
                                ExamSemesters examSemester = new ExamSemesters();
                                string item1 = (string)item["Code"];
                                List<ExamResults> examResults = new List<ExamResults>();
                                string str1 = string.Concat(new string[] { "StudentUnits?$select=Stage,Unit,Unit_Description,Grade&$filter=Student_No eq '", str, "' and Semester eq '", item1, "' and Programme eq '", studentRegisteredProgramme, "' and Released eq true&$format=json" });
                                using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                                {
                                    JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                                    if (jObjects1["value"].Count<JToken>() > 0)
                                    {
                                        string item2 = "";
                                        foreach (JObject jObjects2 in (IEnumerable<JToken>)jObjects1["value"])
                                        {
                                            ExamResults examResult = new ExamResults()
                                            {
                                                Unit = (string)jObjects2["Unit"],
                                                UnitName = (string)jObjects2["Unit_Description"],
                                                Grade = (string)jObjects2["Grade"]
                                            };
                                            item2 = (string)jObjects2["Stage"];
                                            examResults.Add(examResult);
                                        }
                                        examSemester.Stage = item2;
                                        examSemester.Semester = string.Concat(item2, "-", item1);
                                        examSemester.ListOfResults = examResults;
                                        examSemesters.Add(examSemester);
                                    }
                                }
                            }
                        }
                    }
                    action = base.View((
                        from x in examSemesters
                        orderby x.Stage
                        select x).ToList<ExamSemesters>());
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