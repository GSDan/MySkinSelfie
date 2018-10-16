using Newtonsoft.Json;
using SQLite;
using System;

namespace SkinSelfie.AppModels
{
    public class ReminderData
    {
        [PrimaryKey]
        public int UserConditionId{get; set;}
        public int ReminderType { get; set; }
    }

    public class PendingPhotoUpload
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public string PhotoDescription { get; set; }
        public int? Rating { get; set; }
        public string LocalPhotoLoc { get; set; }
        public int UserConditionId { get; set; }
        public string UserConditionJson { get; set; }
    }

    public class ConditionData
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string JsonData { get; set; }
        private UserCondition condition;

        public ConditionData() { }

        public ConditionData(UserCondition con)
        {
            Id = con.Id;
            UpdateCondition(con);
        }

        public UserCondition GetCondition()
        {
            if (condition == null && !string.IsNullOrWhiteSpace(JsonData))
            {
                condition = JsonConvert.DeserializeObject<UserCondition>(JsonData);
            }
            return condition;
        }

        public void UpdateCondition(UserCondition updated)
        {
            condition = updated;
            JsonData = JsonConvert.SerializeObject(updated, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }
}
