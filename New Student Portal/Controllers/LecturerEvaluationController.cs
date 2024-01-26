using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.Controllers
{
    public class LecturerEvaluationController : Controller
    {
        // GET: LecturerEvaluation
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveSectionData(Lecturer Lec, List<LecEvaluationQuiz> lecQuiz, bool SectionOne, bool LastSection)
        {
            try
            {
                if (Session["Username"] == null)
                {
                    Response.Redirect(Url.Action("Login", "Login"));
                }
                string RegNo = Session["Username"].ToString();
                string Sem = "";
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester(Session["CurrentProgram"].ToString());
                }
                Sem = Session["CurrentSem"].ToString();

                if (Session["CurrentProgDetails"] == null)
                {
                    Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                }
                string[] s = (string[])Session["CurrentProgDetails"];

                string Stage = "";
                if (s[1] != null)
                {
                    Stage = s[1];
                }
                if (SectionOne)
                {
                    //Credentials.ObjNav.LecturerEvaluationHeader(RegNo, Lec.Unit, Sem, Stage, Lec.LecNo,
                    //    s[0], Lec.LecName, "");
                }

                foreach (var c in lecQuiz)
                {
                    string quizC = c.QuizCategory.ToUpper().Trim();
                    string quiz = c.Quiz.Trim();
                    decimal score = Convert.ToDecimal(c.Score.Trim());
                    //Credentials.ObjNav.SaveLecturerEvaluationForm(RegNo, Lec.Unit, Sem, Lec.LecNo, "", quiz, "",
                    //    s[0], score, 0, quizC, false, 0);
                }
                return Json(new { message = "Evaluation Section submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveELearningCourseEvaluation(Lecturer Lec, List<LecEvaluationQuiz> lecQuiz, List<ELearningQuiz> EQuiz)
        {
            try
            {
                string RegNo = Session["Username"].ToString();
                string Sem = "";
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester(Session["CurrentProgram"].ToString());
                }
                Sem = Session["CurrentSem"].ToString();

                if (Session["CurrentProgDetails"] == null)
                {
                    Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                }
                string[] s = (string[])Session["CurrentProgDetails"];
                //Credentials.ObjNav.DeleteLecturerEvaluationOnlineLearning(RegNo, Lec.Unit, Sem, "", Lec.LecNo, "");

                foreach (var c in lecQuiz)
                {
                    string quizC = c.QuizCategory.ToUpper().Trim();
                    string quiz = c.Quiz.Trim();
                    int score = Convert.ToInt32(c.Score.Trim());
                    //Credentials.ObjNav.SaveLecturerEvaluationForm(RegNo, Lec.Unit, Sem, Lec.LecNo, "", quiz, "",
                    //    s[0], score, 0, quizC, true, score);
                }
                foreach (var c in EQuiz)
                {
                    int Cat = Convert.ToInt32(c.Cat);
                    //Credentials.ObjNav.LecturerEvaluationOnlineLearning(RegNo, Lec.Unit, Sem, "", Lec.LecNo, "", c.Quiz, c.Asnwer, Cat);
                }
                return Json(new { message = "Evaluation Section submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult CompleteCourseEvaluation(string Unit, string UnitName, string StaffNo, string sug1, string sug2, string sug3, string sug4)
        {
            try
            {
                string RegNo = Session["Username"].ToString();
                string Sem = "";
                if (Session["CurrentSem"] == null)
                {
                    Session["CurrentSem"] = CommonClass.CurrentSemester(Session["CurrentProgram"].ToString());
                }
                Sem = Session["CurrentSem"].ToString();

                if (Session["CurrentProgDetails"] == null)
                {
                    Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(RegNo, Sem);
                }
                string[] s = (string[])Session["CurrentProgDetails"];

                //Credentials.ObjNav.SaveGeneralEvaluationComments(RegNo, Unit, Sem, StaffNo, "", sug1, sug2, sug3, sug4);

                return Json(new { message = "Unit " + UnitName + " Evaluated successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { message = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}