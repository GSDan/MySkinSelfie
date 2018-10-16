namespace SkinSelfie.WebAPI.Models
{
    public class SkinRegion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BodyPart BodyPart { get; set; }

        public static ServiceData.Models.SkinRegion ToServiceModel(SkinRegion given, bool includeBodyPart)
        {
            ServiceData.Models.SkinRegion region = new ServiceData.Models.SkinRegion
            {
                Id = given.Id,
                Name = given.Name
            };

            if (includeBodyPart) region.BodyPart = BodyPart.ToServiceModel(given.BodyPart, false);

            return region;
        }

        public static SkinRegion ToAppModel(ServiceData.Models.SkinRegion given, bool includeBodyPart)
        {
            SkinRegion region = new SkinRegion
            {
                Id = given.Id,
                Name = given.Name
            };

            if (includeBodyPart) region.BodyPart = BodyPart.ToAppModel(given.BodyPart, false);

            return region;
        }
    }
}
