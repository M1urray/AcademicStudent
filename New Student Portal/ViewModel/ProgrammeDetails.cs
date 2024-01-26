using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace New_Student_Portal.ViewModel
{
    public class Programmes
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }
    public class ProgrammeList
    {
        public string Code { get; set; }
        public List<SelectListItem> ListOfProgrammes { get; set; }
    }
    public class ProgrammeOptionsList
    {
        public string Option { get; set; }
        public List<SelectListItem> ListOfProgOption { get; set; }
    }
    public class ProgrammeOptions
    {
        public string Code { get; set; }
        public string Desription { get; set; }
    }
    public class UnitSubject
    {
        public string Class { get; set; }
        public IEnumerable<CoreUnitSubject> ListOfCoreUnitsSubjects { get; set; }
        public IEnumerable<GeneralUnitSubject> ListOfgeneralEduSubjects { get; set; }
        public IEnumerable<ElectiveUnitSubject> ListOfElectiveUnitsSubjects { get; set; }
        public IEnumerable<FreeElectiveUnitSubject> ListOfFreeElectiveUnitsSubjects { get; set; }
        public List<SelectListItem> ListOfClasses { get; set; }
        public bool ShowClass { get; set; }
        public int MinimumCourses { get; set; }
        public int MaximumCourses { get; set; }
        public int SelectedCourses { get; set; }
        public string HostelCode { get; set; }
    }
    public class FreeElectiveUnits
    {
        public string Class { get; set; }
        public IEnumerable<FreeElectiveUnitSubject> ListOfFreeElectiveUnitsSubjects { get; set; }
        public bool ShowClass { get; set; }
        public int MaximumCourses { get; set; }
        public int SelectedCourses { get; set; }
    }
    public class CoreUnitSubject
    {
        public string Code { get; set; }
        public string Desription { get; set; }
        public string Day { get; set; }
        public string Period { get; set; }
        public string Class { get; set; }
        public string Lec { get; set; }
        public string CF { get; set; }
        public string Campus { get; set; }
        public string UnitType { get; set; }
        public string HasPrelqUnit { get; set; }
        public string ClassFull { get; set; }
        public string Room { get; set; }
    }
    public class GeneralUnitSubject
    {
        public string Code { get; set; }
        public string Desription { get; set; }
        public string Day { get; set; }
        public string Period { get; set; }
        public string Class { get; set; }
        public string Lec { get; set; }
        public string CF { get; set; }
        public string Campus { get; set; }
        public string UnitType { get; set; }
        public string HasPrelqUnit { get; set; }
        public string ClassFull { get; set; }

    }
    public class CourseClasses
    {
        public string Code { get; set; }
        public string Desription { get; set; }
    }
    public class ElectiveUnitSubject
    {
        public string Code { get; set; }
        public string Desription { get; set; }
        public string Day { get; set; }
        public string Period { get; set; }
        public string Class { get; set; }
        public string Lec { get; set; }
        public string CF { get; set; }
        public string Campus { get; set; }
        public string UnitType { get; set; }
        public string HasPrelqUnit { get; set; }
        public string ClassFull { get; set; }
    }
    public class FreeElectiveUnitSubject
    {
        public string Code { get; set; }
        public string Desription { get; set; }
        public string Day { get; set; }
        public string Period { get; set; }
        public string Class { get; set; }
        public string Lec { get; set; }
        public string CF { get; set; }
        public string Campus { get; set; }
        public string UnitType { get; set; }
        public string HasPrelqUnit { get; set; }
        public string ClassFull { get; set; }
    }
    public class UnitRegistration
    {
        public string UnitCode { get; set; }
        public string ClassCode { get; set; }
        public string Campus { get; set; }
        public string UnitType { get; set; }
        public string Day { get; set; }
        public string Period { get; set; }
    }
    public class UnitFilters
    {
        public string prog { get; set; }
        public string stage { get; set; }
        public string Unit { get; set; }
        public string UnitName { get; set; }
        public string sem { get; set; }
        public string RegFor { get; set; }
    }
}