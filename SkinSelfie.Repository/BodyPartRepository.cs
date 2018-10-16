using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace SkinSelfie.Repository
{
    public class BodyPartRepository : IReadOnlyRepository<ServiceData.Models.BodyPart>
    {
        private readonly SkinSelfieDB context;

        public BodyPartRepository()
        {
            context = new SkinSelfieDB();
        }

        public IQueryable<ServiceData.Models.BodyPart> GetAll()
        {
            return context.BodyParts.Select(bp => new ServiceData.Models.BodyPart
            {
                Id = bp.Id,
                Name = bp.Name,
                SkinRegions = bp.SkinRegions.Select(sr => new ServiceData.Models.SkinRegion
                {
                    Id = sr.Id,
                    Name = sr.Name
                })
            });
        }

        public ServiceData.Models.BodyPart GetById(int id)
        {
            return GetAll().Single(part => part.Id == id);
        }

        public IQueryable<ServiceData.Models.BodyPart> Search(Expression<Func<ServiceData.Models.BodyPart, bool>> predicate)
        {
            return GetAll().Where(predicate);
        }
    }
}
