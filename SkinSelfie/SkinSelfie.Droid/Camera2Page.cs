using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Views;
using SkinSelfie.AppModels;
using SkinSelfie.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


/*
Uses code from the open source Xamarin project 'Moments'
https://github.com/pierceboggan/Moments/blob/master/Moments%20-%20CSharp/Moments.Droid/Pages/CameraPage.cs
*/
[assembly: ExportRenderer(typeof(CameraPage), typeof(SkinSelfie.Droid.Camera2Page))]
namespace SkinSelfie.Droid
{
    /*
	 * https://github.com/xamarin/monodroid-samples/tree/master/android5.0/Camera2Basic
	 */
    public class Camera2Page : PageRenderer, SeekBar.IOnSeekBarChangeListener, Android.Hardware.Camera.IPictureCallback, Android.Hardware.Camera.IAutoFocusCallback, TextureView.ISurfaceTextureListener
    {

        // Fields for Camera2 upgrade
        private static readonly SparseIntArray ORIENTATIONS = new SparseIntArray();
        private CaptureRequest.Builder mPreviewBuilder;
        private CameraCaptureSession mPreviewSession;
        public CameraDevice mCameraDevice;
        private Android.Util.Size mPreviewSize;
        private CameraSurfaceTextureListener mSurfaceTextureListener;
        private CameraStateListener mStateListener;

        // Camera1 fields
#pragma warning disable CS0618 // Type or member is obsolete
        Android.Hardware.Camera camera;
        bool camEnabled = false;
        SurfaceTexture surfaceTexture;
        SurfaceTexture surface;
        TextureView textureView1;
        CameraFacing cameraType;
        bool cameraClosed = false;
        bool takePicture = false;
#pragma warning restore CS0618 // Type or member is obsolete

        private Android.Widget.ProgressBar spinner;
        private Surface previewSurface;
        private SeekBar OverlaySeekbar;
        public bool mOpeningCamera;

        global::Android.Widget.Button takePhotoButton;
        global::Android.Widget.Button switchCameraButton;

        public int CamPermReqId = 8675309;

        public MainActivity Activity;
        public ImageViewAsync previousPhoto;
        AutoFitTextureView textureView;
        global::Android.Views.View view;

        private bool overlayLoaded = false;
        private bool overlayOn = false;
        private bool backCam = true;

        private byte[] imageBytes;
        private int width;
        private int height;

        public Camera2Page()
        {
            Activity = this.Context as MainActivity;
            Activity.CamPage = this;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                view = Activity.LayoutInflater.Inflate(Resource.Layout.CameraLayout, this, false);
            }
            else
            {
                view = Activity.LayoutInflater.Inflate(Resource.Layout.CameraLayoutOld, this, false);
            }

            previousPhoto = view.FindViewById<ImageViewAsync>(Resource.Id.previousPhoto);

            OverlaySeekbar = view.FindViewById<SeekBar>(Resource.Id.overlaySeekbar);

            OverlaySeekbar.SetOnSeekBarChangeListener(this);

            if (App.SelectedCondition.Photos != null && App.SelectedCondition.Photos.Count > 0)
            {
                ShowHidePreviousPhoto(App.SelectedCondition.Photos[App.SelectedCondition.Photos.Count - 1]);
            }
            OverlaySeekbar.Visibility = (App.SelectedCondition.Photos.Count > 0) ? ViewStates.Visible : ViewStates.Gone;

            takePhotoButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.takePhotoButton);
            takePhotoButton.Click += TakePictureTapped;
            takePhotoButton.Enabled = false;

            switchCameraButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.switchCameraButton);
            switchCameraButton.Click += SwitchCameraButton_Click;
            switchCameraButton.Enabled = false;
            switchCameraButton.Visibility = ViewStates.Gone;

            spinner = view.FindViewById<Android.Widget.ProgressBar>(Resource.Id.progressBar1);
            spinner.Indeterminate = true;
            spinner.Visibility = ViewStates.Gone;

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                mStateListener = new CameraStateListener() { Page = this };
                mSurfaceTextureListener = new CameraSurfaceTextureListener(this);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation0, 90);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation90, 0);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation180, 270);
                ORIENTATIONS.Append((int)SurfaceOrientation.Rotation270, 180);

                textureView = view.FindViewById<AutoFitTextureView>(Resource.Id.textureView);
                textureView.SurfaceTextureListener = mSurfaceTextureListener;

                string[] camPermissions = { Android.Manifest.Permission.Camera };

                // Make sure we can use the camera
                if ((int)Android.OS.Build.VERSION.SdkInt >= 23 && Activity.CheckSelfPermission(camPermissions[0]) != (Permission.Granted))
                {
                    Toast.MakeText(Activity, AppResources.Camera_permissionNeeded, ToastLength.Long).Show();
                    Activity.RequestPermissions(camPermissions, CamPermReqId);
                }
                else
                {
                    OpenCamera();
                }
            }
            else
            {
                cameraType = CameraFacing.Back;

                textureView1 = view.FindViewById<TextureView>(Resource.Id.textureView);
                textureView1.SurfaceTextureListener = this;
            }

            AddView(view);
        }

        private void SwitchCameraButton_Click(object sender, EventArgs e)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                string[] camPermissions = { Android.Manifest.Permission.Camera };

                // Make sure we can use the camera
                if ((int)Android.OS.Build.VERSION.SdkInt >= 23 && Activity.CheckSelfPermission(camPermissions[0]) != (Permission.Granted))
                {
                    Toast.MakeText(Activity, AppResources.Camera_permissionNeeded, ToastLength.Long).Show();
                    Activity.RequestPermissions(camPermissions, CamPermReqId);
                    return;
                }

                mCameraDevice.Close();

                int newCamIndex = backCam ? 1 : 0;
                backCam = !backCam;

                try
                {
                    OpenCamera(newCamIndex);
                }
                catch (System.Exception ex)
                {
                    System.Console.WriteLine(ex);
                }
            }
            else
            {
                if (cameraType == CameraFacing.Front)
                {
                    cameraType = CameraFacing.Back;

                    camera.StopPreview();
                    camera.Release();
                    camera = Android.Hardware.Camera.Open((int)cameraType);
                    camera.SetPreviewTexture(surfaceTexture);
                    PrepareAndStartCamera1();
                }
                else
                {
                    cameraType = CameraFacing.Front;

                    camera.StopPreview();
                    camera.Release();
                    camera = Android.Hardware.Camera.Open((int)cameraType);
                    camera.SetPreviewTexture(surfaceTexture);
                    PrepareAndStartCamera1();
                }
            }
        }

        private void TakePictureTapped(object sender, EventArgs e)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
            {
                TakePicture();
            }
            else
            {
                takePicture = true;
                UserDialogs.Instance.ShowLoading(title: "Please hold still...");

                if (takePicture)
                {
                    takePicture = false;
                    camera.TakePicture(null, null, this);
                }

                //camera.AutoFocus(this);
            }
        }

        /// <summary>
        /// Open the camera. Camera API used depends on the device's sdk version num
        /// </summary>
        /// <param name="cameraIndex"></param>
		private void OpenCamera(int cameraIndex = 0)
        {
            Activity activity = Activity;
            if (activity == null || activity.IsFinishing || mOpeningCamera)
            {
                return;
            }
            mOpeningCamera = true;

            try
            {
                // Camera2 API is only supported after Lollipop, but newer devices may not support Camera1
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
                {
                    CameraManager manager = (CameraManager)activity.GetSystemService(Android.Content.Context.CameraService);

                    string[] idList = manager.GetCameraIdList();

                    string cameraId = idList[cameraIndex];

                    switchCameraButton.Enabled = idList.Length > 1;
                    switchCameraButton.Visibility = idList.Length > 1 ? ViewStates.Visible : ViewStates.Gone;

                    // The onion skin has to be repositioned for the front facing camera
                    if (cameraIndex == 1)
                    {
                        previousPhoto.SetScaleType(ImageView.ScaleType.FitStart);
                    }
                    else
                    {
                        previousPhoto.SetScaleType(ImageView.ScaleType.CenterCrop);
                    }

                    CameraCharacteristics characteristics = manager.GetCameraCharacteristics(cameraId);

                    StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                    mPreviewSize = map.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture)))[0];
                    Android.Content.Res.Orientation orientation = Resources.Configuration.Orientation;
                    textureView.SetAspectRatio(mPreviewSize.Height, mPreviewSize.Width);

                    manager.OpenCamera(cameraId, mStateListener, null);
                }
                else
                {
                    camera = Android.Hardware.Camera.Open(cameraIndex);
                    camEnabled = true;
                    textureView1.LayoutParameters = new FrameLayout.LayoutParams(width, height, GravityFlags.Center);

                    if (cameraType == CameraFacing.Back)
                    {
                        previousPhoto.LayoutParameters = new FrameLayout.LayoutParams(width, height, GravityFlags.Fill);
                    }
                    else
                    {
                        previousPhoto.LayoutParameters = new FrameLayout.LayoutParams(width, height, GravityFlags.Top);
                    }

                    surfaceTexture = surface;
                    camera.SetPreviewTexture(surface);

                    takePhotoButton.Enabled = true;
                    switchCameraButton.Enabled = true;
                    switchCameraButton.Visibility = ViewStates.Visible;

                    PrepareAndStartCamera1();
                }
            }
            catch (CameraAccessException ex)
            {
                Toast.MakeText(activity, AppResources.Camera_noAccess, ToastLength.Short).Show();
                Activity.Finish();
            }
        }

        #region Camera1Only
#pragma warning disable CS0618 // Type or member is obsolete
        private void PrepareAndStartCamera1()
        {
            if (camera == null)
            {
                Toast.MakeText(Activity, AppResources.Camera_noAccess, ToastLength.Long).Show();
                App.Current.MainPage.Navigation.PopAsync();
                return;
            }

            camera.StopPreview();

            UpdateCameraAspect();

            var display = Activity.WindowManager.DefaultDisplay;
            if (display.Rotation == SurfaceOrientation.Rotation0)
            {
                camera.SetDisplayOrientation(90);
            }

            if (display.Rotation == SurfaceOrientation.Rotation270)
            {
                camera.SetDisplayOrientation(180);
            }

            camera.StartPreview();
        }

        private void UpdateCameraAspect()
        {
            try
            {

                Android.Hardware.Camera.Parameters camParams = camera.GetParameters();
                Android.Hardware.Camera.CameraInfo info = new Android.Hardware.Camera.CameraInfo();
                Android.Hardware.Camera.GetCameraInfo((int)Android.Hardware.CameraFacing.Back, info);

                Android.Hardware.Camera.Size size = GetOptimalPreviewSize(camParams.SupportedPreviewSizes, width, height);

                camParams.SetPreviewSize(size.Width, size.Height);

                int rotation = (info.Orientation + 360) % 360;

                camParams.SetRotation(rotation);

                if (camParams.SupportedFocusModes.Contains(Android.Hardware.Camera.Parameters.FocusModeContinuousPicture))
                {
                    camParams.FocusMode = Android.Hardware.Camera.Parameters.FocusModeContinuousPicture;
                }

                camera.SetParameters(camParams);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

        private Android.Hardware.Camera.Size GetOptimalPreviewSize(IList<Android.Hardware.Camera.Size> sizes, int w, int h)
        {
            const double ASPECT_TOLERANCE = 0.05;
            double targetRatio = (double)w / h;
            int maxSize = 1228800 * 2;

            if (sizes == null)
                return null;

            Android.Hardware.Camera.Size optimalSize = null;
            double minDiff = System.Double.MaxValue;

            int targetHeight = h;

            // Try to find an size match aspect ratio and size
            foreach (Android.Hardware.Camera.Size size in sizes)
            {
                double ratio = (double)size.Height / size.Width;

                if (size.Height * size.Width > maxSize)
                    continue;

                if (System.Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE)
                    continue;

                if (System.Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = System.Math.Abs(size.Height - targetHeight);
                }
            }

            // Cannot find the one match the aspect ratio, ignore the requirement
            if (optimalSize == null)
            {
                minDiff = System.Double.MaxValue;
                foreach (Android.Hardware.Camera.Size size in sizes)
                {
                    if (size.Height * size.Width > maxSize)
                        continue;

                    if (System.Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = System.Math.Abs(size.Height - targetHeight);
                    }
                }
            }
            return optimalSize;
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            if (camEnabled)
            {
                camera.StopPreview();
                camera.Release();
                cameraClosed = true;
            }
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            this.width = width;
            this.height = height;
            PrepareAndStartCamera1();
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            if (camEnabled && cameraClosed)
            {
                cameraClosed = false;
                PrepareAndStartCamera1();
            }
        }

        private void TextureView1_SurfaceTextureAvailable(object sender, TextureView.SurfaceTextureAvailableEventArgs e)
        {
            this.width = e.Width;
            this.height = e.Height;
            this.surface = e.Surface;

            string[] camPermissions = { Android.Manifest.Permission.Camera };

            OpenCamera();
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.surface = surface;

            string[] camPermissions = { Android.Manifest.Permission.Camera };

            OpenCamera();
        }

        public async void OnPictureTaken(byte[] data, Android.Hardware.Camera camera)
        {
            Bitmap bitmapImage = BitmapFactory.DecodeByteArray(data, 0, data.Length);

            // compare to a decent sized photo
            float scale = (bitmapImage.Height * bitmapImage.Width) / 1228800;

            // If the image is significantly bigger than this, scale it down!
            if (scale > 1.2f)
            {
                int newWidth = (int)(bitmapImage.Width / (scale / 2));
                int newHeight = (int)(bitmapImage.Height / (scale / 2));
                bitmapImage = Bitmap.CreateScaledBitmap(bitmapImage, newWidth, newHeight, false);
            }

            if (cameraType == CameraFacing.Back)
            {
                bitmapImage = rotateImage(bitmapImage, 90);
            }
            else
            {
                bitmapImage = rotateImage(bitmapImage, -90);
            }

            using (var imageStream = new MemoryStream())
            {
                await bitmapImage.CompressAsync(Bitmap.CompressFormat.Jpeg, 60, imageStream);
                bitmapImage.Recycle();
                imageBytes = imageStream.ToArray();
            }

            Photo photo = new Photo
            {
                InMemory = imageBytes,
                CreatedAt = DateTime.Now,
                UserCondition = App.SelectedCondition
            };

            var navigationPage = new PhotoEditPage(photo);

            UserDialogs.Instance.HideLoading();
            //camera.Release();
            await App.Current.MainPage.Navigation.PopAsync();
            await App.Current.MainPage.Navigation.PushAsync(navigationPage);
        }

        public void OnAutoFocus(bool success, Android.Hardware.Camera camera)
        {
            if (takePicture)
            {
                takePicture = false;
                camera.TakePicture(null, null, this);
            }
        }

        private Bitmap rotateImage(Bitmap bitmap, int degree)
        {
            int w = bitmap.Width;
            int h = bitmap.Height;

            Matrix mtx = new Matrix();
            mtx.PostRotate(degree);

            return Bitmap.CreateBitmap(bitmap, 0, 0, w, h, mtx, true);
        }
#pragma warning restore CS0618 // Type or member is obsolete
        #endregion

        public void StartPreview()
        {
            if (mCameraDevice == null || !textureView.IsAvailable || mPreviewSize == null)
            {
                return;
            }
            try
            {
                SurfaceTexture texture = textureView.SurfaceTexture;
                System.Diagnostics.Debug.Assert(texture != null);

                // We configure the size of the default buffer to be the size of the camera preview we want
                texture.SetDefaultBufferSize(mPreviewSize.Width, mPreviewSize.Height);

                // This is the output Surface we need to start the preview
                previewSurface = new Surface(texture);

                // We set up a CaptureRequest.Builder with the output Surface
                mPreviewBuilder = mCameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
                mPreviewBuilder.AddTarget(previewSurface);

                // Here, we create a CameraCaptureSession for camera preview.
                mCameraDevice.CreateCaptureSession(new List<Surface>() { previewSurface },
                    new CameraCaptureStateListener()
                    {
                        OnConfigureFailedAction = (CameraCaptureSession session) =>
                        {
                            Activity activity = Activity;
                            if (activity != null)
                            {
                                Toast.MakeText(activity, "Failed", ToastLength.Short).Show();
                            }
                        },
                        OnConfiguredAction = (CameraCaptureSession session) =>
                        {
                            mPreviewSession = session;
                            UpdatePreview();
                        }
                    },
                    null);

                takePhotoButton.Enabled = true;
            }
            catch (CameraAccessException ex)
            {
                Log.WriteLine(LogPriority.Info, "Camera2BasicFragment", ex.StackTrace);
            }
        }

        /// <summary>
		/// Updates the camera preview, StartPreview() needs to be called in advance
		/// </summary>
		private void UpdatePreview()
        {
            if (mCameraDevice == null)
            {
                return;
            }

            try
            {
                // The camera preview can be run in a background thread. This is a Handler for the camere preview
                SetUpCaptureRequestBuilder(mPreviewBuilder);
                HandlerThread thread = new HandlerThread("CameraPreview");
                thread.Start();
                Handler backgroundHandler = new Handler(thread.Looper);

                // Finally, we start displaying the camera preview
                mPreviewSession.SetRepeatingRequest(mPreviewBuilder.Build(), null, backgroundHandler);
            }
            catch (CameraAccessException ex)
            {
                Log.WriteLine(LogPriority.Info, "Camera2BasicFragment", ex.StackTrace);
            }
        }

        private void TakePicture()
        {
            try
            {
                Activity activity = Activity;
                if (activity == null || mCameraDevice == null)
                {
                    return;
                }
                CameraManager manager = (CameraManager)activity.GetSystemService(Android.Content.Context.CameraService);

                spinner.Visibility = ViewStates.Visible;

                // Pick the best JPEG size that can be captures with this CameraDevice
                CameraCharacteristics characteristics = manager.GetCameraCharacteristics(mCameraDevice.Id);
                Android.Util.Size[] jpegSizes = null;
                if (characteristics != null)
                {
                    jpegSizes = ((StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap)).GetOutputSizes((int)ImageFormatType.Jpeg);
                }

                width = 640;
                height = 480;
                int total = width * height;
                int targetRes = 3145728; //About 5MP
                int currentTargetDiff = System.Math.Abs((width * height) - targetRes); ;

                if (jpegSizes != null && jpegSizes.Length > 0)
                {
                    foreach (Android.Util.Size size in jpegSizes)
                    {
                        int targetDiff = System.Math.Abs((size.Width * size.Height) - targetRes);
                        if (targetDiff < currentTargetDiff)
                        {
                            width = size.Width;
                            height = size.Height;
                            currentTargetDiff = targetDiff;
                        }
                    }
                }

                // We use an ImageReader to get a JPEG from CameraDevice
                // Here, we create a new ImageReader and prepare its Surface as an output from the camera
                ImageReader reader = ImageReader.NewInstance(width, height, ImageFormatType.Jpeg, 1);
                List<Surface> outputSurfaces = new List<Surface>();
                outputSurfaces.Add(reader.Surface);
                //outputSurfaces.Add(new Surface(textureView.SurfaceTexture));

                CaptureRequest.Builder captureBuilder = mCameraDevice.CreateCaptureRequest(CameraTemplate.StillCapture);
                captureBuilder.AddTarget(reader.Surface);
                SetUpCaptureRequestBuilder(captureBuilder);
                // Orientation
                SurfaceOrientation rotation = activity.WindowManager.DefaultDisplay.Rotation;
                captureBuilder.Set(CaptureRequest.JpegOrientation, new Java.Lang.Integer(ORIENTATIONS.Get((int)rotation)));

                Java.IO.File file = new Java.IO.File(activity.GetExternalFilesDir(null), "pic.jpg");

                // This listener is called when an image is ready in ImageReader 
                // Right click on ImageAvailableListener in your IDE and go to its definition
                ImageAvailableListener readerListener = new ImageAvailableListener(this);

                // We create a Handler since we want to handle the resulting JPEG in a background thread
                HandlerThread thread = new HandlerThread("CameraPicture");
                thread.Start();
                Handler backgroundHandler = new Handler(thread.Looper);
                reader.SetOnImageAvailableListener(readerListener, backgroundHandler);

                //This listener is called when the capture is completed
                // Note that the JPEG data is not available in this listener, but in the ImageAvailableListener we created above
                // Right click on CameraCaptureListener in your IDE and go to its definition
                CameraCaptureListener captureListener = new CameraCaptureListener() { Page = this };


                mCameraDevice.CreateCaptureSession(outputSurfaces, new CameraCaptureStateListener()
                {
                    OnConfiguredAction = (CameraCaptureSession session) => {
                        try
                        {
                            CaptureRequest capReq = captureBuilder.Build();
                            session.Capture(capReq, captureListener, backgroundHandler);
                        }
                        catch (CameraAccessException ex)
                        {
                            Log.WriteLine(LogPriority.Info, "Capture Session error: ", ex.ToString());
                        }
                    },
                    OnConfigureFailedAction = (CameraCaptureSession session) => {
                        Log.WriteLine(LogPriority.Error, "Session setup error", session.ToString());
                    }
                }, backgroundHandler);
            }
            catch (CameraAccessException ex)
            {
                Log.WriteLine(LogPriority.Info, "Taking picture error: ", ex.StackTrace);
            }
        }

        /// <summary>
		/// Sets up capture request builder.
		/// </summary>
		private void SetUpCaptureRequestBuilder(CaptureRequest.Builder builder)
        {
            // In this sample, w just let the camera device pick the automatic settings
            builder.Set(CaptureRequest.ControlMode, new Java.Lang.Integer((int)ControlMode.Auto));
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            previousPhoto.ImageAlpha = (int)(255 * 0.01 * progress);
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
        }

        public void PermissionsResultCallback([GeneratedEnum] Permission[] grantResults)
        {
            if (grantResults[0] == Permission.Granted)
            {
                OpenCamera();
            }
            else
            {
                PermissionsRefused();
            }
        }

        private async void PermissionsRefused()
        {
            await UserDialogs.Instance.AlertAsync(new AlertConfig
            {
                Title = AppResources.Oops,
                Message = AppResources.Camera_permissionMessage
            });
            await App.Current.MainPage.Navigation.PopAsync();
        }

        private async void ShowHidePreviousPhoto(Photo prev)
        {
            try
            {
                if (!overlayOn)
                {
                    if (!overlayLoaded)
                    {
                        await ImageService.Instance.LoadUrl(prev.GetReqUrl(false)).IntoAsync(previousPhoto);
                        overlayLoaded = true;
                    }

                    previousPhoto.ImageAlpha = 100;
                    previousPhoto.Visibility = ViewStates.Visible;

                }
                else
                {
                    previousPhoto.ImageAlpha = 0;
                    previousPhoto.Visibility = ViewStates.Invisible;
                }

                OverlaySeekbar.Progress = previousPhoto.ImageAlpha / (255 / 100);

                overlayOn = !overlayOn;
            }
            catch
            {
                Toast.MakeText(this.Activity, AppResources.Camera_overlayErr, ToastLength.Long).Show();
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            view.Measure(msw, msh);
            view.Layout(0, 0, r - l, b - t);
        }

        /// <summary>
		/// Configures the necessary transformation to mTextureView.
		/// This method should be called after the camera preciew size is determined in openCamera, and also the size of mTextureView is fixed
		/// </summary>
		/// <param name="viewWidth">The width of mTextureView</param>
		/// <param name="viewHeight">VThe height of mTextureView</param>
		public void ConfigureTransform(int viewWidth, int viewHeight)
        {
            Activity activity = Activity;
            if (textureView == null || mPreviewSize == null || activity == null)
            {
                return;
            }

            SurfaceOrientation rotation = activity.WindowManager.DefaultDisplay.Rotation;
            Matrix matrix = new Matrix();
            RectF viewRect = new RectF(0, 0, viewWidth, viewHeight);
            RectF bufferRect = new RectF(0, 0, mPreviewSize.Width, mPreviewSize.Height);
            float centerX = viewRect.CenterX();
            float centerY = viewRect.CenterY();
            if (rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270)
            {
                bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
                matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
                float scale = System.Math.Max((float)viewHeight / mPreviewSize.Height, (float)viewWidth / mPreviewSize.Width);
                matrix.PostScale(scale, scale, centerX, centerY);
                matrix.PostRotate(90 * ((int)rotation - 2), centerX, centerY);
            }
            textureView.SetTransform(matrix);
        }

        public async void OnPictureTaken(byte[] data)
        {
            try
            {

                Bitmap bitmapImage = BitmapFactory.DecodeByteArray(data, 0, data.Length);
                data = null;

                if (!backCam)
                {
                    Matrix matrix = new Matrix();
                    matrix.PreScale(-1.0f, 1.0f);

                    bitmapImage = Bitmap.CreateBitmap(bitmapImage, 0, 0, bitmapImage.Width, bitmapImage.Width, matrix, false);
                }

                using (var imageStream = new MemoryStream())
                {
                    await bitmapImage.CompressAsync(Bitmap.CompressFormat.Jpeg, 80, imageStream);
                    bitmapImage.Recycle();
                    imageBytes = imageStream.ToArray();
                }

                Photo photo = new Photo
                {
                    InMemory = imageBytes,
                    CreatedAt = DateTime.Now,
                    UserCondition = App.SelectedCondition
                };

                var navigationPage = new PhotoEditPage(photo);

                Activity.RunOnUiThread(() => {
                    App.Current.MainPage.Navigation.PopAsync();
                    App.Current.MainPage.Navigation.PushAsync(navigationPage);
                });

            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e);
            }

        }

    }
}