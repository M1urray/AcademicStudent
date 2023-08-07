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

    public class NoticeBoard
    {
        public string Description { get; set; }
        public string Campus { get; set; }
        public string DatePosted { get; set; }
    }

    public class ImportantDepartments
    {
        public string Description { get; set; }
        public string  Contacts { get; set; }
        public string Campus { get; set; }
        public string School { get; set; }
    }
}