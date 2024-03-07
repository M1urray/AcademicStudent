namespace New_Student_Portal.ViewModel
{
    public class StudentThesis
    {

        public string No { get; set; }
        public string StudentNo { get; set; }
        public string Unit { get; set; }
        public string UnitName { get; set; }
        public string ThesisDescription { get; set; }
        public string ThesisTitle { get; set; }
        public string Publication { get; set; }
        public string Publication2 { get; set; }
        public string Publication3 { get; set; }
        public string DateRequested { get; set; }
        public string Supervisor1 { get; set; }
        public string Supervisor2 { get; set; }
        public string Supervisor3 { get; set; }
    }

    public class StudentThesisStatus
    {
        public int ThesisStatus { get; set; }
    }
}