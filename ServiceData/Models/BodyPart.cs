using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinSelfie.ServiceData.Models
{
    public class BodyPart : Model
    {
        public string Name { get; set; }
        public IEnumerable<SkinRegion> SkinRegions { get; set; }
    }
}
