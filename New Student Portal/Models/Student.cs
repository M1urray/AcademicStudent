using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace New_Student_Portal.Models
{
    public class Student : ApplicationUser
    {
        public string FName { get; set; }
        public string LName { get; set; }
        public string age { get; set; }
        public string country { get; set; }
    }
    public class CreateStudentModel
    {
        public string FName { get; set; }
        [Required]
        public string LName { get; set; }
        [Required]
        public string age { get; set; }
        [Required]
        public string country { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}