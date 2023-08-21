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
            try
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
                        Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
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
                            Details.Date_Of_Birth = ((DateTime)config["Date_Of_Birth"]).ToString("dd/MM/yyyy");
                            Details.Phone_No = (string)config["Phone_No"];
                            Details.Address = (string)config["Address"];
                            Details.E_Mail = (string)config["E_Mail"];
                            Details.Campus = (string)config["Global_Dimension_1_Code"];
                            Details.Balance = (decimal)config["Balance_LCY"];
                            Details.Debit_Amount = (decimal)config["Debit_Amount"];
                            Details.Credit_Amount = (decimal)config["Credit_Amount"];
                            Details.RegUnits = RegUnits;
                            Details.AttemptedUnits = (string)config["Completed_Units"];
                            //if ((decimal)config["Completed_Units"] > 0)
                            //{
                            //    Details.GPA = (Math.Round(((decimal)config["Programme_GPA_Points"] / ((decimal)config["Completed_Units"] + (decimal)config["Exempted_Units"])), 2)).ToString();
                            //}
                            //else
                            //{
                            //    Details.GPA = "0";
                            //}
                            Details.GPA = Math.Round((decimal)config["Cumm_GPA"], 2).ToString();

                            //GroupLeader glD = GetBSGroup(RegNo, sem);
                            //Details.IsBsLeader = glD.IsBSLeader;
                            //Details.BSGroup = glD.BsGroup;

                            //Details.IsDCFLeader = glD.IsBSLeader;
                            //Details.DCFGroup = glD.DCFGroup;
                            Details.ProfilePic = CommonClass.ProfilePicture(RegNo);
                            Details.ProgName = (string)config["Programme_Name"];
                            if ((string)config["Territory_Code"] == "")
                            {
                                Details.Cat_Token = "Not Found. Generate";
                            }
                            else
                            {
                                Details.Cat_Token = (string)config["Territory_Code"];
                            }
                            Details.Qualify_For_Catering = (bool)config["Tax_Liable"];
                            if ((string)config["Major_Description"] != "")
                            {
                                Details.OtherProg = (string)config["Major_Description"];
                            }
                            else
                            {
                                Details.OtherProg = "";
                            }
                            if ((string)config["Second_Conc_Description"] != "")
                            {
                                Details.SecondConc = (string)config["Second_Conc_Description"];
                            }
                            else
                            {
                                Details.SecondConc = "";
                            }
                            if ((string)config["Minor_Description"] != "")
                            {
                                Details.Minor = (string)config["Minor_Description"];
                            }
                            else
                            {
                                Details.Minor = "";
                            }
                            Details.Semester = sem;
                            Details.DisplinaryCases = GetDisplinaryCase();
                            Details.AcademicStatus = CommonClass.StudentStatusDescription((string)config["Academic_Status"]);
                            Details.NotfCount = CommonClass.GetDocumentCount();
                            Details.ListInternalMemos = ImportantDocuments(RegNo);
                            Details.BstudyDetails = GetBSGroupDetails(RegNo, sem);
                            Details.AcademicStatus = CommonClass.StudentStatusDescription((string)config["Academic_Status"]);
                            // Details.LeadershipOption = CommonClass.LeadershipOption();
                        }
                    }
                    return View(Details);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return View("~/Views/Common/ErrorMessage.cshtml", error);
            }
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
                                        if ((string)config1["Department"] != "" && (string)config1["School"] != "" && (string)config1["Campus"] != "")
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                }
                if (Session["StudentDetails"] == null || Session["CurrentSem"].ToString() == "")
                {
                    Session["StudentDetails"] = CommonClass.StudentProgrammeDetails(RegNo);
                }
                string[] s = (string[])Session["StudentDetails"];
                string sem = Session["CurrentSem"].ToString();
            }
        }
        protected List<DisplinaryCases> GetDisplinaryCase()
        {
            List<DisplinaryCases> displcList = new List<DisplinaryCases>();

            string RegNo = Session["Username"].ToString();

            string page = "StudentDisciplinaryDetails?$filter=Student_No eq '" + RegNo + "'&$format=json";
            HttpWebResponse httpResponse = Credentials.GetOdataData(page);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);

                if (details["value"].Count() > 0)
                {
                    foreach (JObject config in details["value"])
                    {
                        DisplinaryCases c = new DisplinaryCases();
                        if (DateTime.Today <= (DateTime)config["End_Date"])
                        {
                            c.Remarks = (string)config["Remarks"];
                            c.EndDate = ((DateTime)config["End_Date"]).ToString("dd/MM/yyyy");
                            displcList.Add(c);
                        }
                    }
                }
            }
            return displcList;
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
                        Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
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
                return PartialView("~/Views/Dashboard/ViewUnitSummery.cshtml", regUnits.DistinctBy(x => x.Unit).ToList());
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult GetBsDetailForm()
        {
            ChaplainGroupDetails LeaderD = new ChaplainGroupDetails();
            try
            {
                string StdNo = Session["Username"].ToString();
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester(StdNo);
                }
                string sem = Session["CurrentSem"].ToString();

                LeaderD.IsBSLeader = false;
                LeaderD.RequestToBeLeader = false;
                LeaderD.BsGroup = "";

                LeaderD.IsDCFLeader = false;
                LeaderD.DCFGroup = "";

                bool RequestedLeadership = CommonClass.RequestedTobeABsLeader(StdNo, sem);
                if (RequestedLeadership)
                {
                    LeaderD.RequestToBeLeader = true;
                    LeaderD.BsLeaderRequestStatus = "Open";
                }
                else
                {
                    #region BS Group
                    //string page = "CourseReg?$select=Bible_Study_Group&$filter=StudentNo eq '" + StdNo + "' and Bible_Study_Group ne '' and Semester eq '" + sem + "'&$format=json";
                    string page = "BsGroupMembers?$select=Group_Name&$filter=Student_No_ eq '" + StdNo + "' and Semester eq '" + sem + "'&$format=json";
                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        if (details["value"].Count() > 0)
                        {
                            foreach (JObject config in details["value"])
                            {
                                LeaderD.BsGroup = (string)config["Group_Name"];
                                LeaderD.BsGroupName = CommonClass.GetBibleDescription((string)config["Group_Name"]);
                            }
                        }
                    }
                    #endregion
                    #region BS Leader
                    string pageL = "BsLeader?$filter=No eq '" + StdNo + "' and Semester eq '" + sem + "'&$format=json";
                    HttpWebResponse httpResponseL = Credentials.GetOdataData(pageL);
                    using (var streamReader = new StreamReader(httpResponseL.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        if (details["value"].Count() > 0)
                        {
                            foreach (JObject config in details["value"])
                            {                                
                                LeaderD.RequestToBeLeader = true;
                                LeaderD.BsLeaderRequestStatus = (string)config["Status"];
                                if ((string)config["Status"] == "Approved")
                                {
                                    LeaderD.IsBSLeader = true;
                                    LeaderD.BsLeaderRequestApproved = true;
                                }
                                else
                                {
                                    LeaderD.BsLeaderRequestApproved = false;
                                }
                            }
                        }
                    }
                    #endregion
                }
                return View("~/Views/Dashboard/Partial Views/BsDiv.cshtml", LeaderD);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        //protected GroupLeader GetBSGroup(string StdNo, string Sem)
        //{
        //    GroupLeader LeaderD = new GroupLeader();
        //    try
        //    {

        //        #region DCF
        //        //string pageDCF = "CourseReg?$select=Bible_Study_Group&$filter=StudentNo eq '" + StdNo + "' and Bible_Study_Group ne '' and Semester eq '" + Sem + "'&$format=json";
        //        //HttpWebResponse httpResponse = Credentials.GetOdataData(page);
        //        //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //        //{
        //        //    var result = streamReader.ReadToEnd();

        //        //    var details = JObject.Parse(result);

        //        //    if (details["value"].Count() > 0)
        //        //    {
        //        //        foreach (JObject config in details["value"])
        //        //        {
        //        //            LeaderD.IsBSLeader = true;
        //        //            LeaderD.BsGroup = (string)config["Bible_Study_Group"];
        //        //        }
        //        //    }
        //        //}
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        ex.Data.Clear();
        //    }
        //    return LeaderD;
        //}
        protected Bs GetBSGroupDetails(string StdNo, string Sem)
        {
            Bs BsD = new Bs();
            try
            {
                string page = "BSGroupList?$filter=Group_Leader eq '" + StdNo + "' and Semester eq '" + Sem + "'&$format=json";
                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            BsD.Code = (string)config["Code"];
                            BsD.Description = (string)config["Description"];
                            BsD.LeaderName = (string)config["Leader_Name"];
                            BsD.LeaderContact = (string)config["Leader_Contact"];
                            BsD.StdCount = (string)config["Student_Count"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return BsD;
        }
        public ActionResult JoinBSGroup(string BSG)
        {
            try
            {
                BSGroupList BSGList = new BSGroupList();
                if (Session["Username"] == null)
                {
                    RedirectToAction("Login", "Login");
                }
                else
                {
                    string RegNo = Session["Username"].ToString();
                    string page = "BSGroupList?$filter=Description ne ''&$format=json";

                    List<DropdownList> ddlList = new List<DropdownList>();
                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            DropdownList d = new DropdownList();
                            d.Value = (string)config["Code"];
                            d.Text = (string)config["Description"];
                            ddlList.Add(d);
                        }
                    }
                    BSGList = new BSGroupList
                    {
                        Code = "",
                        ListOfBSGroup = ddlList.Select(x =>
                                          new SelectListItem()
                                          {
                                              Text = x.Text,
                                              Value = x.Value
                                          }).ToList()
                    };
                }
                return PartialView("~/Views/Dashboard/Partial Views/JoinBS.cshtml", BSGList);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult JoinBSGroupAgainstGroup(string BSG)
        {
            try
            {
                BSGroupList BSGList = new BSGroupList();
                if (Session["Username"] == null)
                {
                    RedirectToAction("Login", "Login");
                }
                else
                {
                    string RegNo = Session["Username"].ToString();
                    string page = "BSGroupList?$filter=Description ne ''&$format=json";

                    List<DropdownList> ddlList = new List<DropdownList>();
                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            DropdownList d = new DropdownList();
                            d.Value = (string)config["Code"];
                            d.Text = (string)config["Description"];
                            ddlList.Add(d);
                        }
                    }
                    BSGList = new BSGroupList
                    {
                        Code = "",
                        ListOfBSGroup = ddlList.Select(x =>
                                          new SelectListItem()
                                          {
                                              Text = x.Text,
                                              Value = x.Value
                                          }).ToList()
                    };
                }
                return PartialView("~/Views/Dashboard/Partial Views/AskLeadershipWithGroup.cshtml", BSGList);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult GetBSDetails(string BSG)
        {
            try
            {
                BSDetails BSGDetail = new BSDetails();
                string RegNo = Session["Username"].ToString();
                if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                }

                string sem = Session["CurrentSem"].ToString();

                string page = "BSGroupList?$filter=Code eq '" + BSG + "'&$format=json";


                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        BSGDetail.Bs = (string)config["Description"];
                        BSGDetail.GLeader = (string)config["Leader_Name"];
                        BSGDetail.GLeader_PNo = (string)config["Leader_Contact"];
                        BSGDetail.Location = (string)config["Room"];
                        BSGDetail.Time = "";
                    }
                }
                return PartialView("~/Views/Dashboard/Partial Views/BSDetails.cshtml", BSGDetail);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [HttpPost]
        public JsonResult SubmitBSRequest(string BSG)
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                }

                Sem = Session["CurrentSem"].ToString();

                Credentials.ObjNav.RequestToJoinBibleStudy(RegNo, BSG, Sem);
                return Json(new { message = "BS group request submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult RequestBsGroupLeader()
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                }

                Sem = Session["CurrentSem"].ToString();

                Credentials.ObjNav.RequestBsLeader(RegNo, 0, Sem, "");
                return Json(new { message = "Rsequest submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CreateDocument(string Type)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    RedirectToAction("Login", "Login");
                }
                NewBS newBs = new NewBS();
                newBs.Type = Type;

                List<BsList> BsList = new List<BsList>();
                string page1 = "BSGroupList?$format=json";
                HttpWebResponse httpResponse1 = Credentials.GetOdataData(page1);
                using (var streamReader = new StreamReader(httpResponse1.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        BsList d = new BsList();
                        d.Code = (string)config["Code"];
                        d.Description = (string)config["Description"];
                        BsList.Add(d);
                    }
                }
                newBs.BsList = BsList;
                return PartialView("~/Views/Dashboard/Partial Views/CreateDoc.cshtml", newBs);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [HttpPost]
        public JsonResult SubmitDocument(string Code, string Desc, string Type)
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                }

                Sem = Session["CurrentSem"].ToString();
                string msg = "";
                Credentials.ObjNav.CreateBSGroup(RegNo, Code, Desc, Sem);
                msg = "BS group request submitted successfully";

                return Json(new { message = msg, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult CreateDCFDocument()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    RedirectToAction("Login", "Login");
                }
                NewDCF newDCF = new NewDCF();
                string page = "DCFList?$format=json";

                List<DropdownList> ddlList = new List<DropdownList>();
                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        DropdownList d = new DropdownList();
                        d.Value = (string)config["Code"];
                        d.Text = (string)config["Description"];
                        ddlList.Add(d);
                    }
                }
                List<DCFList> EventList = new List<DCFList>();
                string page1 = "DCFLists?$format=json";
                HttpWebResponse httpResponse1 = Credentials.GetOdataData(page1);
                using (var streamReader = new StreamReader(httpResponse1.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        DCFList d = new DCFList();
                        d.DCF = (string)config["Type_No"];
                        d.Date = (string)config["Date"];
                        d.Time = (string)config["Time"];
                        d.Description = (string)config["Event_Remarks"];
                        EventList.Add(d);
                    }
                }
                newDCF = new NewDCF
                {
                    Code = "",
                    ListOfDCF = ddlList.Select(x =>
                                      new SelectListItem()
                                      {
                                          Text = x.Text,
                                          Value = x.Value
                                      }).ToList(),
                    EventList = EventList
                };
                return PartialView("~/Views/Dashboard/Partial Views/CreateDCFActivity.cshtml", newDCF);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [HttpPost]
        public JsonResult SubmitDCFDocument(string DCF, string Code, string Desc)
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                }

                Sem = Session["CurrentSem"].ToString();
                Credentials.ObjNav.CreateDCFActivity(RegNo, DCF, Code, Desc);
                string msg = "DCF Activity created successfully";

                return Json(new { message = msg, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult BibleStudyMembers(bool waitingRoom, string BsG)
        {
            try
            {
                #region
                List<BsMembers> BsMembersList = new List<BsMembers>();
                if (Session["Username"] == null)
                {
                    RedirectToAction("Login", "Login");
                }
                else
                {
                    string Sem = "";
                    string RegNo = Session["Username"].ToString();
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                    }

                    Sem = Session["CurrentSem"].ToString();
                    string page = "BsGMembers?$filter=Group_Name eq '" + BsG + "' and Semester eq '" + Sem + "'&$format=json";
                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            BsMembers d = new BsMembers();
                            d.Type = (string)config["Type"];
                            d.Student_No = (string)config["StudentNo"];
                            d.Name = (string)config["Student_Staff_Name"];
                            d.Contact = (string)config["Contact"];
                            d.Email = (string)config["Email"];
                            d.Email = (string)config["Email"];
                            d.Programme = (string)config["Programme_Name"];
                            BsMembersList.Add(d);
                        }
                    }
                }
                #endregion
                BsGList newList = new BsGList
                {
                    ListOfBsList = BsMembersList,
                    Waiting = waitingRoom
                };
                return PartialView("~/Views/Dashboard/Partial Views/BsMembersList.cshtml", newList);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [HttpPost]
        public JsonResult ApproveBSRequest(string Bs, string StdNo)
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
                    Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                }

                Sem = Session["CurrentSem"].ToString();
                Credentials.ObjNav.JoinBSGroup(StdNo, Sem, Bs);
                string msg = "DCF Activity created successfully";

                return Json(new { message = msg, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DCFEventsList()
        {
            try
            {
                List<DCFList> ddlList = new List<DCFList>();
                if (Session["Username"] == null)
                {
                    RedirectToAction("Login", "Login");
                }
                else
                {
                    string RegNo = Session["Username"].ToString();
                    string page = "DCFLists?$format=json";
                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            DCFList d = new DCFList();
                            d.DCF = (string)config["Type_No"];
                            d.Date = (string)config["Date"];
                            d.Time = (string)config["Time"];
                            d.Description = (string)config["Event_Remarks"];
                            ddlList.Add(d);
                        }
                    }
                }
                return PartialView("~/Views/Dashboard/Partial Views/DCFEvents.cshtml", ddlList);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public ActionResult GetUnitsClassAttendance()
        {
            try
            {
                List<ClassAttendance> UnitL = new List<ClassAttendance>();
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    string Student = Session["Username"].ToString();
                    if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(Student);
                    }

                    string Sem = Session["CurrentSem"].ToString();
                    string page = "StudentUnits?$filter=Student_No eq '" + Student + "' and Semester eq '" + Sem + "'&$format=json";
                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            decimal MaxA = 0;
                            MaxA = Convert.ToDecimal(CommonClass.GetMaximumNoOfAttendance(Sem, (string)config["Unit"], (string)config["Campus"], (string)config["Unit_Class_Code"]));
                            ClassAttendance Att = new ClassAttendance();
                            Att.Unit = (string)config["Unit"];
                            Att.Description = (string)config["Unit_Description"];
                            if (MaxA > 0)
                            {
                                Att.PercAtte = Math.Round(CommonClass.GetPercentageAttendance(Student, Sem, (string)config["Unit"], MaxA), 0);
                            }
                            else
                            {
                                Att.PercAtte = 0;
                            }
                            UnitL.Add(Att);
                        }
                    }
                }
                return PartialView("~/Views/Dashboard/Partial Views/ClassAttendance.cshtml", UnitL);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [HttpPost]
        public JsonResult GenerateNewCateringToken()
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                Random rnd = new Random();
                int value = rnd.Next(1000, 9999);
                string Last2 = RegNo.Substring(RegNo.Length - 2);
                string Token = value.ToString() + Last2;
                Credentials.ObjNav.UpdateCateringToken(RegNo, Token);
                string msg = Token;

                return Json(new { message = msg, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}