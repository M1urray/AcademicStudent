using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.ViewModel
{
    public class StudentCourseDetails
    {
        public string StdNo { get; set; }
        public string Name { get; set; }
        public string CurrentSem { get; set; }
        public string Email { get; set; }
        public string Prog { get; set; }
        public string HostelCode { get; set; }
    }
    public class StudentCourseRegistration
    {
        public string Code { get; set; }
        public IEnumerable<CourseReg> ListOfCourseRegistration { get; set; }
        public IEnumerable<StudentUnits> ListOfStudentBasketUnits { get; set; }
        public List<SelectListItem> ListOfUnitCategory { get; set; }
        public string HostelCode { get; set; }
        public string Campus { get; set; }
    }
    public class CourseReg
    {
        public string RegID { get; set; }
        public string Programme { get; set; }
        public string ProgrammeName { get; set; }
        public string Stage { get; set; }
        public string Semester { get; set; }
        public string Register_for { get; set; }
        public string Settlement_Type { get; set; }
        public string Registration_Date { get; set; }
        public string UnitsTaken { get; set; }
        public string TotalBilled { get; set; }
        public string HostelCode { get; set; }
        public string YList { get; set; }
    }
    public class StudentUnits
    {
        public string RegID { get; set; }
        public string Programme { get; set; }
        public string Unit { get; set; }
        public string Unit_Name { get; set; }
        public string Stage { get; set; }
        public string Semester { get; set; }
        public string RegFor { get; set; }
        public string Section { get; set; }
        public string CF { get; set; }
    }
    public class GetRegisteredUnit
    {
        public string RegDeadlineDate { get; set; }
        public List<StudentUnits> ListOfRegUnit { get; set; }       
    }
    public class UnitRegistrationSummery
    {
        public List<StudentUnits> ListOfRegUnit { get; set; }
        public List<StudentUnits> ListOfAttemptedUnit { get; set; }
    }
    public class ProgrammeConcentration
    {
        public string Code { get; set; }
        public List<SelectListItem> ListOfConcentration { get; set; }
    }
}