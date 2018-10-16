using System;
using System.Collections.Generic;

namespace SkinSelfie.ServiceData.Models
{
    public class Study : Model
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ManagerId { get; set; }
        public bool Active { get; set; }

        public User Manager { get; set; }
        public IEnumerable<StudyEnrolment> StudyEnrolments { get; set; }
    }
}
