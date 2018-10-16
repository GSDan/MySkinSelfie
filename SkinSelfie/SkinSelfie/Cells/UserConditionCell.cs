using Acr.UserDialogs;
using FFImageLoading;
using FFImageLoading.Forms;
using SkinSelfie.AppModels;
using SkinSelfie.Pages;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SkinSelfie.Cells
{
    public class UserConditionCell : StackLayout
    {
        public UserCondition Condition;
        private Image ShareButton;

        public UserConditionCell(UserCondition condition)
        {
            RelativeLayout iconLayout = new RelativeLayout
            {
                HeightRequest = 160,
                WidthRequest = 160,
                Padding = new Thickness(0),
                BackgroundColor = Color.FromHex("00BCD4")
            };

            if (condition.Passcode == null && condition.Photos != null && condition.Photos.Count > 0)
            {
                CachedImage photo = new CachedImage()
                {
                    Aspect = Aspect.AspectFill,
                    Source = condition.Photos[0].GetReqUrl(true),
                    BitmapOptimizations = true,
                    DownsampleToViewSize = true,
                    CacheDuration = TimeSpan.FromDays(30),
                    FadeAnimationEnabled = true
                };

                iconLayout.Children.Add(
                    photo,
                    Constraint.Constant(0),
                    Constraint.Constant(0),
                    Constraint.RelativeToParent(parent => parent.Width),
                    Constraint.RelativeToParent(parent => parent.Height)
                );
            }

            BoxView shadeBox = new BoxView
            {
                Color = Color.FromRgba(0, 0, 0, 140),
                InputTransparent = true
            };
            iconLayout.Children.Add(
                shadeBox,
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Height * 0.25),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height * 0.5)
                );

            int fontSize = 19;

            if (condition.Condition.Length > 30)
            {
                fontSize = 15;
            }
            else if (condition.Condition.Length > 20)
            {
                fontSize = 17;
            }

            var nameReference = new Label
            {
                FontSize = fontSize,
                LineBreakMode = LineBreakMode.WordWrap,
                Text = condition.Condition,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                Opacity = 0
            };

            Label nameLabel = new Label()
            {
                FontSize = fontSize,
                LineBreakMode = LineBreakMode.WordWrap,
                Text = condition.Condition,
                HorizontalTextAlignment = TextAlignment.Center,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.White
            };

            var centerX = Constraint.RelativeToParent(parent => parent.Width / 2);
            var centerY = Constraint.RelativeToParent(parent => parent.Height / 2);

            // Have to have a hacky centre reference point
            // https://forums.xamarin.com/discussion/22902/how-to-add-a-label-to-a-relative-layout-and-center-it-horizontally
            iconLayout.Children.Add(nameReference, centerX, centerY);

            iconLayout.Children.Add(
                nameLabel,
                Constraint.RelativeToView(nameReference, (parent, sibling) => sibling.X - sibling.Width / 2),
                Constraint.RelativeToView(nameReference, (parent, sibling) => sibling.Y - sibling.Height * 0.8)
                );

            Label regionLabel = new Label()
            {
                FontSize = 15,
                Text = condition.SkinRegion.DisplayName,
                TextColor = Color.White
            };

            Label regionReference = new Label()
            {
                FontSize = 15,
                Text = condition.SkinRegion.DisplayName,
                Opacity = 0
            };

            iconLayout.Children.Add(
                regionReference,
                centerX,
                Constraint.RelativeToView(nameLabel, (parent, sibling) => sibling.Y + sibling.Height * .8 + 20)
                );
            iconLayout.Children.Add(
                regionLabel,
                Constraint.RelativeToView(regionReference, (parent, sibling) => sibling.X - sibling.Width / 2),
                Constraint.RelativeToView(regionReference, (parent, sibling) => sibling.Y - sibling.Height / 2)
                );

            StackLayout countLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                Padding = new Thickness(10, 5),
                Children =
                {
                    new Label()
                    {
                        FontSize = 15,
                        TextColor = Color.White,
                        FontAttributes = FontAttributes.Bold,
                        Text = condition.Photos.Count.ToString(),
                        HorizontalOptions = LayoutOptions.EndAndExpand,
                        VerticalOptions = LayoutOptions.EndAndExpand,
                        HorizontalTextAlignment = TextAlignment.End
                    }
                }
            };

            iconLayout.Children.Add(
                countLayout,
                Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent(parent => parent.Width),
                Constraint.RelativeToParent(parent => parent.Height)
                );

            StackLayout detailsLayout = new StackLayout()
            {
                Spacing = 0,
                Padding = new Thickness(10),
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                BackgroundColor = Color.White
            };

            if (condition.Passcode != null)
            {
                detailsLayout.Children.Add(new Image
                {
                    Source = "ic_lock_outline_black_18dp",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.CenterAndExpand
                });
            }

            ShareButton = new Image
            {
                Source = "ic_share_black_18dp",
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };

            TapGestureRecognizer shareRecog = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            shareRecog.Tapped += ShareRecog_Tapped;
            ShareButton.GestureRecognizers.Add(shareRecog);

            detailsLayout.Children.Add(ShareButton);

            TapGestureRecognizer tapRecog = new TapGestureRecognizer { NumberOfTapsRequired = 1 };
            tapRecog.Tapped += TapRecog_Tapped;
            iconLayout.GestureRecognizers.Add(tapRecog);

            Condition = condition;
            Spacing = 0;
            Padding = new Thickness(3);
            Orientation = StackOrientation.Vertical;
            HorizontalOptions = LayoutOptions.CenterAndExpand;
            Children.Add(iconLayout);
            Children.Add(detailsLayout);
            BackgroundColor = Color.FromRgba(221, 221, 221, 255);
        }

        private void ShareRecog_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new NewSharePage(Condition));
        }

        private async void TapRecog_Tapped(object sender, EventArgs e)
        {
            if (Condition.Passcode != null)
            {
                MessagingCenter.Subscribe<UnlockPage, KeyValuePair<int, string>>(this, "Unlock-Condition", (senderPage, vals) => {
                    // Unlock after double checking details
                    if (vals.Key != Condition.Id || vals.Value != Condition.Passcode.ToString())
                    {
                        return;
                    }

                    MessagingCenter.Unsubscribe<UnlockPage, KeyValuePair<int, string>>(this, "Unlock-Condition");

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.SelectedCondition = Condition;
                        Navigation.PushAsync(new GalleryPage(Condition));
                    });
                });

                await Navigation.PushAsync(new UnlockPage(false, true, Condition.Passcode.ToString(), Condition.Id)
                {
                    Title = "Enter Condition's PIN"
                });
                return;
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                App.SelectedCondition = Condition;
                Navigation.PushAsync(new GalleryPage(Condition));
            });
        }
    }
}
