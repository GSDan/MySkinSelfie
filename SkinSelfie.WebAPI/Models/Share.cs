using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkinSelfie.WebAPI.Models
{
    public class Share
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireDate { get; set; }
        public User Owner { get; set; }
        public UserCondition UserCondition { get; set; }
        public string SharedEmail { get; set; }
        public bool Updated { get; set; }

        public static ServiceData.Models.Share ToServiceModel(Share given, bool includeOwner)
        {
            ServiceData.Models.Share serviceShare = new ServiceData.Models.Share
            {
                Id = given.Id,
                CreatedAt = given.CreatedAt,
                ExpireDate = given.ExpireDate,
                SharedEmail = given.SharedEmail,
                Updated = given.Updated
            };

            if(given.Owner != null && includeOwner)
            {
                serviceShare.Owner = User.ToServiceModel(given.Owner);
            }

            if (given.UserCondition != null)
            {
                serviceShare.UserCondition = UserCondition.ToServiceModel(given.UserCondition, true);
            }

            return serviceShare;
        }

        public static Share ToAppModel(ServiceData.Models.Share given, bool includeOwner)
        {
            if (given == null) return null;

            Share appShare = new Share
            {
                Id = given.Id,
                CreatedAt = given.CreatedAt,
                ExpireDate = given.ExpireDate,
                SharedEmail = given.SharedEmail,
                Updated = given.Updated
            };

            if (given.Owner != null && includeOwner)
            {
                appShare.Owner = User.ToAppModel(given.Owner);
            }

            if (given.UserCondition != null)
            {
                appShare.UserCondition = UserCondition.ToAppModel(given.UserCondition, true);
            }

            return appShare;
        }
    }
}