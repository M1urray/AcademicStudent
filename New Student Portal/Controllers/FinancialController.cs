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
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.Controllers
{
    [CustomAuthorization(Role = "STUD")]
    [CustomeAuthentication]
    public class FinancialController : Controller
    {
        public FinancialController()
        {
        }

        public ActionResult FeeStatement()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    List<FeeStatementDetails> feeStatementDetails = new List<FeeStatementDetails>();
                    string str1 = string.Concat("DCust?$filter=Customer_No eq '", str, "' and Entry_Type eq 'Initial Entry' and Reversed eq false and Cust__Ledger_Entry_No gt 0&format=json");
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            FeeStatementDetails feeStatementDetail = new FeeStatementDetails()
                            {
                                DateOrder = (DateTime)item["Posting_Date"],
                                Posting_Date = (string)item["Posting_Date"],
                                Document_No = (string)item["Document_No"],
                                Description = (string)item["Description"],
                                Debit_Amount = (string)item["Debit_Amount"],
                                Credit_Amount = (string)item["Credit_Amount"],
                                RunnningBal = (string)item["Amount"]
                            };
                            feeStatementDetails.Add(feeStatementDetail);
                        }
                    }
                    action = base.View(
                        from x in feeStatementDetails
                        orderby x.DateOrder
                        select x);
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

        public PartialViewResult GetPaymentPlan()
        {
            PartialViewResult partialViewResult;
            try
            {
                string str = base.Session["Username"].ToString();
                if ((base.Session["CurrentSem"] == null ? true : base.Session["CurrentSem"].ToString() == ""))
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                string str1 = base.Session["CurrentSem"].ToString();
                if (CommonClass.GetSemesterEndDate(str1) == "")
                {
                    Error error = new Error()
                    {
                        Message = "Semester end date not set"
                    };
                    partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
                }
                else
                {
                    List<PaymentPlan> paymentPlans = new List<PaymentPlan>();
                    string str2 = string.Concat(new string[] { "StudentPaymentPlan?$filter=Student_No eq '", str, "' and Semester eq '", str1, "'&format=json" });
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str2).GetResponseStream()))
                    {
                        JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                        if (jObjects["value"].Count<JToken>() > 0)
                        {
                            foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                            {
                                PaymentPlan paymentPlan = new PaymentPlan();
                                DateTime dateTime = (DateTime)item["Due_Date"];
                                paymentPlan.ByDate = dateTime.ToString("dd/MM/yyyy");
                                paymentPlan.InstallNo = (string)item["Installment_No"];
                                paymentPlan.Percentage = (string)item["Installment_Percentage"];
                                decimal num = Convert.ToDecimal((string)item["Expected_Payment"]);
                                paymentPlan.AmountDue = num.ToString("#,##0.00");
                                paymentPlans.Add(paymentPlan);
                            }
                        }
                        partialViewResult = this.PartialView("~/Views/Financial/Payment Plan/PaymentPlanData.cshtml",
                            from x in paymentPlans
                            orderby x.InstallNo
                            select x);
                    }
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

        public PartialViewResult GetSponsorshipApplications()
        {
            PartialViewResult partialViewResult;
            try
            {
                string str = base.Session["Username"].ToString();
                List<SponsorshipApplication> sponsorshipApplications = new List<SponsorshipApplication>();
                string str1 = string.Concat("SponsorshipApplication?$filter=Student_No eq '", str, "'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            SponsorshipApplication sponsorshipApplication = new SponsorshipApplication()
                            {
                                Student_No = (string)item["Student_No"],
                                Application_No = (string)item["Application_No"]
                            };
                            DateTime dateTime = (DateTime)item["Application_Date"];
                            sponsorshipApplication.Application_Date = dateTime.ToString("dd/MM/yyyy");
                            decimal num = Convert.ToDecimal((string)item["Applied_Amount"]);
                            sponsorshipApplication.Applied_Amount = num.ToString("#,##0.00");
                            num = Convert.ToDecimal((string)item["Approved_Amount"]);
                            sponsorshipApplication.Approved_Amount = num.ToString("#,##0.00");
                            sponsorshipApplication.Remarks = (string)item["Remarks"];
                            sponsorshipApplication.Status = (string)item["Status"];
                            sponsorshipApplications.Add(sponsorshipApplication);
                        }
                    }
                    partialViewResult = this.PartialView("~/Views/Financial/SponsorshipApp/SponsorshipApplications.cshtml",
                        from x in sponsorshipApplications
                        orderby x.Application_No
                        select x);
                }
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

        public ActionResult PaymentPlan()
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

        public ActionResult Receipts()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    List<Receipts> receipts = new List<Receipts>();
                    string str1 = string.Concat("Receipts?$filter=Student_No eq '", str, "'&format=json");
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            Receipts receipt = new Receipts()
                            {
                                Receipt_No = (string)item["Receipt_No"],
                                Date = (string)item["Date"],
                                Payment_Mode = (string)item["Payment_Mode"],
                                Amount = (string)item["Amount"]
                            };
                            receipts.Add(receipt);
                        }
                    }
                    action = base.View(receipts);
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
        public JsonResult SavePaymentPlan(List<Array> pPlan)
        {
            bool flag = false;
            string str = "";
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str1 = base.Session["Username"].ToString();
                if ((base.Session["CurrentSem"] == null ? true : base.Session["CurrentSem"].ToString() == ""))
                {
                    string studentRegisteredProgramme = CommonClass.GetStudentRegisteredProgramme(str1);
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(studentRegisteredProgramme);
                }
                string str2 = base.Session["CurrentSem"].ToString();
                int num = pPlan.Count<Array>();
                if (str2 == "")
                {
                    flag = false;
                    str = "Not registsred in the current semester";
                }
                else
                {
                    decimal studentBalance = CommonClass.GetStudentBalance(str1);
                    decimal num1 = new decimal();
                    decimal num2 = new decimal();
                    decimal num3 = new decimal();
                    for (int i = 0; i < num; i++)
                    {
                        string[] item = (string[])pPlan[i];
                        string str3 = item[0].Trim();
                        string str4 = item[1].Trim();
                        string str5 = item[2].Trim();
                        if ((!(str4 != "") || !(str3 != "") ? false : str5 != ""))
                        {
                            num2 = Convert.ToDecimal(str5) - num1;
                            num3 = studentBalance * (num2 / new decimal(100));
                            DateTime.ParseExact(str4.Replace("-", "/"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                            num1 = Convert.ToDecimal(str5);
                        }
                    }
                    flag = true;
                    str = "Payment Plan saved successfully";
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                flag = false;
                str = exception.Message.Replace("'", "");
            }
            JsonResult jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }

        public ActionResult SponsorshipApplication()
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
    }
}