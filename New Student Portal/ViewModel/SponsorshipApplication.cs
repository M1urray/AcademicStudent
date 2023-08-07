using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace New_Student_Portal.ViewModel
{
    public class SponsorshipApplication
    {
        public string Student_No { get; set; }
        public string Application_No { get; set; }
        public string Application_Date { get; set; }
        public string Applied_Amount { get; set; }
        public string Remarks { get; set; }
        public string Status { get; set; }
        public string Approved_Amount { get; set; }
        public string ApprovalStatus { get; set; }
        public string Recommendation { get; set; }
    }
}