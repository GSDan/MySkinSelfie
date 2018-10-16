using Newtonsoft.Json;
using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SkinSelfie.AppModels
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

        [JsonIgnore]
        public bool IsSelected = false;

        [JsonIgnore]
        public byte[] InMemory { get; set; }

        // Returns the service URL to retrieve this image (needs auth)
        public string GetReqUrl(bool thumb)
        {
            return string.Format("{0}api/Upload?imageId={1}&thumb={2}", ConfidentialData.SiteUrl, Id, thumb);
        }

    }
}
