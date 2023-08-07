using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.ViewModel
{
    public class Grade_Compaint
    {
        public string Doc_No { get; set; }
        public string Unit { get; set; }
        public string Unit_Name { get; set; }
        public string Semester { get; set; }
        public string Date_Applied { get; set; }
        public string Current_Grade { get; set; }
        public string New_Grade { get; set; }
        public string Status { get; set; }
    }
    public class New_Requisition
    {
        public string Code { get; set; }
        public List<SelectListItem> ListOfSemester { get; set; }
        public List<SelectListItem> ListOfUnits { get; set; }
    }
}