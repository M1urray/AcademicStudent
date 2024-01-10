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
            string pageLine =
                "CompayInformation?$select=Notificaion,Start_date&$filter=Category eq 'Student'&$format=json";
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

            return PartialView("~/Views/Common/Notification.cshtml",
                notList.OrderByDescending(x => x.StartDate).ToList());
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
                return Json(new { message = ex.Message, success = false, view = false }, JsonRequestBehavior.AllowGet);
                ;
            }
        }

        [HttpGet]
        public virtual ActionResult AttachmentDownload(string fileName)
        {
            string fullPath = Server.MapPath("~/Uploads/" + fileName);
            return File(fullPath, "application/octet-stream", fileName);
        }


        public ActionResult GetNotice()
        {
            List<NoticeBoard> notList = new List<NoticeBoard>();
            string pageLine = "StudentNoticeBoard?$select=Description,Campus,Active,Date_Posted&$format=json";
            HttpWebResponse httpResponse = Credentials.GetOdataData(pageLine);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                foreach (JObject config in details["value"])
                {
                    bool check = ((bool)config["Active"]);
                    NoticeBoard noticeBoard = new NoticeBoard();
                    if (check)
                    {
                        noticeBoard.Description = (string)config["Description"];
                        noticeBoard.Campus = (string)config["Campus"];
                        noticeBoard.DatePosted = (string)config["Date_Posted"];
                        notList.Add(noticeBoard);
                    }
                }
            }

            return View(notList.OrderByDescending(x => x.DatePosted).ToList());
        }
        public ActionResult GetImportantDepartments()
        {
            List<ImportantDepartments> depList = new List<ImportantDepartments>();
            string pageLine = "InstitutionalDepartments?$select=Description,Campus,Contacts,School&$format=json";
            HttpWebResponse httpResponse = Credentials.GetOdataData(pageLine);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                foreach (JObject config in details["value"])
                {
                    // string studentCampus = Session["Campus"].ToString();
                    // string checkCampus = (string)config["Campus"].ToString();
                    ImportantDepartments importantDepartments = new ImportantDepartments();
                    importantDepartments.Description = (string)config["Description"];
                    importantDepartments.Campus = (string)config["Campus"];
                    importantDepartments.Contacts = (string)config["Contacts"];
                    importantDepartments.School = (string)config["School"];
                    depList.Add(importantDepartments);
                }
            }

            return View(depList);
        }
    }
}