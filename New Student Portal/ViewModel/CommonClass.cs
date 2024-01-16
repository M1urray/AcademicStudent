using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.ViewModel
{
    public class DimensionValues
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class CampusList
    {
        public List<SelectListItem> ListOfCampus { get; set; }
    }
    public class DropdownList
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class DropdownListValues
    {
        public string Code { get; set; }
        public string Value { get; set; }
        public List<SelectListItem> ListOfValues { get; set; }
    }
    public class ApprovalEntries
    {
        public string DocNo { get; set; }
        public string UserID { get; set; }
        public string DateSendForApproval { get; set; }
        public string DueDate { get; set; }
        public string Status { get; set; }
        public int Sequence { get; set; }
    }
    public class ApprovalComment
    {
        public string Comment { get; set; }
    }
    public class GroupLeader
    {
        public bool IsBSLeader { get; set; }
        public string BsGroup { get; set; }
        public bool IsDCFLeader { get; set; }
        public string DCFGroup { get; set; }
    }
}