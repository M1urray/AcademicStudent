using Microsoft.Ajax.Utilities;
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
    public class GradeCompaintController : Controller
    {
        // GET: GradeCompain
        public ActionResult Grade_Complaint_List()
        {
            return View();
        }
        public ActionResult Grade_Complaint_List_Views()
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
                    List<Grade_Compaint> newDocList = new List<Grade_Compaint>();
                    string pageReg = "GradeComplaint?$filter=StudentNo eq '" + RegNo + "'&$format=json";
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
                                Grade_Compaint newDoc = new Grade_Compaint();
                                newDoc.Doc_No = (string)config["DocNo"];
                                newDoc.Unit = (string)config["Unit"];
                                newDoc.Unit_Name = (string)config["UnitDescription"];
                                newDoc.Semester = (string)config["Semester"];
                                newDoc.Date_Applied = ((DateTime)config["DateApplied"]).ToString("dd/MM/yyyy");
                                newDoc.Current_Grade = (string)config["OriginalGrade"];
                                newDoc.New_Grade = (string)config["NewGrade"];
                                newDoc.Status = (string)config["Status"];
                            }
                        }
                    }
                    return View("~/Views/GradeCompaint/Partial View/Grade_Complaint_List_Views.cshtml", newDocList);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public PartialViewResult NewRequistion()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string StudentNo = Session["Username"].ToString();
                List<DropdownList> semesterList = new List<DropdownList>();
                string page = "StudentUnits?$select=Semester&$filter=Student_No eq '" + StudentNo + "' and Released eq true&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        DropdownList ddl = new DropdownList();
                        ddl.Value = (string)config["Semester"];
                        ddl.Text = (string)config["Semester"];
                        semesterList.Add(ddl);
                    }
                }
                New_Requisition newR = new New_Requisition
                {
                    Code = "",
                    ListOfSemester = semesterList.Select(x =>
                                                 new SelectListItem()
                                                 {
                                                     Text = x.Text,
                                                     Value = x.Value
                                                 }).DistinctBy(x => x.Value).OrderBy(x => x.Value).ToList()
                };
                return PartialView("~/Views/GradeCompaint/Partial View/NewRequisition.cshtml", newR);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetStudentUnitsList(string Sem)
        {
            try
            {
                string StudentNo = Session["Username"].ToString();
                #region Unit List
                List<DropdownList> unitList = new List<DropdownList>();
                string page = "StudentUnits?$select=Unit,Unit_Description&$filter=Student_No eq '" + StudentNo + "' and Semester eq '" + Sem + "' and Released eq true&$format=json";

                HttpWebResponse httpResponseCampus = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseCampus.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);


                    foreach (JObject config in details["value"])
                    {
                        DropdownList Unit = new DropdownList();
                        Unit.Value = (string)config["Unit"];
                        Unit.Text = (string)config["Unit"] + "-" + (string)config["Unit_Description"];
                        unitList.Add(Unit);
                    }
                }
                #endregion
                New_Requisition newR = new New_Requisition
                {
                    ListOfUnits = unitList.Select(x =>
                                       new SelectListItem()
                                       {
                                           Text = x.Text,
                                           Value = x.Value
                                       }).DistinctBy(x => x.Value).ToList()
                };
                return Json(newR, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}