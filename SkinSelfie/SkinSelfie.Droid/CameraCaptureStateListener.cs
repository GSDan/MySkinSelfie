using System;
using Android.Hardware.Camera2;

namespace SkinSelfie.Droid
{
    // This CameraCaptureSession.StateListener uses Action delegates to allow the methods to be defined inline, as they are defined more than once
    public class CameraCaptureStateListener : CameraCaptureSession.StateCallback
    {
        public Action<CameraCaptureSession> OnConfigureFailedAction;
        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            if (OnConfigureFailedAction != null)
            {
                OnConfigureFailedAction(session);
            }
        }

        public Action<CameraCaptureSession> OnConfiguredAction;
        public override void OnConfigured(CameraCaptureSession session)
        {
            if (OnConfiguredAction != null)
            {
                OnConfiguredAction(session);
            }
        }

    }
}