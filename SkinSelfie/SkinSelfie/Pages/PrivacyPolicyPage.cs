using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class PrivacyPolicyPage : ContentPage
    {
        public PrivacyPolicyPage()
        {
            Title = AppResources.PrivacyPolicy_title;

            ScrollView scroller = new ScrollView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Content = new StackLayout
                {
                    Padding = new Thickness(20),
                    Spacing = 30,
                    Children = {
                        new Image { Source = "icon_badge.png", HorizontalOptions = LayoutOptions.Center, WidthRequest = 120},
                        new Label { Text = AppResources.PrivacyPolicy_title, FontAttributes= FontAttributes.Bold, FontSize=25},
                        new Label { Text = AppResources.PrivacyPolicy_intro, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },
                        new Label { Text = AppResources.PrivacyPolicy_KeyPointsTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.PrivacyPolicy_KeyPoints, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },
                        new Label { Text = AppResources.PrivacyPolicy_FullTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.PrivacyPolicy_Full, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 }
                    }
                }
            };

            Content = scroller;
        }
    }
}
