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
    public class CommonController : Controller
    {
        // GET: Common
        public ActionResult Unauthorized()
        {
            return View();
        }
        public void NullibySessions()
        {
            Session["SuccessMsg"] = null;
            Session["ErrorMsg"] = null;
        }
        public PartialViewResult NotificationMessages()
        {
            List<Notifications> notList = new List<Notifications>();
            string pageLine = "CompayInformation?$select=Notificaion,Start_date&$filter=Category eq 'Student'&$format=json";
            HttpWebResponse httpResponse = Credentials.GetOdataData(pageLine);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                foreach (JObject config in details["value"])
                {
                    Notifications newNotif = new Notifications();
                    newNotif.Message = (string)config["Notificaion"];
                    newNotif.StartDate = (DateTime)config["Start_date"];
                    notList.Add(newNotif);
                }
            }
            return PartialView("~/Views/Common/Notification.cshtml", notList.OrderByDescending(x => x.StartDate).ToList());
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult DocumentAttachmentview(int tblID, string No, int ID, string fileName, string ext)
        {
            try
            {
                bool success = false, view = false;
                string msg = "";
                string Attachment = Credentials.GetDocumentAttachmet(tblID, No, ID);

                string fName = fileName + "." + ext;
                Byte[] bytes = Convert.FromBase64String(Attachment);
                string path = Server.MapPath("~/Uploads/" + fName);
                Credentials.DownloadAttachment(path, bytes);
                msg = fName;
                view = false;
                success = true;
                return Json(new { message = msg, success, view }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false, view = false }, JsonRequestBehavior.AllowGet); ;
            }
        }
        [HttpGet]
        public virtual ActionResult AttachmentDownload(string fileName)
        {
            string fullPath = Server.MapPath("~/Uploads/" + fileName);
            return File(fullPath, "application/octet-stream", fileName);
        }
        public PartialViewResult GetAcademicCalender(string Sem)
        {
            List<Academic_Calender> calenderList = new List<Academic_Calender>();
            string pageLine = "AcademicCalender?$filter=Semester eq '" + Sem + "'&$format=json";
            HttpWebResponse httpResponse = Credentials.GetOdataData(pageLine);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                foreach (JObject config in details["value"])
                {
                    Academic_Calender c = new Academic_Calender();
                    c.Event = (string)config["Event_Name"];
                    c.SD = (DateTime)config["Start_Date"];
                    c.StartDate = ((DateTime)config["Start_Date"]).ToString("dd/MM/yyyy");
                    c.EndDate = ((DateTime)config["End_Date"]).ToString("dd/MM/yyyy");
                    calenderList.Add(c);
                }
            }
            return PartialView("~/Views/Common/AcademicCalender.cshtml", calenderList.OrderBy(x => x.SD).ToList());
        }
        public PartialViewResult DocumentApprovalTrail(string DocNo)
        {
            List<ApprovalEntries> ApprovalTrail = new List<ApprovalEntries>();

            string page = "StudentReqApprovalList?$select=Approver_ID,Date_Time_Sent_for_Approval,Due_Date,Status,Sequence_No,ApproverNames&$filter=Document_No eq '" + DocNo + "' and Status ne 'Canceled' and Status ne 'Rejected'&format=json";

            HttpWebResponse httpResponse = Credentials.GetOdataData(page);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                foreach (JObject config in details["value"])
                {
                    ApprovalEntries AppTra = new ApprovalEntries();
                    AppTra.DocNo = DocNo;
                    string EmplName = (string)config["ApproverNames"];
                    if (EmplName != null)
                    {
                        AppTra.UserID = EmplName;
                    }
                    else
                    {
                        AppTra.UserID = (string)config["Approver_ID"];
                    }

                    AppTra.DateSendForApproval = Convert.ToDateTime((string)config["Date_Time_Sent_for_Approval"]).ToString("dd/MM/yyyy");
                    AppTra.DueDate = Convert.ToDateTime((string)config["Due_Date"]).ToString("dd/MM/yyyy");
                    AppTra.Status = (string)config["Status"];
                    AppTra.Sequence = Convert.ToInt32((string)config["Sequence_No"]);
                    ApprovalTrail.Add(AppTra);
                }
            }
            return PartialView("~/Views/Shared/Partial Views/ApprovalTrail.cshtml", ApprovalTrail.OrderBy(x => x.Sequence));
        }
        public PartialViewResult DocumentApprovalComments(string DocNo, string Seq)
        {
            List<ApprovalComment> CommentList = new List<ApprovalComment>();

            string page = "ApprovalComments?$select=Comment&$filter=Document_No eq '" + DocNo + "' and Sequence_No eq " + Convert.ToInt32(Seq) + "&$format=json";

            HttpWebResponse httpResponse = Credentials.GetOdataData(page);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                foreach (JObject config in details["value"])
                {
                    ApprovalComment c = new ApprovalComment();
                    c.Comment = (string)config["Comment"];
                    CommentList.Add(c);
                }
            }
            return PartialView("~/Views/Common/ApprovalComments.cshtml", CommentList);
        }
    }
}