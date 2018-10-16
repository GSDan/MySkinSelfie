using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SkinSelfie.Repository
{
    public class ShareRepository : IReadWriteRepository<ServiceData.Models.Share>
    {
        private readonly SkinSelfieDB context;

        public ShareRepository()
        {
            context = new SkinSelfieDB();
        }

        public async Task Delete(int id)
        {
            context.Shares.Remove(context.Shares.Single(s => s.Id == id));
            await context.SaveChangesAsync();
        }

        public IQueryable<ServiceData.Models.Share> GetAll()
        {
            return context.Shares.Select(s => new ServiceData.Models.Share
            {
                Id = s.Id,
                CreatedAt = s.CreatedAt,
                ExpireDate = s.ExpireDate,
                SharedEmail = s.SharedEmail,
                Updated = s.Updated,
                UserCondition = new ServiceData.Models.UserCondition
                {
                    Id = s.UserCondition.Id,
                    Condition = s.UserCondition.Condition,
                    Passcode = s.UserCondition.Passcode,
                    SkinRegion = new ServiceData.Models.SkinRegion
                    {
                        Id = s.UserCondition.SkinRegion.Id,
                        Name = s.UserCondition.SkinRegion.Name,
                        BodyPart = new ServiceData.Models.BodyPart
                        {
                            Id = s.UserCondition.SkinRegion.BodyPart.Id,
                            Name = s.UserCondition.SkinRegion.BodyPart.Name
                        }
                    },
                    Photos = s.UserCondition.Photos.Select(p => new ServiceData.Models.Photo
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
                    StartDate = s.UserCondition.StartDate,
                    Treatment = s.UserCondition.Treatment,
                    Owner = new ServiceData.Models.User
                    {
                        Id = s.Owner.Id,
                        Name = s.Owner.Name,
                        BirthDate = s.Owner.BirthDate,
                        Email = s.Owner.Email
                    }
                },
                Owner = new ServiceData.Models.User
                {
                    Id = s.Owner.Id,
                    Name = s.Owner.Name,
                    BirthDate = s.Owner.BirthDate,
                    Email = s.Owner.Email
                }
            });
        }

        public ServiceData.Models.Share GetById(int id)
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

        public ServiceData.Models.Share Insert(ServiceData.Models.Share model)
        {
            try
            {
                Share result = context.Shares.Add(new Share
                {
                    Id = model.Id,
                    CreatedAt = model.CreatedAt,
                    ExpireDate = model.ExpireDate,
                    OwnerId = model.Owner.Id,
                    Updated = model.Updated,
                    SharedEmail = model.SharedEmail
                });

                if(model.UserCondition != null)
                {
                    result.UserCondition = context.UserConditions.FirstOrDefault(c => c.Id == model.UserCondition.Id);
                }
                
                context.SaveChanges();
                model.Id = result.Id;
                return model;
            }
            catch
            {
                return null;
            }
        }

        public IQueryable<ServiceData.Models.Share> Search(Expression<Func<ServiceData.Models.Share, bool>> predicate)
        {
            IQueryable<ServiceData.Models.Share> all = GetAll();
            return all.Where(predicate);
        }

        public ServiceData.Models.Share Update(ServiceData.Models.Share model)
        {
            var entity = context.Shares.Single(share => share.Id == model.Id);
            entity.CreatedAt = model.CreatedAt;
            entity.ExpireDate = model.ExpireDate;
            entity.OwnerId = model.Owner.Id;
            entity.SharedEmail = model.SharedEmail;
            entity.Updated = model.Updated;
            entity.ConditionId = model.UserCondition.Id;

            context.SaveChangesAsync();
            return model;
        }
    }
}
