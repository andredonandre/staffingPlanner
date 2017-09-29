﻿using System;
using System.Linq;
using System.Web.Mvc;
using StaffingPlanner.DAL;
using StaffingPlanner.ViewModels;
using StaffingPlanner.Models;
using System.Collections.Generic;

namespace StaffingPlanner.Controllers
{
	public class CourseController : Controller
	{
        //Methods handling returning of View
        #region View methods

        public ActionResult Courses()
        {
            var db = StaffingPlanContext.GetContext();
            var offerings = db.CourseOfferings.Where(c => c.Course != null).ToList();

            var courses = GenerateCourseViewModelList(offerings);

            return View(courses);
        }

        public ActionResult CourseDetails(Guid id)
        {
            var db = StaffingPlanContext.GetContext();
            var offering = db.CourseOfferings.Where(c => c.Id == id).ToList().First();
            var teachers = db.Workloads.Where(w => w.Course.Course.Code == offering.Course.Code).Select(x => x.Teacher).ToList();

            var vm = GenerateCourseDetailViewModel(offering, teachers);

            return View(vm);
        }

        #endregion

        #region Static methods

        //Functions to generate derived values for a course(Allocated hours and remaining hours)
        public static int GetAllocatedHours(CourseOffering offering)
        {
            var db = StaffingPlanContext.GetContext();
            return db.Workloads.Where(w => w.Course.Id.Equals(offering.Id)).Select(w => w.Workload).Sum();
        }

        public static int GetRemainingHours(CourseOffering offering)
        {
            return (offering.TotalHours - GetAllocatedHours(offering));
        }

        //Methods to generate view models
        private static DetailedCourseViewModel GenerateCourseDetailViewModel(CourseOffering offering, List<Teacher> teachers)
        {
            var teacherList = TeacherController.GenerateTeacherViewModelList(teachers);
            var vm = new DetailedCourseViewModel
            {
                Code = offering.Course.Code,
                Name = offering.Course.Name,
                TermYear = offering.TermYear,
                Credits = offering.Credits,
                CourseResponsible = offering.CourseResponsible,
                HST = offering.HST,
                NumStudents = offering.NumStudents,
                TotalHours = offering.TotalHours,
                AllocatedHours = GetAllocatedHours(offering),
                RemainingHours = GetRemainingHours(offering),
                Teachers = teacherList
            };
            return vm;
        }

        private static List<SimpleCourseViewModel> GenerateCourseViewModelList(List<CourseOffering> offerings)
        {
            List<SimpleCourseViewModel> courses = new List<SimpleCourseViewModel>();
            foreach (var o in offerings)
            {
                var vm = new SimpleCourseViewModel
                {
                    Id = o.Id,
                    Code = o.Course.Code,
                    Name = o.Course.Name,
                    TermYear = o.TermYear,
                    Credits = o.Credits,
                    AllocatedHours = GetAllocatedHours(o),
                    RemainingHours = GetRemainingHours(o),
                };
                courses.Add(vm);
            }
            return courses;
        }

        #endregion

    }
}