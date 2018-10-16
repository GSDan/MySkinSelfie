using System;

namespace SkinSelfie.ServiceData.Models
{
    public class StudyEnrolment : Model
    {
        public int StudyId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Enrolled { get; set; }

        public Study Study { get; set; }
        public User User { get; set; }
    }
}
