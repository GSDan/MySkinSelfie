using Newtonsoft.Json;
using System.Collections.Generic;

namespace SkinSelfie.AppModels
{
    public class BodyPart
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }

        [JsonProperty]
        private List<SkinRegion> SkinRegions;

        [JsonIgnore]
        public string JsonData { get; set; }

        public List<SkinRegion> GetSkinRegions()
        {
            if (SkinRegions == null && !string.IsNullOrWhiteSpace(JsonData))
            {
                SkinRegions = JsonConvert.DeserializeObject<BodyPart>(JsonData).SkinRegions;
            }
            return SkinRegions;
        }

        public void Translate(Dictionary<string,string> trans)
        {
            string translated = trans[Name];
            DisplayName = (!string.IsNullOrWhiteSpace(translated)) ? translated : Name;

            GetSkinRegions();
            for(int i = 0; i < SkinRegions.Count; i++)
            {
                translated = trans[SkinRegions[i].Name];
                SkinRegions[i].DisplayName = (!string.IsNullOrWhiteSpace(translated)) ? translated : SkinRegions[i].Name;
            }

            UpdateJson();
        }

        public void UpdateJson()
        {
            JsonData = null;
            JsonData = JsonConvert.SerializeObject(this, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}
