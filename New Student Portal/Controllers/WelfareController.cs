using New_Student_Portal.Models;
using Newtonsoft.Json.Linq;
using New_Student_Portal.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using New_Student_Portal.CustomSecurity;

namespace New_Student_Portal.Controllers
{
    [CustomeAuthentication]
    [CustomAuthorization(Role = "STUD")]
    public class WelfareController : Controller
    {
        // GET: Welfare      
        public ActionResult HostelList()
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
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                    }
                    string sem = Session["CurrentSem"].ToString();

                    string[] r = CommonClass.CurrentCourseRegistration(RegNo, Sem,"0");
                    if (r[0] == "")
                    {
                        Error errormsg = new Error();
                        errormsg.Message = "You have not been registered in the current semester. Register for Units first";
                        return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                    }
                    else if (Convert.ToInt32(r[3]) < 1)
                    {
                        Error errormsg = new Error();
                        errormsg.Message = "You need to register for units before booking for hostel";
                        return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                    }
                    else if (r[5] != "University Accommodation")
                    {
                        Error errormsg = new Error();
                        errormsg.Message = "You did not choose University Accommondation !!";
                        return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                    }
                    else
                    {

                        bool AllowBooking = CommonClass.AllowOnlyY1S1(RegNo);
                        if (AllowBooking)
                        {
                            string residency = CommonClass.GetStudentResidence(RegNo, sem);
                            if (residency == "Non Resident")
                            {
                                #region Residential Details
                                NonResidenceData resDetails = new NonResidenceData();
                                string page = "NonResidence?$filter=Student_No eq '" + RegNo + "' and Semester eq '" + sem + "'&$format=json";

                                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                                {
                                    var result = streamReader.ReadToEnd();

                                    var details = JObject.Parse(result);

                                    foreach (JObject config in details["value"])
                                    {
                                        resDetails = new NonResidenceData
                                        {
                                            Student = RegNo,
                                            Sem = sem,
                                            Premise = (string)config["Residential_Premise"],
                                            RoomNo = (string)config["Room_No"],
                                            LandLoard = (string)config["LardLord_Name"],
                                            Caretaker = (string)config["Caretaker_Name"],
                                            Witness = (string)config["Witness"],
                                            AreaName = (string)config["Area_Name"]
                                        };
                                    }
                                }
                                #endregion
                                return View("~/Views/Welfare/NonResidenceForm.cshtml", resDetails);
                            }
                            else
                            {
                                bool s = HasBookedHostel(RegNo, sem);

                                if (s)
                                {
                                    RoomSpaces bookedSpaceDetails = new RoomSpaces();
                                    #region Hostel Booked Details
                                    string page = "StudentHostelRooms?$filter=Student eq '" + RegNo + "' and Semester eq '" + sem + "' and Cleared eq false&$format=json";

                                    HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                                    using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                                    {
                                        var result = streamReader.ReadToEnd();

                                        var details = JObject.Parse(result);

                                        foreach (JObject config in details["value"])
                                        {
                                            bookedSpaceDetails = new RoomSpaces
                                            {
                                                Student = RegNo,
                                                Sem = sem,
                                                HostelCode = (string)config["Hostel_No"],
                                                RoomCode = (string)config["Room_No"],
                                                SpaceCode = (string)config["Space_No"],
                                                Cost = Convert.ToDecimal((string)config["Accomodation_Fee"]).ToString("#,##0.00"),
                                                Billed = (bool)config["Billed"]
                                            };
                                        }
                                    }
                                    #endregion

                                    return PartialView("~/Views/Welfare/BookedSpaceDetails.cshtml", bookedSpaceDetails);
                                }
                                else
                                {
                                    bool allowHostelBooking = CommonClass.AllowHostelBooking();
                                    if (allowHostelBooking)
                                    {
                                        #region Hostel List
                                        string gender = CommonClass.GetStudentGender(RegNo);
                                        if (gender != "")
                                        {
                                            List<Hostel> HostelList = new List<Hostel>();
                                            string page = "";

                                            //string Hostel = CommonClass.GetHostelFromCourseReg(RegNo, sem);
                                            //if (Hostel != "")
                                            //{
                                            //    page = "HostelCard?$filter=Asset_No eq '" + Hostel + "' and Gender eq '" + gender + "' and Not_Available eq false&$format=json";
                                            //}
                                            //else
                                            //{
                                            //    page = "HostelCard?$filter=Gender eq '" + gender + "' and Not_Available eq false&$format=json";
                                            //}
                                            page = "HostelCard?$filter=Gender eq '" + gender + "' and Not_Available eq false and Total_Vacant gt 0&$format=json";
                                            HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                                            using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                                            {
                                                var result = streamReader.ReadToEnd();

                                                var details = JObject.Parse(result);

                                                foreach (JObject config in details["value"])
                                                {
                                                    Hostel Hlist = new Hostel();
                                                    Hlist.AssetNo = (string)config["Asset_No"];
                                                    Hlist.Description = (string)config["Discription"];
                                                    //Hlist.VacantSpaces = (int)config["Semester"];
                                                    HostelList.Add(Hlist);
                                                }
                                            }
                                            return View(HostelList);
                                        }
                                        else
                                        {
                                            Error erroMsg = new Error();
                                            erroMsg.Message = "Your gender has not been set. Contact admission";
                                            return PartialView("~/Views/Shared/ErrorMessange.cshtml", erroMsg);
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        Error errormsg = new Error();
                                        errormsg.Message = "Hostel Booking not allowed at the moment";
                                        return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Error errormsg = new Error();
                            errormsg.Message = "Hostel Booking allowed only for first years at the moment";
                            return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return View("~/Views/Common/ErrorMessage.cshtml", error);
            }
        }
        protected bool HasBookedHostel(string StdNo, string sem)
        {
            bool s = false;
            try
            {
                string page = "StudentHostelRooms?$filter=Student eq '" + StdNo + "' and Semester eq '" + sem + "' and Cleared eq false&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);

                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        s = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        public ActionResult HostelRoomList(string HostelCode, string HostelName)
        {
            return View();
        }
        public PartialViewResult HostelBlockRooms(string HostelCode)
        {
            try
            {
                #region Hostel Room List

                List<Rooms> HostelRoomList = new List<Rooms>();
                string page = "HostelBlockRooms?$filter=Hostel_Code eq '" + HostelCode + "' and (Status eq 'Vaccant' or Status eq 'Partially Occupied')  and NotAvaillable eq false&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        Rooms room = new Rooms();
                        room.HostelCode = HostelCode;
                        room.RoomCode = (string)config["RoomCode"];
                        room.Cost = Convert.ToDecimal((string)config["RoomCost"]).ToString("#,##0.00");
                        room.Status = (string)config["Status"];
                        //room.VacantSpaces = (string)config["Discription"];
                        HostelRoomList.Add(room);
                    }
                }
                #endregion

                return PartialView("~/Views/Welfare/Partial View/HostelBlockRooms.cshtml", HostelRoomList);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        public PartialViewResult RoomSpacesList(string HostelCode, string RoomCode, string Cost)
        {
            try
            {
                #region Room Space List

                List<RoomSpaces> RoomSpaceList = new List<RoomSpaces>();
                string page = "RoomSpaces?$select=BedSpaces,Status&$filter=HostelCode eq '" + HostelCode + "' and RoomCode eq '" + RoomCode + "' and Status eq 'Vaccant' and RoomNotAvaillable eq false&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        RoomSpaces space = new RoomSpaces();
                        space.HostelCode = HostelCode;
                        space.RoomCode = RoomCode;
                        space.SpaceCode = (string)config["BedSpaces"];
                        space.Cost = Cost;
                        space.Status = (string)config["Status"];
                        RoomSpaceList.Add(space);
                    }
                }
                #endregion

                return PartialView("~/Views/Welfare/Partial View/RoomSpaces.cshtml", RoomSpaceList);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [HttpPost]
        public JsonResult SaveSelectedSpace(RoomSpaces spaceDetails)
        {
            try
            {
                string msg = "";
                bool ValSucc = false;
                string StdNo = Session["Username"].ToString();
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester(StdNo);
                }
                string sem = Session["CurrentSem"].ToString();

                Credentials.ObjNav.GenerateHostelAllocationWithCateringCharge(StdNo, sem, spaceDetails.HostelCode, spaceDetails.RoomCode, spaceDetails.SpaceCode, Convert.ToDecimal(spaceDetails.Cost));
                msg = "Space booked successfully";
                ValSucc = true;
                return Json(new { message = msg, success = ValSucc }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message.Replace("'", ""), success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult PrintHostelInvoice()
        {
            bool success = false;
            try
            {
                string message = "";

                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                else
                {
                    string StudentNo = Session["Username"].ToString();
                    string filename = StudentNo.Replace("/", "");

                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(StudentNo);
                    }
                    string sem = Session["CurrentSem"].ToString();

                    string page = "StudentHostelRooms?$filter=Student eq '" + StudentNo + "' and Semester eq '" + sem + "' and Cleared eq false&$format=json";

                    HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            Credentials.ObjNav.PrintHostelInvoice(StudentNo, (string)config["Hostel_No"], (string)config["Room_No"], (string)config["Space_No"], sem, "HOSTEL INV-" + filename + ".pdf");
                            filename = "HOSTEL INV-" + filename + ".pdf";
                        }
                    }

                    MoveFile(filename);
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = @"/Downloads/" + filename;
                    }
                    else
                    {
                        success = false;
                        message = "File Not Found";
                    }

                }
                return Json(new { message = message, success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult PrintHostelClearanceForm()
        {
            bool success = false;
            try
            {
                string message = "";

                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                else
                {
                    string StudentNo = Session["Username"].ToString();
                    string filename = StudentNo.Replace("/", "");


                    Credentials.ObjNav.PrintHostelClearanceForm(StudentNo, "HOSTEL CLEARANCE-" + filename + ".pdf");
                    filename = "HOSTEL CLEARANCE-" + filename + ".pdf";

                    //MoveFile(filename);
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = @"/Downloads/" + filename;
                    }
                    else
                    {
                        success = false;
                        message = "File Not Found";
                    }

                }
                return Json(new { message = message, success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult PrintNonResidentialForm()
        {
            bool success = false;
            try
            {
                string message = "";

                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                else
                {
                    string StudentNo = Session["Username"].ToString();
                    string filename = StudentNo.Replace("/", "");

                    Credentials.ObjNav.PrintResidentialForm(Session["username"].ToString(), "RESIDENTIALFORM-" + filename + ".pdf");
                    filename = "RESIDENTIALFORM-" + filename + ".pdf";

                    //MoveFile(filename);
                    string DestinationPath = Server.MapPath("~/Downloads/" + filename);
                    System.IO.FileInfo file = new System.IO.FileInfo(DestinationPath);
                    if (file.Exists)
                    {
                        success = true;
                        message = @"/Downloads/" + filename;
                    }
                    else
                    {
                        success = false;
                        message = "File Not Found";
                    }

                }
                return Json(new { message = message, success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success }, JsonRequestBehavior.AllowGet);
            }
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetHostelList()
        {
            try
            {
                string RegNo = Session["Username"].ToString();

                HostelList HostD = new HostelList();
                string gender = CommonClass.GetStudentGender(RegNo);
                if (gender != "")
                {
                    #region Hostel List
                    List<Hostel> HostelList = new List<Hostel>();
                    string page = "HostelList?$filter=Gender eq '" + gender + "' and Not_Available eq false and Total_Vacant gt 0&$format=json";

                    HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            Hostel Hlist = new Hostel();
                            Hlist.AssetNo = (string)config["AssetNo"];
                            Hlist.Description = (string)config["Discription"];
                            HostelList.Add(Hlist);
                        }
                    }
                    #endregion
                    HostD = new HostelList
                    {
                        ListOfHostels = HostelList.Select(x =>
                                        new SelectListItem()
                                        {
                                            Text = x.Description,
                                            Value = x.AssetNo
                                        }).ToList()
                    };
                }
                return Json(new { message = HostD, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult MealBooking()
        {
            try
            {
                string RegNo = Session["Username"].ToString();
                if (Session["Username"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                    }
                    string sem = Session["CurrentSem"].ToString();
                    string[] r = CommonClass.CurrentCourseRegistration(RegNo, Sem,"0");
                    if (r[0] == "")
                    {
                        Error errormsg = new Error();
                        errormsg.Message = "You have not been registered in the current semester. Register for Units first";
                        return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                    }
                    else if (Convert.ToInt32(r[3]) < 1)
                    {
                        Error errormsg = new Error();
                        errormsg.Message = "You need to register for units before booking for Meals";
                        return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                    }
                    else if (r[5] != "University Accommodation")
                    {
                        Error errormsg = new Error();
                        errormsg.Message = "You did not choose University Accommondation !!";
                        return View("~/Views/Shared/ErrorMessange.cshtml", errormsg);
                    }
                    else
                    {
                        return View();
                    }
                }
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return View("~/Views/Common/ErrorMessage.cshtml", error);
            }
        }
        public PartialViewResult MealBookingForm()
        {
            try
            {
                string RegNo = Session["Username"].ToString();
                MealBooking mealB = new MealBooking();
                try
                {
                    if (Session["CurrentSem"] == null)
                    {
                        Session["CurrentSem"] = CommonClass.CurrentSemester(RegNo);
                    }
                    string sem = Session["CurrentSem"].ToString();
                    string pageRoom = "CourseReg?$filter=StudentNo eq '" + RegNo + "' and Semester eq '" + sem + "' and Meals_Booked eq true&$format=json";

                    HttpWebResponse httpResponseRoom = Credentials.GetOdataData(pageRoom);
                    using (var streamReaderRoom = new StreamReader(httpResponseRoom.GetResponseStream()))
                    {
                        var resultRoom = streamReaderRoom.ReadToEnd();

                        var detailsRoom = JObject.Parse(resultRoom);

                        if (detailsRoom["value"].Count() > 0)
                        {
                            mealB.Message = "You have already booked for meals";
                            mealB.BookedMeals = true;
                        }
                        else
                        {
                            string pageCharge = "ChargeList?$select=Amount&$filter=Code eq 'CAFETERIA'&$format=json";

                            HttpWebResponse httpResponseCharge = Credentials.GetOdataData(pageCharge);
                            using (var streamReaderCharge = new StreamReader(httpResponseCharge.GetResponseStream()))
                            {
                                var resultCharge = streamReaderCharge.ReadToEnd();

                                var detailsCharge = JObject.Parse(resultCharge);

                                if (detailsCharge["value"].Count() > 0)
                                {
                                    foreach (JObject config2 in detailsCharge["value"])
                                    {
                                        mealB.Message = "Total Meal Charge = Ksh. " + Math.Round(((decimal)config2["Amount"]), 2).ToString("#,##0.00");
                                        mealB.BookedMeals = false;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    mealB.Message = ex.Message;
                    mealB.BookedMeals = false;
                }
                return PartialView("~/Views/Welfare/Partial View/MealBookingForm.cshtml", mealB);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }
        [HttpPost]
        public JsonResult SubmitMealBooking()
        {
            try
            {
                string msg = "";
                bool ValSucc = false;
                string StdNo = Session["Username"].ToString();
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester(StdNo);
                }
                string sem = Session["CurrentSem"].ToString();

                Credentials.ObjNav.MealBooking(StdNo, sem);
                msg = "Meals booked successfully";
                ValSucc = true;
                return Json(new { message = msg, success = ValSucc }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message.Replace("'", ""), success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        private bool MoveFile(string FileName)
        {
            bool s = false;
            try
            {
                string sourcefile = Credentials.fileSourcePath + FileName;
                string destinationfile = Server.MapPath("~/Downloads/" + FileName);
                if (System.IO.File.Exists(destinationfile) == true)
                {
                    System.IO.File.Delete(destinationfile);
                    System.IO.File.Move(sourcefile, destinationfile);
                }
                if (System.IO.File.Exists(destinationfile) == false)
                {
                    System.IO.File.Move(sourcefile, destinationfile);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        public ActionResult SponsorshipApplication()
        {
            try
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
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return View("~/Views/Common/ErrorMessage.cshtml", erroMsg);
            }
        }
        public PartialViewResult SponsorshipApplicationList()
        {
            try
            {
                string StdNo = Session["Username"].ToString();
                List<SponsorshipApplication> SPNSHPList = new List<SponsorshipApplication>();

                string page = "SponsorshipApplication?$filter=Student_No eq '" + StdNo + "'&$format=json";
                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    foreach (JObject config in details["value"])
                    {
                        SponsorshipApplication SPList = new SponsorshipApplication();
                        SPList.Student_No = (string)config["Student_No"];
                        SPList.Application_No = (string)config["Application_No"];
                        SPList.Application_Date = ((DateTime)config["Application_Date"]).ToString("dd/MM/yyyy");
                        SPList.Applied_Amount = Convert.ToDecimal((string)config["Applied_Amount"]).ToString("#,##0.00");
                        SPList.Status = (string)config["Status"];
                        SPNSHPList.Add(SPList);
                    }
                }
                return PartialView("~/Views/Welfare/Partial View/SponsorshipApplicationList.cshtml", SPNSHPList.OrderByDescending(x => x.Application_No));
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
            }
        }
        public PartialViewResult NewSponsorshipApplication()
        {
            try
            {
                SponsorshipApplication SPApp = new SponsorshipApplication();
                return PartialView("~/Views/Welfare/Partial View/NewSponsorshipApplication.cshtml", SPApp);
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
            }
        }
        public JsonResult SubmitSponsorshipApplication(string Amount, string Remarks)
        {
            try
            {
                string StdNo = Session["Username"].ToString();

                //Credentials.ObjNav.InsertStudentSponsorshipApplication(StdNo,Convert.ToDecimal(Amount), Remarks);
                return Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult SponsorshipApplicationDoc(string DocNo)
        {
            try
            {
                SponsorshipApplication SPApp = new SponsorshipApplication();
                string page = "SponsorshipApplication?$filter=Application_No eq '" + DocNo + "'&$format=json";
                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    foreach (JObject config in details["value"])
                    {
                        SPApp.Student_No = (string)config["Student_No"];
                        SPApp.Application_No = (string)config["Application_No"];
                        SPApp.Application_Date = ((DateTime)config["Application_Date"]).ToString("dd/MM/yyyy");
                        SPApp.Applied_Amount = Convert.ToDecimal((string)config["Applied_Amount"]).ToString("#,##0.00");
                        SPApp.Approved_Amount = Convert.ToDecimal((string)config["Approved_Amount"]).ToString("#,##0.00");
                        SPApp.Remarks = (string)config["Remarks"];
                        SPApp.Status = (string)config["Status"];
                        SPApp.ApprovalStatus = (string)config["Approval_Status"];
                        SPApp.Recommendation = (string)config["Recommendation"];
                    }
                }
                return PartialView("~/Views/Welfare/Partial View/SponsorshipApplicationDoc.cshtml", SPApp);
            }
            catch (Exception ex)
            {
                Error erroMsg = new Error();
                erroMsg.Message = ex.Message;
                return PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", erroMsg);
            }
        }
        public ActionResult ScolorshipApplcationForm()
        {
            return View();
        }
        public PartialViewResult StepView(string Step)
        {
            return PartialView("~/Views/Welfare/Steps/" + Step + ".cshtml");
        }
        [HttpPost]
        public JsonResult SaveStepTwoSection()
        {
            try
            {
                string StdNo = Session["Username"].ToString();

                //Credentials.ObjNav.InsertStudentSponsorshipApplication(StdNo,Convert.ToDecimal(Amount), Remarks);
                return Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult SaveStepThreeSection()
        {
            try
            {
                string StdNo = Session["Username"].ToString();

                //Credentials.ObjNav.InsertStudentSponsorshipApplication(StdNo,Convert.ToDecimal(Amount), Remarks);
                return Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult SaveStepFourSection()
        {
            try
            {
                string StdNo = Session["Username"].ToString();

                //Credentials.ObjNav.InsertStudentSponsorshipApplication(StdNo,Convert.ToDecimal(Amount), Remarks);
                return Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult SaveStepFiveSection()
        {
            try
            {
                string StdNo = Session["Username"].ToString();

                //Credentials.ObjNav.InsertStudentSponsorshipApplication(StdNo,Convert.ToDecimal(Amount), Remarks);
                return Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult SaveStepSixSection()
        {
            try
            {
                string StdNo = Session["Username"].ToString();

                //Credentials.ObjNav.InsertStudentSponsorshipApplication(StdNo,Convert.ToDecimal(Amount), Remarks);
                return Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult SaveStepSevenrSection()
        {
            try
            {
                string StdNo = Session["Username"].ToString();

                //Credentials.ObjNav.InsertStudentSponsorshipApplication(StdNo,Convert.ToDecimal(Amount), Remarks);
                return Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}