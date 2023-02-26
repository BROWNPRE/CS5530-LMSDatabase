using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.Controllers
{
    public class CommonController : Controller
    {

        /*******Begin code to modify********/

        // TODO: Uncomment and change 'X' after you have scaffoled


        protected Team1LMSContext db;

        public CommonController()
        {
            db = new Team1LMSContext();
        }


        /*
         * WARNING: This is the quick and easy way to make the controller
         *          use a different LibraryContext - good enough for our purposes.
         *          The "right" way is through Dependency Injection via the constructor 
         *          (look this up if interested).
        */

        // TODO: Uncomment and change 'X' after you have scaffoled

        public void UseLMSContext(Team1LMSContext ctx)
        {
            db = ctx;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }




        /// <summary>
        /// Retreive a JSON array of all departments from the database.
        /// Each object in the array should have a field called "name" and "subject",
        /// where "name" is the department name and "subject" is the subject abbreviation.
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetDepartments()
        {
            var query =
                    from d in db.Departments
                    select new
                    {
                        name = d.Name,
                        subject = d.Subject
                    };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array representing the course catalog.
        /// Each object in the array should have the following fields:
        /// "subject": The subject abbreviation, (e.g. "CS")
        /// "dname": The department name, as in "Computer Science"
        /// "courses": An array of JSON objects representing the courses in the department.
        ///            Each field in this inner-array should have the following fields:
        ///            "number": The course number (e.g. 5530)
        ///            "cname": The course name (e.g. "Database Systems")
        /// </summary>
        /// <returns>The JSON array</returns>
        public IActionResult GetCatalog()
        {
            var query =
                  from d in db.Departments
                  select new
                  {
                      subject = d.Subject,
                      dname = d.Name,
                      courses = d.Courses.Select((k, i) => new { number = k.Num, cname = k.Name })
                  };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all class offerings of a specific course.
        /// Each object in the array should have the following fields:
        /// "season": the season part of the semester, such as "Fall"
        /// "year": the year part of the semester
        /// "location": the location of the class
        /// "start": the start time in format "hh:mm:ss"
        /// "end": the end time in format "hh:mm:ss"
        /// "fname": the first name of the professor
        /// "lname": the last name of the professor
        /// </summary>
        /// <param name="subject">The subject abbreviation, as in "CS"</param>
        /// <param name="number">The course number, as in 5530</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetClassOfferings(string subject, int number)
        {
            uint did = getDID(subject);

            var query =
                        from c in db.Classes
                        where c.Course.Num == number.ToString() && c.Course.DId == did
                        select new
                        {
                            season = c.SemesterSeason,
                            year = c.SemesterYear,
                            location = c.Location,
                            start = c.Start,
                            end = c.End,
                            fname = c.P.FName,
                            lname = c.P.LName
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <returns>The assignment contents</returns>
        public IActionResult GetAssignmentContents(string subject, int num, string season, int year, string category, string asgname)
        {
            uint did = getDID(subject);

            var query =
                      from cats in db.AssignmentCategories
                      join a in db.Assignments
                      on cats.CategoryId equals a.CategoryId
                      join c in db.Classes
                      on cats.ClassId equals c.ClassId
                      where c.Course.Num == num.ToString()
                      && c.Course.DId == did
                      && c.SemesterSeason == season
                      && c.SemesterYear == year
                      && cats.Name == category
                      && a.Name == asgname
                      select new
                      {
                          contents = a.Contents
                      };

            return Content(query.ToArray().Length == 0 ? "" : query.FirstOrDefault().contents);
        }


        /// <summary>
        /// This method does NOT return JSON. It returns plain text (containing html).
        /// Use "return Content(...)" to return plain text.
        /// Returns the contents of an assignment submission.
        /// Returns the empty string ("") if there is no submission.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment in the category</param>
        /// <param name="uid">The uid of the student who submitted it</param>
        /// <returns>The submission text</returns>
        public IActionResult GetSubmissionText(string subject, int num, string season, int year, string category, string asgname, string uid)
        {
            uint did = getDID(subject);

            var query =
                      from s in db.Submissions
                      join a in db.Assignments
                      on s.AssignmentId equals a.AssignmentId
                      join cats in db.AssignmentCategories
                      on a.CategoryId equals cats.CategoryId
                      join c in db.Classes
                      on cats.ClassId equals c.ClassId
                      where c.Course.Num == num.ToString()
                      && c.Course.DId == did
                      && c.SemesterSeason == season
                      && c.SemesterYear == year
                      && cats.Name == category
                      && a.Name == asgname
                      && s.UId == uid
                      select new
                      {
                          contents = s.Contents
                      };

            return Content(query.ToArray().Length == 0 ? "" : query.FirstOrDefault().contents);
        }


        /// <summary>
        /// Gets information about a user as a single JSON object.
        /// The object should have the following fields:
        /// "fname": the user's first name
        /// "lname": the user's last name
        /// "uid": the user's uid
        /// "department": (professors and students only) the name (such as "Computer Science") of the department for the user. 
        ///               If the user is a Professor, this is the department they work in.
        ///               If the user is a Student, this is the department they major in.    
        ///               If the user is an Administrator, this field is not present in the returned JSON
        /// </summary>
        /// <param name="uid">The ID of the user</param>
        /// <returns>
        /// The user JSON object 
        /// or an object containing {success: false} if the user doesn't exist
        /// </returns>
        public IActionResult GetUser(string uid)
        {
            try
            {
                var studentQuery =
                                from s in db.Students
                                where s.UId == uid
                                select new
                                {
                                    fname = s.FName,
                                    lname = s.LName,
                                    uid = s.UId,
                                    department = s.MajorNavigation.Name
                                };

                var profQuery =
                              from p in db.Professors
                              where p.UId == uid
                              select new
                              {
                                  fname = p.FName,
                                  lname = p.LName,
                                  uid = p.UId,
                                  department = p.WorksInNavigation.Name
                              };

                var adminQuery =
                              from a in db.Administrators
                              where a.UId == uid
                              select new
                              {
                                  fname = a.FName,
                                  lname = a.LName,
                                  uid = a.UId
                              };

                if (studentQuery != null) 
                    return Json(studentQuery.FirstOrDefault());

                if (profQuery != null) 
                    return Json(profQuery.FirstOrDefault());

                if (adminQuery != null) 
                    return Json(adminQuery.FirstOrDefault());

                return Json(new { success = false });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }


        /*******End code to modify********/

        private uint getDID(string abbrv)
        {
            var query = from d in db.Departments
                        where d.Subject == abbrv
                        select d;

            return query.First().DId;
        }
    }
}