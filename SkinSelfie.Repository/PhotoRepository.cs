using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkinSelfie.ServiceData.Models;
using System.Linq.Expressions;

namespace SkinSelfie.Repository
{
    public class PhotoRepository : IReadWriteRepository<ServiceData.Models.Photo>
    {
        private readonly SkinSelfieDB context;

        public PhotoRepository()
        {
            context = new SkinSelfieDB();
        }

        public async Task Delete(int id)
        {
            context.Photos.Remove(context.Photos.Single(p => p.Id == id));
            await context.SaveChangesAsync();
        }

        public IQueryable<ServiceData.Models.Photo> GetAll()
        {
            return context.Photos.Select(p => new ServiceData.Models.Photo
            {
                Id = p.Id,
                CreatedAt = p.CreatedAt,
                Url = p.Url,
                ThumbUrl = p.ThumbUrl,
                Notes = p.Notes,
                PhotoDescription = p.PhotoDescription,
                Rating = p.Rating,
                Treatment = p.Treatment,
                UserCondition = new ServiceData.Models.UserCondition
                {
                    Id = p.UserCondition.Id,
                    Condition = p.UserCondition.Condition,
                    Passcode = p.UserCondition.Passcode,
                    SkinRegion = new ServiceData.Models.SkinRegion
                    {
                        Id = p.UserCondition.SkinRegion.Id,
                        Name = p.UserCondition.SkinRegion.Name,
                        BodyPart = new ServiceData.Models.BodyPart
                        {
                            Id = p.UserCondition.SkinRegion.BodyPart.Id,
                            Name = p.UserCondition.SkinRegion.BodyPart.Name
                        }
                    },
                    StartDate = p.UserCondition.StartDate,
                    Treatment = p.UserCondition.Treatment
                }

                
            });
        }

        public ServiceData.Models.Photo GetById(int id)
        {
            try
            {
                return GetAll().Single(photo => photo.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public ServiceData.Models.Photo Insert(ServiceData.Models.Photo model)
        {
            try
            {
                Photo result = context.Photos.Add(new Photo
                {
                    Notes = model.Notes,
                    CreatedAt = model.CreatedAt,
                    PhotoDescription = model.PhotoDescription,
                    Rating = model.Rating,
                    Treatment = model.Treatment,
                    Url = model.Url,
                    ThumbUrl = model.ThumbUrl,
                    UserConditionId = model.UserCondition.Id,
                });

                context.SaveChanges();

                result.UserCondition = new UserCondition
                {
                    Id = model.UserCondition.Id,
                    Condition = model.UserCondition.Condition,
                    Owner = new User
                    {
                        Id = model.UserCondition.Owner.Id,
                        Name = model.UserCondition.Owner.Name,
                        Email = model.UserCondition.Owner.Email,
                        BirthDate = model.UserCondition.Owner.BirthDate
                    },
                    OwnerId = model.UserCondition.Owner.Id,
                    Passcode = model.UserCondition.Passcode,
                    SkinRegion = new SkinRegion
                    {
                        Id = model.UserCondition.SkinRegion.Id,
                        Name = model.UserCondition.SkinRegion.Name
                    }
                };

                return ToServiceModel(result);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public IQueryable<ServiceData.Models.Photo> Search(Expression<Func<ServiceData.Models.Photo, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public ServiceData.Models.Photo Update(ServiceData.Models.Photo model)
        {
            var entity = context.Photos.Single(photo => photo.Id == model.Id);
            entity.Notes = model.Notes;
            entity.PhotoDescription = model.PhotoDescription;
            entity.Rating = model.Rating;
            entity.Treatment = model.Treatment;
            entity.ThumbUrl = model.ThumbUrl;
            entity.Url = model.Url;

            context.SaveChangesAsync();
            return model;
        }

        private ServiceData.Models.Photo ToServiceModel(Photo photo)
        {
            return new ServiceData.Models.Photo
            {
                Id = photo.Id,
                CreatedAt = photo.CreatedAt,
                Notes = photo.Notes,
                PhotoDescription = photo.PhotoDescription,
                Rating = photo.Rating,
                Treatment = photo.Treatment,
                Url = photo.Url,
                ThumbUrl = photo.ThumbUrl,
                UserCondition = new ServiceData.Models.UserCondition
                {
                    Id = photo.UserCondition.Id,
                    Condition = photo.UserCondition.Condition,
                    Passcode = photo.UserCondition.Passcode,
                    StartDate = photo.UserCondition.StartDate,
                    Treatment = photo.UserCondition.Treatment,
                    Owner = new ServiceData.Models.User
                    {
                        Id = photo.UserCondition.Owner.Id,
                        Name = photo.UserCondition.Owner.Name,
                        Email = photo.UserCondition.Owner.Email,
                        BirthDate = photo.UserCondition.Owner.BirthDate 
                    }
                }
            };
        }
    }
}
