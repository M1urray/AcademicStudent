using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.ViewModel
{
    public class Hostel
    {
        public string AssetNo { get; set; }
        public string Description { get; set; }
        public string Gender { get; set; }
        public int VacantSpaces { get; set; }
    }
    public class Rooms
    {
        public string HostelCode { get; set; }
        public string RoomCode { get; set; }
        public int VacantSpaces { get; set; }
        public string Cost { get; set; }
        public string Status { get; set; }
    }
    public class RoomSpaces
    {
        public string HostelCode { get; set; }
        public string RoomCode { get; set; }
        public string SpaceCode { get; set; }
        public string Cost { get; set; }
        public string Status { get; set; }
        public string Student { get; set; }
        public string Sem { get; set; }
        public bool Billed { get; set; }
    }
    public class HostelList
    {
        public List<SelectListItem> ListOfHostels { get; set; }
    }
    public class MealBooking
    {
        public string Message { get; set; }
        public bool BookedMeals { get; set; }
    }
}