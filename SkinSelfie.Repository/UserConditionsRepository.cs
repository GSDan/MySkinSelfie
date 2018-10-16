using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SkinSelfie.Repository
{
    public class UserConditionsRepository : IReadWriteRepository<ServiceData.Models.UserCondition>
    {
        private readonly SkinSelfieDB context;

        public UserConditionsRepository()
        {
            context = new SkinSelfieDB();
        }

        public IQueryable<ServiceData.Models.UserCondition> Search(Expression<Func<ServiceData.Models.UserCondition, bool>> predicate)
        {
            IQueryable<ServiceData.Models.UserCondition> all = GetAll();
            return all.Where(predicate);
        }

        public IQueryable<ServiceData.Models.UserCondition> GetAll()
        {
            return context.UserConditions.Select(uc => new ServiceData.Models.UserCondition
            {
                Id = uc.Id,
                Condition = uc.Condition,
                Passcode = uc.Passcode,
                Finished = uc.Finished,
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
                Treatment = uc.Treatment,
                Owner = new ServiceData.Models.User
                {
                    Id = uc.Owner.Id,
                    Name = uc.Owner.Name,
                    BirthDate = uc.Owner.BirthDate,
                    Email = uc.Owner.Email
                }
            });
        }

        ServiceData.Models.UserCondition IReadOnlyRepository<ServiceData.Models.UserCondition>.GetById(int id)
        {
            try
            {
                return GetAll().Single(cond => cond.Id == id);
            }
            catch
            {
                return null;
            }
        }

        ServiceData.Models.UserCondition IReadWriteRepository<ServiceData.Models.UserCondition>.Insert(ServiceData.Models.UserCondition model)
        {
            try
            {
                UserCondition result = context.UserConditions.Add(new UserCondition
                {
                    Condition = model.Condition,
                    OwnerId = model.Owner.Id,
                    SkinRegionId = model.SkinRegion.Id,
                    Passcode = model.Passcode,
                    StartDate = model.StartDate,
                    Finished = model.Finished,
                    Treatment = model.Treatment
                });

                context.SaveChanges();

                result.SkinRegion = new SkinRegion
                {
                    Id = model.SkinRegion.Id,
                    Name = model.SkinRegion.Name,
                    BodyPartId = model.SkinRegion.BodyPart.Id,
                    BodyPart = new BodyPart
                    {
                        Id = model.SkinRegion.BodyPart.Id,
                        Name = model.SkinRegion.BodyPart.Name
                    }
                };

                result.Owner = new User
                {
                    Id = model.Owner.Id,
                    Name = model.Owner.Name,
                    Email = model.Owner.Email,
                    BirthDate = model.Owner.BirthDate
                };

                return ToServiceModel(result);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        ServiceData.Models.UserCondition IReadWriteRepository<ServiceData.Models.UserCondition>.Update(ServiceData.Models.UserCondition model)
        {
            UserCondition en = context.UserConditions.Single(cond => cond.Id == model.Id);
            en.Passcode = model.Passcode;
            en.SkinRegionId = model.SkinRegion.Id;
            en.StartDate = model.StartDate;
            en.Condition = model.Condition;
            en.Finished = model.Finished;
            context.SaveChangesAsync();
            return model;
        }

        public async Task Delete(int id)
        {
            context.UserConditions.Remove(context.UserConditions.Single(uc => uc.Id == id));
            await context.SaveChangesAsync();
        }

        private ServiceData.Models.UserCondition ToServiceModel(UserCondition uc)
        {
            return new ServiceData.Models.UserCondition
            {
                Id = uc.Id,
                Condition = uc.Condition,
                Passcode = uc.Passcode,
                Finished = uc.Finished,
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
                Treatment = uc.Treatment,
                Owner = new ServiceData.Models.User
                {
                    Id = uc.Owner.Id,
                    Name = uc.Owner.Name,
                    BirthDate = uc.Owner.BirthDate
                }
            };
        }
    }
}
