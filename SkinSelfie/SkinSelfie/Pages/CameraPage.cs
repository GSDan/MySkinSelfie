using Acr.UserDialogs;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace SkinSelfie.Pages
{
    public class CameraPage : ContentPage
    {
        public CameraPage()
        {
            Title = AppResources.Camera_title;
        }

		public static async Task ShowTip()
		{
			// TODO: These mess with the camera screen opening on iOS. 
			// Set up a separate camera tips section?
			/*if (App.user.CameraTipsSeen < 1)
			{
				await UserDialogs.Instance.AlertAsync(new AlertConfig
					{
						Title = AppResources.Camera_tip1_title,
						Message = AppResources.Camera_tip1_message,
						OkText = AppResources.Dialog_understand,
						OnOk = IncrementCamTipsCount
					});
			}
			else if(App.user.CameraTipsSeen < 2 && App.SelectedCondition.Photos.Count > 0)
			{
				await UserDialogs.Instance.AlertAsync(new AlertConfig
					{
                    Title = AppResources.Camera_tip2_title,
                    Message = AppResources.Camera_tip2_message,
                    OkText = AppResources.Dialog_understand,
						OnOk = IncrementCamTipsCount
					});
			}*/
		}

        public static void IncrementCamTipsCount()
        {
            App.user.CameraTipsSeen++;
            App.db.AddUser(App.user);
        }
    }
}
