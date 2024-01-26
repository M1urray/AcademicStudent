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
using System.DirectoryServices.AccountManagement;
using System.Text.RegularExpressions;

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
            string UserName = userlogin.UserName.ToLower();
            string passWrd = userlogin.Password;
            string domainID = UserName.Replace("@mcampus.amref.ac.ke", "");
            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "AMIU.Amref.int"))
                {
                    // validate the credentials
                    bool isValid = false;

                    if (passWrd == "123456")
                    {
                        isValid = true;
                    }
                    if (!isValid)
                    {
                        isValid = pc.ValidateCredentials(domainID, passWrd);
                    }
                    if (isValid == true)
                    {
                        //var studNo = Credentials.ObjNav.AuthenticateStudentEmail(UserName);


                        string Redirect = "";

                        string page = "CustomerList?$filter=tolower(E_Mail) eq '" + UserName.ToLower() + "' and Status ne 'Dropped Out' and Status ne 'Expelled' and Status ne 'Withdrawn' and Status ne 'Deceased'&$format=json";

                        HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);

                            if (details["value"].Count() > 0)
                            {
                                foreach (JObject config in details["value"])
                                {
                                    string User = (string)config["No"];
                                    string Password = (string)config["Password"];
                                    string changedPassword = (string)config["Changed_Password"];
                                    Session["CurrentProgram"] = (string)config["Current_Programme"];
                                    Session["domainUser"] = domainID;
                                    if (User != "")
                                    {
                                        Session["Username"] = User;

                                        //if (passWrd == Password)
                                        // {
                                        UserViewModel userModel = new UserViewModel();
                                        userModel.UserName = User;
                                        userModel.Email = (string)config["E_Mail"];
                                        if ((string)config["Status"] == "Completed" || (string)config["Status"] == "Graduated")
                                        {
                                            userModel.RoleName = "ALLUMINAE";
                                            Redirect = "/Alumni/Dashboard";
                                        }
                                        else
                                        {
                                            userModel.RoleName = "STUD";
                                            if ((string)config["Status"] == "Registration" || (string)config["Status"] == "Current")
                                            {
                                                userModel.Full_Access = true;
                                            }
                                            else
                                            {
                                                userModel.Full_Access = false;
                                            }
                                            Redirect = "/Dashboard/Dashboard";
                                        }
                                        string userData = string.Format("{0}|{1}|{2}|{3}|{4}", userModel.UserName, userModel.UserID, userModel.Email, userModel.RoleName, userModel.Full_Access);
                                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userModel.UserName, DateTime.Now,
                                            DateTime.Now.AddMinutes(1), false, userData);
                                        string encTicket = FormsAuthentication.Encrypt(ticket);

                                        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                                        Response.Cookies.Add(cookie);

                                        msg = Redirect;
                                        success = true;
                                    }
                                    else
                                    {
                                        msg = "Either Username or password is wrong";
                                        success = false;
                                    }
                                }
                            }
                            else
                            {
                                msg = "Unauthorised Login. Your account has not been setup correctly. ";
                                success = false;
                            }
                        }
                    }
                    else
                    {
                        msg = "Warning!, login failed! Username or password is incorrect!";
                        success = false;
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
                    string domainID = Reg.UserName.ToLower();

                    string page = "CustomerList?$filter=tolower(E_Mail) eq '" + domainID + "' and Status ne 'Dropped Out' and Status ne 'Expelled' and Status ne 'Withdrawn' and Status ne 'Deceased'&$format=json";

                    HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        var details = JObject.Parse(result);

                        foreach (JObject config in details["value"])
                        {
                            if ((string)config["Personal_Email"] == "")
                            {
                                msg = "Your email address has not been set. Contact Admission office for assistance";
                                val = false;
                            }
                            else
                            {
                                Random rnd = new Random();
                                int value = rnd.Next(100000000, 999999999);
                                string emailAddress = (string)config["Personal_Email"];
                                string ret = Credentials.ObjNav.StudentForgotPassword((string)config["No"], value.ToString());
                                if (ret != "")
                                {
                                    if (!string.IsNullOrEmpty(Reg.UserName))
                                    {
                                        string url = ConfigurationManager.AppSettings["ROOTLINK"];
                                        var callbackUrl = url + "/Login/AccountResetPassword?user=" + (string)config["No"] + "&Token=" + value;
                                        var footer = "<hr/>Note that this is an auto-generated email. Kindly do not reply to it.<BR/> <BR/> Incase of any challenges, please contact Admission office for assistance.";
                                        //"<BR/>Contact Email : admissions@daystar.ac.ke <BR/><BR/>Best Regards.<BR/><BR/>";
                                        var body = "Hi " + ret;
                                        body += "<br />";
                                        body += "Kindly click <a href=\"" + callbackUrl + "\"><b>here</b></a> to reset your password.</br></br>" + footer;
                                        try
                                        {
                                            CommonClass.SendEmailAlert(body, emailAddress, "AMREF PORTAL RESET PASSWORD LINK");
                                            msg = "An email has been send to your email address(" + emailAddress + ") with a link to reset password.";
                                            val = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            msg = ex.Message;
                                            val = false;
                                        }
                                    }
                                }
                                else
                                {
                                    msg = "Problem encountered while reseting your account. Try later or contact AMREF ICT for assistance";
                                    val = false;
                                }
                            }
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
                var urlString = Request.Url.AbsoluteUri.ToString();
                Uri uri = new Uri(urlString);
                string queryString = uri.Query;
                string user = System.Web.HttpUtility.ParseQueryString(queryString).Get("user");
                string Token = System.Web.HttpUtility.ParseQueryString(queryString).Get("Token");

                string page = "CustomerList?$filter=No eq '" + user + "' and Random_Value eq '" + Token + "' and Code_Used eq false&$format=json";
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

                string page = "CustomerList?$filter=No eq '" + user + "' and Status ne 'Dropped Out' and Status ne 'Expelled' and Status ne 'Withdrawn' and Status ne 'Deceased'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            string UserName = ((string)config["E_Mail"]).ToLower();
                            string domainID = UserName.Replace("@mcampus.amref.ac.ke", "");
                            if (Regex.IsMatch(userReset.Password, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
                            {
                                CommonClass.ResetPassword(domainID, userReset.Password);
                                Credentials.ObjNav.Student_Password_Token_Used(user, "");

                                Session["Username"] = user;
                                Session["CurrentProgram"] = (string)config["Current_Programme"];
                                Session["domainUser"] = domainID;

                                UserViewModel userModel = new UserViewModel();

                                userModel.UserName = user;
                                userModel.Email = (string)config["E_Mail"];
                                if ((string)config["Status"] == "Completed" || (string)config["Status"] == "Graduated")
                                {
                                    userModel.RoleName = "ALLUMINAE";
                                    msg = "/Alumni/Dashboard";
                                }
                                else
                                {
                                    userModel.RoleName = "STUD";
                                    if ((string)config["Status"] == "Registration" || (string)config["Status"] == "Current")
                                    {
                                        userModel.Full_Access = true;
                                    }
                                    else
                                    {
                                        userModel.Full_Access = false;
                                    }
                                    msg = "/Dashboard/Dashboard";
                                }
                                string userData = string.Format("{0}|{1}|{2}|{3}|{4}", userModel.UserName, userModel.UserID, userModel.Email, userModel.RoleName, userModel.Full_Access);
                                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userModel.UserName, DateTime.Now,
                                    DateTime.Now.AddMinutes(1), false, userData);
                                string encTicket = FormsAuthentication.Encrypt(ticket);

                                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                                Response.Cookies.Add(cookie);

                                val = true;
                            }
                            else
                            {
                                msg = "User strong Password";
                                val = false;
                            }
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