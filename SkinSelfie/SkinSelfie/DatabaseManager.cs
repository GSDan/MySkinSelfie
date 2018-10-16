using SkinSelfie.AppModels;
using SkinSelfie.Interfaces;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace SkinSelfie
{
    public class DatabaseManager
    {
        private SQLiteConnection connection;

        public DatabaseManager()
        {
            connection = DependencyService.Get<ISQLite>().GetConnection();
            connection.CreateTable<User>();
            connection.CreateTable<AccessToken>();
            connection.CreateTable<ReminderData>();
            connection.CreateTable<ConditionData>();
            connection.CreateTable<PendingPhotoUpload>();
            connection.CreateTable<BodyPart>();
        }

        public void UpdateBodyParts(List<BodyPart> data)
        {
            connection.DropTable<BodyPart>();
            connection.CreateTable<BodyPart>();
            
            foreach(BodyPart b in data)
            {
                b.UpdateJson();
                connection.Insert(b);
            }
        }

        public IEnumerable<BodyPart> GetBodyParts()
        {
            return (from t in connection.Table<BodyPart>()
                    select t).ToList();
        }

        public void AddOrUpdateCondition(ConditionData data)
        {
            connection.InsertOrReplace(data);
        }

        public void DeleteCondition(int conditionId)
        {
            connection.Delete<ConditionData>(conditionId);
        }

        public ConditionData GetCondition(int id)
        {
            return connection.Table<ConditionData>().Where(r => r.Id == id).FirstOrDefault();
        }

        public IEnumerable<ConditionData> GetConditions()
        {
            return (from t in connection.Table<ConditionData>()
                    select t).ToList();
        }

        public IEnumerable<AccessToken> GetTokens()
        {
            return (from t in connection.Table<AccessToken>()
                    select t).ToList();
        }

        public AccessToken GetToken()
        {
            return connection.Table<AccessToken>().FirstOrDefault();
        }

        public User GetUser()
        {
            return connection.Table<User>().FirstOrDefault();
        }

        public void DeleteToken(int id)
        {
            connection.Delete<AccessToken>(id);
        }

        public void DeleteUser(int id)
        {
            connection.Delete<User>(id);
        }

        public void AddUser(User data)
        {
            connection.DeleteAll<User>();
            connection.Insert(data);
        }

        public void AddToken(AccessToken data)
        {
            connection.DeleteAll<AccessToken>();
            connection.Insert(data);
        }

        public void AddOrUpdateReminder(ReminderData data)
        {
            connection.InsertOrReplace(data);
        }

        public void DeleteReminder(int userConditionId)
        {
            connection.Delete<ReminderData>(userConditionId);
        }

        public ReminderData GetReminder(int conditionId)
        {
            return connection.Table<ReminderData>().Where(r => r.UserConditionId == conditionId).FirstOrDefault();
        }

        public void AddUpload(PendingPhotoUpload data)
        {
            connection.InsertOrReplace(data);
        }

        public void DeleteUpload(int id)
        {
            connection.Delete<PendingPhotoUpload>(id);
        }

        public PendingPhotoUpload GetUpload(int id)
        {
            return connection.Table<PendingPhotoUpload>().Where(r => r.Id == id).FirstOrDefault();
        }

        public IEnumerable<PendingPhotoUpload> GetUploads()
        {
            return (from t in connection.Table<PendingPhotoUpload>()
                    select t).ToList();
        }

        public IEnumerable<PendingPhotoUpload> GetUploadsForCondition(int conditionId)
        {
            return connection.Table<PendingPhotoUpload>().Where(u => u.UserConditionId == conditionId).ToList();
        }

        public void DeleteUploadsForCondition(int conditionId)
        {
            IEnumerable<PendingPhotoUpload> matches = GetUploadsForCondition(conditionId);

            if (matches == null) return;

            foreach(PendingPhotoUpload u in matches)
            {
                connection.Delete<PendingPhotoUpload>(u.Id);
            }
        }
    }
}
