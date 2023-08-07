using Latest_Staff_Portal.Models;
using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Student.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            Session.Remove("Username");
            Session.RemoveAll();
            Authedication user = new Authedication();
            return View(user);
        }
        [HttpPost]
        public ActionResult LoginUser(Authedication userlogin)
        {
            string msg = "Either Username or password is wrong";
            bool success = false;
            string UserName = userlogin.UserName.ToUpper();
            string passWrd = userlogin.Password;
            try
            {
                string Redirect = "";
                string page = "CustomerList?$filter=No eq '" + UserName + "' and (Status eq 'Attachment' or Status eq 'Current')&$format=json";
                //string page = "CustomerList?$filter=No eq '" + UserName + "' and (Status eq 'Registration' or Status eq 'Current' or Status eq 'Alluminae')&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        Session["Username"] = UserName;
                        string User = (string)config["No"];
                        string Password = (string)config["Password"];
                        string changedPassword = (string)config["Changed_Password"];
                        if (User != "")
                        {
                            if (passWrd == Password)
                            {
                                if (changedPassword == "True")
                                {
                                    Redirect = "RESET";
                                }
                                else
                                {
                                    UserViewModel userModel = new UserViewModel();
                                    userModel.UserName = UserName;
                                    userModel.Email = (string)config["E_Mail"];
                                    if ((string)config["Status"] == "Alluminae")
                                    {
                                        userModel.RoleName = "ALLUMINAE";
                                        Redirect = "ALL";// "/Alumni/Dashboard";
                                    }
                                    else
                                    {
                                        userModel.RoleName = "STUD";
                                        Redirect = "STUD";// "/Dashboard/Dashboard";
                                    }

                                    string userData = string.Format("{0}|{1}|{2}|{3}", userModel.UserName, userModel.UserID, userModel.Email, userModel.RoleName);
                                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userModel.UserName, DateTime.Now,
                                        DateTime.Now.AddMinutes(1), false, userData);
                                    string encTicket = FormsAuthentication.Encrypt(ticket);

                                    HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                                    Response.Cookies.Add(cookie);
                                }
                                msg = Redirect;
                                success = true;
                            }
                            else
                            {
                                msg = "Either Username or password is wrong. If forgotten your password, then reset";
                                success = false;
                            }
                        }
                        else
                        {
                            msg = "Either Username or password is wrong";
                            success = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                success = false;
            }
            return Json(new
            {
                message = msg,
                success = success
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            Authedication user = new Authedication();
            return View(user);
        }
        [HttpPost]
        public ActionResult ForgotPassword(Authedication Reg)
        {
            string msg = "";
            bool val = false;
            try
            {
                if (Reg.UserName == null || Reg.UserName == "")
                {
                    val = true;
                    msg = "Enter Your Registration Number";
                    val = false;
                }
                else
                {
                    string stdNo = Reg.UserName.ToUpper();
                    string page = "CustomerList?$filter=No eq '" + stdNo + "' and (Status eq 'Registration' or Status eq 'Current' or Status eq 'Alluminae')&$format=json";

                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        if (details["value"].Count() > 0)
                        {
                            foreach (JObject config in details["value"])
                            {
                                if ((string)config["ID_No"] == "")
                                {
                                    msg = "Your National ID Number has not been set. Contact Admission office for assistance";
                                    val = false;
                                }
                                else {
                                    Credentials.ObjNav.UpdateStudentPassword(stdNo, (string)config["ID_No"], true);
                                    msg = "Your password has been reset successfully. Use your ID Number as password";
                                    val = true;
                                }
                            }
                        }
                        else
                        {
                            msg = "Wrong Registration Number!!";
                            val = false;
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                msg = ex.Message;
                val = false;
            }
            return Json(new { message = msg, success = val }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ForgotPassword1(Authedication Reg)
        {
            string msg = "";
            bool val = false;
            try
            {
                if (Reg.UserName == null || Reg.UserName == "")
                {
                    val = true;
                    msg = "Enter Your Registration Number";
                    val = false;
                }
                else
                {
                    string stdNo = Reg.UserName.ToUpper();
                    string page = "CustomerList?$filter=No eq '" + stdNo + "' and (Status eq 'Registration' or Status eq 'Current' or Status eq 'Alluminae')&$format=json";

                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        if (details["value"].Count() > 0)
                        {
                            foreach (JObject config in details["value"])
                            {
                                if (Reg.Mode == "1" && (string)config["Phone_No"] == "")
                                {
                                    msg = "Your Phone Number has not been set. Contact Admission office for assistance";
                                    val = false;
                                }
                                else if (Reg.Mode == "2" && (string)config["E_Mail"] == "")
                                {
                                    msg = "Your email address has not been set. Contact Admission office for assistance";
                                    val = false;
                                }
                                else
                                {                                  

                                    if (!string.IsNullOrEmpty(Reg.UserName))
                                    {
                                        if (Reg.Mode == "1")
                                        {
                                            try
                                            {
                                                Random rnd = new Random();
                                                int value = rnd.Next(10000, 99999);
                                                Credentials.ObjNav.UpdateStudentPassword(stdNo, value.ToString(), true);
                                                Credentials.ObjNav.SMSSendResetPassord(Reg.UserName, value.ToString());
                                                msg = "An SMS has been send to your phone Number(" + (string)config["Phone_No"] + ") with a new password.";
                                                val = true;
                                            }
                                            catch (Exception ex)
                                            {
                                                msg = ex.Message;
                                                val = false;
                                            }
                                        }
                                        else
                                        {
                                            Random rnd = new Random();
                                            int value = rnd.Next(100000000, 999999999);
                                            string emailAddress = (string)config["E_Mail"];
                                            string ret = Credentials.ObjNav.StudentForgotPassword(stdNo, value.ToString());
                                            if (ret != "")
                                            {
                                                string url = ConfigurationManager.AppSettings["ROOTLINK"];
                                                var callbackUrl = url + "/Login/AccountResetPassword?user=" + stdNo + "&Token=" + value;
                                                var footer = "<hr/>Note that this is an auto-generated email. Kindly do not reply to it.<BR/> <BR/> Incase of any challenges, please contact Admission office for assistance." +
                                                    "<BR/>Contact Email : " + ConfigurationManager.AppSettings["CONTACT_EMAIL"] + " <BR/><BR/>Best Regards.<BR/><BR/>";
                                                var body = "Hi " + ret;
                                                body += "<br />";
                                                body += "Kindly click <a href=\"" + callbackUrl + "\"><b>here</b></a> to reset your password.</br></br>" + footer;
                                                try
                                                {
                                                    CommonClass.SendEmailAlert(body, emailAddress, "PORTAL RESET PASSWORD LINK");
                                                    msg = "An email has been send to your email address(" + emailAddress + ") with a link to reset password.";
                                                    val = true;
                                                }
                                                catch (Exception ex)
                                                {
                                                    msg = ex.Message;
                                                    val = false;
                                                }
                                            }
                                            else
                                            {
                                                msg = "Problem encountered while reseting your account. Try later or contact DAYSTAR ICT for assistance";
                                                val = false;
                                            }
                                        }
                                    }

                                }

                            }
                        }
                        else
                        {
                            msg = "Wrong Registration Number!!";
                            val = false;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                val = false;
            }
            return Json(new { message = msg, success = val }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult AccountResetPassword()
        {
            try
            {
                //var urlString = Request.Url.AbsoluteUri.ToString();
                //Uri uri = new Uri(urlString);
                //string queryString = uri.Query;
                //string user = System.Web.HttpUtility.ParseQueryString(queryString).Get("user");
                //string Token = System.Web.HttpUtility.ParseQueryString(queryString).Get("Token");
                string user = Session["Username"].ToString();
                string page = "CustomerList?$filter=No eq '" + user + "' &$format=json";//and Random_Value eq '" + Token + "' and Code_Used eq false&$format=json";
                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        ResetPasswordAccount newUser = new ResetPasswordAccount();
                        newUser.UserName = user;
                        newUser.Password = "";
                        newUser.ConfirmPassword = "";
                        return View(newUser);
                    }
                    else
                    {
                        return View("~/Views/Shared/Unauthorised.cshtml");
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                return View("~/Views/Shared/Unauthorised.cshtml");
            }
        }
        [HttpPost]
        public ActionResult ApplicantResetPassword(ResetPasswordAccount userReset)
        {
            string msg = "";
            bool val = false;
            try
            {
                string user = userReset.UserName.ToUpper();

                string page = "CustomerList?$filter=No eq '" + user + "' and (Status eq 'Registration' or Status eq 'Current' or Status eq 'Alluminae')&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            Credentials.ObjNav.UpdateStudentPassword(user, userReset.Password, false);

                            Session["Username"] = user;
                            UserViewModel userModel = new UserViewModel();

                            userModel.UserName = user;
                            userModel.Email = (string)config["E_Mail"];
                            if ((string)config["Status"] == "Alluminae")
                            {
                                userModel.RoleName = "ALLUMINAE";
                                msg = "ALL";
                            }
                            else
                            {
                                userModel.RoleName = "STUD";
                                msg = "STUD";
                            }
                            string userData = string.Format("{0}|{1}|{2}|{3}", userModel.UserName, userModel.UserID, userModel.Email, userModel.RoleName);
                            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userModel.UserName, DateTime.Now,
                                DateTime.Now.AddMinutes(1), false, userData);
                            string encTicket = FormsAuthentication.Encrypt(ticket);

                            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                            Response.Cookies.Add(cookie);

                            val = true;
                        }
                    }
                    else
                    {
                        msg = "Problem while reseting password. Try later";
                        val = false;
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                val = false;
            }
            return Json(new { message = msg, success = val }, JsonRequestBehavior.AllowGet);
        }
    }
}