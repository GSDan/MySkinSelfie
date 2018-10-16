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
using Java.IO;
using Android.Media;

namespace SkinSelfie.Droid
{
    public class CameraCaptureListener : CameraCaptureSession.CaptureCallback
    {
        public Camera2Page Page;
        public File File;
        public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request, TotalCaptureResult result)
        {
            MediaActionSound shutterSound = new MediaActionSound();
            shutterSound.Play(MediaActionSoundType.ShutterClick);
        }
    }
}