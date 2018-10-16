using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using FFImageLoading.Forms.Touch;
using Foundation;
using UIKit;
using XLabs.Ioc;
using XLabs.Platform.Device;
using XLabs.Platform.Services;

namespace SkinSelfie.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {

			#region Resolver Init
			SimpleContainer container = new SimpleContainer();
			container.Register<IDevice>(t => AppleDevice.CurrentDevice);
			container.Register<IDisplay>(t => t.Resolve<IDevice>().Display);
			container.Register<INetwork>(t => t.Resolve<IDevice>().Network);

			Resolver.SetResolver(container.GetResolver());
			#endregion

            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");

			CachedImageRenderer.Init();
        }
    }
}
