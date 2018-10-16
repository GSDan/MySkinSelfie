using Newtonsoft.Json;
using SQLite;
using System;

namespace SkinSelfie.AppModels
{
    public class AccessToken
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
        [JsonProperty(PropertyName = ".issued")]
        public DateTime issued { get; set; }
        [JsonProperty(PropertyName = ".expires")]
        public DateTime expires { get; set; }
    }
}
