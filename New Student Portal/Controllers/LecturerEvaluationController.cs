using New_Student_Portal.Models;
using New_Student_Portal.NAVWS;
using New_Student_Portal.ViewModel;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.Controllers
{
    public class LecturerEvaluationController : Controller
    {
        public LecturerEvaluationController()
        {
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult CompleteCourseEvaluation(string Unit, string UnitName, string StaffNo, string sug1, string sug2, string sug3, string sug4)
        {
            JsonResult jsonResult;
            try
            {
                string str = base.Session["Username"].ToString();
                string str1 = "";
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                str1 = base.Session["CurrentSem"].ToString();
                if (base.Session["CurrentProgDetails"] == null)
                {
                    base.Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(str, str1, "0");
                }
                string[] item = (string[])base.Session["CurrentProgDetails"];
                Credentials.ObjNav.SaveGeneralEvaluationComments(str, Unit, str1, StaffNo, "", sug1, sug2, sug3, sug4);
                jsonResult = base.Json(new { message = string.Concat("Unit ", UnitName, " Evaluated successfully"), success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveELearningCourseEvaluation(Lecturer Lec, List<LecEvaluationQuiz> lecQuiz, List<ELearningQuiz> EQuiz)
        {
            JsonResult jsonResult;
            try
            {
                string str = base.Session["Username"].ToString();
                string str1 = "";
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                str1 = base.Session["CurrentSem"].ToString();
                if (base.Session["CurrentProgDetails"] == null)
                {
                    base.Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(str, str1, "0");
                }
                string[] item = (string[])base.Session["CurrentProgDetails"];
                foreach (LecEvaluationQuiz lecEvaluationQuiz in lecQuiz)
                {
                    lecEvaluationQuiz.QuizCategory.ToUpper().Trim();
                    lecEvaluationQuiz.Quiz.Trim();
                    Convert.ToInt32(lecEvaluationQuiz.Score.Trim());
                }
                foreach (ELearningQuiz eQuiz in EQuiz)
                {
                    Convert.ToInt32(eQuiz.Cat);
                }
                jsonResult = base.Json(new { message = "Evaluation Section submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public JsonResult SaveSectionData(Lecturer Lec, List<LecEvaluationQuiz> lecQuiz, bool SectionOne, bool LastSection)
        {
            JsonResult jsonResult;
            try
            {
                if (base.Session["Username"] == null)
                {
                    base.Response.Redirect(base.Url.Action("Login", "Login"));
                }
                string str = base.Session["Username"].ToString();
                string str1 = "";
                if (base.Session["CurrentSem"] == null)
                {
                    base.Session["CurrentSem"] = CommonClass.CurrentSemester(base.Session["CurrentProgram"].ToString());
                }
                str1 = base.Session["CurrentSem"].ToString();
                if (base.Session["CurrentProgDetails"] == null)
                {
                    base.Session["CurrentProgDetails"] = CommonClass.CurrentCourseRegistration(str, str1, "0");
                }
                string[] item = (string[])base.Session["CurrentProgDetails"];
                string str2 = "";
                if (item[1] != null)
                {
                    str2 = item[1];
                }
                if (SectionOne)
                {
                    Credentials.ObjNav.LecturerEvaluationHeader(str, Lec.Unit, str1, str2, Lec.LecNo, item[0], Lec.LecName, "");
                }
                foreach (LecEvaluationQuiz lecEvaluationQuiz in lecQuiz)
                {
                    string str3 = lecEvaluationQuiz.QuizCategory.ToUpper().Trim();
                    string str4 = lecEvaluationQuiz.Quiz.Trim();
                    decimal num = Convert.ToDecimal(lecEvaluationQuiz.Score.Trim());
                    Credentials.ObjNav.SaveLecturerEvaluationForm(str, Lec.Unit, str1, Lec.LecNo, "", str4, "", item[0], num, 0, str3, false, 0);
                }
                jsonResult = base.Json(new { message = "Evaluation Section submitted successfully", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                jsonResult = base.Json(new { message = exception.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }
    }
}