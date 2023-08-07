using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
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
    public class ExamResultsController : Controller
    {
        // GET: ExamResults
        public ActionResult ProvisionalResults()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                string RegNo = Session["Username"].ToString();

                string Prog = CommonClass.GetStudentRegisteredProgramme(RegNo);
                List<ExamSemesters> examResults = new List<ExamSemesters>();
                
                string pageSem = "SemesterList?$select=Code&$filter=Allow_Online_Results eq true&$format=json";
                HttpWebResponse httpResponseSem = Credentials.GetOdataData(pageSem);
                using (var streamReaderSem = new StreamReader(httpResponseSem.GetResponseStream()))
                {
                    var resultSem = streamReaderSem.ReadToEnd();

                    var detailsSem = JObject.Parse(resultSem);

                    if (detailsSem["value"].Count() > 0)
                    {
                        foreach (JObject config1 in detailsSem["value"])
                        {
                            ExamSemesters examSemResults = new ExamSemesters();
                            string Sem = (string)config1["Code"];
                            List<ExamResults> examR = new List<ExamResults>();
                            string page = "StudentsUnits?$select=Unit,UnitDescription,Grade,GPA,GPA_Quality_Points,NoOfUnits,Earned_No_of_Units,FinalScore&$filter=StudentNo eq '"
                                + RegNo + "' and Semester eq '" + Sem + "' and Programme eq '"+ Prog + "'&$format=json";
                            HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                            {
                                var result = streamReader.ReadToEnd();

                                var details = JObject.Parse(result);

                                if (details["value"].Count() > 0)
                                {
                                    foreach (JObject config in details["value"])
                                    {
                                        ExamResults R = new ExamResults();
                                        R.Unit = (string)config["Unit"];
                                        R.UnitName = (string)config["UnitDescription"];
                                        R.Grade = (string)config["Grade"];
                                        R.GPA = (string)config["GPA"];
                                        R.Credits = (string)config["Earned_No_of_Units"];
                                        R.GltyPoints = (string)config["GPA_Quality_Points"];
                                        R.TotalUnits = (string)config["NoOfUnits"];
                                        R.Marks = (string)config["FinalScore"];
                                        examR.Add(R);
                                    }
                                    examSemResults.Semester = Sem;
                                    examSemResults.ListOfResults = examR;
                                    examResults.Add(examSemResults);
                                }
                            }                            
                        }
                    }
                }
                return View(examResults);
            }
        }
    }
}