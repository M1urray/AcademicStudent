using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.ViewModel
{
    public class HostelBookingView
    {
        public string Hostel { get; set; }
        public string Discription { get; set; }        
        public string RoomCost { get; set; }
        public List<SelectListItem> ListOfHostels { get; set; }
    }
    public class RoomList
    {
        public string RoomCode { get; set; }
        public List<SelectListItem> ListOfRooms { get; set; }
    }
    public class SpaceList
    {
        public string SpaceCode { get; set; }
        public List<SelectListItem> ListOfRoomSpaces { get; set; }
    }
    public class BookedSpaceDetails
    {
        public string Hostel { get; set; }
        public string Room { get; set; }
        public string Space { get; set; }
        public string Cost { get; set; }
    }
}