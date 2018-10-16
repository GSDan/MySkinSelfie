using System;
using System.Linq;
using System.Collections.Generic;

namespace SkinSelfie.WebAPI.Models
{
    public class UserCondition
    {
        public int Id { get; set; }
        public User Owner { get; set; }
        public SkinRegion SkinRegion { get; set; }
        public string Treatment { get; set; }
        public string Condition { get; set; }
        public DateTime StartDate { get; set; }
        public int? Passcode { get; set; }
        public List<Photo> Photos { get; set; }
        public bool Finished { get; set; }

        public static ServiceData.Models.UserCondition ToServiceModel(UserCondition given, bool includeOwner)
        {
            ServiceData.Models.UserCondition cond = new ServiceData.Models.UserCondition
            {
                Id = given.Id,
                Passcode = given.Passcode,
                StartDate = given.StartDate,
                Treatment = given.Treatment,
                Condition = given.Condition,
                Finished = given.Finished,
                SkinRegion = SkinRegion.ToServiceModel(given.SkinRegion, true)
            };

            if (includeOwner && given.Owner != null) cond.Owner = User.ToServiceModel(given.Owner);

            if (given.Photos != null)
            {
                List<ServiceData.Models.Photo> photos = new List<ServiceData.Models.Photo>();

                foreach (Photo p in given.Photos)
                {
                    photos.Add(Photo.ToServiceModel(p, false));
                }
                cond.Photos = photos;
            }

            return cond;
        }

        public static UserCondition ToAppModel(ServiceData.Models.UserCondition given, bool includeOwner)
        {
            UserCondition cond = new UserCondition
            {
                Id = given.Id,
                Passcode = given.Passcode,
                StartDate = given.StartDate,
                Treatment = given.Treatment,
                Condition = given.Condition,
                Finished = given.Finished,
                SkinRegion = SkinRegion.ToAppModel(given.SkinRegion, true),
                Photos = new List<Photo>()
            };

            if (includeOwner && given.Owner != null) cond.Owner = User.ToAppModel(given.Owner);

            if (given.Photos == null) return cond;

            List<ServiceData.Models.Photo> photos = given.Photos.ToList();

            foreach (ServiceData.Models.Photo p in photos)
            {
                cond.Photos.Add(Photo.ToAppModel(p, false));
            }

            return cond;
        }
    }
}
