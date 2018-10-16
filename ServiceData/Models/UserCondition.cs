using System;
using System.Collections.Generic;

namespace SkinSelfie.ServiceData.Models
{
    public class UserCondition : Model
    {
        public User Owner { get; set; }
        public SkinRegion SkinRegion { get; set; }
        public string Treatment { get; set; }
        public string Condition { get; set; }
        public DateTime StartDate { get; set; }
        public int? Passcode { get; set; }
        public IEnumerable<Photo> Photos { get; set; }
        public bool Finished { get; set; }
    }
}
