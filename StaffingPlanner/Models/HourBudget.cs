﻿using System;
using System.Linq;
using StaffingPlanner.DAL;

namespace StaffingPlanner.Models
{
    public class HourBudget
    {
        public Teacher teacher { get; set; }
        public TermYear TermYear { get; set; }
        public int TotalTermHours { get; }
        public int RemainingHours { get; }
        public int TeachingHours { get; }
        public int ResearchHours { get; }
        public int AdminHours { get; }
        public int OtherHours { get; }

        public HourBudget(Teacher teacher, TermYear termYear)
        {
            this.teacher = teacher;
            this.TermYear = termYear;

            //Get the availability for the term
            var db = StaffingPlanContext.GetContext();
            decimal termAvailability = db.TeacherTermAvailability.Where(tta =>
                tta.Teacher.Id == teacher.Id &&
                tta.TermYear.Term == termYear.Term &&
                tta.TermYear.Year == termYear.Year)
                .AsEnumerable()
                .Select(tta => tta.Availability)
                .First();

            //Multiply the total hours based on availability
            TotalTermHours = (int)(totalHoursPerYear/2 * (termAvailability/100));

            //Get the shares (% for teaching, research etc) for this teachers academic title
            var result = db.AcademicProfiles.Where(ap => ap.Title == teacher.AcademicTitle)
                .Select(ap => new { TeachingShare = ap.TeachingShare, ResearchShare = ap.ResearchShare, AdminShare = ap.AdminShare, OtherShare = ap.OtherShare });
            var shares = result.First();

            TeachingHours = TotalTermHours * (int)shares.TeachingShare;
            ResearchHours = TotalTermHours * (int)shares.ResearchShare;
            AdminHours = TotalTermHours * (int)shares.AdminShare;
            OtherHours = TotalTermHours * (int)shares.OtherShare;
        }

        private int totalHoursPerYear
        {
            get
            {
                var year = int.Parse("19" + teacher.PersonalNumber.Substring(0, 2));
                var month = int.Parse(teacher.PersonalNumber.Substring(2, 2));
                var day = int.Parse(teacher.PersonalNumber.Substring(4, 2));
                var birthdate = new DateTime(year, month, day);
                var age = new DateTime().Subtract(birthdate);

                if (age.Days / 365 > 40)
                {
                    return 1700;
                }
                if (age.Days / 365 > 30 && age.Days / 365 <= 40)
                {
                    return 1735;
                }

                return 1756;
            }
        }

    }
}