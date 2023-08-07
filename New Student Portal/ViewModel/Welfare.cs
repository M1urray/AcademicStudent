using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.ViewModel
{
    public class BSGroupList
    {
        public string Code { get; set; }
        public List<SelectListItem> ListOfBSGroup { get; set; }
    }
    public class BSDetails
    {
        public string Bs { get; set; }
        public string GLeader { get; set; }
        public string GLeader_PNo { get; set; }
        public string Location { get; set; }
        public string Time { get; set; }
    }
    public class BsList
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
    public class NewBS
    {
        public string Type { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public List<BsList> BsList { get; set; }
    }
    public class DCFList
    {
        public string DCF { get; set; }
        public string Code { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Description { get; set; }
    }
    public class NewDCF
    {
        public List<SelectListItem> ListOfDCF { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public List<DCFList> EventList { get; set; }
    }
}