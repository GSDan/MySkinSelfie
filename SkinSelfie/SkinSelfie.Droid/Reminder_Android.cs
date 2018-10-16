using Android.App;
using Android.Content;
using Android.OS;
using SkinSelfie.Droid;
using SkinSelfie.Interfaces;
using System;
using Xamarin.Forms;

[assembly: Dependency(typeof(Reminder_Android))]
namespace SkinSelfie.Droid
{
    [BroadcastReceiver]
    public class Reminder_Android : BroadcastReceiver, IReminder
    {
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
                PowerManager.WakeLock w1 = pm.NewWakeLock(WakeLockFlags.Partial, "NotificationReceiver");
                w1.Acquire();

                var pendingIntent = PendingIntent.GetActivity(context, 0, new Intent(context, typeof(MainActivity)), 0);

                Notification.Builder notifBuilder = new Notification.Builder(context)
                    .SetContentIntent(pendingIntent)
                    .SetContentTitle("Skin Selfie Reminder")
                    .SetContentText("Remember to take a photo of your condition '" + intent.GetStringExtra("conditionName") + "'!")
                    .SetSmallIcon(Resource.Drawable.icon)
                    .SetAutoCancel(true);

                Notification notification = notifBuilder.Build();
                NotificationManager notificationManager =
                                context.GetSystemService(Context.NotificationService) as NotificationManager;

                notificationManager.Notify(0, notification);
                w1.Release();
            }
            catch(Exception e)
            {
                Console.WriteLine("ON RECEIVE ERR: " + e.Message);
            }
        }

        public void CancelAlarm(AppModels.UserCondition condition)
        {
            Context context = Xamarin.Forms.Forms.Context;
            AlarmManager am = (AlarmManager)context.GetSystemService(Context.AlarmService);
            Intent intent = new Intent(context, this.Class);
            intent.PutExtra("conditionId", condition.Id);
            intent.PutExtra("conditionName", condition.Condition);

            PendingIntent pi = PendingIntent.GetBroadcast(context, condition.Id, intent, 0);
            am.Cancel(pi);
        }

        public void SetAlarm(AppModels.UserCondition condition)
        {
            long now = SystemClock.ElapsedRealtime();
            Context context = Forms.Context;
            AlarmManager am = (AlarmManager)context.GetSystemService(Context.AlarmService);

            Intent intent = new Intent(context, this.Class);
            intent.PutExtra("conditionId", condition.Id);
            intent.PutExtra("conditionName", condition.Condition);

            long waitTime = condition.ReminderIntervalMillis();

            if (waitTime <= 0) return;

            PendingIntent pi = PendingIntent.GetBroadcast(context, 0, intent, 0);
            am.Cancel(pi);
            am.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, now + waitTime, waitTime, pi);
        }
    }
}