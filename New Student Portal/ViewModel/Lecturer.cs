using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace New_Student_Portal.ViewModel
{
    public class Lecturer
    {
        public string LecNo { get; set; }
        public string LecName { get; set; }
        public string Unit { get; set; }
        public string UnitName { get; set; }
        public string Comments { get; set; }
    }
    public class LecEvaluationQuiz
    {
        public string QuizCategory { get; set; }
        public string Quiz { get; set; }
        public string Score { get; set; }
        public string Index { get; set; }
    }
    public class Eval_Form
    {
        public List<LecEvaluationQuiz>  Eval_Quiz { get; set; }
        public string Category { get; set; }
    }
    public class Evaluation_Form
    {
        public Lecturer LecDet { get; set; }
        public List<Eval_Form> Eval_Form_Quiz { get; set; }
        public string Category { get; set; }
    }
}