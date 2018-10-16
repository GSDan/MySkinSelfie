using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class DisclaimerModal : ContentPage
    {
        public DisclaimerModal()
        {

            Button agreeBtn = new Button
            {
                Text = AppResources.Disclaimer_btn_agree,
                FontSize = 18
            };
            agreeBtn.Clicked += AgreeBtn_Clicked;

            Content = new ScrollView
            {
                Padding = new Thickness(15),
                Content = new StackLayout
                {
                    Children = {
						new Label { Text = AppResources.Disclaimer_thanks, FontAttributes = FontAttributes.Bold, FontSize = 23, HorizontalTextAlignment = TextAlignment.Center },
                        new Label { Text = AppResources.Disclaimer_imageControl, FontSize = 18 },
                        new Label { Text = AppResources.Disclaimer_appIntent, FontSize = 18 },
                        new Label { Text = AppResources.Disclaimer_notTreatment, FontAttributes = FontAttributes.Bold, FontSize = 18 },
                        agreeBtn
                    },
                    Spacing = 10
                }
                
            };
        }

        private void AgreeBtn_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        // User must agree to continue
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
