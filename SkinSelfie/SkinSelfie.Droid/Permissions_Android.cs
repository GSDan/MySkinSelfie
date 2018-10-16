using System;
using SkinSelfie.Interfaces;
using SkinSelfie.Droid;
using Xamarin.Forms;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;

[assembly: Dependency(typeof(Permissions_Android))]
namespace SkinSelfie.Droid
{
	public class Permissions_Android : IPermissions
	{
		public static bool PermReady = false;
		public static readonly int RequestCode = 101271093;

        public async Task<bool> GetCameraPermissions()
        {
            return await GetPermission(new string[] { Android.Manifest.Permission.Camera });
        }

        public async Task<bool> GetContactsPermissions()
        {
            return await GetPermission(new string[] { Android.Manifest.Permission.ReadContacts });
        }

        private async Task<bool> GetPermission(string[] perms)
        {
            MainActivity appMain = Xamarin.Forms.Forms.Context as MainActivity;

            if ((int)Android.OS.Build.VERSION.SdkInt < 23)
            {
                return true;
            }
            else if (appMain.CheckSelfPermission(perms[0]) != (Permission.Granted))
            {
                PermReady = false;
                appMain.RequestPermissions(perms, RequestCode);

                while (!PermReady)
                {
                    await Task.Delay(100);
                }

                return appMain.CheckSelfPermission(perms[0]) == Permission.Granted;
            }

            return true;
        }

    }
}

