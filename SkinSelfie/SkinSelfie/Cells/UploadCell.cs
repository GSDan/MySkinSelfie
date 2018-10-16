using FFImageLoading.Forms;
using SkinSelfie.AppModels;
using SkinSelfie.Pages;
using System;

using Xamarin.Forms;

namespace SkinSelfie.Cells
{
    public class UploadCell : ViewCell
    {
        private static readonly int baseSize = 100;
        private static int scaledSize = -1;
        private static Color originalColor;
        private static readonly Color highlightColor = Color.FromHex("6CC6D2");
        private static readonly string buttonDefaultText = AppResources.UploadCell_btnSelect;
        private static readonly string buttonHighlightText = AppResources.UploadCell_btnDeselect;
        private CachedImage thumbImage;
        private Button selectBtn;
        private Label createdDateLabel;
        private Label createdTimeLabel;
        private string filepath;

        public UploadCell()
        {
            createdDateLabel = new Label()
            {
                FontSize = 19,
                LineBreakMode = LineBreakMode.WordWrap
            };

            createdTimeLabel = new Label()
            {
                FontSize = 15,
                TextColor = Color.Gray
            };

            selectBtn = new Button()
            {
                Text = buttonDefaultText,
                FontSize = 13,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            selectBtn.Clicked += Button_Clicked;

            if (scaledSize == -1)
            {
                scaledSize = (int)(baseSize * (App.screenSizeInches / 5));
            }

            thumbImage = new CachedImage()
            {
                HeightRequest = scaledSize,
                WidthRequest = scaledSize,
                BitmapOptimizations = true,
                DownsampleToViewSize = true,
                CacheDuration = TimeSpan.FromDays(30),
                LoadingPlaceholder = "ToggleOverlayButton.png",
                ErrorPlaceholder = "icon.png",
                RetryCount = 3,
                RetryDelay = 250
            };
            thumbImage.Success += ThumbImage_Success;

            StackLayout detailsLayout = new StackLayout()
            {
                Spacing = 0,
                HeightRequest = scaledSize,
                Padding = new Thickness(10, 0, 0, 5),
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Children = { createdDateLabel, createdTimeLabel }
            };

            StackLayout cellLayout = new StackLayout()
            {
                Spacing = 0,
                Padding = new Thickness(10, 5, 10, 5),
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children = { thumbImage, detailsLayout, selectBtn }
            };

            this.View = cellLayout;
        }

        private void ThumbImage_Success(object sender, CachedImageEvents.SuccessEventArgs e)
        {
            filepath = e.ImageInformation.FilePath;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            var photo = ((Button)sender).BindingContext as WrappedCell<PendingPhotoUpload>;

            bool shouldHighlight = UploadsQueuePage.OnCellTapped(this);

            if (!photo.IsSelected && originalColor == null)
            {
                originalColor = View.BackgroundColor;
            }

            photo.IsSelected = shouldHighlight;

            selectBtn.Text = (shouldHighlight) ? buttonHighlightText : buttonDefaultText;

            View.BackgroundColor = (photo.IsSelected) ? highlightColor : originalColor;
        }

        protected override void OnBindingContextChanged()
        {
            var c = (WrappedCell<PendingPhotoUpload>)BindingContext;
            if (c == null) return;

            thumbImage.Source = c.Item.LocalPhotoLoc;
            createdDateLabel.Text = c.Item.CreatedAt.ToString("dd/MM/yyyy");
            createdTimeLabel.Text = c.Item.CreatedAt.ToString("HH:mm");
            base.OnBindingContextChanged();
        }
    }
}
