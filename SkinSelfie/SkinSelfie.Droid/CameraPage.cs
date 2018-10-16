using System;
using System.IO;
using Android.App;
using Android.Graphics;
using Android.Hardware;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using SkinSelfie.Pages;
using Camera = Android.Hardware.Camera;
using System.Collections.Generic;
using Acr.UserDialogs;
using SkinSelfie.AppModels;
using System.Net;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Runtime;

/*
Uses code from the open source Xamarin project 'Moments'
https://github.com/pierceboggan/Moments/blob/master/Moments%20-%20CSharp/Moments.Droid/Pages/CameraPage.cs
*/

//[assembly: ExportRenderer(typeof(CameraPage), typeof(SkinSelfie.Droid.CameraPage))]
namespace SkinSelfie.Droid
{
    /*
	 * Display Camera Stream: http://developer.xamarin.com/recipes/android/other_ux/textureview/display_a_stream_from_the_camera/
	 * Camera Rotation: http://stackoverflow.com/questions/3841122/android-camera-preview-is-sideways
	 */
    public class CameraPage : PageRenderer, TextureView.ISurfaceTextureListener, Camera.IPictureCallback, Camera.IAutoFocusCallback
    {
        Camera camera;
        global::Android.Widget.Button takePhotoButton;
        global::Android.Widget.Button switchCameraButton;
        global::Android.Widget.Button toggleOverlayButton;

        public int CamPermReqId = 8675309;

        public MainActivity activity;
        CameraFacing cameraType;
        TextureView textureView;
        ImageView previousPhoto;
        SurfaceTexture surfaceTexture;
        global::Android.Views.View view;

        bool takePicture = false;
        bool cameraClosed = false;
        bool overlayLoaded = false;
        bool overlayOn = false;
		bool camEnabled = false;

        byte[] imageBytes;
        SurfaceTexture surface;
        int width;
        int height;

        public CameraPage()
        {
            activity = this.Context as MainActivity;

			view = activity.LayoutInflater.Inflate(Resource.Layout.CameraLayoutOld, this, false);
            cameraType = CameraFacing.Back;

            textureView = view.FindViewById<TextureView>(Resource.Id.textureView);
            textureView.SurfaceTextureListener = this;

            previousPhoto = view.FindViewById<ImageView>(Resource.Id.previousPhoto);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
                return;

            try
            {
                if (App.SelectedCondition.Photos != null && App.SelectedCondition.Photos.Count > 0)
                {
                    ShowHidePreviousPhoto(App.SelectedCondition.Photos[App.SelectedCondition.Photos.Count - 1].Url);
                }

                takePhotoButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.takePhotoButton);
                takePhotoButton.Click += TakePhotoButtonTapped;
                takePhotoButton.Enabled = false;

                switchCameraButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.switchCameraButton);
                switchCameraButton.Click += SwitchCameraButtonTapped;
                switchCameraButton.Enabled = false;

                toggleOverlayButton = view.FindViewById<Android.Widget.Button>(Resource.Id.toggleOverlayButton);
                toggleOverlayButton.Click += ToggleOverlayButton_Click;
				toggleOverlayButton.Enabled = App.SelectedCondition.Photos.Count > 0;
				toggleOverlayButton.Visibility = (App.SelectedCondition.Photos.Count > 0)? ViewStates.Visible : ViewStates.Gone;

                AddView(view);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);//Xamarin.Insights.Report(ex);
            }
        }

        private void ToggleOverlayButton_Click(object sender, EventArgs e)
        {
            ShowHidePreviousPhoto(App.SelectedCondition.Photos[App.SelectedCondition.Photos.Count - 1].Url);
        }

        public void PermissionsResultCallback([GeneratedEnum] Permission[] grantResults)
        {
            if(grantResults[0] == Permission.Granted)
            {
                StartCamPrep();
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

        private async void ShowHidePreviousPhoto(string url)
        {
			try
			{
				if(!overlayOn)
				{
					if(!overlayLoaded)
					{
						Bitmap imageBitmap = null;

						using (var webClient = new WebClient())
						{
							var imageBytes = await webClient.DownloadDataTaskAsync(url);
							if (imageBytes != null && imageBytes.Length > 0)
							{
								imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
							}
						}
						previousPhoto.SetImageBitmap(imageBitmap);
						overlayLoaded = true;
					}

					previousPhoto.ImageAlpha = 100;
					previousPhoto.Visibility = ViewStates.Visible;

                    if (cameraType == CameraFacing.Back)
                    {
                        previousPhoto.LayoutParameters = new FrameLayout.LayoutParams(width, height, GravityFlags.Fill);
                    }
                    else
                    {
                        previousPhoto.LayoutParameters = new FrameLayout.LayoutParams(width, height, GravityFlags.Top);
                    }

                }
				else
				{
					previousPhoto.ImageAlpha = 0;
					previousPhoto.Visibility = ViewStates.Invisible;
				}

				overlayOn = !overlayOn;
			}
			catch
			{
				Toast.MakeText (this.activity, AppResources.Camera_overlayErr, ToastLength.Long).Show ();
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

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            try
            {
                this.width = width;
                this.height = height;
                this.surface = surface;

                string[] camPermissions = { Android.Manifest.Permission.Camera };

				// Make sure we can use the camera
				if ((int)Android.OS.Build.VERSION.SdkInt >= 23 && activity.CheckSelfPermission(camPermissions[0]) != (Permission.Granted))
                {
                    Toast.MakeText(activity, AppResources.Camera_permissionNeeded, ToastLength.Long).Show();
                    activity.RequestPermissions(camPermissions, CamPermReqId);
                }
                else
                {
                    StartCamPrep();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartCamPrep()
        {
            camera = Camera.Open((int)cameraType);
			camEnabled = true;
            textureView.LayoutParameters = new FrameLayout.LayoutParams(width, height, GravityFlags.Center);

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

            PrepareAndStartCamera();
        }

        private void UpdateCameraAspect()
        {
            try
            {
                Camera.Parameters camParams = camera.GetParameters();
                Camera.CameraInfo info = new Camera.CameraInfo();
                Camera.GetCameraInfo((int)Android.Hardware.CameraFacing.Back, info);

                Camera.Size size = GetOptimalPreviewSize(camParams.SupportedPreviewSizes, width, height);

                camParams.SetPreviewSize(size.Width, size.Height);

                int rotation = (info.Orientation + 360) % 360;

                camParams.SetRotation(rotation);

                if (camParams.SupportedFocusModes.Contains(Camera.Parameters.FocusModeContinuousPicture))
                {
                    camParams.FocusMode = Camera.Parameters.FocusModeContinuousPicture;
                }

                camera.SetParameters(camParams);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private Camera.Size GetOptimalPreviewSize(IList<Camera.Size> sizes, int w, int h)
        {
            const double ASPECT_TOLERANCE = 0.05;
            double targetRatio = (double)w / h;
            int maxSize = 1228800 * 2;

            if (sizes == null)
                return null;

            Camera.Size optimalSize = null;
            double minDiff = Double.MaxValue;

            int targetHeight = h;

            // Try to find an size match aspect ratio and size
            foreach (Camera.Size size in sizes)
            {
                double ratio = (double)size.Height / size.Width;

                if (size.Height * size.Width > maxSize)
                    continue;

                if (Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE)
                    continue;

                if (Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = Math.Abs(size.Height - targetHeight);
                }
            }

            // Cannot find the one match the aspect ratio, ignore the requirement
            if (optimalSize == null)
            {
                minDiff = Double.MaxValue;
                foreach (Camera.Size size in sizes)
                {
                    if (size.Height * size.Width > maxSize)
                        continue;

                    if (Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = Math.Abs(size.Height - targetHeight);
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
            PrepareAndStartCamera();
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            if (camEnabled && cameraClosed)
            {
                cameraClosed = false;
                PrepareAndStartCamera();
            }
        }

        private void PrepareAndStartCamera()
        {
            if (camera == null)
            {
                Toast.MakeText(activity, AppResources.Camera_noAccess, ToastLength.Long).Show();
                App.Current.MainPage.Navigation.PopAsync();
                return;
            }

            camera.StopPreview();

            UpdateCameraAspect();

            var display = activity.WindowManager.DefaultDisplay;
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

        private void SwitchCameraButtonTapped(object sender, EventArgs e)
        {
            if (cameraType == CameraFacing.Front)
            {
                cameraType = CameraFacing.Back;

                camera.StopPreview();
                camera.Release();
                camera = Camera.Open((int)cameraType);
                camera.SetPreviewTexture(surfaceTexture);
                PrepareAndStartCamera();
            }
            else
            {
                cameraType = CameraFacing.Front;

                camera.StopPreview();
                camera.Release();
                camera = Camera.Open((int)cameraType);
                camera.SetPreviewTexture(surfaceTexture);
                PrepareAndStartCamera();
            }
        }

        private void TakePhotoButtonTapped(object sender, EventArgs e)
        {
            takePicture = true;
            UserDialogs.Instance.ShowLoading(title: "Please hold still...");
            camera.AutoFocus(this);
        }

        public async void OnPictureTaken(byte[] data, Camera camera)
        {
			Bitmap bitmapImage = BitmapFactory.DecodeByteArray(data, 0, data.Length);

            // compare to a decent sized photo
            float scale = (bitmapImage.Height * bitmapImage.Width) / 1228800;

            // If the image is significantly bigger than this, scale it down!
            if (scale > 1.2f)
            {
                int newWidth = (int)(bitmapImage.Width / (scale/2));
                int newHeight = (int)(bitmapImage.Height / (scale/2));
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

        private Bitmap rotateImage(Bitmap bitmap, int degree)
        {
            int w = bitmap.Width;
            int h = bitmap.Height;

            Matrix mtx = new Matrix();
            mtx.PostRotate(degree);

            return Bitmap.CreateBitmap(bitmap, 0, 0, w, h, mtx, true);
        }

        public void OnAutoFocus(bool success, Camera camera)
        {
            if (takePicture)
            {
                takePicture = false;
                camera.TakePicture(null, null, this);
            }
        }
    }
}