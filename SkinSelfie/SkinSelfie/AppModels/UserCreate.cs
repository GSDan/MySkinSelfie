using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkinSelfie.AppModels
{
    public class UserCreate
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Username { get; set; }
        public string Grant_type { get; set; }
    }
}
