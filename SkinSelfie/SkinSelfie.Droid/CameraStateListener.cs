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
using Android.Hardware.Camera2;

namespace SkinSelfie.Droid
{
    public class CameraStateListener : CameraDevice.StateCallback
    {
        public Camera2Page Page;

        public override void OnOpened(CameraDevice camera)
        {
            if (Page != null)
            {
                Page.mCameraDevice = camera;
                Page.StartPreview();
                Page.mOpeningCamera = false;
            }
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            if (Page != null)
            {
                camera.Close();
                Page.mCameraDevice = null;
                Page.mOpeningCamera = false;
            }
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            camera.Close();
            if (Page != null)
            {
                Page.mCameraDevice = null;
                Activity activity = Page.Activity;
                Page.mOpeningCamera = false;
                if (activity != null)
                {
                    activity.Finish();
                }
            }

        }
    }
}