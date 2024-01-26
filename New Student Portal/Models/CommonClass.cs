using New_Student_Portal.NAVWS;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Configuration;

namespace New_Student_Portal.Models
{
    public class CommonClass
    {
        public CommonClass()
        {
        }

        public static bool AllowClearanceApplication()
        {
            bool flag = false;
            try
            {
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData("GeneralSetups?$select=Allow_Student_Clearance_Req&$filter=Allow_Student_Clearance_Req eq true&$format=json").GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() > 0)
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

        public static bool AllowGradClearanceApplication(string StdNo)
        {
            bool flag = false;
            try
            {
                string str = string.Concat("GraduatingStudentList?$select=No&$filter=No eq '", StdNo, "' and Allow_Online_Clearance eq true&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() > 0)
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

        public static bool AllowGraduationApplication()
        {
            bool flag = false;
            try
            {
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData("GeneralSetups?$select=Allow_Graduation_Application&$filter=Allow_Graduation_Application eq true&$format=json").GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() > 0)
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

        public static bool AllowHostelBooking()
        {
            bool flag = false;
            try
            {
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData("GeneralSetups?$select=Allow_Hostel_Booking&$filter=Allow_Hostel_Booking eq true&$format=json").GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() > 0)
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

        public static bool AllowOnlyY1S1(string RegNo)
        {
            bool flag = false;
            try
            {
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData("GeneralSetups?$select=Allow_Only_Y1&$filter=Allow_Only_Y1 eq true&$format=json").GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() <= 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        string str = string.Concat("CustomerList?$select=Completed_Units&$filter=No eq '", RegNo, "'&$format=json");
                        using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                        {
                            foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader1.ReadToEnd())["value"])
                            {
                                flag = ((decimal)item["Completed_Units"] >= decimal.One ? false : true);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public static bool ChangeStudentPassword(string User, string password)
        {
            bool flag = false;
            try
            {
                flag = true;
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public static string[] CurrentCourseRegistration(string RegNo, string CurrSem, string RegF)
        {
            string[] item = new string[6];
            try
            {
                string studentRegisteredProgramme = CommonClass.GetStudentRegisteredProgramme(RegNo);
                string str = "";
                if (RegF == "0")
                {
                    str = string.Concat(new string[] { "CourseReg?$select = Programme,Stage,Class_Code,UnitsTaken,Booked_Hostel_No,StudentType,SettlementType&$filter=StudentNo eq '", RegNo, "' and Semester eq '", CurrSem, "' and Programme eq '", studentRegisteredProgramme, "' and (Registerfor eq 'Stage' or Registerfor eq 'Unit/Subject')&$format=json" });
                }
                if (RegF == "2")
                {
                    str = string.Concat(new string[] { "CourseReg?$select = Programme,Stage,Class_Code,UnitsTaken,Booked_Hostel_No,StudentType,SettlementType&$filter=StudentNo eq '", RegNo, "' and Semester eq '", CurrSem, "' and Programme eq '", studentRegisteredProgramme, "' and Registerfor eq '", RegF, "'&$format=json" });
                }
                if (RegF == "3")
                {
                    str = string.Concat(new string[] { "CourseReg?$select = Programme,Stage,Class_Code,UnitsTaken,Booked_Hostel_No,StudentType,SettlementType&$filter=StudentNo eq '", RegNo, "' and Semester eq '", CurrSem, "' and Programme eq '", studentRegisteredProgramme, "' and Registerfor eq '", RegF, "'&$format=json" });
                }
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item[0] = (string)jObjects["Programme"];
                        item[1] = (string)jObjects["Stage"];
                        item[2] = (string)jObjects["Class_Code"];
                        item[3] = (string)jObjects["UnitsTaken"];
                        item[4] = (string)jObjects["Booked_Hostel_No"];
                        item[5] = (string)jObjects["StudentType"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string CurrentSemester(string Prog)
        {
            string item = "";
            try
            {
                string str = string.Concat("ProgrammeSemesters?$select=Semester,Desc&$filter=Current eq true and Programme_Code eq '", Prog, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Semester"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string GetClearanceApprovalStatus(string DocNo, int Sequence)
        {
            string item = "Open";
            try
            {
                string str = string.Concat(new string[] { "StudentReqApprovalList?$select=Status&$filter=Document_No eq '", DocNo, "' and Sequence_No eq ", Sequence.ToString(), "&format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Status"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string GetDocRejectionComment(string DocNo, int SeqNo)
        {
            string item = "";
            try
            {
                string str = string.Concat(new string[] { "ApprovalComments?$select=Comment&$filter=Document_No eq '", DocNo, "' and Sequence_No eq ", SeqNo.ToString(), "&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item1 in (IEnumerable<JToken>)jObjects["value"])
                        {
                            item = (string)item1["Comment"];
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static int GetDocumentCount()
        {
            int item = 0;
            using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData("CompayInformation?$count=true&$filter=Category eq 'Student'&format=json").GetResponseStream()))
            {
                JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                if ((int)jObjects["@odata.count"] > 0)
                {
                    item = (int)jObjects["@odata.count"];
                }
            }
            return item;
        }

        public static string GetEmployeeName(string StaffNo)
        {
            string str = "";
            string str1 = string.Concat("EmployeeList?$select=First_Name,Middle_Name,Last_Name&$filter=No eq '", StaffNo, "'&$format=json");
            using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
            {
                JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                if (jObjects["value"].Count<JToken>() > 0)
                {
                    foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                    {
                        str = string.Concat(new string[] { (string)item["First_Name"], " ", (string)item["Middle_Name"], " ", (string)item["Last_Name"] });
                    }
                }
            }
            return str;
        }

        public static string GetHostelFromCourseReg(string RegNo, string Sem)
        {
            string str = "";
            try
            {
                string str1 = string.Concat(new string[] { "CourseReg?$select=Student_Residence&$filter=StudentNo eq '", RegNo, "' and Semester eq '", Sem, "'&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        str = ((string)item["Student_Residence"]).Trim();
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return str;
        }

        public static int GetMaximumNoOfAttendance(string Sem, string Unit, string Campus, string Section)
        {
            int item = 0;
            string str = string.Concat(new string[] { "Timetable?$filter=Semester eq '", Sem, "' and Unit eq '", Unit, "' and Campus_Code eq '", Campus, "' and Unit_Class eq '", Section, "'&$format=json" });
            using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
            {
                JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                if (jObjects["value"].Count<JToken>() > 0)
                {
                    foreach (JObject item1 in (IEnumerable<JToken>)jObjects["value"])
                    {
                        string str1 = string.Concat(new string[] { "WeeksList?$count=true&$filter=Semester eq '", Sem, "' and Day eq '", (string)item1["DayofWeek"], "' and Inactive eq false&$format=json" });
                        using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                        {
                            JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                            if ((int)jObjects1["@odata.count"] > 0)
                            {
                                item = (int)jObjects1["@odata.count"];
                            }
                        }
                    }
                }
            }
            return item;
        }

        public static decimal GetPercentageAttendance(string StdNo, string Sem, string Unit, decimal MaxA)
        {
            decimal maxA = new decimal();
            try
            {
                decimal num = Convert.ToDecimal(CommonClass.GetTotalAttendance(StdNo, Sem, Unit));
                maxA = (num / MaxA) * new decimal(100);
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return maxA;
        }

        public static string[] GetProgDepSchool(string Prog)
        {
            string[] item = new string[2];
            try
            {
                string str = string.Concat("ProgrammeList?$filter=Code eq '", Prog, "'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() <= 0)
                    {
                        item[0] = "";
                        item[1] = "";
                    }
                    else
                    {
                        foreach (JObject item1 in (IEnumerable<JToken>)jObjects["value"])
                        {
                            item[0] = (string)item1["DepartmentCode"];
                            item[1] = (string)item1["SchoolCode"];
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string GetProgrammeCategory(string Prog)
        {
            string item = "";
            try
            {
                string str = string.Concat("ProgrammeList?$select=Category&$filter=Code eq '", Prog, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Category"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string GetProgrammeName(string Prog)
        {
            string item = "";
            try
            {
                string str = string.Concat("ProgrammeList?$filter=Code eq '", Prog, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Description"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string GetSemesterEndDate(string Sem)
        {
            string str = "";
            try
            {
                string str1 = string.Concat("SemesterList?$select=To&$filter=Code eq '", Sem, "' and CurrentSemester eq true&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        str = ((DateTime)item["To"]).ToString("dd/MM/yyyy");
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return str;
        }

        public static string GetStaffGender(string User)
        {
            string item = "";
            try
            {
                string str = string.Concat("EmployeeList?$filter=No eq '", User, "'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Gender"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static decimal GetStudentBalance(string RegNo)
        {
            decimal num = new decimal();
            try
            {
                string str = string.Concat("CustomerList?$select=Balance_LCY&$filter=No eq '", RegNo, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        num = Convert.ToDecimal((string)item["Balance_LCY"]);
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return num;
        }

        public static string[] GetStudentCampus(string User)
        {
            string[] item = new string[2];
            try
            {
                string str = string.Concat("CustomerList?$select=Global_Dimension_3_Code,Mode_of_Study&$filter=No eq '", User, "'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item[0] = (string)jObjects["Global_Dimension_3_Code"];
                        item[1] = (string)jObjects["Mode_of_Study"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string[] GetStudentDimensions(string RegNo)
        {
            string[] item = new string[3];
            try
            {
                string str = string.Concat("CustomerList?$filter=No eq '", RegNo, "'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item1 in (IEnumerable<JToken>)jObjects["value"])
                        {
                            if ((string)item1["Current_Programme"] == "")
                            {
                                item[0] = "";
                                item[1] = "";
                                item[2] = "";
                            }
                            else
                            {
                                string[] progDepSchool = CommonClass.GetProgDepSchool((string)item1["Current_Programme"]);
                                item[0] = (string)item1["Global_Dimension_1_Code"];
                                item[1] = progDepSchool[0];
                                item[2] = progDepSchool[1];
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string GetStudentGender(string User)
        {
            string item = "";
            try
            {
                string str = string.Concat("CustomerList?$select=Gender&$filter=No eq '", User, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Gender"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string GetStudentIntake(string User)
        {
            string item = "";
            try
            {
                string str = string.Concat("CustomerList?$select=Gender&$filter=No eq '", User, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Gender"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string GetStudentName(string RegNo)
        {
            string item = "";
            try
            {
                string str = string.Concat("CustomerList?$filter=No eq '", RegNo, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Name"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string GetStudentRegisteredProgramme(string RegNo)
        {
            string item = "";
            try
            {
                string str = string.Concat("CustomerList?$select=Current_Programme&$filter=No eq '", RegNo, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Current_Programme"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string GetStudentResidence(string RegNo, string CurrSem)
        {
            string item = "";
            try
            {
                string str = string.Concat(new string[] { "CourseReg?$select=Residency&$filter=StudentNo eq '", RegNo, "' and Semester eq '", CurrSem, "' and Reversed eq false&format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Residency"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static int GetTotalAttendance(string StdNo, string Sem, string Unit)
        {
            int item = 0;
            string str = string.Concat(new string[] { "ClassAttendanceLines?$count=true&$filter=StudentNo eq '", StdNo, "' and Semester eq '", Sem, "' and UnitCode eq '", Unit, "' and Attendance eq 1 and Posted eq true&$format=json" });
            using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
            {
                JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                if ((int)jObjects["@odata.count"] > 0)
                {
                    item = (int)jObjects["@odata.count"];
                }
            }
            return item;
        }

        public static bool[] LecEvaluationAllowed(string Sem)
        {
            bool[] item = new bool[2];
            try
            {
                string str = string.Concat("SemesterList?$select=Allow_Exam_Card_Generation,Allow_Lecturer_Evaluation&$filter=Code eq '", Sem, "' and CurrentSemester eq true&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item[0] = (bool)jObjects["Allow_Exam_Card_Generation"];
                        item[1] = (bool)jObjects["Allow_Lecturer_Evaluation"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static bool MadeGraduationrequest(string RegNo)
        {
            bool flag = false;
            try
            {
                string str = string.Concat("GraduationRequest?$filter=StudentNo eq '", RegNo, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    if (JObject.Parse(streamReader.ReadToEnd())["value"].Count<JToken>() > 0)
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

        public static bool MoveFile(string FileName, string DestinationPath)
        {
            bool flag = false;
            try
            {
                string str = string.Concat(Credentials.fileSourcePath, FileName);
                string destinationPath = DestinationPath;
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                    File.Move(str, destinationPath);
                }
                if (!File.Exists(destinationPath))
                {
                    File.Move(str, destinationPath);
                }
                flag = true;
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public static string ProfilePicture(string User)
        {
            string profilePictureStudent = "";
            try
            {
                profilePictureStudent = Credentials.ObjNav.GetProfilePictureStudent(User);
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return profilePictureStudent;
        }

        public static string RegistrationDeadline(string Sem)
        {
            string str = "";
            try
            {
                string str1 = string.Concat("SemesterList?$select=RegistrationDeadline&$filter=Code eq '", Sem, "' and CurrentSemester eq true&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        str = ((DateTime)item["RegistrationDeadline"]).ToString("dd/MM/yyyy");
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return str;
        }

        public static string[] RequisitionApprovalLevel(string DocNo)
        {
            string[] item = new string[2];
            try
            {
                string str = string.Concat(new string[] { "ApprovalEntries?$select=Sequence_No&$top=1&$filter=Table_ID eq ", 70134894.ToString(), " and Document_No eq '", DocNo, "' and Status eq 'Open'&format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item1 in (IEnumerable<JToken>)jObjects["value"])
                        {
                            int num = (int)item1["Sequence_No"];
                            string str1 = string.Concat("ClearanceApprovalCodes?$select=ClearanceLevelCode&$filter=Sequence eq ", num.ToString(), "&format=json");
                            using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                            {
                                JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                                if (jObjects1["value"].Count<JToken>() > 0)
                                {
                                    foreach (JObject item2 in (IEnumerable<JToken>)jObjects1["value"])
                                    {
                                        item[0] = (string)item2["ClearanceLevelCode"];
                                        item[1] = CommonClass.GetDocRejectionComment(DocNo, (int)item1["Sequence_No"]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string ResetPassword(string username, string newpass)
        {
            string message = "";
            try
            {
                string str = "";
                if (!username.Contains("\\\\"))
                {
                    str = (!username.Contains("\\") ? username.Trim() : username.Replace("\\", "").Trim());
                }
                else
                {
                    str = username.Replace("\\\\", "").Trim();
                }
                string item = WebConfigurationManager.AppSettings["AD_USER"];
                string item1 = WebConfigurationManager.AppSettings["ADW_PWD"];
                using (PrincipalContext principalContext = new PrincipalContext(ContextType.Domain, "AMIU.Amref.int", item, item1))
                {
                    UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(principalContext, username);
                    if (userPrincipal != null)
                    {
                        userPrincipal.SetPassword(newpass);
                        userPrincipal.Save();
                        message = "CHANGED";
                    }
                }
            }
            catch (Exception exception)
            {
                message = exception.InnerException.Message;
            }
            return message;
        }

        public static bool SendEmailAlert(string body, string recepient, string subject)
        {
            bool flag = false;
            try
            {
                flag = Credentials.ObjNav.SendEmail(recepient, subject, body);
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public static IsClearanceRequest StudentHasRaisedRequest(string StdNo, string RegType)
        {
            IsClearanceRequest isClearanceRequest = new IsClearanceRequest();
            try
            {
                isClearanceRequest.ReqNo = "";
                isClearanceRequest.Requested = false;
                string str = string.Concat(new string[] { "StudentRequisitions?$filter=Student_No eq '", StdNo, "' and Requisition_Type eq '", RegType, "'&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            isClearanceRequest.ReqNo = (string)item["Code"];
                            isClearanceRequest.Requested = true;
                            if ((string)item["Status"] != "Approved")
                            {
                                isClearanceRequest.print = false;
                            }
                            else
                            {
                                isClearanceRequest.print = true;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return isClearanceRequest;
        }

        public static string[] StudentProgrammeDetails(string RegNo)
        {
            string[] item = new string[4];
            try
            {
                string str = string.Concat("CustomerList?$filter=No eq '", RegNo, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    string studentRegisteredProgramme = "";
                    foreach (JObject item1 in (IEnumerable<JToken>)jObjects["value"])
                    {
                        studentRegisteredProgramme = (string)item1["Student_Programme"];
                        if (studentRegisteredProgramme == "")
                        {
                            studentRegisteredProgramme = CommonClass.GetStudentRegisteredProgramme(RegNo);
                        }
                        item[0] = (string)item1["Name"];
                        item[1] = studentRegisteredProgramme;
                        item[2] = (string)item1["E_Mail"];
                        item[3] = (string)item1["Global_Dimension_1_Code"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static string StudentStatusDescription(string StatusCode)
        {
            string item = "";
            try
            {
                string str = string.Concat("StudentStatus?$select=Description&$filter=Code eq '", StatusCode, "'&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject jObjects in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        item = (string)jObjects["Description"];
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return item;
        }

        public static decimal TotalSemesterBilling(string RegNo, string Sem)
        {
            decimal num = new decimal();
            try
            {
                string str = string.Concat("CustomerList?$select=Balance_LCY&$filter=No eq '", RegNo, "'&format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    foreach (JObject item in (IEnumerable<JToken>)JObject.Parse(streamReader.ReadToEnd())["value"])
                    {
                        num = Convert.ToDecimal((string)item["Balance_LCY"]);
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return num;
        }

        public static bool UnitHasPreliquisites(string RegNo, string Unit)
        {
            bool flag = false;
            try
            {
                string str = string.Concat("UnitPrerequisite?$select=Prerequisite_Unit&$filter=Unit eq '", Unit, "' and Prerequisite_Unit ne ''&$format=json");
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            string str1 = string.Concat(new string[] { "StudentUnitsAudit?$select=Unit&$filter=StudentNo eq '", RegNo, "' and Unit eq '", (string)item["Prerequisite_Unit"], "' and Progress_Status eq 'Future'&$format=json" });
                            using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                            {
                                if (JObject.Parse(streamReader1.ReadToEnd())["value"].Count<JToken>() > 0)
                                {
                                    flag = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }

        public static bool UnitsNotInTimeTable(string RegNo, string sem)
        {
            bool flag = false;
            try
            {
                string studentCampus = CommonClass.GetStudentCampus(RegNo)[0];
                string str = string.Concat(new string[] { "StudentUnits?$select=Unit,Unit_Description,Unit_Class_Code&$filter=Student_No eq '", RegNo, "' and Semester eq '", sem, "'&$format=json" });
                using (StreamReader streamReader = new StreamReader(Credentials.GetOdataData(str).GetResponseStream()))
                {
                    JObject jObjects = JObject.Parse(streamReader.ReadToEnd());
                    if (jObjects["value"].Count<JToken>() > 0)
                    {
                        foreach (JObject item in (IEnumerable<JToken>)jObjects["value"])
                        {
                            string str1 = string.Concat(new string[] { "Timetable?$filter=Unit eq '", (string)item["Unit"], "' and Unit_Class eq '", (string)item["Unit_Class_Code"], "' and Semester eq '", sem, "'&$format=json" });
                            using (StreamReader streamReader1 = new StreamReader(Credentials.GetOdataData(str1).GetResponseStream()))
                            {
                                JObject jObjects1 = JObject.Parse(streamReader1.ReadToEnd());
                                if (jObjects1["value"].Count<JToken>() <= 0)
                                {
                                    flag = true;
                                }
                                else
                                {
                                    foreach (JObject item1 in (IEnumerable<JToken>)jObjects1["value"])
                                    {
                                        if (((string)item1["Campus_Code"] == studentCampus ? false : !(bool)item1["Multi_Campus"]))
                                        {
                                            flag = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                exception.Data.Clear();
            }
            return flag;
        }
    }
}