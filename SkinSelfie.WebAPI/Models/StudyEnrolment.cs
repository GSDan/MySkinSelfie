using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkinSelfie.WebAPI.Models
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

        public static ServiceData.Models.StudyEnrolment ToServiceModel(StudyEnrolment given, bool includeStudy, bool includeUser)
        {
            if (given == null) return null;

            ServiceData.Models.StudyEnrolment serviceEnrolment = new ServiceData.Models.StudyEnrolment
            {
                Id = given.Id,
                CreatedAt = given.CreatedAt,
                StudyId = given.StudyId,
                UserId = given.UserId,
                Enrolled = given.Enrolled
            };

            if (given.Study != null && includeStudy)
            {
                serviceEnrolment.Study = Study.ToServiceModel(given.Study, true);
            }

            if (given.User != null && includeUser)
            {
                serviceEnrolment.User = User.ToServiceModel(given.User);
            }

            return serviceEnrolment;
        }

        public static StudyEnrolment ToAppModel(ServiceData.Models.StudyEnrolment given, bool includeStudy, bool includeUser)
        {
            if (given == null) return null;

            StudyEnrolment appEnrolment = new StudyEnrolment
            {
                Id = given.Id,
                CreatedAt = given.CreatedAt,
                StudyId = given.StudyId,
                UserId = given.UserId,
                Enrolled = given.Enrolled
            };

            if (given.Study != null && includeStudy)
            {
                appEnrolment.Study = Study.ToAppModel(given.Study, true);
            }

            if (given.User != null && includeUser)
            {
                appEnrolment.User = User.ToAppModel(given.User);
            }

            return appEnrolment;
        }
    }
}