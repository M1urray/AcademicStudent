using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.ViewModel
{
    public class ExamResults
    {
        public string Unit { get; set; }
        public string UnitName { get; set; }
        public string Grade { get; set; }
        public string Credits { get; set; }
        public string GPA { get; set; }
        public string Marks { get; set; }
        public string GltyPoints { get; set; }
        public string TotalUnits { get; set; }
    }
    public class ExamSemesters
    {
        public string Semester { get; set; }
        public string AcademicYear { get; set; }
        public List<ExamResults> ListOfResults { get; set; }
    }

    public class StudentProgrammes
    {
        public string Programme { get; set; }
        public string AcademicYear { get; set; }

        public List<SelectListItem> ListOfProgrammes { get; set; }
    }

    public class StudentSemesters
    {
        public string Semester { get; set; }
        public string AcademicYear { get; set; }

        public List<SelectListItem> ListOfSemesters { get; set; }
    }

    public class Progs
    {
        public string Programme { get; set;}
    }
}