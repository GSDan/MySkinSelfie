
using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SkinSelfie.Repository
{
    public class StudyEnrolmentRepository : IReadWriteRepository<ServiceData.Models.StudyEnrolment>
    {
        private readonly SkinSelfieDB context;

        public StudyEnrolmentRepository()
        {
            context = new SkinSelfieDB();
        }

        public async Task Delete(int id)
        {
            context.StudyEnrolments.Remove(context.StudyEnrolments.Single(s => s.Id == id));
            await context.SaveChangesAsync();
        }

        public IQueryable<ServiceData.Models.StudyEnrolment> GetAll()
        {
            return context.StudyEnrolments.Select(s => new ServiceData.Models.StudyEnrolment
            {
                Id = s.Id,
                CreatedAt = s.CreatedAt,
                Enrolled = s.Enrolled,
                StudyId = s.StudyId,
                UserId = s.UserId,
                User = new ServiceData.Models.User
                {
                    Id = s.User.Id,
                    Name = s.User.Name,
                    BirthDate = s.User.BirthDate,
                    Email = s.User.Email
                },
                Study = new ServiceData.Models.Study
                {
                    Id = s.Study.Id,
                    Active = s.Study.Active,
                    CreatedAt = s.Study.CreatedAt,
                    Code = s.Study.Code,
                    Name = s.Study.Name,
                    ManagerId = s.Study.ManagerId,
                    Manager = new ServiceData.Models.User
                    {
                        Id = s.Study.Manager.Id,
                        Name = s.Study.Manager.Name,
                        BirthDate = s.Study.Manager.BirthDate,
                        Email = s.Study.Manager.Email
                    }
                }
            });
        }

        public ServiceData.Models.StudyEnrolment GetById(int id)
        {
            try
            {
                return GetAll().Single(s => s.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public ServiceData.Models.StudyEnrolment Insert(ServiceData.Models.StudyEnrolment s)
        {
            try
            {
                StudyEnrolment result = context.StudyEnrolments.Add(new StudyEnrolment
                {
                    CreatedAt = s.CreatedAt,
                    Enrolled = s.Enrolled,
                    StudyId = s.StudyId,
                    UserId = s.UserId
                });

                context.SaveChanges();

                s.Id = result.Id;
                return s;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public IQueryable<ServiceData.Models.StudyEnrolment> Search(Expression<Func<ServiceData.Models.StudyEnrolment, bool>> predicate)
        {
            IQueryable<ServiceData.Models.StudyEnrolment> all = GetAll();
            return all.Where(predicate);
        }

        public ServiceData.Models.StudyEnrolment Update(ServiceData.Models.StudyEnrolment en)
        {
            var entity = context.StudyEnrolments.Single(s => s.Id == en.Id);
            entity.CreatedAt = en.CreatedAt;
            entity.Enrolled = en.Enrolled;
            entity.StudyId = en.StudyId;
            entity.UserId = en.UserId;

            context.SaveChangesAsync();
            return en;
        }
    }
}