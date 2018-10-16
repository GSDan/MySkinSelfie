using System;
using System.Collections.Generic;

namespace SkinSelfie.WebAPI.Models
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

        public static ServiceData.Models.Study ToServiceModel(Study given, bool includeOwner)
        {
            if (given == null) return null;

            ServiceData.Models.Study serviceStudy = new ServiceData.Models.Study
            {
                Id = given.Id,
                CreatedAt = given.CreatedAt,
                Name = given.Name,
                Active = given.Active,
                Code = given.Code,
                ManagerId = given.ManagerId
            };

            if (given.Manager != null && includeOwner)
            {
                serviceStudy.Manager = User.ToServiceModel(given.Manager);
            }

            if (given.StudyEnrolments != null)
            {
                List<ServiceData.Models.StudyEnrolment> enrols = new List<ServiceData.Models.StudyEnrolment>();
                foreach(StudyEnrolment se in given.StudyEnrolments)
                {
                    enrols.Add(StudyEnrolment.ToServiceModel(se, false, true));
                }
                serviceStudy.StudyEnrolments = enrols;
            }

            return serviceStudy;
        }

        public static Study ToAppModel(ServiceData.Models.Study given, bool includeOwner)
        {
            if (given == null) return null;

            Study appStudy = new Study
            {
                Id = given.Id,
                CreatedAt = given.CreatedAt,
                Name = given.Name,
                Active = given.Active,
                Code = given.Code,
                ManagerId = given.ManagerId
            };

            if (given.Manager != null && includeOwner)
            {
                appStudy.Manager = User.ToAppModel(given.Manager);
            }

            if (given.StudyEnrolments != null)
            {
                appStudy.StudyEnrolments = new List<StudyEnrolment>();
                foreach (ServiceData.Models.StudyEnrolment se in given.StudyEnrolments)
                {
                    appStudy.StudyEnrolments.Add(StudyEnrolment.ToAppModel(se, false, true));
                }
            }

            return appStudy;
        }
    }
}