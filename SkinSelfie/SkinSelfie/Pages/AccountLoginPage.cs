using Acr.UserDialogs;
using SkinSelfie.AppModels;
using System;

using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class AccountLoginPage : ContentPage
    {
        Entry emailEntry;
        Entry passwordEntry;
        Switch agreeSwitch;

        public AccountLoginPage()
        {
            Title = AppResources.Login_title;

            emailEntry = new Entry
            {
                Placeholder = AppResources.CreateAcc_emailTemp,
                Keyboard = Keyboard.Email
            };

            if(App.user != null && string.IsNullOrWhiteSpace(App.user.Email))
            {
                emailEntry.Text = App.user.Email;
            }

            passwordEntry = new Entry
            {
                Placeholder = AppResources.CreateAcc_passwordTemp,
                IsPassword = true
            };

            Button readPrivacyPolicy = new Button
            {
                Text = AppResources.CreateAcc_btn_privacy
            };
            readPrivacyPolicy.Clicked += ReadPrivacyPolicy_Clicked;

            Button readTerms = new Button
            {
                Text = AppResources.CreateAcc_btn_terms
            };
            readTerms.Clicked += ReadTerms_Clicked; ;

            StackLayout agreeLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            agreeSwitch = new Switch
            {
                IsToggled = false,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End
            };

            agreeLayout.Children.Add(new Label { Text = AppResources.CreateAcc_termsPrivacyCheck, FontSize = 15, HorizontalOptions = LayoutOptions.StartAndExpand });
            agreeLayout.Children.Add(agreeSwitch);


            Button submit = new Button
            {
                Text = AppResources.Login_btn_login
            };
            submit.Clicked += Submit_Clicked;

            Content = new StackLayout
            {
                Children = {
                    new Label { Text = AppResources.CreateAcc_email},
                    emailEntry,
                    new Label { Text = AppResources.CreateAcc_password},
                    passwordEntry,
                    readPrivacyPolicy,
                    readTerms,
                    agreeLayout,
                    submit
                },
                Spacing = 15,
                Padding = new Thickness(20, 20)
            };
        }

        private async void Submit_Clicked(object sender, EventArgs e)
        {
            if (!agreeSwitch.IsToggled)
            {
                await DisplayAlert(AppResources.CreateAcc_confirmTitle, AppResources.CreateAcc_termsPrivacyFail, AppResources.Dialog_understand);
                return;
            }

            if(string.IsNullOrWhiteSpace(emailEntry.Text))
            {
                await DisplayAlert(AppResources.Dialog_error, AppResources.Login_errorNoEmail, AppResources.Dialog_understand);
                return;
            }

            if (string.IsNullOrWhiteSpace(passwordEntry.Text))
            {
                await DisplayAlert(AppResources.CreateAcc_passwordEmptyTitle, AppResources.CreateAcc_passwordEmpty, AppResources.Dialog_understand);
                return;
            }

            TryLogin();
        }

        private async void TryLogin()
        {
            UserDialogs.Instance.ShowLoading(title: AppResources.Dialog_loading);

            // Get api access token
            ServerResponse<AccessToken> res = await App.FetchToken(emailEntry.Text, passwordEntry.Text);

			if (res.Response != null && res.Response.IsSuccessStatusCode)
            {
                // Get user's details
                ServerResponse<User> loginRes = await App.FetchAccount(emailEntry.Text);

                UserDialogs.Instance.HideLoading();

                if(loginRes.Response.IsSuccessStatusCode)
                {
                    await App.ResetNavStackToMain();
                    return;
                }
            }

            UserDialogs.Instance.HideLoading();
            await DisplayAlert(AppResources.Dialog_error, AppResources.Login_error, AppResources.Dialog_understand);
        }

        void ReadPrivacyPolicy_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new PrivacyPolicyPage());
        }

        void ReadTerms_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new TermsOfUsePage());
        }

    }
}
