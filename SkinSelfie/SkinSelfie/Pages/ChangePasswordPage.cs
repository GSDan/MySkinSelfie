using Acr.UserDialogs;
using Newtonsoft.Json;
using SkinSelfie.AppModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class ChangePasswordPage : ContentPage
    {
        Entry oldPasswordEntry;
        Entry newPasswordEntry;
        Entry confirmPasswordEntry;

        public ChangePasswordPage()
        {
            Title = AppResources.ChangePass_title;

            oldPasswordEntry = new Entry
            {
                Placeholder = AppResources.ChangePass_currentPassTemp,
                IsPassword = true
            };

            newPasswordEntry = new Entry
            {
                Placeholder = AppResources.ChangePass_newPassTemp,
                IsPassword = true
            };

            confirmPasswordEntry = new Entry
            {
                Placeholder = AppResources.ChangePass_confirmPassTemp,
                IsPassword = true
            };

            Button submit = new Button
            {
                Text = AppResources.CreateAcc_btn_submit
            };
            submit.Clicked += Submit_Clicked;

            Content = new StackLayout
            {
                Children = {
                    new Label { Text = AppResources.ChangePass_currentPass},
                    oldPasswordEntry,
                    new Label { Text = AppResources.ChangePass_newPass},
                    newPasswordEntry,
                    new Label { Text = AppResources.ChangePass_confirmPass},
                    confirmPasswordEntry,
                    submit
                },
                Spacing = 15,
                Padding = new Thickness(30, 50)
            };
        }

        private async void Submit_Clicked(object sender, EventArgs e)
        {
            PasswordRequest data = new PasswordRequest
            {
                OldPassword = oldPasswordEntry.Text,
                NewPassword = newPasswordEntry.Text,
                ConfirmPassword = confirmPasswordEntry.Text
            };

            UserDialogs.Instance.ShowLoading(title: AppResources.Dialog_loading);
            ServerResponse<object> resp = await NetworkUtils.PostRequest<object>("api/Account/ChangePassword", JsonConvert.SerializeObject(data));
            UserDialogs.Instance.HideLoading();

			if (resp.Response != null || !resp.Response.IsSuccessStatusCode)
            {
				string message = AppResources.Error_connection;

				if (resp.BadRequest != null && resp.BadRequest.ModelState != null && resp.BadRequest.ModelState.Count > 0) 
				{
					message = resp.BadRequest.ModelState.First().Value.First();
				}

                await DisplayAlert(AppResources.ChangePass_error, message, AppResources.Dialog_understand);
            }
			else
            {
                UserDialogs.Instance.ShowSuccess(AppResources.ChangePass_successTitle, 4000);
                App.SignOut();
            }
        }
    }

    public class PasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
