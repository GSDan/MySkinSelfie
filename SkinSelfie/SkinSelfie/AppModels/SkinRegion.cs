namespace SkinSelfie.AppModels
{
    public class SkinRegion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        [SQLite.Ignore]
        public BodyPart BodyPart { get; set; }
    }
}
