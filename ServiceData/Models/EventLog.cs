using SkinSelfie.ServiceData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinSelfie.ServiceData.Models
{
    public class EventLog : Model
    {
        public int UserId { get; set; }
        public string Action { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}
