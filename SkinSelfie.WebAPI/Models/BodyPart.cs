using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinSelfie.WebAPI.Models
{
    public class BodyPart
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<SkinRegion> SkinRegions { get; set; }

        public static ServiceData.Models.BodyPart ToServiceModel(BodyPart given, bool includeRegions)
        {
            ServiceData.Models.BodyPart bp = new ServiceData.Models.BodyPart
            {
                Id = given.Id,
                Name = given.Name
            };

            if (!includeRegions) return bp;

            List<ServiceData.Models.SkinRegion> regions = new List<ServiceData.Models.SkinRegion>();

            foreach (SkinRegion region in given.SkinRegions)
            {
                regions.Add(SkinRegion.ToServiceModel(region, false));
            }
            bp.SkinRegions = regions;

            return bp;
        }

        public static BodyPart ToAppModel(ServiceData.Models.BodyPart given, bool includeRegions)
        {
            BodyPart bp = new BodyPart
            {
                Id = given.Id,
                Name = given.Name
            };

            if (!includeRegions) return bp;

            bp.SkinRegions = new List<SkinRegion>();

            foreach (ServiceData.Models.SkinRegion region in given.SkinRegions)
            {
                bp.SkinRegions.Add(SkinRegion.ToAppModel(region, false));
            }

            return bp;
        }
    }
}
