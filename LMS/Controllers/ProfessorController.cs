using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Controllers
{
  [Authorize(Roles = "Professor")]
  public class ProfessorController : CommonController
  {
    public IActionResult Index()
    {
      return View();
    }

    public IActionResult Students(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult Class(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult Categories(string subject, string num, string season, string year)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      return View();
    }

    public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      return View();
    }

    public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      return View();
    }

    public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      return View();
    }

    public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
    {
      ViewData["subject"] = subject;
      ViewData["num"] = num;
      ViewData["season"] = season;
      ViewData["year"] = year;
      ViewData["cat"] = cat;
      ViewData["aname"] = aname;
      ViewData["uid"] = uid;
      return View();
    }

    /*******Begin code to modify********/

    /// <summary>
    /// Helper method that updates a student's letter grade.
    /// </summary>
    public void UpdateStudentGrade(string subject, int num, string season, int year, string uID)
    {
            var query =
                    from a in db.Assignments
                    join ac in db.AssignmentCategories
                    on a.CategoryId equals ac.CategoryId
                    join cl in db.Classes
                    on ac.ClassId equals cl.ClassId
                    join co in db.Courses
                    on cl.CourseId equals co.CourseId
                    join d in db.Departments
                    on co.DId equals d.DId
                    where d.Subject == subject &&
                          co.Num == num.ToString() &&
                          cl.SemesterSeason == season &&
                          cl.SemesterYear == (uint)year
                    select new
                    {
                        AsgnID = a.AssignmentId,
                        AsgnCatID = ac.CategoryId,
                        CatWeight = ac.Weight,
                        Points = a.Points
                    };

            var query2 =
                    from q1 in query
                    join s in db.Submissions
                    on new { AsgnID = q1.AsgnID, UID = uID } equals new
                    { AsgnID = s.AssignmentId, UID = s.UId }
                    into joined
                    from j in joined.DefaultIfEmpty()
                    select new
                    {
                        AsgnID = q1.AsgnID,
                        AsgnCatID = q1.AsgnCatID,
                        CatWeight = q1.CatWeight,
                        Points = q1.Points,
                        Score = j.Score,
                        UID = j.UId
                    };

            Dictionary<uint?, uint?> catWeight = new Dictionary<uint?, uint?>();
            Dictionary<uint?, uint?> catTotal = new Dictionary<uint?, uint?>();
            Dictionary<uint?, uint?> catEarned = new Dictionary<uint?, uint?>();
            List<uint?> cats = new List<uint?>();
            foreach (var res in query2)
            {
                if (catTotal.ContainsKey(res.AsgnCatID))
                {
                    catTotal[res.AsgnCatID] = catTotal[res.AsgnCatID] + res.Points;
                    catEarned[res.AsgnCatID] = catEarned[res.AsgnCatID] + (res.Score == null ? 0 : res.Score);
                }
                else
                {
                    cats.Add(res.AsgnCatID);
                    catTotal.Add(res.AsgnCatID, res.Points);
                    catWeight.Add(res.AsgnCatID, res.CatWeight);
                    catEarned.Add(res.AsgnCatID, (res.Score == null ? 0 : res.Score));
                }
            }

            uint? weightTotal = 0;
            float total = 0.0f;
            foreach (uint? cat in cats)
            {
                total += ((float)catEarned[cat] / (float)catTotal[cat]) * (float)catWeight[cat];
                weightTotal += catWeight[cat];
            }
            float final = (100.0f / (float)weightTotal) * total;
            string grade = "";
            if (final < 0.0001f)
                grade = "--";
            else if (final < 60f)
                grade = "E";
            else if (final < 63f)
                grade = "D-";
            else if (final < 67f)
                grade = "D";
            else if (final < 70f)
                grade = "D+";
            else if (final < 73f)
                grade = "C-";
            else if (final < 77f)
                grade = "C";
            else if (final < 80f)
                grade = "C+";
            else if (final < 83f)
                grade = "B-";
            else if (final < 87f)
                grade = "B";
            else if (final < 90f)
                grade = "B+";
            else if (final < 93f)
                grade = "A-";
            else
                grade = "A";

            var updateGradeQuery =
                from e in db.Enrolled
                join cl in db.Classes
                on e.ClassId equals cl.ClassId
                join co in db.Courses
                on cl.CourseId equals co.CourseId
                join d in db.Departments
                on co.DId equals d.DId
                where d.Subject == subject &&
                      co.Num == num.ToString() &&
                      cl.SemesterSeason == season &&
                      cl.SemesterYear == (uint)year &&
                      e.SId == uID
                select e;
            Enrolled enr = updateGradeQuery.First();
            enr.Grade = grade;
            db.SaveChanges();
        }


    /// <summary>
    /// Returns a JSON array of all the students in a class.
    /// Each object in the array should have the following fields:
    /// "fname" - first name
    /// "lname" - last name
    /// "uid" - user ID
    /// "dob" - date of birth
    /// "grade" - the student's grade in this class
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
    {
            var query =
                    from s in db.Students
                    join e in db.Enrolled
                    on s.UId equals e.SId
                    join cl in db.Classes
                    on e.ClassId equals cl.ClassId
                    join co in db.Courses
                    on cl.CourseId equals co.CourseId
                    join d in db.Departments
                    on co.DId equals d.DId
                    where d.Subject == subject &&
                          co.Num == num.ToString() &&
                          cl.SemesterSeason == season &&
                          cl.SemesterYear == (uint)year
                    select new
                    {
                        fname = s.FName,
                        lname = s.LName,
                        uid = s.UId,
                        dob = s.Dob,
                        grade = e.Grade == null ? "--" : e.Grade
                    };

            return Json(query.ToArray());
    }



    /// <summary>
    /// Returns a JSON array with all the assignments in an assignment category for a class.
    /// If the "category" parameter is null, return all assignments in the class.
    /// Each object in the array should have the following fields:
    /// "aname" - The assignment name
    /// "cname" - The assignment category name.
    /// "due" - The due DateTime
    /// "submissions" - The number of submissions to the assignment
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class, 
    /// or null to return assignments from all categories</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
    {
            var query =
                    from a in db.Assignments
                    join ac in db.AssignmentCategories
                    on a.CategoryId equals ac.CategoryId
                    join cl in db.Classes
                    on ac.ClassId equals cl.ClassId
                    join co in db.Courses
                    on cl.CourseId equals co.CourseId
                    join d in db.Departments
                    on co.DId equals d.DId
                    where d.Subject == subject &&
                          co.Num == num.ToString() &&
                          cl.SemesterSeason == season &&
                          cl.SemesterYear == (uint)year &&
                          (category == null || ac.Name == category)
                    select new
                    {
                        aname = a.Name,
                        cname = ac.Name,
                        due = a.Due,
                        submissions = db.Submissions.Where(s=>s.AssignmentId == a.AssignmentId).Count()
                    };
            return Json(query.ToArray());
    }


    /// <summary>
    /// Returns a JSON array of the assignment categories for a certain class.
    /// Each object in the array should have the folling fields:
    /// "name" - The category name
    /// "weight" - The category weight
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
    {
            var query =
                    from ac in db.AssignmentCategories
                    join cl in db.Classes
                    on ac.ClassId equals cl.ClassId
                    join co in db.Courses
                    on cl.CourseId equals co.CourseId
                    join d in db.Departments
                    on co.DId equals d.DId
                    where d.Subject == subject &&
                          co.Num == num.ToString() &&
                          cl.SemesterSeason == season &&
                          cl.SemesterYear == (uint)year
                    select new
                    {
                        name = ac.Name,
                        weight = ac.Weight
                    };
             return Json(query.ToArray());
    }

    /// <summary>
    /// Creates a new assignment category for the specified class.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The new category name</param>
    /// <param name="catweight">The new category weight</param>
    /// <returns>A JSON object containing {success = true/false},
    ///	false if an assignment category with the same name already exists in the same class.</returns>
    public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            try
            {
                var query =
                    from cl in db.Classes
                    join co in db.Courses
                    on cl.CourseId equals co.CourseId
                    join d in db.Departments
                    on co.DId equals d.DId
                    where d.Subject == subject &&
                          co.Num == num.ToString() &&
                          cl.SemesterSeason == season &&
                          cl.SemesterYear == (uint)year
                    select cl;

                if (query.Count() == 0)
                    return Json(new { success = false });

                AssignmentCategories ac = new AssignmentCategories
                {
                    Name = category,
                    ClassId = query.First().ClassId,
                    Weight = (uint)catweight
                };

                db.AssignmentCategories.Add(ac);
                int changes = db.SaveChanges();
                return Json(new { success = (changes > 0) });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false });
            }
    }

    /// <summary>
    /// Creates a new assignment for the given class and category.
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The new assignment name</param>
    /// <param name="asgpoints">The max point value for the new assignment</param>
    /// <param name="asgdue">The due DateTime for the new assignment</param>
    /// <param name="asgcontents">The contents of the new assignment</param>
    /// <returns>A JSON object containing success = true/false,
	/// false if an assignment with the same name already exists in the same assignment category.</returns>
    public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
    {
            try
            {
                var query =
                    from ac in db.AssignmentCategories
                    join cl in db.Classes
                    on ac.ClassId equals cl.ClassId
                    join co in db.Courses
                    on cl.CourseId equals co.CourseId
                    join d in db.Departments
                    on co.DId equals d.DId
                    where d.Subject == subject &&
                          co.Num == num.ToString() &&
                          cl.SemesterSeason == season &&
                          cl.SemesterYear == (uint)year &&
                          ac.Name == category
                    select ac;

                if (query.Count() == 0)
                    return Json(new { success = false });

                Assignments a = new Assignments
                {
                    CategoryId = query.First().CategoryId,
                    Name = asgname,
                    Contents = asgcontents,
                    Due = asgdue,
                    Points = (uint)asgpoints
                };

                db.Assignments.Add(a);
                int changes = db.SaveChanges();


                // Update grades
                var gradeQuery =
                    from e in db.Enrolled
                    join cl in db.Classes
                    on e.ClassId equals cl.ClassId
                    join co in db.Courses
                    on cl.CourseId equals co.CourseId
                    join d in db.Departments
                    on co.DId equals d.DId
                    where d.Subject == subject &&
                          co.Num == num.ToString() &&
                          cl.SemesterSeason == season &&
                          cl.SemesterYear == (uint)year
                    select e.SId;
                foreach (string student in gradeQuery)
                    UpdateStudentGrade(subject, num, season, year, student);


                return Json(new { success = (changes > 0) });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false });
            }
        }


    /// <summary>
    /// Gets a JSON array of all the submissions to a certain assignment.
    /// Each object in the array should have the following fields:
    /// "fname" - first name
    /// "lname" - last name
    /// "uid" - user ID
    /// "time" - DateTime of the submission
    /// "score" - The score given to the submission
    /// 
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
    {
            var query =
                    from u in db.Students
                    join s in db.Submissions
                    on u.UId equals s.UId
                    join a in db.Assignments
                    on s.AssignmentId equals a.AssignmentId
                    join ac in db.AssignmentCategories
                    on a.CategoryId equals ac.CategoryId
                    join cl in db.Classes
                    on ac.ClassId equals cl.ClassId
                    join co in db.Courses
                    on cl.CourseId equals co.CourseId
                    join d in db.Departments
                    on co.DId equals d.DId
                    where d.Subject == subject &&
                          co.Num == num.ToString() &&
                          cl.SemesterSeason == season &&
                          cl.SemesterYear == (uint)year &&
                          ac.Name == category &&
                          a.Name == asgname
                    select new
                    {
                        fname = u.FName,
                        lname = u.LName,
                        uid = u.UId,
                        time = s.Time,
                        score = s.Score
                    };

            return Json(query.ToArray());
        }


    /// <summary>
    /// Set the score of an assignment submission
    /// </summary>
    /// <param name="subject">The course subject abbreviation</param>
    /// <param name="num">The course number</param>
    /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
    /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
    /// <param name="category">The name of the assignment category in the class</param>
    /// <param name="asgname">The name of the assignment</param>
    /// <param name="uid">The uid of the student who's submission is being graded</param>
    /// <param name="score">The new score for the submission</param>
    /// <returns>A JSON object containing success = true/false</returns>
    public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
    {
            try
            {
                var query =
                    from sub in db.Submissions
                    join a in db.Assignments
                    on sub.AssignmentId equals a.AssignmentId
                    join ac in db.AssignmentCategories
                    on a.CategoryId equals ac.CategoryId
                    join cl in db.Classes
                    on ac.ClassId equals cl.ClassId
                    join co in db.Courses
                    on cl.CourseId equals co.CourseId
                    join d in db.Departments
                    on co.DId equals d.DId
                    where d.Subject == subject &&
                          co.Num == num.ToString() &&
                          cl.SemesterSeason == season &&
                          cl.SemesterYear == (uint)year &&
                          ac.Name == category &&
                          a.Name == asgname &&
                          sub.UId == uid
                    select sub;

                if (query.Count() > 0)
                {
                    Submissions s = query.First();
                    s.Score = (uint)score;
                    int changed = db.SaveChanges();
                    UpdateStudentGrade(subject, num, season, year, uid);
                    return Json(new { success = (changed > 0) });
                }
                else
                {
                    return Json(new { success = false });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Json(new { success = false });
            }
        }


    /// <summary>
    /// Returns a JSON array of the classes taught by the specified professor
    /// Each object in the array should have the following fields:
    /// "subject" - The subject abbreviation of the class (such as "CS")
    /// "number" - The course number (such as 5530)
    /// "name" - The course name
    /// "season" - The season part of the semester in which the class is taught
    /// "year" - The year part of the semester in which the class is taught
    /// </summary>
    /// <param name="uid">The professor's uid</param>
    /// <returns>The JSON array</returns>
    public IActionResult GetMyClasses(string uid)
    {
            var query =
                    from cl in db.Classes
                    join co in db.Courses
                    on cl.CourseId equals co.CourseId
                    join d in db.Departments
                    on co.DId equals d.DId
                    where cl.PId == uid
                    select new
                    {
                        subject = d.Subject,
                        number = co.Num,
                        name = co.Name,
                        season = cl.SemesterSeason,
                        year = cl.SemesterYear
                    };
            return Json(query.ToArray());
    }


    /*******End code to modify********/

  }
}