using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace New_Student_Portal.ViewModel
{
    public class TimeTableView
    {
        public string Unit { get; set; }
        public string Period { get; set; }
        public string Semester { get; set; }
        public string Day_of_Week { get; set; }
        public string Lecture_Room { get; set; }
        public string Lecturer { get; set; }
        public string Campus { get; set; }
        public string Section { get; set; }
        public string Registered { get; set; }
        public string CF { get; set; }
    }
    public class TimeTableSummeryView
    {
        public List<TimeTableView> RegisteredUnitsTimeT { get; set; }
        public List<TimeTableView> FutureUnitsTimeT { get; set; }
    }
}