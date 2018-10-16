using System;
using System.Linq;
using System.Collections.Generic;

namespace SkinSelfie.WebAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool Admin { get; set; }
        public DateTime BirthDate { get; set; }
        public List<UserCondition> Conditions { get; set; }
        public List<Share> Shares { get; set; }
        public List<Study> ManagedStudies { get; set; }
        public List<StudyEnrolment> StudyEnrolments { get; set; }

        public static ServiceData.Models.User ToServiceModel(User given)
        {
            ServiceData.Models.User serviceUser = new ServiceData.Models.User
            {
                Id = given.Id,
                BirthDate = given.BirthDate,
                Name = given.Name,
                Email = given.Email,
                Admin = given.Admin
            };

            if(given.Conditions != null)
            {
                List<ServiceData.Models.UserCondition> conditions = new List<ServiceData.Models.UserCondition>();
                foreach (UserCondition cond in given.Conditions)
                {
                    conditions.Add(UserCondition.ToServiceModel(cond, true));
                }
                serviceUser.Conditions = conditions;
            }

            if (given.Shares != null)
            {
                List<ServiceData.Models.Share> shares = new List<ServiceData.Models.Share>();
                foreach (Share sh in given.Shares)
                {
                    shares.Add(Share.ToServiceModel(sh, false));
                }
                serviceUser.Shares = shares;
            }

            if (given.ManagedStudies != null)
            {
                List<ServiceData.Models.Study> studies = new List<ServiceData.Models.Study>();
                foreach (Study st in given.ManagedStudies)
                {
                    studies.Add(Study.ToServiceModel(st, false));
                }
                serviceUser.ManagedStudies = studies;
            }

            if (given.StudyEnrolments != null)
            {
                List<ServiceData.Models.StudyEnrolment> enrolled = new List<ServiceData.Models.StudyEnrolment>();
                foreach (StudyEnrolment st in given.StudyEnrolments)
                {
                    enrolled.Add(StudyEnrolment.ToServiceModel(st, true, false));
                }
                serviceUser.StudyEnrolments = enrolled;
            }

            return serviceUser;
        }

        public static User ToAppModel(ServiceData.Models.User given)
        {
            if (given == null) return null;

            User appUser = new User
            {
                Id = given.Id,
                BirthDate = given.BirthDate,
                Name = given.Name,
                Conditions = new List<UserCondition>(),
                Email = given.Email,
                Admin = given.Admin
            };

            if(given.Conditions != null)
            {
                foreach (ServiceData.Models.UserCondition cond in given.Conditions.ToList())
                {
                    appUser.Conditions.Add(UserCondition.ToAppModel(cond, true));
                }
            }

            if (given.Shares != null)
            {
                List<Share> shares = new List<Share>();
                foreach (ServiceData.Models.Share sh in given.Shares)
                {
                    shares.Add(Share.ToAppModel(sh, false));
                }
                appUser.Shares = shares;
            }

            if (given.ManagedStudies != null)
            {
                List<Study> studies = new List<Study>();
                foreach (ServiceData.Models.Study st in given.ManagedStudies)
                {
                    studies.Add(Study.ToAppModel(st, false));
                }
                appUser.ManagedStudies = studies;
            }

            if (given.StudyEnrolments != null)
            {
                List<StudyEnrolment> enrolled = new List<StudyEnrolment>();
                foreach (ServiceData.Models.StudyEnrolment st in given.StudyEnrolments)
                {
                    enrolled.Add(StudyEnrolment.ToAppModel(st, true, false));
                }
                appUser.StudyEnrolments = enrolled;
            }

            return appUser;
        }
    }
}
