using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SkinSelfie.Repository
{
    public class StudyRepository : IReadWriteRepository<ServiceData.Models.Study>
    {
        private readonly SkinSelfieDB context;

        public StudyRepository()
        {
            context = new SkinSelfieDB();
        }

        public async Task Delete(int id)
        {
            context.Studies.Remove(context.Studies.Single(s => s.Id == id));
            await context.SaveChangesAsync();
        }

        public IQueryable<ServiceData.Models.Study> GetAll()
        {
            return context.Studies.Select(s => new ServiceData.Models.Study
            {
                Id = s.Id,
                Active = s.Active,
                CreatedAt = s.CreatedAt,
                Code = s.Code,
                Name = s.Name,
                ManagerId = s.ManagerId,
                Manager = new ServiceData.Models.User
                {
                    Id = s.Manager.Id,
                    Name = s.Manager.Name,
                    BirthDate = s.Manager.BirthDate,
                    Email = s.Manager.Email
                },
                StudyEnrolments = s.StudyEnrolments.Select(en => new ServiceData.Models.StudyEnrolment
                {
                    Id = en.Id,
                    CreatedAt = en.CreatedAt,
                    Enrolled = en.Enrolled,
                    StudyId = en.StudyId,
                    UserId = en.UserId,
                    User = new ServiceData.Models.User
                    {
                        Id = en.User.Id,
                        Name = en.User.Name,
                        BirthDate = en.User.BirthDate,
                        Email = en.User.Email
                    }
                })
            });
        }

        public ServiceData.Models.Study GetById(int id)
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

        public ServiceData.Models.Study Insert(ServiceData.Models.Study s)
        {
            try
            {
                Study result = context.Studies.Add(new Study
                {
                    Active = s.Active,
                    CreatedAt = s.CreatedAt,
                    Code = s.Code,
                    Name = s.Name,
                    ManagerId = s.ManagerId
                });

                if (s.Manager != null)
                {
                    result.Manager = context.Users.FirstOrDefault(c => c.Id == s.Manager.Id);
                }

                context.SaveChanges();
                s.Id = result.Id;
                return s;
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public IQueryable<ServiceData.Models.Study> Search(Expression<Func<ServiceData.Models.Study, bool>> predicate)
        {
            IQueryable<ServiceData.Models.Study> all = GetAll();
            return all.Where(predicate);
        }

        public ServiceData.Models.Study Update(ServiceData.Models.Study en)
        {
            var entity = context.Studies.Single(s => s.Id == en.Id);
            entity.CreatedAt = en.CreatedAt;
            entity.Code = en.Code;
            entity.Name = en.Name;
            entity.ManagerId = en.ManagerId;
            entity.Active = entity.Active;

            context.SaveChangesAsync();
            return en;
        }
    }
}