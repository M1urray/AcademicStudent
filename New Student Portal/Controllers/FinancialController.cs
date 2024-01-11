using New_Student_Portal.CustomSecurity;
using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebGrease.Css.Ast.Selectors;

namespace New_Student_Portal.Controllers
{
    [CustomeAuthentication]
    [CustomAuthorization(Role = "STUD")]
    public class FinancialController : Controller
    {
        // GET: Financial
        public ActionResult FeeStatement()
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
                    List<FeeStatementDetails> FDetails = new List<FeeStatementDetails>();
                    string page = "DCust?$filter=Customer_No eq '" + RegNo + "' and Entry_Type eq 'Initial Entry' and Reversed eq false and Cust__Ledger_Entry_No gt 0&format=json";

                    HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            FeeStatementDetails FD = new FeeStatementDetails();
                            FD.DateOrder = (DateTime)config["Posting_Date"];
                            FD.Posting_Date = (string)config["Posting_Date"];
                            FD.Document_No = (string)config["Document_No"];
                            FD.Description = (string)config["Description"];
                            FD.Debit_Amount = (string)config["Debit_Amount"];
                            FD.Credit_Amount = (string)config["Credit_Amount"];
                            FD.RunnningBal = (string)config["Amount"];
                            FDetails.Add(FD);
                        }
                    }
                    return View(FDetails.OrderBy(x => x.DateOrder));
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return View("~/Views/Common/ErrorMessage.cshtml", error);
            }
        }
        public ActionResult Receipts()
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
                    List<Receipts> RCDetails = new List<Receipts>();
                    string page = "Receipts?$filter=Student_No eq '" + RegNo + "'&format=json";

                    HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            Receipts RC = new Receipts();
                            RC.Receipt_No = (string)config["Receipt_No"];
                            RC.Date = (string)config["Date"];
                            RC.Payment_Mode = (string)config["Payment_Mode"];
                            RC.Amount = (string)config["Amount"];
                            RCDetails.Add(RC);
                        }
                    }
                    return View(RCDetails);
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return View("~/Views/Common/ErrorMessage.cshtml", error);
            }
        }
        public ActionResult PaymentPlan()
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
        public PartialViewResult GetPaymentPlan()
        {
            try
            {
                string RegNo = Session["Username"].ToString();

                string semEndDate = "";

                if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester("");
                }
                string sem = Session["CurrentSem"].ToString();
                semEndDate = CommonClass.GetSemesterEndDate(sem);
                if (semEndDate != "")
                {
                    List<PaymentPlan> PPlan = new List<PaymentPlan>();
                    string page = "StudentPaymentPlan?$filter=Student_No eq '" + RegNo + "' and Semester eq '" + sem + "'&format=json";

                    HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        if (details["value"].Count() > 0)
                        {
                            foreach (JObject config in details["value"])
                            {
                                PaymentPlan RC = new PaymentPlan();
                                RC.ByDate = ((DateTime)config["Due_Date"]).ToString("dd/MM/yyyy");
                                RC.InstallNo = (string)config["Installment_No"];
                                RC.Percentage = (string)config["Installment_Percentage"];
                                RC.AmountDue = Convert.ToDecimal((string)config["Expected_Payment"]).ToString("#,##0.00");
                                PPlan.Add(RC);
                            }
                            //return PartialView("~/Views/Financial/Payment Plan/PaymentPlanData.cshtml", PPlan.OrderBy(x => x.InstallNo));
                        }
                        return PartialView("~/Views/Financial/Payment Plan/PaymentPlanData.cshtml", PPlan.OrderBy(x => x.InstallNo));
                        //else
                        //{
                        //    SemeterEndDate enddate = new SemeterEndDate();
                        //    enddate.SemEndDate = semEndDate;
                        //    return PartialView("~/Views/Financial/Payment Plan/PaymentPlanForm.cshtml", enddate);
                        //}
                    }
                }
                else
                {

                    Error erroMsg = new Error();
                    erroMsg.Message = "Semester end date not set";
                    return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
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
        public JsonResult SavePaymentPlan(List<Array> pPlan)
        {
            bool Val = false;
            string msg = "";
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();

                if (Session["CurrentSem"] == null || Session["CurrentSem"].ToString() == "")
                {
                    string Prog = CommonClass.GetStudentRegisteredProgramme(RegNo);
                    Session["CurrentSem"] = CommonClass.CurrentSemester(Prog);
                }
                string sem = Session["CurrentSem"].ToString();

                int RowCount = pPlan.Count();

                if (sem != "")
                {
                    decimal StdBal = CommonClass.GetStudentBalance(RegNo);
                    decimal previousPerc = 0, ActualPerc = 0, AmountDue = 0;
                    for (int i = 0; i < RowCount; i++)
                    {
                        string[] RowText = (string[])pPlan[i];

                        string installNo = RowText[0].Trim();
                        string byDate = RowText[1].Trim();
                        string perc = RowText[2].Trim();

                        if (byDate != "" && installNo != "" && perc != "")
                        {
                            ActualPerc = Convert.ToDecimal(perc) - previousPerc;
                            AmountDue = StdBal * (ActualPerc / 100);
                            DateTime Dby = DateTime.ParseExact(byDate.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            Credentials.ObjNav.InsertStudentPaymentPlan(RegNo, Dby, sem, installNo, Convert.ToDecimal(perc), AmountDue);
                            previousPerc = Convert.ToDecimal(perc);
                        }
                    }
                    Val = true;
                    msg = "Payment Plan saved successfully";
                }
                else
                {
                    Val = false;
                    msg = "Not registsred in the current semester";
                }
            }
            catch (Exception ex)
            {
                Val = false;
                msg = ex.Message.Replace("'", "");
            }
            return Json(new
            {
                message = msg,
                success = Val
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SponsorshipApplication()
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
        public PartialViewResult GetSponsorshipApplications()
        {
            try
            {
                string RegNo = Session["Username"].ToString();

                List<SponsorshipApplication> sponsApp = new List<SponsorshipApplication>();
                string page = "SponsorshipApplication?$filter=Student_No eq '" + RegNo + "'&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            SponsorshipApplication SApp = new SponsorshipApplication();
                            SApp.Student_No = (string)config["Student_No"];
                            SApp.Application_No = (string)config["Application_No"];
                            SApp.Application_Date = ((DateTime)config["Application_Date"]).ToString("dd/MM/yyyy");
                            SApp.Applied_Amount = Convert.ToDecimal((string)config["Applied_Amount"]).ToString("#,##0.00");
                            SApp.Approved_Amount = Convert.ToDecimal((string)config["Approved_Amount"]).ToString("#,##0.00");
                            SApp.Remarks = (string)config["Remarks"];
                            SApp.Status = (string)config["Status"];
                            sponsApp.Add(SApp);
                        }
                    }
                    return PartialView("~/Views/Financial/SponsorshipApp/SponsorshipApplications.cshtml", sponsApp.OrderBy(x => x.Application_No));
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }

        public ActionResult PayFees()
        {

            if (Session["Username"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                try
                {
                    string sem = Session["CurrentSem"].ToString();
                    string page = "CustomerList?$filter=No eq '" + Session["Username"] + "'&$format=json";

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
                            Details.Date_Of_Birth = ((DateTime)config["Date_Of_Birth"]).ToString("dd/MM/yyyy");
                            Details.Phone_No = (string)config["Phone_No"];
                            Details.Address = (string)config["Address"];
                            Details.E_Mail = (string)config["E_Mail"];
                            Details.Campus = (string)config["Global_Dimension_1_Code"];
                            Details.Balance = (decimal)config["Balance_LCY"];
                            Details.Debit_Amount = (decimal)config["Debit_Amount"];
                            Details.Credit_Amount = (decimal)config["Credit_Amount"];                            
                            Details.ProgName = (string)config["Programme_Name"];
                            
                            Details.Semester = sem;
                            Details.AcademicStatus = CommonClass.StudentStatusDescription((string)config["Academic_Status"]);
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
        }
    }
}