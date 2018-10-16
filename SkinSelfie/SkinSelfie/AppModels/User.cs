using SQLite;
using System;
using System.Collections.Generic;

namespace SkinSelfie.AppModels
{
    public class User
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public string AppCode { get; set; }
        public bool CodePrompted { get; set; }
        public DateTime FirstStart { get; set; }
        public int CameraTipsSeen { get; set; }
        public bool GDPRConsent { get; set; }
        public AccessToken Token;

        public List<UserCondition> Conditions;
    }
}
