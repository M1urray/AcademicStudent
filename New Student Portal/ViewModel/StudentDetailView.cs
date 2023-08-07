using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.ViewModel
{
    public class StudentDetailView
    {
        public string No { get; set; }
        public string Name { get; set; }
        public string ID_No { get; set; }
        public string Gender { get; set; }
        public string Date_Of_Birth { get; set; }
        public string Phone_No { get; set; }
        public string Address { get; set; }
        public string E_Mail { get; set; }
        public decimal Balance { get; set; }
        public decimal Debit_Amount { get; set; }
        public decimal Credit_Amount { get; set; }
        public string AttemptedUnits { get; set; }
        public string RegUnits { get; set; }
        public string  Campus { get; set; }
        public string  Stage { get; set; }
        public string  Position { get; set; }

        public string Prog { get; set; }
        public string ProgName { get; set; }
        public bool MadeRequest { get; set; }
        public string DocNo { get; set; }
        public string PersonalMail { get; set; }
        public string Profession { get; set; }
        public string Company { get; set; }
        public string CurrentPhoneNo { get; set; }
        public string Gown { get; set; }
        public string CollectionPoint { get; set; }
        public string ProfilePic { get; set; }
        public string AcademicStatus { get; set; }
        public string GPA { get; set; }
        public int NotfCount { get; set; }
        public List<ClassAttendance> ClassAttendance { get; set; }
        public ListOfInternalMemos ListInternalMemos { get; set; }
    }
    public class ClassAttendance
    {
        public string Unit { get; set; }
        public string Description { get; set; }
        public decimal PercAtte { get; set; }
    }
    public class InternalMemos
    {
        public string description { get; set; }
        public DateTime Date { get; set; }
    }
    public class ListOfInternalMemos
    {
        public List<InternalMemos> ListInternalMemos { get; set; }
        public List<DocumentAttachment> ListOfIntMemos { get; set; }
        public bool hasFiles { get; set; }
    }
    public class DocumentAttachment
    {
        public int TabelID { get; set; }
        public string No { get; set; }
        public string FileName { get; set; }
        public string FileExt { get; set; }
        public int ID { get; set; }
        public string LineNo { get; set; }
        public string DocType { get; set; }
        public string Remarks { get; set; }
        public string Date { get; set; }
    }
}