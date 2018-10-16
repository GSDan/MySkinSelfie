using System;
using System.Collections.Generic;

namespace SkinSelfie.ServiceData.Models
{
    public class Share : Model
    {
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireDate { get; set; }
        public User Owner { get; set; }
        public UserCondition UserCondition { get; set; }
        public string SharedEmail { get; set; }
        public bool Updated { get; set; }
    }
}
