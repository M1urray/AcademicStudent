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
        public string Campus { get; set; }
        public decimal Balance { get; set; }
        public decimal Debit_Amount { get; set; }
        public decimal Credit_Amount { get; set; }
        public string AttemptedUnits { get; set; }
        public string RegUnits { get; set; }
        public string Prog { get; set; }
        public string ProgName { get; set; }
        public string OtherProg { get; set; }
        public string SecondConc { get; set; }
        public string Minor { get; set; }
        public bool MadeRequest { get; set; }
        public string DocNo { get; set; }
        public string DateRequested { get; set; }
        public string PersonalMail { get; set; }
        public string Profession { get; set; }
        public string Company { get; set; }
        public string CurrentPhoneNo { get; set; }
        public string Gown { get; set; }
        public string CollectionPoint { get; set; }
        public string ProfilePic { get; set; }
        public string AcademicStatus { get; set; }
        public string GPA { get; set; }
        public string Semester { get; set; }
        public string Status { get; set; }
        public int NotfCount { get; set; }
        public bool IsBsLeader { get; set; }
        public string BSGroup { get; set; }
        public bool IsDCFLeader { get; set; }
        public string DCFGroup { get; set; }
        public string ModeOfStudy { get; set; }
        public List<DisplinaryCases> DisplinaryCases { get; set; }
        public ListOfInternalMemos ListInternalMemos { get; set; }
        public Bs BstudyDetails { get; set; }
        public bool Qualify_For_Catering { get; set; }
        public string Cat_Token { get; set; }
        public string LeadershipOption { get; set; }
        public ProgrammeList Enrolled_Prog { get; set; }
    }
    public class InternalMemos
    {
        public string description { get; set; }
        public DateTime Date { get; set; }
    }
    public class ClassAttendance
    {
        public string Unit { get; set; }
        public string Description { get; set; }
        public decimal PercAtte { get; set; }
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
    public class DisplinaryCases
    {
        public string Remarks { get; set; }
        public string EndDate { get; set; }
    }
    public class Bs
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string LeaderName { get; set; }
        public string LeaderContact { get; set; }
        public string StdCount { get; set; }
        public string Status { get; set; }
    }
    public class BsGList
    {
        public List<BsMembers> ListOfBsList { get; set; }
        public bool Waiting { get; set; }
    }
    public class BsMembers
    {
        public string Type { get; set; }
        public string Student_No { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string Programme { get; set; }
    }
    public class EmpHist
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
    }
    public class QualHist
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Institution { get; set; }
        public string Award { get; set; }
    }
    public class ProgEnrol
    {
        public string Prog { get; set; }
        public string ProgName { get; set; }
    }
}