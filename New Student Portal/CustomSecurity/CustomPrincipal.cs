using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace New_Student_Portal.CustomSecurity
{
    public class CustomPrincipal : IPrincipal
    {

        public CustomPrincipal(string userName)
        {
            Identity = new GenericIdentity(userName);
        }
        public IIdentity Identity
        {
            get;
            private set;
        }

        public bool IsInRole(string role)
        {
            if (role == RoleName)
                return true;
            else
                return false;
        }
        public string UserID { get; set; }
        public string RoleName { get; set; }
        public bool Full_Access { get; set; }
        public string Email { get; set; }
    }
}