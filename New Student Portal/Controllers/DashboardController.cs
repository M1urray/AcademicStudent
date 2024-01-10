using New_Student_Portal.CustomSecurity;
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

namespace Student.Controllers
{
    [CustomeAuthentication]
    [CustomAuthorization(Role = "STUD")]
    public class DashboardController : Controller
    {
        // GET: Dashboard
        public ActionResult Dashboard()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                string RegNo = Session["Username"].ToString();
                string RegUnits = "0";
                string AttempUnits = "0";
                if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester();
                }

                string sem = Session["CurrentSem"].ToString();

                string[] Regunits = GetUnitsSummery(RegNo, sem);
                if (Regunits[0] != null && Regunits[1] != null)
                {
                    RegUnits = Regunits[0];
                    AttempUnits = Regunits[1];
                }
                string page = "CustomerList?$filter=No eq '" + RegNo + "'&$format=json";

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
                        Session["CurrProg"] = (string)config["Current_Programme"];
                        Session["SettlementType"] = (string)config["Current_Settlement_Type"];
                        Details.Date_Of_Birth = ((DateTime)config["Date_Of_Birth"]).ToString("dd/MM/yyyy");
                        Details.Phone_No = (string)config["Phone_No"];
                        Details.Address = (string)config["Address"];
                        Details.E_Mail = (string)config["E_Mail"];
                        Details.Balance = (decimal)config["Balance_LCY"];
                        Details.Debit_Amount = (decimal)config["Debit_Amount"];
                        Details.Credit_Amount = (decimal)config["Credit_Amount"];
                        Details.RegUnits = RegUnits;
                        Details.AttemptedUnits = (string)config["Completed_Units"];
                        Details.Campus = (string)config["Global_Dimension_1_Code"] ;
                        Details.Stage = (string)config["Current_Stage"];
                        Details.Position = (string)config["Leadership_Position"];
                        Details.ProfilePic = CommonClass.ProfilePicture(RegNo);
                        Details.Prog = (string)config["Current_Programme"];
                        Details.ProgName = (string)config["Programme_Name"];
                        Details.AcademicStatus = CommonClass.StudentStatusDescription((string)config["Academic_Status"]);
                        Details.NotfCount = CommonClass.GetDocumentCount();
                        Details.ClassAttendance = GetUnitsClassAttendance(RegNo,sem);
                        //Details.PercAtte = Math.Round(CommonClass.GetPercentageAttendance(RegNo, sem), 2);

                        Details.ListInternalMemos = ImportantDocuments(RegNo);
                    }
                }
                return View(Details);
            }
        }
        protected List<ClassAttendance> GetUnitsClassAttendance(string Student, string Sem)
        {
            List<ClassAttendance> UnitL = new List<ClassAttendance>();
            try
            {
                decimal MaxA = Convert.ToDecimal(CommonClass.GetMaximumNoOfAttendance(Sem));
                string page = "StudentUnits?$filter=Student_No eq '" + Student + "' and Semester eq '" + Sem + "'&$format=json";
                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        ClassAttendance Att = new ClassAttendance();
                        Att.Unit = (string)config["Unit"];
                        Att.Description = (string)config["Unit_Description"];
                        Att.PercAtte = Math.Round(CommonClass.GetPercentageAttendance(Student,Sem,(string)config["Unit"], MaxA),0);
                        UnitL.Add(Att);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return UnitL;
        }
        public ListOfInternalMemos ImportantDocuments(string RegNo)
        {
            List<DocumentAttachment> DocAttachment = new List<DocumentAttachment>();
            bool hasFile = false;
            try
            {
                string[] s = CommonClass.GetStudentDimensions(RegNo);
                string page = "InternalMemos?$filter=Category eq 'STUDENT'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            string page1 = "DocumentAttachment?$filter=No eq '" + (string)config["Code"] + "'&$format=json";

                            HttpWebResponse httpResponse1 = Credentials.GetOdataData(page1);
                            using (var streamReader1 = new StreamReader(httpResponse1.GetResponseStream()))
                            {
                                var result1 = streamReader1.ReadToEnd();

                                var details1 = JObject.Parse(result1);
                                if (details1["value"].Count() > 0)
                                {
                                    hasFile = true;
                                    foreach (JObject config1 in details1["value"])
                                    {
                                        DocumentAttachment docAttList = new DocumentAttachment();
                                        if ((string)config1["Programme"] != "")
                                        {
                                            if ((string)config1["Programme"] == s[3])
                                            {
                                                docAttList.TabelID = (int)config1["Table_ID"];
                                                docAttList.No = (string)config1["No"];
                                                docAttList.FileName = (string)config1["File_Name"];
                                                docAttList.Remarks = (string)config1["Document_Description"];
                                                docAttList.FileExt = (string)config1["File_Extension"];
                                                docAttList.ID = (int)config1["ID"];
                                                docAttList.LineNo = (string)config1["Line_No"];
                                                docAttList.DocType = (string)config1["Document_Type"];
                                                docAttList.Date = ((DateTime)config1["Attached_Date"]).ToString("dd/MM/yyyy");
                                                DocAttachment.Add(docAttList);
                                            }
                                        }
                                        else if ((string)config1["Department"] != "" && (string)config1["School"] != "" && (string)config1["Campus"] != "")
                                        {
                                            if ((string)config1["Department"] == s[1] && (string)config1["School"] == s[2] && (string)config1["Campus"] == s[0])
                                            {
                                                docAttList.TabelID = (int)config1["Table_ID"];
                                                docAttList.No = (string)config1["No"];
                                                docAttList.FileName = (string)config1["File_Name"];
                                                docAttList.Remarks = (string)config1["Document_Description"];
                                                docAttList.FileExt = (string)config1["File_Extension"];
                                                docAttList.ID = (int)config1["ID"];
                                                docAttList.LineNo = (string)config1["Line_No"];
                                                docAttList.DocType = (string)config1["Document_Type"];
                                                docAttList.Date = ((DateTime)config1["Attached_Date"]).ToString("dd/MM/yyyy");
                                                DocAttachment.Add(docAttList);
                                            }
                                        }
                                        else if ((string)config1["Department"] != "" && (string)config1["School"] != "" && (string)config1["Campus"] == "")
                                        {
                                            if ((string)config1["Department"] == s[1] && (string)config1["School"] == s[2])
                                            {
                                                docAttList.TabelID = (int)config1["Table_ID"];
                                                docAttList.No = (string)config1["No"];
                                                docAttList.FileName = (string)config1["File_Name"];
                                                docAttList.Remarks = (string)config1["Document_Description"];
                                                docAttList.FileExt = (string)config1["File_Extension"];
                                                docAttList.ID = (int)config1["ID"];
                                                docAttList.LineNo = (string)config1["Line_No"];
                                                docAttList.DocType = (string)config1["Document_Type"];
                                                docAttList.Date = ((DateTime)config1["Attached_Date"]).ToString("dd/MM/yyyy");
                                                DocAttachment.Add(docAttList);
                                            }

                                        }
                                        else if ((string)config1["Department"] != "" && (string)config1["School"] == "" && (string)config1["Campus"] != "")
                                        {
                                            if ((string)config1["Department"] == s[1] && (string)config1["Campus"] == s[0])
                                            {
                                                docAttList.TabelID = (int)config1["Table_ID"];
                                                docAttList.No = (string)config1["No"];
                                                docAttList.FileName = (string)config1["File_Name"];
                                                docAttList.Remarks = (string)config1["Document_Description"];
                                                docAttList.FileExt = (string)config1["File_Extension"];
                                                docAttList.ID = (int)config1["ID"];
                                                docAttList.LineNo = (string)config1["Line_No"];
                                                docAttList.DocType = (string)config1["Document_Type"];
                                                docAttList.Date = ((DateTime)config1["Attached_Date"]).ToString("dd/MM/yyyy");
                                                DocAttachment.Add(docAttList);
                                            }
                                        }
                                        else if ((string)config1["Department"] == "" && (string)config1["School"] != "" && (string)config1["Campus"] != "")
                                        {
                                            if ((string)config1["School"] == s[2] && (string)config1["Campus"] == s[0])
                                            {
                                                docAttList.TabelID = (int)config1["Table_ID"];
                                                docAttList.No = (string)config1["No"];
                                                docAttList.FileName = (string)config1["File_Name"];
                                                docAttList.Remarks = (string)config1["Document_Description"];
                                                docAttList.FileExt = (string)config1["File_Extension"];
                                                docAttList.ID = (int)config1["ID"];
                                                docAttList.LineNo = (string)config1["Line_No"];
                                                docAttList.DocType = (string)config1["Document_Type"];
                                                docAttList.Date = ((DateTime)config1["Attached_Date"]).ToString("dd/MM/yyyy");
                                                DocAttachment.Add(docAttList);
                                            }
                                        }
                                        else if ((string)config1["Department"] != "" && (string)config1["School"] == "" && (string)config1["Campus"] == "")
                                        {
                                            if ((string)config1["Department"] == s[1])
                                            {
                                                docAttList.TabelID = (int)config1["Table_ID"];
                                                docAttList.No = (string)config1["No"];
                                                docAttList.FileName = (string)config1["File_Name"];
                                                docAttList.Remarks = (string)config1["Document_Description"];
                                                docAttList.FileExt = (string)config1["File_Extension"];
                                                docAttList.ID = (int)config1["ID"];
                                                docAttList.LineNo = (string)config1["Line_No"];
                                                docAttList.DocType = (string)config1["Document_Type"];
                                                docAttList.Date = ((DateTime)config1["Attached_Date"]).ToString("dd/MM/yyyy");
                                                DocAttachment.Add(docAttList);
                                            }
                                        }
                                        else if ((string)config1["Department"] == "" && (string)config1["School"] != "" && (string)config1["Campus"] == "")
                                        {
                                            if ((string)config1["School"] == s[2])
                                            {
                                                docAttList.TabelID = (int)config1["Table_ID"];
                                                docAttList.No = (string)config1["No"];
                                                docAttList.FileName = (string)config1["File_Name"];
                                                docAttList.Remarks = (string)config1["Document_Description"];
                                                docAttList.FileExt = (string)config1["File_Extension"];
                                                docAttList.ID = (int)config1["ID"];
                                                docAttList.LineNo = (string)config1["Line_No"];
                                                docAttList.DocType = (string)config1["Document_Type"];
                                                docAttList.Date = ((DateTime)config1["Attached_Date"]).ToString("dd/MM/yyyy");
                                                DocAttachment.Add(docAttList);
                                            }
                                        }
                                        else if ((string)config1["Department"] == "" && (string)config1["School"] == "" && (string)config1["Campus"] != "")
                                        {
                                            if ((string)config1["Campus"] == s[0])
                                            {
                                                docAttList.TabelID = (int)config1["Table_ID"];
                                                docAttList.No = (string)config1["No"];
                                                docAttList.FileName = (string)config1["File_Name"];
                                                docAttList.Remarks = (string)config1["Document_Description"];
                                                docAttList.FileExt = (string)config1["File_Extension"];
                                                docAttList.ID = (int)config1["ID"];
                                                docAttList.LineNo = (string)config1["Line_No"];
                                                docAttList.DocType = (string)config1["Document_Type"];
                                                docAttList.Date = ((DateTime)config1["Attached_Date"]).ToString("dd/MM/yyyy");
                                                DocAttachment.Add(docAttList);
                                            }
                                        }
                                        else
                                        {
                                            docAttList.TabelID = (int)config1["Table_ID"];
                                            docAttList.No = (string)config1["No"];
                                            docAttList.FileName = (string)config1["File_Name"];
                                            docAttList.Remarks = (string)config1["Document_Description"];
                                            docAttList.FileExt = (string)config1["File_Extension"];
                                            docAttList.ID = (int)config1["ID"];
                                            docAttList.LineNo = (string)config1["Line_No"];
                                            docAttList.DocType = (string)config1["Document_Type"];
                                            docAttList.Date = ((DateTime)config1["Attached_Date"]).ToString("dd/MM/yyyy");
                                            DocAttachment.Add(docAttList);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DocumentAttachment docAttList = new DocumentAttachment();
                docAttList.FileName = ex.Message;
                docAttList.Date = DateTime.Now.ToString("dd/MM/yyyy");
                DocAttachment.Add(docAttList);
                hasFile = false;
            }
            ListOfInternalMemos fileList = new ListOfInternalMemos
            {
                ListOfIntMemos = DocAttachment.OrderByDescending(x => x.Date).ToList(),
                hasFiles = hasFile
            };
            return fileList;
        }
        protected void LoadRegisteredUnits()
        {
            if (Session["Username"] == null)
            {
                RedirectToAction("Login", "Login");
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
            }
        }
        public string[] GetUnitsSummery(string stdNo, string Sem)
        {
            string[] S = new string[2];
            try
            {
                List<StudentUnits> CurrentUnitCount = new List<StudentUnits>();
                List<StudentUnits> AttemptedUnitCount = new List<StudentUnits>();
                string page = "StudentUnits?$filter=Student_No eq '" + stdNo + "'&format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            StudentUnits CCount = new StudentUnits();
                            StudentUnits ACount = new StudentUnits();
                            if ((string)config["Semester"] == Sem)
                            {
                                CCount.Unit = (string)config["Unit"];
                                CurrentUnitCount.Add(CCount);
                            }
                            else
                            {
                                ACount.Unit = (string)config["Unit"];
                                AttemptedUnitCount.Add(ACount);
                            }
                        }
                    }
                    UnitRegistrationSummery SummeryReg = new UnitRegistrationSummery
                    {
                        ListOfRegUnit = CurrentUnitCount.DistinctBy(x => x.Unit).ToList(),
                        ListOfAttemptedUnit = AttemptedUnitCount.DistinctBy(x => x.Unit).ToList()
                    };
                    S[0] = SummeryReg.ListOfRegUnit.Count().ToString();
                    S[1] = SummeryReg.ListOfAttemptedUnit.Count().ToString();
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return S;
        }
        public ActionResult ViewStudentRegisteredUnits(string Type)
        {
            try
            {
                List<StudentUnits> regUnits = new List<StudentUnits>();
                if (Session["Username"] == null)
                {
                    RedirectToAction("Login", "Login");
                }
                else
                {
                    string RegNo = Session["Username"].ToString();
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester();
                    }
                    string sem = Session["CurrentSem"].ToString();

                    string page = "";

                    if (Type == "CurrReg")
                    {
                        page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Semester eq '" + sem + "'&format=json";
                    }
                    else
                    {
                        page = "StudentUnits?$filter=Student_No eq '" + RegNo + "' and Semester ne '" + sem + "'&format=json";
                    }


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
                }
                return PartialView("~/Views/Dashboard/ViewUitSummery.cshtml", regUnits.DistinctBy(x => x.Unit).ToList());
            }
            catch (Exception ex)
            {
                return PartialView(ex.Message);
            }
        }
        [HttpGet]
        public virtual ActionResult Download(string fileName)
        {
            string fullPath = Credentials.ImportantDocParth + fileName;
            return File(fullPath, "application/octet-stream", fileName);
        }
    }
}