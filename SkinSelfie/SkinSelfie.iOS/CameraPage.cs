using System;
using Xamarin.Forms;
using SkinSelfie.Pages;
using AVFoundation;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Foundation;
using CoreGraphics;
using System.Threading.Tasks;
using SkinSelfie.AppModels;
using Acr.UserDialogs;
using System.Net;
using FFImageLoading;

[assembly: ExportRenderer(typeof(CameraPage), typeof(SkinSelfie.iOS.CameraPage))]
namespace SkinSelfie.iOS
{
	public class CameraPage : PageRenderer
	{
		AVCaptureSession captureSession;
		AVCaptureDeviceInput captureDeviceInput;
		UIButton toggleCameraButton;
		UIView liveCameraStream;
		AVCaptureStillImageOutput stillImageOutput;
		UIButton takePhotoButton;
		UISlider slider;
		UIImageView imageView;
		bool isSelfie = false;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			CheckCameraPermissions ();
		}

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);

			UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
		}

		public async void CheckCameraPermissions ()
		{
			try
			{
				var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus (AVMediaType.Video);

				if (authorizationStatus != AVAuthorizationStatus.Authorized) {
					bool success = await AVCaptureDevice.RequestAccessForMediaTypeAsync (AVMediaType.Video);

					if (!success) {
						await UserDialogs.Instance.AlertAsync(new AlertConfig
							{
								Title = "Camera Permissions Required",
								Message = "Please enable camera access in your device's settings"
							});

						Xamarin.Forms.Device.BeginInvokeOnMainThread (() => App.Homepage.Navigation.PopAsync());
						return;
					}
				}

				SetupUserInterface ();
				SetupEventHandlers ();
				SetupLiveCameraStream ();
			}
			catch(Exception e) {
				await UserDialogs.Instance.AlertAsync(new AlertConfig
					{
						Title = "Error",
						Message = e.Message
					});
				await App.Homepage.Navigation.PopAsync();
				return;
			}
		}

		public async void SetupLiveCameraStream ()
		{
			captureSession = new AVCaptureSession ();

			var viewLayer = liveCameraStream.Layer;
			var videoPreviewLayer = new AVCaptureVideoPreviewLayer (captureSession) {
				Frame = liveCameraStream.Bounds
			};
			liveCameraStream.Layer.AddSublayer (videoPreviewLayer);

			var captureDevice = AVCaptureDevice.DefaultDeviceWithMediaType (AVMediaType.Video);
			ConfigureCameraForDevice (captureDevice);
			captureDeviceInput = AVCaptureDeviceInput.FromDevice (captureDevice);

			var dictionary = new NSMutableDictionary();
			dictionary[AVVideo.CodecKey] = new NSNumber((int) AVVideoCodec.JPEG);
			stillImageOutput = new AVCaptureStillImageOutput () {
				OutputSettings = new NSDictionary ()
			};

			captureSession.AddOutput (stillImageOutput);
			captureSession.AddInput (captureDeviceInput);
			captureSession.StartRunning ();

			await SkinSelfie.Pages.CameraPage.ShowTip ();
		}

		public async void CapturePhoto ()
		{
			var videoConnection = stillImageOutput.ConnectionFromMediaType (AVMediaType.Video);
			var sampleBuffer = await stillImageOutput.CaptureStillImageTaskAsync (videoConnection);

			// var jpegImageAsBytes = AVCaptureStillImageOutput.JpegStillToNSData (sampleBuffer).ToArray ();
			var jpegImageAsNsData = AVCaptureStillImageOutput.JpegStillToNSData (sampleBuffer);
			var image = new UIImage (jpegImageAsNsData);

			// We have to rotate the image to portrait, as Apple are weird like that
			var rotated = RotateImage (image);
			var data = rotated.AsJPEG (0.4f).ToArray ();

			// SendPhoto (data);
			SendPhoto (data);
		}

		private UIImage RotateImage(UIImage image)
		{
			CGImage imgRef = image.CGImage;

			int width = (int)imgRef.Width;
			int height = (int)imgRef.Height;

			int kMaxResolution = width > height ? width : height;

			CGAffineTransform transform = CGAffineTransform.MakeIdentity();

			if (isSelfie) {
				transform = CGAffineTransform.Scale(transform, -1, 1);
			}

			CGRect bounds = new CGRect(0, 0, width, height);
			if (width > kMaxResolution || height > kMaxResolution) {
				float ratio = width/height;
				if (ratio > 1) {
					bounds.Size = new CGSize (kMaxResolution, bounds.Size.Width / ratio);
				}
				else {
					bounds.Size = new CGSize (bounds.Size.Height * ratio, kMaxResolution);
				}
			}

			float scaleRatio = (float)bounds.Size.Width / width;
			CGSize imageSize = new CGSize(imgRef.Width, imgRef.Height);
			float boundHeight;
			UIImageOrientation orient = image.Orientation;
			switch(orient) {

				case UIImageOrientation.Up: //EXIF = 1
				transform = CGAffineTransform.MakeIdentity();
					break;

				case UIImageOrientation.UpMirrored: //EXIF = 2
					transform = CGAffineTransform.MakeTranslation(imageSize.Width, 0.0f);
					transform = CGAffineTransform.Scale(transform, -1.0f, 1.0f);
					break;

				case UIImageOrientation.Down: //EXIF = 3
					transform = CGAffineTransform.MakeTranslation(imageSize.Width, imageSize.Height);
				transform = CGAffineTransform.Rotate(transform, (float)Math.PI);
					break;

				case UIImageOrientation.DownMirrored: //EXIF = 4
					transform = CGAffineTransform.MakeTranslation(0.0f, imageSize.Height);
					transform = CGAffineTransform.Scale(transform, 1.0f, -1.0f);
					break;

				case UIImageOrientation.LeftMirrored: //EXIF = 5
					boundHeight = (float)bounds.Size.Height;
					bounds.Size = new CGSize (boundHeight, bounds.Size.Width);
					transform = CGAffineTransform.MakeTranslation(imageSize.Height, imageSize.Width);
					transform = CGAffineTransform.Scale(transform, -1.0f, 1.0f);
					transform = CGAffineTransform.Rotate(transform, (float)(3.0f * Math.PI / 2.0f));
					break;

				case UIImageOrientation.Left: //EXIF = 6
					boundHeight = (float)bounds.Size.Height;
					bounds.Size = new CGSize (boundHeight, bounds.Size.Width);
					transform = CGAffineTransform.MakeTranslation(0.0f, imageSize.Width);
				transform = CGAffineTransform.Rotate(transform, (float)(3.0f * Math.PI / 2.0f));
					break;

				case UIImageOrientation.RightMirrored: //EXIF = 7
					boundHeight = (float)bounds.Size.Height;
					bounds.Size = new CGSize (boundHeight, bounds.Size.Width);
					transform = CGAffineTransform.MakeScale(-1.0f, 1.0f);
				transform = CGAffineTransform.Rotate(transform, (float)(Math.PI / 2.0f));
					break;

				case UIImageOrientation.Right: //EXIF = 8
					boundHeight = (float)bounds.Size.Height;
					bounds.Size = new CGSize (boundHeight, bounds.Size.Width);
					transform = CGAffineTransform.MakeTranslation(imageSize.Height, 0.0f);
				transform = CGAffineTransform.Rotate(transform, (float)(Math.PI / 2.0f));
					break;

			}

			UIGraphics.BeginImageContext(bounds.Size);

			CGContext context = UIGraphics.GetCurrentContext();

			if (orient == UIImageOrientation.Right || orient == UIImageOrientation.Left) {
				context.ScaleCTM(-scaleRatio, scaleRatio);
				context.TranslateCTM(-height, 0);
			}
			else {
				context.ScaleCTM(scaleRatio, -scaleRatio);
				context.TranslateCTM(0, -height);
			}

			context.ConcatCTM(transform);

			context.DrawImage(new CGRect(0, 0, width, height), imgRef);
			UIImage imageCopy = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			return imageCopy;
		}

		public void ToggleFrontBackCamera ()
		{
			var devicePosition = captureDeviceInput.Device.Position;
			if (devicePosition == AVCaptureDevicePosition.Front) {
				devicePosition = AVCaptureDevicePosition.Back;
				isSelfie = false;
			} else {
				devicePosition = AVCaptureDevicePosition.Front;
				isSelfie = true;
			}

			var device = GetCameraForOrientation (devicePosition);
			ConfigureCameraForDevice (device);

			captureSession.BeginConfiguration ();
			captureSession.RemoveInput (captureDeviceInput);
			captureDeviceInput = AVCaptureDeviceInput.FromDevice (device);
			captureSession.AddInput (captureDeviceInput);
			captureSession.CommitConfiguration ();
		}

		public void ConfigureCameraForDevice (AVCaptureDevice device)
		{
			var error = new NSError ();
			if (device.IsFocusModeSupported (AVCaptureFocusMode.ContinuousAutoFocus)) {
				device.LockForConfiguration (out error);
				device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
				device.UnlockForConfiguration ();
			} else if (device.IsExposureModeSupported (AVCaptureExposureMode.ContinuousAutoExposure)) {
				device.LockForConfiguration (out error);
				device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
				device.UnlockForConfiguration ();
			} else if (device.IsWhiteBalanceModeSupported (AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance)) {
				device.LockForConfiguration (out error);
				device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
				device.UnlockForConfiguration ();
			}
		}

		public AVCaptureDevice GetCameraForOrientation (AVCaptureDevicePosition orientation)
		{
			var devices = AVCaptureDevice.DevicesWithMediaType (AVMediaType.Video);

			foreach (var device in devices) {
				if (device.Position == orientation) {
					return device;
				}
			}

			return null;
		}

		private void SetupUserInterface ()
		{
			// Get window's UI view -- avoid using the permission popup's
			UIWindow window = UIApplication.SharedApplication.KeyWindow;
			UIView mView = window.RootViewController.View;

			var centerButtonX = View.Bounds.GetMidX () - 35f;
			var topLeftX = View.Bounds.X + 25;
			var topRightX = View.Bounds.Right - 65;
			var bottomButtonY = View.Bounds.Bottom - 150;
			var topButtonY = View.Bounds.Top + 15;
			var buttonWidth = 70;
			var buttonHeight = 70;

			liveCameraStream = new UIView () {
				Frame = new CGRect (0f, 0f, View.Bounds.Width, View.Bounds.Height)
			};

			slider = new UISlider(new CGRect(topLeftX, topButtonY, 210, 20));
			slider.MinValue = 0f;
			slider.MaxValue = 1f;
			slider.Hidden = true;
			slider.Layer.ZPosition = 1;

			if(App.SelectedCondition.Photos != null && App.SelectedCondition.Photos.Count > 0)
			{
				LoadPreviousPhoto(App.SelectedCondition.Photos[App.SelectedCondition.Photos.Count - 1]);
			}

			takePhotoButton = new UIButton () {
				Frame = new CGRect (centerButtonX, bottomButtonY, buttonWidth, buttonHeight)
			};
			takePhotoButton.SetBackgroundImage (UIImage.FromFile ("TakePhotoButton.png"), UIControlState.Normal);
			takePhotoButton.Layer.ZPosition = 1;

			toggleCameraButton = new UIButton () {
				Frame = new CGRect (topRightX, topButtonY + 5, 35, 26)
			};
			toggleCameraButton.SetBackgroundImage (UIImage.FromFile ("ToggleCameraButton.png"), UIControlState.Normal);
			toggleCameraButton.Layer.ZPosition = 1;



			View.Add (liveCameraStream);
			View.Add (slider);
			View.Add (takePhotoButton);
			View.Add (toggleCameraButton);
		}

		private void LoadPreviousPhoto(Photo prevPhoto)
		{
			try
			{
				imageView = new UIImageView ();
				ImageService.Instance.LoadUrl(prevPhoto.GetReqUrl(false)).Into(imageView);
				imageView.Frame = new CoreGraphics.CGRect (0, 0, View.Bounds.Width, View.Bounds.Height);
				imageView.Alpha = 0.28f;
				imageView.Layer.ZPosition = 0.5f;
				imageView.ContentMode = UIViewContentMode.ScaleAspectFill;
				View.Add(imageView);

				slider.Value = (float)imageView.Alpha;
				slider.ValueChanged += (object sender, EventArgs e) => {
					imageView.Alpha = slider.Value;
				};
				slider.Hidden = false;
			}
			catch(Exception e) {
				Console.WriteLine (e.Message);
			}

		}

	private void SetupEventHandlers ()
		{
			takePhotoButton.TouchUpInside += (object sender, EventArgs e) =>  {
				CapturePhoto ();
			};

			toggleCameraButton.TouchUpInside += (object sender, EventArgs e) =>  {
				ToggleFrontBackCamera ();
			};
		}

		public async Task SendPhoto (byte[] imageBytes)
		{
			Photo photo = new Photo
			{
				InMemory = imageBytes,
				CreatedAt = DateTime.Now,
				UserCondition = App.SelectedCondition
			};

			var navigationPage = new PhotoEditPage(photo);

			await App.Current.MainPage.Navigation.PopAsync();
			await App.Current.MainPage.Navigation.PushAsync(navigationPage);
		}
	}
}

