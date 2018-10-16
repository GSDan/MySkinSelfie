using System;

namespace SkinSelfie.WebAPI.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string ThumbUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public string PhotoDescription { get; set; }
        public int? Rating { get; set; }
        public UserCondition UserCondition { get; set; }

        public static ServiceData.Models.Photo ToServiceModel(Photo given, bool includeCondition)
        {
            ServiceData.Models.Photo cond = new ServiceData.Models.Photo
            {
                Id = given.Id,
                Url = given.Url,
                ThumbUrl = given.ThumbUrl,
                CreatedAt = given.CreatedAt,
                Treatment = given.Treatment,
                Notes = given.Notes,
                PhotoDescription = given.PhotoDescription,
                Rating = given.Rating
            };

            if (includeCondition && given.UserCondition != null) cond.UserCondition = UserCondition.ToServiceModel(given.UserCondition, true);

            return cond;
        }

        public static Photo ToAppModel(ServiceData.Models.Photo given, bool includeCondition)
        {
            Photo cond = new Photo
            {
                Id = given.Id,
                Url = given.Url,
                ThumbUrl = given.ThumbUrl,
                CreatedAt = given.CreatedAt,
                Treatment = given.Treatment,
                Notes = given.Notes,
                PhotoDescription = given.PhotoDescription,
                Rating = given.Rating
            };

            if (includeCondition && given.UserCondition != null) cond.UserCondition = UserCondition.ToAppModel(given.UserCondition, true);

            return cond;
        }
    }
}
