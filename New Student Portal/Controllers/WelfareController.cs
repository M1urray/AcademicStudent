using New_Student_Portal.CustomSecurity;
using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public class WelfareController : Controller
    {
        public WelfareController()
        {
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult GetHostelList()
        {
            JsonResult jsonResult;
            try
            {
                string str = base.Session["Username"].ToString();
                HostelList hostelList = new HostelList();
                string studentGender = CommonClass.GetStudentGender(str);
                if (studentGender != "")
                {
                    List<Hostel> hostels = new List<Hostel>();
                    string str1 = string.Concat("HostelList?$filter=Gender eq '", studentGender, "' and Not_Available eq false and Total_Vacant gt 0&$format=json");
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            Hostel hostel = new Hostel()
                            {
                                AssetNo = (string)item["AssetNo"],
                                Description = (string)item["Discription"]
                            };
                            hostels.Add(hostel);
                        }
                    }
                    hostelList = new HostelList()
                    {
                        ListOfHostels = (
                            from x in hostels
                            select new SelectListItem()
                            {
                                Text = x.Description,
                                Value = x.AssetNo
                            }).ToList<SelectListItem>()
                    };
                }
                jsonResult = base.Json(new { message = hostelList, success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        protected bool HasBookedHostel(string StdNo, string sem)
        {
            bool flag = false;
            try
            {
                string str = string.Concat(new string[] { "StudentHostelRooms?$filter=Student eq '", StdNo, "' and Semester eq '", sem, "' and Cleared eq false&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        flag = true;
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public PartialViewResult HostelBlockRooms(string HostelCode)
        {
            PartialViewResult partialViewResult;
            try
            {
                List<Rooms> rooms = new List<Rooms>();
                string str = string.Concat("HostelBlockRooms?$filter=Hostel_Code eq '", HostelCode, "' and (Status eq 'Vaccant' or Status eq 'Partially Occupied')  and NotAvaillable eq false&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        Rooms room = new Rooms()
                        {
                            HostelCode = HostelCode,
                            RoomCode = (string)item["RoomCode"]
                        };
                        decimal num = Convert.ToDecimal((string)item["RoomCost"]);
                        room.Cost = num.ToString("#,##0.00");
                        room.Status = (string)item["Status"];
                        rooms.Add(room);
                    }
                }
                partialViewResult = this.PartialView("~/Views/Welfare/Partial View/HostelBlockRooms.cshtml", rooms);
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

        public ActionResult HostelList()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    string str = base.Session["Username"].ToString();
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    string str1 = base.Session["CurrentSem"].ToString();
                    string[] strArrays = CommonClass.CurrentCourseRegistration(str, str1, "0");
                    if (strArrays[0] == "")
                    {
                        Error error = new Error()
                        {
                            Message = "You have not been registered in the current semester. Register for Units first"
                        };
                        action = base.View("~/Views/Shared/ErrorMessange.cshtml", error);
                    }
                    else if (Convert.ToInt32(strArrays[3]) < 1)
                    {
                        Error error1 = new Error()
                        {
                            Message = "You need to register for units before booking for hostel"
                        };
                        action = base.View("~/Views/Shared/ErrorMessange.cshtml", error1);
                    }
                    else if (!CommonClass.AllowOnlyY1S1(str))
                    {
                        Error error2 = new Error()
                        {
                            Message = "Hostel Booking allowed only for first years at the moment"
                        };
                        action = base.View("~/Views/Shared/ErrorMessange.cshtml", error2);
                    }
                    else if (CommonClass.GetStudentResidence(str, str1) == "Non Resident")
                    {
                        NonResidenceData nonResidenceDatum = new NonResidenceData();
                        string str2 = string.Concat(new string[] { "NonResidence?$filter=Student_No eq '", str, "' and Semester eq '", str1, "'&$format=json" });
                        using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str2).GetResponseStream()))
                        {
                            foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                            {
                                nonResidenceDatum = new NonResidenceData()
                                {
                                    Student = str,
                                    Sem = str1,
                                    Premise = (string)item["Residential_Premise"],
                                    RoomNo = (string)item["Room_No"],
                                    LandLoard = (string)item["LardLord_Name"],
                                    Caretaker = (string)item["Caretaker_Name"],
                                    Witness = (string)item["Witness"],
                                    AreaName = (string)item["Area_Name"]
                                };
                            }
                        }
                        action = base.View("~/Views/Welfare/NonResidenceForm.cshtml", nonResidenceDatum);
                    }
                    else if (this.HasBookedHostel(str, str1))
                    {
                        RoomSpaces roomSpace = new RoomSpaces();
                        string str3 = string.Concat(new string[] { "StudentHostelRooms?$filter=Student eq '", str, "' and Semester eq '", str1, "' and Cleared eq false&$format=json" });
                        using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str3).GetResponseStream()))
                        {
                            foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader1.ReadToEnd())["value"])
                            {
                                RoomSpaces roomSpace1 = new RoomSpaces()
                                {
                                    Student = str,
                                    Sem = str1,
                                    HostelCode = (string)jObjects["Hostel_No"],
                                    RoomCode = (string)jObjects["Room_No"],
                                    SpaceCode = (string)jObjects["Space_No"]
                                };
                                decimal num = Convert.ToDecimal((string)jObjects["Accomodation_Fee"]);
                                roomSpace1.Cost = num.ToString("#,##0.00");
                                roomSpace1.Billed = (bool)jObjects["Billed"];
                                roomSpace = roomSpace1;
                            }
                        }
                        action = this.PartialView("~/Views/Welfare/BookedSpaceDetails.cshtml", roomSpace);
                    }
                    else if (!CommonClass.AllowHostelBooking())
                    {
                        Error error3 = new Error()
                        {
                            Message = "Hostel Booking not allowed at the moment"
                        };
                        action = base.View("~/Views/Shared/ErrorMessange.cshtml", error3);
                    }
                    else
                    {
                        string studentGender = CommonClass.GetStudentGender(str);
                        if (studentGender == "")
                        {
                            Error error4 = new Error()
                            {
                                Message = "Your gender has not been set. Contact admission"
                            };
                            action = this.PartialView("~/Views/Shared/ErrorMessange.cshtml", error4);
                        }
                        else
                        {
                            List<Hostel> hostels = new List<Hostel>();
                            string str4 = "";
                            str4 = string.Concat("HostelCard?$filter=Gender eq '", studentGender, "' and Not_Available eq false and Total_Vacant gt 0&$format=json");
                            using (StreamReader streamReader2 = new StreamReader(Credentials.GetOdataData(str4).GetResponseStream()))
                            {
                                foreach (JObject item1 in (IEnumerable<JToken>)JObject.Parse(streamReader2.ReadToEnd())["value"])
                                {
                                    Hostel hostel = new Hostel()
                                    {
                                        AssetNo = (string)item1["Asset_No"],
                                        Description = (string)item1["Discription"]
                                    };
                                    hostels.Add(hostel);
                                }
                            }
                            action = base.View(hostels);
                        }
                    }
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error5 = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                action = base.View("~/Views/Common/ErrorMessage.cshtml", error5);
            }
            return action;
        }

        public ActionResult HostelRoomList(string HostelCode, string HostelName)
        {
            return base.View();
        }

        public ActionResult MealBooking()
        {
            ActionResult action;
            try
            {
                string str = base.Session["Username"].ToString();
                if (base.Session["Username"] != null)
                {
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    string str1 = base.Session["CurrentSem"].ToString();
                    string[] strArrays = CommonClass.CurrentCourseRegistration(str, str1, "0");
                    if (strArrays[0] == "")
                    {
                        Error error = new Error()
                        {
                            Message = "You have not been registered in the current semester. Register for Units first"
                        };
                        action = base.View("~/Views/Shared/ErrorMessange.cshtml", error);
                    }
                    else if (Convert.ToInt32(strArrays[3]) >= 1)
                    {
                        action = base.View();
                    }
                    else
                    {
                        Error error1 = new Error()
                        {
                            Message = "You need to register for units before booking for Meals"
                        };
                        action = base.View("~/Views/Shared/ErrorMessange.cshtml", error1);
                    }
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                Error error2 = new Error()
                {
                    Message = exception.Message.Replace("'", "")
                };
                action = base.View("~/Views/Common/ErrorMessage.cshtml", error2);
            }
            return action;
        }

        public PartialViewResult MealBookingForm()
        {
            PartialViewResult partialViewResult;
            try
            {
                string str = base.Session["Username"].ToString();
                MealBooking mealBooking = new MealBooking();
                try
                {
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    string str1 = base.Session["CurrentSem"].ToString();
                    string str2 = string.Concat(new string[] { "CourseReg?$filter=StudentNo eq '", str, "' and Semester eq '", str1, "' and Meals_Booked eq true&$format=json" });
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str2).GetResponseStream()))
                    {
                        if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() <= 0)
                        {
                            using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData("ChargeList?$select=Amount&$filter=Code eq 'CAFETERIA'&$format=json").GetResponseStream()))
                            {
                                JObject jObjects = JObject.Parse(streamReader1.ReadToEnd());
                                if (jObjects["value"].Count<JToken>() > 0)
                                {
                                    foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                                    {
                                        decimal num = Math.Round((decimal)item["Amount"], 2);
                                        mealBooking.Message = string.Concat("Total Meal Charge = Ksh. ", num.ToString("#,##0.00"));
                                        mealBooking.BookedMeals = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            mealBooking.Message = "You have already booked for meals";
                            mealBooking.BookedMeals = true;
                        }
                    }
                }
                catch (Exception exception)
                {
                    mealBooking.Message = exception.Message;
                    mealBooking.BookedMeals = false;
                }
                partialViewResult = this.PartialView("~/Views/Welfare/Partial View/MealBookingForm.cshtml", mealBooking);
            }
            catch (Exception exception2)
            {
                Exception exception1 = exception2;
                Error error = new Error()
                {
                    Message = exception1.Message.Replace("'", "")
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        private bool MoveFile(string FileName)
        {
            bool flag = false;
            try
            {
                string str = string.Concat(Credentials.fileSourcePath, FileName);
                string str1 = base.Server.MapPath(string.Concat("~/Downloads/", FileName));
                //if (File.Exists(str1))
                //{
                //    File.Delete(str1);
                //    File.Move(str, str1);
                //}
                //if (!File.Exists(str1))
                //{
                //    File.Move(str, str1);
                //}
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public PartialViewResult NewSponsorshipApplication()
        {
            PartialViewResult partialViewResult;
            try
            {
                partialViewResult = this.PartialView("~/Views/Welfare/Partial View/NewSponsorshipApplication.cshtml", new SponsorshipApplication());
            }
            catch (Exception exception)
            {
                Error error = new Error()
                {
                    Message = exception.Message
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        public JsonResult PrintHostelClearanceForm()
        {
            JsonResult jsonResult;
            bool flag = false;
            try
            {
                string str = "";
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    string str2 = str1.Replace("/", "");
                    str2 = string.Concat("HOSTEL CLEARANCE-", str2, ".pdf");
                    if (!(new FileInfo(base.Server.MapPath(string.Concat("~/Downloads/", str2)))).Exists)
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                    else
                    {
                        flag = true;
                        str = string.Concat("/Downloads/", str2);
                    }
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = flag }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public JsonResult PrintHostelInvoice()
        {
            JsonResult jsonResult;
            bool flag = false;
            try
            {
                string str = "";
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    string str2 = str1.Replace("/", "");
                    if (base.Session["CurrentSem"] == null)
                    {
                        base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                    }
                    string str3 = base.Session["CurrentSem"].ToString();
                    string str4 = string.Concat(new string[] { "StudentHostelRooms?$filter=Student eq '", str1, "' and Semester eq '", str3, "' and Cleared eq false&$format=json" });
                    using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str4).GetResponseStream()))
                    {
                        foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                        {
                            str2 = string.Concat("HOSTEL INV-", str2, ".pdf");
                        }
                    }
                    this.MoveFile(str2);
                    if (!(new FileInfo(base.Server.MapPath(string.Concat("~/Downloads/", str2)))).Exists)
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                    else
                    {
                        flag = true;
                        str = string.Concat("/Downloads/", str2);
                    }
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = flag }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public JsonResult PrintNonResidentialForm()
        {
            JsonResult jsonResult;
            bool flag = false;
            try
            {
                string str = "";
                if (base.Session["Username"] != null)
                {
                    string str1 = base.Session["Username"].ToString();
                    string str2 = str1.Replace("/", "");
                    str2 = string.Concat("RESIDENTIALFORM-", str2, ".pdf");
                    if (!(new FileInfo(base.Server.MapPath(string.Concat("~/Downloads/", str2)))).Exists)
                    {
                        flag = false;
                        str = "File Not Found";
                    }
                    else
                    {
                        flag = true;
                        str = string.Concat("/Downloads/", str2);
                    }
                }
                else
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                jsonResult = base.Json(new { message = str, success = flag }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = flag }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public PartialViewResult RoomSpacesList(string HostelCode, string RoomCode, string Cost)
        {
            PartialViewResult partialViewResult;
            try
            {
                List<RoomSpaces> roomSpaces = new List<RoomSpaces>();
                string str = string.Concat(new string[] { "RoomSpaces?$select=BedSpaces,Status&$filter=HostelCode eq '", HostelCode, "' and RoomCode eq '", RoomCode, "' and Status eq 'Vaccant' and RoomNotAvaillable eq false&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        RoomSpaces roomSpace = new RoomSpaces()
                        {
                            HostelCode = HostelCode,
                            RoomCode = RoomCode,
                            SpaceCode = (string)item["BedSpaces"],
                            Cost = Cost,
                            Status = (string)item["Status"]
                        };
                        roomSpaces.Add(roomSpace);
                    }
                }
                partialViewResult = this.PartialView("~/Views/Welfare/Partial View/RoomSpaces.cshtml", roomSpaces);
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

        [HttpPost]
        public JsonResult SaveSelectedSpace(RoomSpaces spaceDetails)
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                base.Session["CurrentSem"].ToString();
                jsonResult = base.Json(new { message = "Space booked successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message.Replace("'", ""), success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveStepFiveSection()
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                jsonResult = base.Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveStepFourSection()
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                jsonResult = base.Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveStepSevenrSection()
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                jsonResult = base.Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveStepSixSection()
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                jsonResult = base.Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveStepThreeSection()
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                jsonResult = base.Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [HttpPost]
        public JsonResult SaveStepTwoSection()
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                jsonResult = base.Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public ActionResult ScolorshipApplcationForm()
        {
            return base.View();
        }

        public ActionResult SponsorshipApplication()
        {
            ActionResult action;
            try
            {
                if (base.Session["Username"] != null)
                {
                    action = base.View();
                }
                else
                {
                    action = base.RedirectToAction("Login", "Login");
                }
            }
            catch (Exception exception)
            {
                Error error = new Error()
                {
                    Message = exception.Message
                };
                action = base.View("~/Views/Common/ErrorMessage.cshtml", error);
            }
            return action;
        }

        public PartialViewResult SponsorshipApplicationDoc(string DocNo)
        {
            PartialViewResult partialViewResult;
            try
            {
                SponsorshipApplication sponsorshipApplication = new SponsorshipApplication();
                string str = string.Concat("SponsorshipApplication?$filter=Application_No eq '", DocNo, "'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        sponsorshipApplication.Student_No = (string)item["Student_No"];
                        sponsorshipApplication.Application_No = (string)item["Application_No"];
                        DateTime dateTime = (DateTime)item["Application_Date"];
                        sponsorshipApplication.Application_Date = dateTime.ToString("dd/MM/yyyy");
                        decimal num = Convert.ToDecimal((string)item["Applied_Amount"]);
                        sponsorshipApplication.Applied_Amount = num.ToString("#,##0.00");
                        num = Convert.ToDecimal((string)item["Approved_Amount"]);
                        sponsorshipApplication.Approved_Amount = num.ToString("#,##0.00");
                        sponsorshipApplication.Remarks = (string)item["Remarks"];
                        sponsorshipApplication.Status = (string)item["Status"];
                        sponsorshipApplication.ApprovalStatus = (string)item["Approval_Status"];
                        sponsorshipApplication.Recommendation = (string)item["Recommendation"];
                    }
                }
                partialViewResult = this.PartialView("~/Views/Welfare/Partial View/SponsorshipApplicationDoc.cshtml", sponsorshipApplication);
            }
            catch (Exception exception)
            {
                Error error = new Error()
                {
                    Message = exception.Message
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        public PartialViewResult SponsorshipApplicationList()
        {
            PartialViewResult partialViewResult;
            try
            {
                string str = base.Session["Username"].ToString();
                List<SponsorshipApplication> sponsorshipApplications = new List<SponsorshipApplication>();
                string str1 = string.Concat("SponsorshipApplication?$filter=Student_No eq '", str, "'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
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
                        sponsorshipApplication.Status = (string)item["Status"];
                        sponsorshipApplications.Add(sponsorshipApplication);
                    }
                }
                partialViewResult = this.PartialView("~/Views/Welfare/Partial View/SponsorshipApplicationList.cshtml",
                    from x in sponsorshipApplications
                    orderby x.Application_No descending
                    select x);
            }
            catch (Exception exception)
            {
                Error error = new Error()
                {
                    Message = exception.Message
                };
                partialViewResult = this.PartialView("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
            return partialViewResult;
        }

        public PartialViewResult StepView(string Step)
        {
            PartialViewResult partialViewResult = base.PartialView(string.Concat("~/Views/Welfare/Steps/", Step, ".cshtml"));
            return partialViewResult;
        }

        [HttpPost]
        public JsonResult SubmitMealBooking()
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                base.Session["CurrentSem"].ToString();
                jsonResult = base.Json(new { message = "Meals booked successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message.Replace("'", ""), success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public JsonResult SubmitSponsorshipApplication(string Amount, string Remarks)
        {
            JsonResult jsonResult;
            try
            {
                base.Session["Username"].ToString();
                jsonResult = base.Json(new { message = "Sponsorship Application Submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }
    }
}