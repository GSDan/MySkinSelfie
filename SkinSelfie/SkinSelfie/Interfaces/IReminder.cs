namespace SkinSelfie.Interfaces
{
    public interface IReminder
    {
        void CancelAlarm(AppModels.UserCondition condition);
        void SetAlarm(AppModels.UserCondition condition);
    }
}
