using System;
using System.Collections.Generic;

namespace SkinSelfie.AppModels
{
    public class UserCondition
    {
        public enum ReminderType {None=0, Daily=1, Weekly=2, Monthly=3}

        public int Id { get; set; }
        public User Owner { get; set; }
        public SkinRegion SkinRegion { get; set; }
        public string Treatment { get; set; }
        public string Condition { get; set; }
        public DateTime StartDate { get; set; }
        public int? Passcode { get; set; }
        public List<Photo> Photos { get; set; }
        public bool Finished { get; set; }
        public ReminderType reminder = ReminderType.None;

        public long ReminderIntervalMillis()
        {
            long dayInterval = 1000 * 60 * 60 * 24;

            switch(reminder)
            {
                case ReminderType.Daily:
                    return dayInterval;
                case ReminderType.Weekly:
                    return dayInterval * 7;
                case ReminderType.Monthly:
                    return dayInterval * 30;
                default:
                    return 0;
            }
        }
    }
}
