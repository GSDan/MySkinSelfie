using Acr.UserDialogs;
using SkinSelfie.AppModels;
using System;
using System.Linq;

using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class AccountCreationPage : ContentPage
    {
        Entry emailEntry;
        Entry nameEntry;
        Entry passwordEntry;
        Entry confirmPasswordEntry;
        Switch agreeSwitch;

        public AccountCreationPage()
        {
            Title = AppResources.CreateAcc_title;

            emailEntry = new Entry
            {
                Placeholder = AppResources.CreateAcc_emailTemp,
                Keyboard = Keyboard.Email
            };

            nameEntry = new Entry
            {
                Placeholder = AppResources.CreateAcc_nameTemp
            };

            passwordEntry = new Entry
            {
                Placeholder = AppResources.CreateAcc_passwordTemp,
                IsPassword = true
            };

            confirmPasswordEntry = new Entry
            {
                Placeholder = AppResources.CreateAcc_passwordConfirm,
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
            readTerms.Clicked += ReadTerms_Clicked;;

            Button submit = new Button
            {
                Text = AppResources.CreateAcc_btn_submit
            };
            submit.Clicked += Submit_Clicked;

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

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Children = {
                        new Label { Text = AppResources.CreateAcc_email},
                        emailEntry,
                        new Label { Text = AppResources.CreateAcc_name},
                        nameEntry,
                        new Label { Text = AppResources.CreateAcc_password},
                        passwordEntry,
                        confirmPasswordEntry,
                        readPrivacyPolicy,
                        readTerms,
                        agreeLayout,
                        submit
                    },
                    Spacing = 15,
                    Padding = new Thickness(20, 20)
                }
            };
        }

        private async void Submit_Clicked(object sender, EventArgs e)
        {
            if (!agreeSwitch.IsToggled)
            {
                await DisplayAlert(AppResources.CreateAcc_confirmTitle, AppResources.CreateAcc_termsPrivacyFail, AppResources.Dialog_understand);
                return;
            }

			bool agreed = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
				{
					Title = AppResources.CreateAcc_confirmTitle,
					Message = AppResources.CreateAcc_confirmMessage,
                    OkText = AppResources.Dialog_agree,
					CancelText = AppResources.Dialog_cancel
            });

			if (agreed) {
				Create();
			}
        }

        private async void Create()
        {
            if (string.IsNullOrWhiteSpace(passwordEntry.Text))
            {
                await DisplayAlert(AppResources.CreateAcc_passwordEmptyTitle, AppResources.CreateAcc_passwordEmpty, AppResources.Dialog_understand);
                return;
            }

            if(passwordEntry.Text != confirmPasswordEntry.Text)
            {
                await DisplayAlert(AppResources.CreateAcc_passwordMismatchTitle, AppResources.CreateAcc_passwordMismatchMessage, AppResources.Dialog_understand);
                return;
            }

            User user = new User
            {
                Name = nameEntry.Text,
                Email = emailEntry.Text,
                BirthDate = DateTime.UtcNow
            };

            UserCreate createData = new UserCreate
            {
                Password = passwordEntry.Text,
                ConfirmPassword = confirmPasswordEntry.Text,
                Email = user.Email,
                Username = user.Email
            };

            UserDialogs.Instance.ShowLoading(title: AppResources.Dialog_loading);
            ServerResponse<object> res = await App.CreateAccount(createData, user);

            UserDialogs.Instance.HideLoading();

			if(res.Response != null && res.Response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    AppResources.CreateAcc_successTitle, 
                    string.Format(AppResources.CreateAcc_successMessage, user.Email), 
                    AppResources.Dialog_understand);

                App.AddPinIfWanted();
                App.ResetNavStackToMain();
            }
            else
            {
				string message = AppResources.Error_connection;

				if (res.BadRequest != null && res.BadRequest.ModelState != null && res.BadRequest.ModelState.Count > 0) 
				{
					message = res.BadRequest.ModelState.First().Value.First();
				}
				
                await DisplayAlert(
                    AppResources.CreateAcc_errorTitle, 
                    message, 
                    AppResources.Dialog_understand
                );
            }
            
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
