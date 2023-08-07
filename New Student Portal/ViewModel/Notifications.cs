using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace New_Student_Portal.ViewModel
{
    public class Notifications
    {
        public string Message { get; set; }
        public DateTime StartDate { get; set; }
    }
    public class Academic_Calender
    {
        public string Event { get; set; }
        public DateTime SD { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}