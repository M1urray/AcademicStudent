using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace New_Student_Portal.ViewModel
{
    public class StudentReqs
    {
        public string Code { get; set; }
        public string StdNo { get; set; }
        public string Prog { get; set; }
        public string StdProg { get; set; }
        public string Concentration { get; set; }
        public string Campus { get; set; }
        public string Date { get; set; }
        public string Requisition_Type { get; set; }
        public string Semester { get; set; }
        public string Status { get; set; }
        public string ApprovalLevel { get; set; }
        public string Remarks { get; set; }
        public string LinesCounter { get; set; }
        public string CollectionDate { get; set; }
        public bool CommentFound { get; set; }
        public string Comment { get; set; }
        public string NoOfCopies { get; set; }
        public bool SendFApproval { get; set; }
        public int ApprovalCount { get; set; }
    }
    public class StudentReqLines
    {
        public string AppNo { get; set; }
        public string Unit { get; set; }
        public string UnitName { get; set; }
        public string Reason { get; set; }
        public string EquivalentUnit { get; set; }
        public string Lecturer { get; set; }
        public string Section { get; set; }
    }
    public class RequisitionLines
    {
        public List<StudentReqLines> ListLines { get; set; }
        public string RegT { get; set; }
        public string Status { get; set; }
    }
    public class GradRequest
    {
        public string PersoanlEmail { get; set; }
        public string CrrProf { get; set; }
        public string Company { get; set; }
        public string CrrPhoneNo { get; set; }
        public bool Gown { get; set; }
        public string CollectionPoint { get; set; }
    }
    public class GradRequestInfomation
    {
        public string ReqNo { get; set; }
        public decimal Balance { get; set; }       
    }
}