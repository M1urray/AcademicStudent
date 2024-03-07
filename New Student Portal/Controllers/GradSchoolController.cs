using System;
using New_Student_Portal.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using New_Student_Portal.ViewModel;
using Latest_Staff_Portal.Models;
using System.DirectoryServices.ActiveDirectory;
using System.Web.Security;
using System.Web;

namespace New_Student_Portal.Controllers
{
    public class GradSchoolController : Controller
    {
        public ActionResult ThesisManagement()
        {
            StudentThesisStatus thesisStatus = new StudentThesisStatus();
            try
            {
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }

                string UserName = Session["Username"].ToString();
;                string page = "CustomerList?$filter=No eq '" + UserName.ToLower() + "' and Status ne 'Dropped Out' and Status ne 'Expelled' and Status ne 'Withdrawn' and Status ne 'Deceased'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Any())
                    {
                        foreach (var jToken in details["value"])
                        {
                            var config = (JObject)jToken;
                            thesisStatus.ThesisStatus = (int)config["Thesis_Status"];
                        }
                    }
                }
            }
            catch(Exception exception)
            {
                Error error = new Error()
                {
                    Message = exception.Message
                };
                return View("~/Views/Shared/ErrorMessange.cshtml", error);
            }
            return View(thesisStatus);
        }
        public PartialViewResult ThesisListPartialViewResult()
        {
            string studentNo = Session["Username"].ToString();
            List<StudentThesis> student = new List<StudentThesis>();
            string page = "StudentThesis?$filter=Student_No eq '" + studentNo+"'&$format=json";

            HttpWebResponse httpResponse = Credentials.GetOdataData(page);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                foreach (var jToken in details["value"])
                {
                    var config = (JObject)jToken;
                    StudentThesis studentships = new StudentThesis()
                    {
                        No = (string)config["No"],
                        StudentNo = (string)config["Current_Programme"],
                        ThesisDescription = (string)config["Thesis_Description"],
                        ThesisTitle = (string)config["Thesis_Title"],
                        DateRequested = (string)config["Date_Requested"],
                        Supervisor1 = (string)config["Supervisor_1"],
                        Supervisor2 = (string)config["Supervisor_2"]
                    };
                    student.Add(studentships);
                }
            }
            return PartialView("~/Views/GradSchool/ThesisListPartialViewResult.cshtml",student.OrderByDescending(x=>x.No));
        }

        public PartialViewResult ConceptRequest()
        {
            return PartialView();
        }
        public PartialViewResult ProposalRequest()
        {
            return PartialView();
        }
        public PartialViewResult ReviewRequest()
        {
            return PartialView();
        }
        public PartialViewResult SchoolPresentation()
        {
            return PartialView();
        }
        public PartialViewResult DefenceRequest()
        {
            return PartialView();
        }
    }
}