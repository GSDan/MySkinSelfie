using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SkinSelfie.Repository
{
    public class UserRepository : IReadWriteRepository<ServiceData.Models.User>
    {
        private readonly SkinSelfieDB context;

        public UserRepository()
        {
            context = new SkinSelfieDB();
        }

        public ServiceData.Models.User Insert(ServiceData.Models.User model)
        {
            try
            {
                User result = context.Users.Add(new User
                {
                    Name = model.Name,
                    BirthDate = model.BirthDate,
                    Email = model.Email,
                    Admin = false
                });

                context.SaveChangesAsync();

                return ToServiceModel(result);
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public ServiceData.Models.User Update(ServiceData.Models.User model)
        {
            var entity = context.Users.Single(user => user.Id == model.Id);
            entity.Name = model.Name;
            entity.BirthDate = model.BirthDate;
            entity.Email = model.Email;

            context.SaveChangesAsync();
            return model;
        }

        public async Task Delete(int id)
        {
            context.Users.Remove(context.Users.Single(u => u.Id == id));
            await context.SaveChangesAsync();
        }

        public IQueryable<ServiceData.Models.User> GetAll()
        {
            return context.Users.Select(u => new ServiceData.Models.User
            {
                Id = u.Id,
                Name = u.Name,
                BirthDate = u.BirthDate,
                Email = u.Email,
                Admin = u.Admin.HasValue && u.Admin.Value,
                Conditions = u.UserConditions.Select(uc => new ServiceData.Models.UserCondition
                {
                    Id = uc.Id,
                    Condition = uc.Condition,
                    Passcode = uc.Passcode,
                    SkinRegion = new ServiceData.Models.SkinRegion
                    {
                        Id = uc.SkinRegion.Id,
                        Name = uc.SkinRegion.Name,
                        BodyPart = new ServiceData.Models.BodyPart
                        {
                            Id = uc.SkinRegion.BodyPart.Id,
                            Name = uc.SkinRegion.BodyPart.Name
                        }
                    },
                    Photos = uc.Photos.Select(p => new ServiceData.Models.Photo
                    {
                        Id = p.Id,
                        CreatedAt = p.CreatedAt,
                        Url = p.Url,
                        ThumbUrl = p.ThumbUrl,
                        Notes = p.Notes,
                        PhotoDescription = p.PhotoDescription,
                        Rating = p.Rating,
                        Treatment = p.Treatment
                    }),
                    StartDate = uc.StartDate,
                    Treatment = uc.Treatment
                })
            });
        }

        public ServiceData.Models.User GetById(int id)
        {
            try
            {
                return GetAll().Single(user => user.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public IQueryable<ServiceData.Models.User> Search(Expression<Func<ServiceData.Models.User, bool>> predicate)
        {
            IQueryable<ServiceData.Models.User> all = GetAll();
            return all.Where(predicate);
        }

        public static ServiceData.Models.User ToServiceModel(User data)
        {
            return new ServiceData.Models.User
            {
                Id = data.Id,
                Name = data.Name,
                BirthDate = data.BirthDate,
                Email = data.Email,
                Conditions = data.UserConditions.Select(uc => new ServiceData.Models.UserCondition
                {
                    Id = uc.Id,
                    Condition = uc.Condition,
                    Passcode = uc.Passcode,
                    SkinRegion = new ServiceData.Models.SkinRegion
                    {
                        Id = uc.SkinRegion.Id,
                        Name = uc.SkinRegion.Name,
                        BodyPart = new ServiceData.Models.BodyPart
                        {
                            Id = uc.SkinRegion.BodyPart.Id,
                            Name = uc.SkinRegion.BodyPart.Name
                        }
                    },
                    Photos = uc.Photos.Select(p => new ServiceData.Models.Photo
                    {
                        Id = p.Id,
                        CreatedAt = p.CreatedAt,
                        Url = p.Url,
                        Notes = p.Notes,
                        PhotoDescription = p.PhotoDescription,
                        Rating = p.Rating,
                        Treatment = p.Treatment
                    }),
                    StartDate = uc.StartDate,
                    Treatment = uc.Treatment
                })
            };
        }
    }
}
