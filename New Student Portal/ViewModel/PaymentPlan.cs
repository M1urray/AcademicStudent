using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace New_Student_Portal.ViewModel
{
    public class PaymentPlan
    {
        public string StudentNo { get; set; }
        public string Semester { get; set; }
        public string Percentage { get; set; }
        public string ByDate { get; set; }
        public string InstallNo { get; set; }
        public string AmountDue { get; set; }
    }
    public class SemeterEndDate
    {
        public string SemEndDate { get; set; }
    }
}