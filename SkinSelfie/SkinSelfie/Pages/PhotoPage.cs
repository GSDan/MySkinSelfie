using FFImageLoading.Forms;
using SkinSelfie.AppModels;
using System;
using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class PhotoPage : ContentPage
    {
        private PhotoCarouselPage thisParent;
        private bool initialized = false;
        private int thisIndex = -1;
        private CachedImage theImage;

        public PhotoPage(PhotoCarouselPage parentPage)
        {
            thisParent = parentPage;
            thisParent.CurrentPageChanged += ThisParent_CurrentPageChanged;
        }

        private void ThisParent_CurrentPageChanged(object sender, EventArgs e)
        {
            if (thisIndex == -1)
            {
                thisIndex = thisParent.Children.IndexOf(this);
            }

            int difference = Math.Abs(thisParent.selectedIndex - thisIndex);

            if (difference <= 3 && !initialized)
            {
                InitialisePage();
            }
            //else if (difference > 2 && initialized)
            //{
            //    initialized = false;
            //    Content = null;
            //    toCollect++;

            //    if (toCollect > 3)
            //    {
            //        GC.Collect();
            //        toCollect = 0;
            //    }
            //}
        }

        public void InitialisePage()
        {
            this.SetBinding(TitleProperty, new Binding("CreatedAt", stringFormat: "{0:dd/MM/yyyy}"));

            StackLayout layout = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            ScrollView scroller = new ScrollView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            RelativeLayout content = new RelativeLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            theImage = new CachedImage
            {
                Aspect = Aspect.AspectFill,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                HeightRequest = App.display.HeightRequestInInches(App.screenHeightInches * 0.8),
                BitmapOptimizations = true,
                DownsampleToViewSize = true,
                LoadingPlaceholder = "ToggleOverlayButton.png",
                ErrorPlaceholder = "icon.png",
                FadeAnimationEnabled = true,
                CacheDuration = TimeSpan.FromDays(30),
                Source = ((Photo)BindingContext).GetReqUrl(false),
                RetryCount = 3,
                RetryDelay = 250
            };

            content.Children.Add(theImage, Constraint.Constant(0), Constraint.Constant(0), Constraint.RelativeToParent((parent) => {
                return (parent.Width);
            }));

            Label dateLabel = new Label { FontAttributes = FontAttributes.Bold, FontSize = 24 };
            dateLabel.SetBinding(Label.TextProperty, new Binding("CreatedAt", stringFormat: "{0:dd/MM/yyyy HH:mm}"));

            Label treatmentLabel = new Label { FontSize = 20 };
            treatmentLabel.SetBinding(Label.TextProperty, "Treatment");

            Label notesLabel = new Label { FontSize = 20 };
            notesLabel.SetBinding(Label.TextProperty, "Notes");

            Label descLabel = new Label { FontSize = 20 };
            descLabel.SetBinding(Label.TextProperty, "PhotoDescription");

            StackLayout details = new StackLayout
            {
                Padding = 20,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Fill,
                Spacing = 12,
                Children =
                {
                    dateLabel,
                    new Label { Text = AppResources.PhotoEdit_treatment, FontAttributes = FontAttributes.Bold, FontSize = 20 },
                    treatmentLabel,
                    new Label { Text = AppResources.PhotoEdit_notes, FontAttributes = FontAttributes.Bold, FontSize = 20 },
                    notesLabel,
                    new Label { Text = AppResources.PhotoEdit_shotConditions, FontAttributes = FontAttributes.Bold, FontSize = 20 },
                    descLabel
                }
            };
            content.Children.Add(details, Constraint.Constant(0),
                Constraint.RelativeToView(theImage, (parent, view) =>
                {
                    return view.Y + view.Height + 10;
                }));

            scroller.Content = content;

            // Windows phone doesn't have an action bar, so we need to add the page title as a label
            if (Device.OS == TargetPlatform.WinPhone)
            {
                ContentView titleView = new ContentView { Padding = 15 };
                titleView.Content = new Label { Text = Title };
                layout.Children.Add(titleView);
            }

            layout.Children.Add(scroller);
            Content = layout;
            initialized = true;
        }
    }
}
