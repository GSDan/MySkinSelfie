using System;
using System.Collections.Generic;

namespace SkinSelfie.ServiceData.Models
{
    public class User : Model
    {
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public bool Admin { get; set; }
        public IEnumerable<Share> Shares { get; set; }
        public IEnumerable<UserCondition> Conditions { get; set; }
        public IEnumerable<Study> ManagedStudies { get; set; }
        public IEnumerable<StudyEnrolment> StudyEnrolments { get; set; }
    }
}
