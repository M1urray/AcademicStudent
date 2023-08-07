using New_Student_Portal.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace New_Student_Portal.Models
{
    public class CommonClass
    {
        public static string[] StudentProgrammeDetails(string RegNo)
        {
            string[] dtMenu = new string[4];
            try
            {
                string page = "CustomerList?$filter=No eq '" + RegNo + "'&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    string Prog = "";
                    foreach (JObject config in details["value"])
                    {

                        Prog = (string)config["Student_Programme"];
                        if (Prog == "")
                        {
                            Prog = GetStudentRegisteredProgramme(RegNo);
                        }
                        dtMenu[0] = (string)config["Name"];
                        dtMenu[1] = Prog;
                        dtMenu[2] = (string)config["E_Mail"];
                        dtMenu[3] = (string)config["Global_Dimension_1_Code"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return dtMenu;
        }
        public static string[] CurrentCourseRegistration(string RegNo, string CurrSem)
        {
            string[] dtMenu = new string[5];
            try
            {
                string page = "CourseReg?$select = Programme,Stage,Class_Code,UnitsTaken,Booked_Hostel_No&$filter=StudentNo eq '" + RegNo + "' and Semester eq '" + CurrSem + "'&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        dtMenu[0] = (string)config["Programme"];
                        dtMenu[1] = (string)config["Stage"];
                        dtMenu[2] = (string)config["Class_Code"];
                        dtMenu[3] = (string)config["UnitsTaken"];
                        dtMenu[4] = (string)config["Booked_Hostel_No"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return dtMenu;
        }
        public static string CurrentSemester()
        {
            string CSem = "";
            try
            {
                string page = "SemesterList?$select=Code,Description&$filter=CurrentSemester eq true&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);


                    foreach (JObject config in details["value"])
                    {
                        CSem = (string)config["Code"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return CSem;
        }
        public static string RegistrationDeadline()
        {
            string RegDeadLine = "";
            try
            {
                string page = "SemesterList?$select=RegistrationDeadline&$filter=CurrentSemester eq true&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);


                    foreach (JObject config in details["value"])
                    {
                        RegDeadLine = ((DateTime)config["RegistrationDeadline"]).ToString("dd/MM/yyyy");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return RegDeadLine;
        }
        public static bool AllowOnlyY1S1(string RegNo)
        {
            bool s = false;
            try
            {
                string pageY1 = "GeneralSetups?$select=Allow_Only_Y1&$filter=Allow_Only_Y1 eq true&$format=json";

                HttpWebResponse httpResponseY1 = Credentials.GetOdataData(pageY1);
                using (var streamReaderY1 = new StreamReader(httpResponseY1.GetResponseStream()))
                {
                    var resultY1 = streamReaderY1.ReadToEnd();

                    var detailsY1 = JObject.Parse(resultY1);


                    if (detailsY1["value"].Count() > 0)
                    {
                        string page = "CustomerList?$select=Completed_Units&$filter=No eq '" + RegNo + "'&$format=json";

                        HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                        using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                        {
                            var result = streamReader.ReadToEnd();

                            var details = JObject.Parse(result);


                            foreach (JObject config in details["value"])
                            {
                                decimal ComUnits = (decimal)config["Completed_Units"];
                                if (ComUnits < 1)
                                {
                                    s = true;
                                }
                                else
                                {
                                    s = false;
                                }
                            }
                        }
                    }
                    else
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
        public static string ProfilePicture(string User)
        {
            string PicString = "";
            try
            {
                PicString = Credentials.ObjNav.GetProfilePictureStudent(User);
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return PicString;
        }
        public static string GetStaffGender(string User)
        {
            string gender = "";
            try
            {
                string StaffNo = User;
                string page = "EmployeeList?$filter=No eq '" + StaffNo + "'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        gender = (string)config["Gender"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return gender;
        }
        public static string GetHostelFromCourseReg(string RegNo, string Sem)
        {
            string Hostel = "";
            try
            {
                string page = "CourseReg?$select=Booked_Hostel_No&$filter=StudentNo eq '" + RegNo + "' and Semester eq '" + Sem + "'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        Hostel = (string)config["Booked_Hostel_No"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return Hostel;
        }
        public static string GetStudentGender(string User)
        {
            string gender = "";
            try
            {
                string page = "CustomerList?$select=Gender&$filter=No eq '" + User + "'&format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        gender = (string)config["Gender"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return gender;
        }
        public static string GetStudentIntake(string User)
        {
            string gender = "";
            try
            {
                string page = "CustomerList?$select=Gender&$filter=No eq '" + User + "'&format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        gender = (string)config["Gender"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return gender;
        }
        public static string GetStudentCampus(string User)
        {
            string Campus = "";
            try
            {
                string page = "CustomerList?$select=Global_Dimension_1_Code&$filter=No eq '" + User + "'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        Campus = (string)config["Global_Dimension_1_Code"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return Campus;
        }
        public static bool UnitHasPreliquisites(string RegNo, string Unit)
        {
            bool s = false;
            try
            {
                string page = "UnitPrerequisite?$select=Unit&$filter=Unit eq '" + Unit + "' and Prerequisite_Unit ne ''&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            string pageAudit = "StudentUnitsAudit?$select=Unit&$filter=StudentNo eq '" + RegNo + "' and Unit eq '" + (string)config["Unit"] + "' and Progress_Status eq 'Future'&$format=json";
                            HttpWebResponse httpResponseAudit = Credentials.GetOdataData(pageAudit);
                            using (var streamReaderAudit = new StreamReader(httpResponseAudit.GetResponseStream()))
                            {
                                var resultAudit = streamReaderAudit.ReadToEnd();

                                var detailsAudit = JObject.Parse(resultAudit);

                                if (detailsAudit["value"].Count() > 0)
                                {
                                    s = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        public static bool MoveFile(string FileName, string DestinationPath)
        {
            bool s = false;
            try
            {
                string sourcefile = Credentials.fileSourcePath + FileName;
                string destinationfile = DestinationPath;
                if (System.IO.File.Exists(destinationfile) == true)
                {
                    System.IO.File.Delete(destinationfile);
                    System.IO.File.Move(sourcefile, destinationfile);
                }
                if (System.IO.File.Exists(destinationfile) == false)
                {
                    System.IO.File.Move(sourcefile, destinationfile);
                }
                s = true;
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        public static string GetStudentName(string RegNo)
        {
            string fulName = "";
            try
            {
                string page = "CustomerList?$filter=No eq '" + RegNo + "'&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    foreach (JObject config in details["value"])
                    {
                        fulName = (string)config["Name"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return fulName;
        }
        public static bool SendEmailAlert(string body, string recepient, string subject)
        {
            Boolean x = false;

            var a = "";
            try
            {
                x = Credentials.ObjNav.SendEmail(ref recepient, subject, body);
                //string SMTPHost = "smtp.gmail.com";
                //string fromAddress = "testjooust@gmail.com";
                //string toAddress = recepient;
                //System.Net.Mail.MailMessage mail_ = new System.Net.Mail.MailMessage();
                //mail_.To.Add(toAddress);
                //mail_.Subject = subject;
                //mail_.From = new System.Net.Mail.MailAddress(fromAddress);
                //mail_.Body = body;
                //mail_.IsBodyHtml = true;

                //var smtp = new SmtpClient(SMTPHost, 587)
                //{
                //    Credentials = new NetworkCredential("testjooust@gmail.com", "123@Team"),
                //    EnableSsl = true
                //};
                //smtp.Send(mail_);
            }
            catch (Exception ex2)
            {
                ex2.Data.Clear();
            }
            return x;
        }
        public static bool ChangeStudentPassword(string User, string password)
        {
            bool changed = false;
            try
            {
                Credentials.ObjNav.UpdateStudentPassword(User, password, true);
                changed = true;
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return changed;
        }
        public static string RegistrationDeadline(string Sem)
        {
            string RegDeadLine = "";
            try
            {
                string page = "SemesterList?$select=RegistrationDeadline&$filter=Code eq '" + Sem + "' and CurrentSemester eq true&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);


                    foreach (JObject config in details["value"])
                    {
                        RegDeadLine = ((DateTime)config["RegistrationDeadline"]).ToString("dd/MM/yyyy");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return RegDeadLine;
        }
        public static string GetSemesterEndDate(string Sem)
        {
            string RegDeadLine = "";
            try
            {
                string page = "SemesterList?$select=To&$filter=Code eq '" + Sem + "' and CurrentSemester eq true&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);


                    foreach (JObject config in details["value"])
                    {
                        RegDeadLine = ((DateTime)config["To"]).ToString("dd/MM/yyyy");
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return RegDeadLine;
        }
        public static string GetEmployeeName(string StaffNo)
        {
            string Name = "";

            string page = "EmployeeList?$select=First_Name,Middle_Name,Last_Name&$filter=No eq '" + StaffNo + "'&$format=json";

            HttpWebResponse httpResponse = Credentials.GetOdataData(page);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                if (details["value"].Count() > 0)
                {
                    foreach (JObject config in details["value"])
                    {
                        Name = (string)config["First_Name"] + " " + (string)config["Middle_Name"] + " " + (string)config["Last_Name"];
                    }
                }
            }
            return Name;
        }
        public static string GetStudentRegisteredProgramme(string RegNo)
        {
            string s = "";
            try
            {
                string page = "CustomerList?$select=Current_Programme&$filter=No eq '" + RegNo + "'&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    foreach (JObject config in details["value"])
                    {
                        s = (string)config["Current_Programme"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        public static string GetProgrammeName(string Prog)
        {
            string s = "";
            try
            {
                string page = "ProgrammeList?$filter=Code eq '" + Prog + "'&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        s = (string)config["Description"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        public static bool StudentHasRaisedRequest(string StdNo, string RegType)
        {
            bool s = false;
            try
            {
                string page = "StudentRequisition?$filter=StudentNo eq '" + StdNo + "' and RequisitionType eq '" + RegType + "'&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    if (details["value"].Count() > 0)
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
        public static string[] RequisitionApprovalLevel(string DocNo)
        {
            string[] s = new String[2];
            try
            {
                string page = "ApprovalEntries?$select=Sequence_No&$top=1&$filter=Table_ID eq " + 70134894 + " and Document_No eq '" + DocNo + "' and Status eq 'Open'&format=json";
                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {

                            string pageApprovalCode = "ClearanceApprovalCodes?$select=ClearanceLevelCode&$filter=Sequence eq " + (int)config["Sequence_No"] + "&format=json";
                            HttpWebResponse httpResponseApprovalCode = Credentials.GetOdataData(pageApprovalCode);
                            using (var streamReaderApprovalCode = new StreamReader(httpResponseApprovalCode.GetResponseStream()))
                            {
                                var resultAppCode = streamReaderApprovalCode.ReadToEnd();

                                var detailsAppCode = JObject.Parse(resultAppCode);
                                if (detailsAppCode["value"].Count() > 0)
                                {
                                    foreach (JObject config1 in detailsAppCode["value"])
                                    {
                                        s[0] = (string)config1["ClearanceLevelCode"];
                                        s[1] = GetDocRejectionComment(DocNo, (int)config["Sequence_No"]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return s;
        }
        public static bool AllowGradClearanceApplication(string StdNo)
        {
            bool Allow = false;
            try
            {
                string page = "GraduatingStudentList?$select=No&$filter=No eq '" + StdNo + "'&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        Allow = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return Allow;
        }
        public static bool AllowGraduationApplication()
        {
            bool Allow = false;
            try
            {
                string page = "GeneralSetups?$select=Allow_Graduation_Application&$filter=Allow_Graduation_Application eq true&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        Allow = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return Allow;
        }
        public static bool AllowClearanceApplication()
        {
            bool Allow = false;
            try
            {
                string page = "GeneralSetups?$select=Allow_Student_Clearance_Req&$filter=Allow_Student_Clearance_Req eq true&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        Allow = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return Allow;
        }
        public static decimal GetStudentBalance(string RegNo)
        {
            decimal bal = 0;
            try
            {
                string page = "CustomerList?$select=Balance_LCY&$filter=No eq '" + RegNo + "'&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    foreach (JObject config in details["value"])
                    {
                        bal = Convert.ToDecimal((string)config["Balance_LCY"]);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return bal;
        }
        public static decimal TotalSemesterBilling(string RegNo, string Sem)
        {
            decimal bal = 0;
            try
            {
                string page = "CustomerList?$select=Balance_LCY&$filter=No eq '" + RegNo + "'&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    foreach (JObject config in details["value"])
                    {
                        bal = Convert.ToDecimal((string)config["Balance_LCY"]);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return bal;
        }
        public static string GetStudentResidence(string RegNo, string CurrSem)
        {
            string Residence = "";

            try
            {
                string page = "CourseReg?$select=Residency&$filter=StudentNo eq '" + RegNo + "' and Semester eq '" + CurrSem + "' and Reversed eq false&format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        Residence = (string)config["Residency"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return Residence;
        }
        public static bool AllowHostelBooking()
        {
            bool Allow = false;
            try
            {
                string page = "GeneralSetups?$select=Allow_Hostel_Booking&$filter=Allow_Hostel_Booking eq true&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        Allow = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return Allow;
        }
        public static string GetDocRejectionComment(string DocNo, int SeqNo)
        {
            string comment = "";
            try
            {
                string page = "ApprovalComments?$select=Comment&$filter=Document_No eq '" + DocNo + "' and Sequence_No eq " + SeqNo + "&format=json";
                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    var details = JObject.Parse(result);
                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            comment = (string)config["Comment"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return comment;
        }
        public static string StudentStatusDescription(string StatusCode)
        {
            string gender = "";
            try
            {
                string page = "StudentStatus?$select=Description&$filter=Code eq '" + StatusCode + "'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    foreach (JObject config in details["value"])
                    {
                        gender = (string)config["Description"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return gender;
        }
        public static bool UnitsNotInTimeTable(string RegNo, string sem)
        {
            bool NotInTimeTable = false;
            try
            {
                string Campus = GetStudentCampus(RegNo);
                string pageReg = "StudentUnits?$select=Unit,Unit_Description,Unit_Class_Code&$filter=Student_No eq '" + RegNo + "' and Semester eq '" + sem + "'&$format=json";

                HttpWebResponse httpResponse = Credentials.GetOdataData(pageReg);
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            string pageTimetable = "Timetable?$filter=Unit eq '" + (string)config["Unit"] + "' and Unit_Class eq '" + (string)config["Unit_Class_Code"] + "' and Semester eq '" + sem + "'&$format=json";

                            HttpWebResponse httpResponseTimeTable = Credentials.GetOdataData(pageTimetable);
                            using (var streamReaderTimeTable = new StreamReader(httpResponseTimeTable.GetResponseStream()))
                            {
                                var resultTimeTable = streamReaderTimeTable.ReadToEnd();

                                var detailsTimeTable = JObject.Parse(resultTimeTable);

                                if (detailsTimeTable["value"].Count() > 0)
                                {
                                    foreach (JObject config1 in detailsTimeTable["value"])
                                    {
                                        if (((string)config1["Campus_Code"] == Campus) || ((bool)config1["Multi_Campus"] == true))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            NotInTimeTable = true;
                                        }
                                    }
                                }
                                else
                                {
                                    NotInTimeTable = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return NotInTimeTable;
        }
        public static int GetDocumentCount()
        {
            int count = 0;
            string pageLine = "CompayInformation?$count=true&$filter=Category eq 'Student'&format=json";
            HttpWebResponse httpResponse = Credentials.GetOdataData(pageLine);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                if ((int)details["@odata.count"] > 0)
                {
                    count = (int)details["@odata.count"];
                }
            }
            return count;
        }
        public static string[] GetStudentDimensions(string RegNo)
        {
            string[] dtMenu = new string[4];
            try
            {
                string page = "CustomerList?$filter=No eq '" + RegNo + "'&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);
                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            if ((string)config["Current_Programme"] != "")
                            {
                                string[] s = GetProgDepSchool((string)config["Current_Programme"]);
                                dtMenu[0] = (string)config["Global_Dimension_1_Code"];
                                dtMenu[1] = s[0];/////department
                                dtMenu[2] = s[1];////school
                                dtMenu[3] = (string)config["Current_Programme"];////Programme
                            }
                            else
                            {
                                dtMenu[0] = "";
                                dtMenu[1] = "";
                                dtMenu[2] = "";
                                dtMenu[3] = "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return dtMenu;
        }
        public static string[] GetProgDepSchool(string Prog)
        {
            string[] dtMenu = new string[2];
            try
            {
                string page = "ProgrammeList?$filter=Code eq '" + Prog + "'&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);

                    if (details["value"].Count() > 0)
                    {
                        foreach (JObject config in details["value"])
                        {
                            dtMenu[0] = (string)config["DepartmentCode"];
                            dtMenu[1] = (string)config["SchoolCode"];
                        }
                    }
                    else
                    {
                        dtMenu[0] = "";
                        dtMenu[1] = "";
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return dtMenu;
        }
        public static int GetMaximumNoOfAttendance(string Sem)
        {
            int maxA = 0;
            string pageLine = "SemesterList?$filter=Code eq '" + Sem + "'&$format=json";
            HttpWebResponse httpResponse = Credentials.GetOdataData(pageLine);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                if (details["value"].Count() > 0)
                {
                    foreach (JObject config in details["value"])
                    {
                        maxA = (int)config["No_of_Weeks"] * (int)config["No_of_Lessons_Per_Week"];
                    }
                }
            }
            return maxA;
        }
        public static int GetTotalAttendance(string StdNo, string Sem, string Unit)
        {
            int count = 0;
            string pageLine = "ClassAttendanceLines?$count=true&$filter=StudentNo eq '" + StdNo + "' and Semester eq '" + Sem + "' and UnitCode eq '" + Unit + "' and Posted_Attendance eq true&$format=json";
            HttpWebResponse httpResponse = Credentials.GetOdataData(pageLine);
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                var details = JObject.Parse(result);
                if ((int)details["@odata.count"] > 0)
                {
                    count = (int)details["@odata.count"];
                }
            }
            return count;
        }
        public static decimal GetPercentageAttendance(string StdNo, string Sem,string Unit,decimal MaxA)
        {
            decimal Perc = 0;
            try
            {
                decimal TotalA = Convert.ToDecimal(GetTotalAttendance(StdNo, Sem,Unit));
                //decimal MaxA = Convert.ToDecimal(GetMaximumNoOfAttendance(Sem));
                Perc = (TotalA / MaxA) * 100;
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return Perc;
        }
    }
}