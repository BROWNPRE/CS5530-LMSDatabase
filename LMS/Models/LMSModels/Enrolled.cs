using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Enrolled
    {
        public uint EnrollmentId { get; set; }
        public string SId { get; set; }
        public uint ClassId { get; set; }
        public string Grade { get; set; }

        public virtual Classes Class { get; set; }
        public virtual Students S { get; set; }
    }
}
