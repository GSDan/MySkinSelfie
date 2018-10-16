using System;

namespace SkinSelfie.AppModels
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
    }
}
