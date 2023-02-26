using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Courses
    {
        public Courses()
        {
            Classes = new HashSet<Classes>();
        }

        public uint CourseId { get; set; }
        public string Num { get; set; }
        public uint DId { get; set; }
        public string Name { get; set; }

        public virtual Departments D { get; set; }
        public virtual ICollection<Classes> Classes { get; set; }
    }
}
