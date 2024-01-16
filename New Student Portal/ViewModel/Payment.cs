using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace New_Student_Portal.ViewModel
{
    public class Payment
    {
        public string apiClientID { get; set; }
        public string secureHash { get; set; }
        public string billDesc { get; set; }
        public string billRefNumber { get; set; }
        public string currency { get; set; }
        public string serviceID { get; set; }
        public string clientMSISDN { get; set; }
        public string clientName { get; set; }
        public string clientIDNumber { get; set; }
        public string clientEmail { get; set; }
        public string callBackURLOnSuccess { get; set; }
        public string notificationURL { get; set; }
        public string amountExpected { get; set; }
    }
}