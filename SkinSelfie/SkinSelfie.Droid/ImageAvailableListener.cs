using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;
using Java.Nio;
using Java.IO;
using Android.Util;

namespace SkinSelfie.Droid
{
    public class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
		Camera2Page Page;

		public ImageAvailableListener(Camera2Page page)
		{
			Page = page;
		}

        public void OnImageAvailable(ImageReader reader)
        {
            Image image = null;
            try
            {
                image = reader.AcquireLatestImage();
                ByteBuffer buffer = image.GetPlanes()[0].Buffer;
                byte[] bytes = new byte[buffer.Capacity()];
                buffer.Get(bytes);
				image.Close();
				image = null;

				if(Page.mCameraDevice != null)
				{
					Page.mCameraDevice.Close();
					Page.mCameraDevice = null;
				}
				Page.OnPictureTaken(bytes);
            }
            catch (FileNotFoundException ex)
            {
                Log.WriteLine(LogPriority.Info, "Camera capture session", ex.StackTrace);
            }
            catch (IOException ex)
            {
                Log.WriteLine(LogPriority.Info, "Camera capture session", ex.StackTrace);
            }
            finally
            {
                if (image != null)
                    image.Close();
            }
        }
    }
}