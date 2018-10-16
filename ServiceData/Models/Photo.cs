using System;

namespace SkinSelfie.ServiceData.Models
{
    public class Photo : Model
    {
        public string DecryptKey { get; set; }
        public string Url { get; set; }
        public string ThumbUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public string PhotoDescription { get; set; }
        public int? Rating { get; set; }
        public UserCondition UserCondition { get; set; }
    }
}
