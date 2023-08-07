using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace New_Student_Portal.ViewModel
{
    public class FeeStatementDetails
    {
        public DateTime DateOrder { get; set; }
        public string Posting_Date { get; set; }
        public string Document_No { get; set; }
        public string Description { get; set; }
        public string Debit_Amount { get; set; }
        public string Credit_Amount { get; set; }
        public string RunnningBal { get; set; }
    }
}