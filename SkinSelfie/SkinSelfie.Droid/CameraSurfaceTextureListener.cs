
using Android.Views;

namespace SkinSelfie.Droid
{
    public class CameraSurfaceTextureListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
		private Camera2Page Page;
        public CameraSurfaceTextureListener(Camera2Page page)
        {
            Page = page;
        }
        public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            Page.ConfigureTransform(width, height);
			Page.previousPhoto.LayoutParameters.Height = height;
			Page.previousPhoto.RequestLayout ();
            Page.StartPreview();
        }

        public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
        {
			if (Page.mCameraDevice != null) {
				Page.mCameraDevice.Close ();
				Page.mCameraDevice = null;
			}

            return true;
        }

        public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            Page.ConfigureTransform(width, height);
			Page.previousPhoto.LayoutParameters.Height = height;
			Page.previousPhoto.RequestLayout ();
            Page.StartPreview();
        }

        public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface)
        {

        }
    }
}