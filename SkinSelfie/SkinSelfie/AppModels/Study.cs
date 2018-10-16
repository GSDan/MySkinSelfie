using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinSelfie.AppModels
{
    public class Study
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ManagerId { get; set; }
        public bool Active { get; set; }

        public User Manager { get; set; }
        public List<StudyEnrolment> StudyEnrolments { get; set; }
    }
}
