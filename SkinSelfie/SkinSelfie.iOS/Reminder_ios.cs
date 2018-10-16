using SkinSelfie.iOS;
using SkinSelfie.Interfaces;
using System;
using Xamarin.Forms;
using UIKit;
using Foundation;

[assembly: Dependency(typeof(Reminder_iOS))]
namespace SkinSelfie.iOS
{
	public class Reminder_iOS :  IReminder
	{
		public void CancelAlarm(AppModels.UserCondition condition)
		{
			var notifs = UIApplication.SharedApplication.ScheduledLocalNotifications;

			foreach (UILocalNotification notif in notifs) {
				if (notif.UserInfo != null && notif.UserInfo.ContainsKey (new NSString (condition.Id.ToString ()))) {
					UIApplication.SharedApplication.CancelLocalNotification(notif);
					break;
				}
			}
		}

		private NSCalendarUnit GetCalUnit(long millis)
		{
			NSCalendarUnit unit = NSCalendarUnit.Day;
			long day = 1000 * 60 *60 * 24;

			if(millis > day * 7)
			{
				unit = NSCalendarUnit.Month;
			}
			else if(millis > day)
			{
				unit = NSCalendarUnit.Week;
			}

			return unit;
		}

		public void SetAlarm(AppModels.UserCondition condition)
		{
			long waitTime = condition.ReminderIntervalMillis();

			if (waitTime <= 0) return;

			CancelAlarm (condition);

			UIApplication.SharedApplication.CancelAllLocalNotifications ();

			UILocalNotification notif = new UILocalNotification ();
			notif.FireDate = NSDate.FromTimeIntervalSinceNow(waitTime / 1000);

			notif.RepeatCalendar = NSCalendar.CurrentCalendar;
			notif.RepeatInterval = GetCalUnit (waitTime);
			notif.AlertAction = string.Format("{0} Reminder", condition.Condition);
			notif.AlertBody = string.Format("Remember to take a photo of your condition '{0}'!", condition.Condition);
			notif.UserInfo = NSDictionary.FromObjectAndKey(new NSString(condition.Id.ToString()), new NSString (condition.Id.ToString()));
			notif.ApplicationIconBadgeNumber += 1;
			notif.SoundName = UILocalNotification.DefaultSoundName;

			UIApplication.SharedApplication.ScheduleLocalNotification(notif);
		}
	}
}