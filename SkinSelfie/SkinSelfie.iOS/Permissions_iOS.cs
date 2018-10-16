using System;
using System.Threading.Tasks;
using AVFoundation;
using SkinSelfie.Interfaces;
using SkinSelfie.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(Permissions_iOS))]
namespace SkinSelfie.iOS
{
    public class Permissions_iOS : IPermissions
	{
		public async Task<bool> GetCameraPermissions()
		{
			var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus (AVMediaType.Video);

			if (authorizationStatus != AVAuthorizationStatus.Authorized) {
				bool success = await AVCaptureDevice.RequestAccessForMediaTypeAsync (AVMediaType.Video);

				return success;
			}

			return true;
		}

        public async Task<bool> GetContactsPermissions()
        {
            Console.WriteLine("Getting book");

			Xamarin.Contacts.AddressBook book = new Xamarin.Contacts.AddressBook();
            bool permission = false;

            Console.WriteLine("Requesting permissions");

            try
            {
                permission = await book.RequestPermission();
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return permission;
        }
    }
}

