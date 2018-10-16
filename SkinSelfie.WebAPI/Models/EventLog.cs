using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SkinSelfie.WebAPI.Models
{
    public class EventLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}