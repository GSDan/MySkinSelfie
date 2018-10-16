using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinSelfie.AppModels
{
    public class StudyEnrolment
    {
        public int Id { get; set; }
        public int StudyId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Enrolled { get; set; }

        public Study Study { get; set; }
        public User User { get; set; }
    }
}
