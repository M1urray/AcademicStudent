using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.ViewModel
{
    public class ClubsSociety
    {
        public List<SelectListItem> ListOfClubsSocieties { get; set; }
    }
    public class MemberOf
    {
        //public IEnumerable<StudentMemberOf> ListOfMemberOf { get; set; }
    }
    public class ClubsSocietySportSubmit
    {
        public string MemberOf { get; set; }
        public string MemberType { get; set; }
        public string Remarks { get; set; }
    }
}