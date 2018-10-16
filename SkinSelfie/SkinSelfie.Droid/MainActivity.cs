using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using FFImageLoading.Forms.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XLabs.Ioc;
using XLabs.Platform.Device;
using XLabs.Platform.Services;

namespace SkinSelfie.Droid
{
    [Activity(Label = "MySkinSelfie", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        public Camera2Page CamPage;

        protected override void OnCreate(Bundle bundle)
        {
            //Window.RequestFeature(WindowFeatures.ActionBar);
            base.OnCreate(bundle);
            Forms.Init(this, bundle);

            FormsAppCompatActivity.ToolbarResource = Resource.Layout.toolbar;
            FormsAppCompatActivity.TabLayoutResource = Resource.Layout.tabs;

            #region Resolver Init
            SimpleContainer container = new SimpleContainer();
            container.Register<IDevice>(t => AndroidDevice.CurrentDevice);
            container.Register<IDisplay>(t => t.Resolve<IDevice>().Display);
            container.Register<INetwork>(t => t.Resolve<IDevice>().Network);

            Resolver.SetResolver(container.GetResolver());
            #endregion

            CachedImageRenderer.Init();

            UserDialogs.Init(this);

            LoadApplication(new App());
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            LockRotation(Orientation.Portrait);
        }

        // This feels hacky, but not sure how else to do it as Forms apps run in a single activity
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (CamPage != null && requestCode == CamPage.CamPermReqId)
            {
                CamPage.PermissionsResultCallback(grantResults);
            }
            else if (requestCode == Permissions_Android.RequestCode)
            {
                Permissions_Android.PermReady = true;
            }
            else
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }

        private void LockRotation(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Portrait:
                    RequestedOrientation = ScreenOrientation.Portrait;
                    break;
                case Orientation.Landscape:
                    RequestedOrientation = ScreenOrientation.Landscape;
                    break;
            }
        }
    }
}

