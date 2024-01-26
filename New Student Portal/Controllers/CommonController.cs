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
    public class CommonController : Controller
    {
        public CommonController()
        {
        }

        [HttpGet]
        public virtual ActionResult AttachmentDownload(string fileName)
        {
            string str = base.Server.MapPath(string.Concat("~/Uploads/", fileName));
            return this.File(str, "application/octet-stream", fileName);
        }

        public PartialViewResult DocumentApprovalTrail(string DocNo, string Sequence)
        {
            List<ApprovalEntries> approvalEntries = new List<ApprovalEntries>();
            string[] docNo = new string[] { "StudentReqApprovalList?select=Approver_ID,Date_Time_Sent_for_Approval,Due_Date,Status,Sequence_No,ApproverNames&$filter=Document_No eq '", DocNo, "' and Status ne 'Canceled' and Status ne 'Rejected' and Sequence_No eq ", null, null };
            int num = Convert.ToInt32(Sequence);
            docNo[3] = num.ToString();
            docNo[4] = "&format=json";
            string str = string.Concat(docNo);
            using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
            {
                foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                {
                    ApprovalEntries approvalEntry = new ApprovalEntries()
                    {
                        DocNo = DocNo
                    };
                    string item1 = (string)item["ApproverNames"];
                    if (item1 == null)
                    {
                        approvalEntry.UserID = (string)item["Approver_ID"];
                    }
                    else
                    {
                        approvalEntry.UserID = item1;
                    }
                    DateTime dateTime = Convert.ToDateTime((string)item["Date_Time_Sent_for_Approval"]);
                    approvalEntry.DateSendForApproval = dateTime.ToString("dd/MM/yyyy");
                    dateTime = Convert.ToDateTime((string)item["Due_Date"]);
                    approvalEntry.DueDate = dateTime.ToString("dd/MM/yyyy");
                    approvalEntry.Status = (string)item["Status"];
                    approvalEntry.Sequence = Convert.ToInt32((string)item["Sequence_No"]);
                    approvalEntries.Add(approvalEntry);
                }
            }
            PartialViewResult partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ApprovalTrail.cshtml",
                from x in approvalEntries
                orderby x.Sequence
                select x);
            return partialViewResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult DocumentAttachmentview(int tblID, string No, int ID, string fileName, string ext)
        {
            JsonResult jsonResult;
            try
            {
                bool flag = false;
                bool flag1 = false;
                string str = "";
                string documentAttachmet = Credentials.GetDocumentAttachmet(tblID, No, ID);
                string str1 = string.Concat(fileName, ".", ext);
                byte[] numArray = Convert.FromBase64String(documentAttachmet);
                string str2 = base.Server.MapPath(string.Concat("~/Uploads/", str1));
                Credentials.DownloadAttachment(str2, numArray);
                str = str1;
                flag1 = false;
                flag = true;
                jsonResult = base.Json(new { message = str, success = flag, view = flag1 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false, view = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public PartialViewResult DocumentComments(string DocNo, string Sequence)
        {
            List<ApprovalComment> approvalComments = new List<ApprovalComment>();
            string[] docNo = new string[] { "ApprovalComments?select=Comment&$filter=Document_No eq '", DocNo, "' and Sequence_No eq ", null, null };
            int num = Convert.ToInt32(Sequence);
            docNo[3] = num.ToString();
            docNo[4] = "&$format=json";
            string str = string.Concat(docNo);
            using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
            {
                foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                {
                    ApprovalComment approvalComment = new ApprovalComment()
                    {
                        Comment = (string)item["Comment"]
                    };
                    approvalComments.Add(approvalComment);
                }
            }
            return this.PartialView("~/Views/Common/ApprovalComments.cshtml", approvalComments);
        }

        public PartialViewResult GetAcademicCalender(string Sem)
        {
            List<Academic_Calender> academicCalenders = new List<Academic_Calender>();
            string str = string.Concat("AcademicCalender?$filter=Semester eq '", Sem, "'&$format=json");
            using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
            {
                foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                {
                    Academic_Calender academicCalender = new Academic_Calender()
                    {
                        Event = (string)item["Event_Name"],
                        SD = (DateTime)item["Start_Date"]
                    };
                    DateTime dateTime = (DateTime)item["Start_Date"];
                    academicCalender.StartDate = dateTime.ToString("dd/MM/yyyy");
                    dateTime = (DateTime)item["End_Date"];
                    academicCalender.EndDate = dateTime.ToString("dd/MM/yyyy");
                    academicCalenders.Add(academicCalender);
                }
            }
            PartialViewResult partialViewResult = this.PartialView("~/Views/Common/AcademicCalender.cshtml", (
                from x in academicCalenders
                orderby x.SD
                select x).ToList<Academic_Calender>());
            return partialViewResult;
        }

        public PartialViewResult NotificationMessages()
        {
            List<Notifications> notifications = new List<Notifications>();
            using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData("CompayInformation?$select=Notificaion,Start_date&$filter=Category eq 'Student'&$format=json").GetResponseStream()))
            {
                foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                {
                    Notifications notification = new Notifications()
                    {
                        Message = (string)item["Notificaion"],
                        StartDate = (DateTime)item["Start_date"]
                    };
                    notifications.Add(notification);
                }
            }
            PartialViewResult partialViewResult = this.PartialView("~/Views/Common/Notification.cshtml", (
                from x in notifications
                orderby x.StartDate descending
                select x).ToList<Notifications>());
            return partialViewResult;
        }

        public void NullibySessions()
        {
            base.Session["SuccessMsg"] = null;
            base.Session["ErrorMsg"] = null;
        }

        public ActionResult Unauthorized()
        {
            return base.View();
        }
    }
}