using Acr.UserDialogs;
using Newtonsoft.Json;
using SkinSelfie.AppModels;
using SkinSelfie.Cells;
using System.Collections.Generic;
using Xamarin.Forms;
using System;

namespace SkinSelfie.Pages
{
    // change email
    // change password
    // reset password
    // change/add pin
    // delete account + data

    public class AppSettingsPage : ContentPage
    {
        public AppSettingsPage()
        {
            Title = AppResources.Settings_title;
        }

        private void SetContent()
        {
            TableSection accountSection = new TableSection(AppResources.Settings_accountHeader)
            {
                new ButtonCell(AppResources.Main_manageShares, () => Navigation.PushAsync(new ManageSharesPage())),
                new ButtonCell(AppResources.Main_manageStudies, () => Navigation.PushAsync(new StudyListPage())),
                new ButtonCell(AppResources.Settings_resendEmail, ResendEmail),
                new ButtonCell(AppResources.Settings_changePassword, ChangePassword),
                new ButtonCell(AppResources.Settings_resetPassword, ResetPassword)
            };

            TableSection appSection = new TableSection(AppResources.Settings_appHeader);

            if (string.IsNullOrWhiteSpace(App.user.AppCode))
            {
                appSection.Add(new ButtonCell(AppResources.Settings_addPIN, AddNewPIN));
            }
            else
            {
                appSection.Add(new ButtonCell(AppResources.Settings_changePIN, AddNewPIN));
                appSection.Add(new ButtonCell(AppResources.Settings_removePIN, RemovePIN));
            }
            appSection.Add(new ButtonCell(AppResources.Settings_logOut, Logout));

            TableSection dangerSection = new TableSection(AppResources.Settings_dangerHeader)
            {
                new ButtonCell(AppResources.Settings_deleteAcc, DeleteAccount)
            };


            Content = new TableView
            {
                Root = new TableRoot
                {
                    accountSection,
                    appSection,
                    dangerSection
                },
                Intent = TableIntent.Settings,
            };
        }

        private async void Logout()
        {
            bool shouldLogout = await UserDialogs.Instance.ConfirmAsync(
                AppResources.Settings_logOutConfirm, 
                AppResources.Settings_logoutConfirmTitle, 
                AppResources.Dialog_confirm, 
                AppResources.Dialog_cancel
            );
            if(shouldLogout)
            {
                App.SignOut();
            }
        }

        private async void ResendEmail()
        {
            var res = await NetworkUtils.PostRequest<object>("api/Account/ResendConfirmation", "");

			if(res.Response != null && res.Response.IsSuccessStatusCode)
            {
                UserDialogs.Instance.ShowSuccess(AppResources.Settings_resendEmailSuccess);
            }
            else
            {
                UserDialogs.Instance.ShowError(AppResources.Settings_resendEmailFail);
            }
        }

        private void ChangePassword()
        {
            Navigation.PushAsync(new ChangePasswordPage());
        }

        private async void ResetPassword()
        {
            bool res = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
            {
                Title = AppResources.Settings_warning,
                Message = string.Format(AppResources.Settings_resetPasswordWarning, App.user.Email),
                OkText = AppResources.Dialog_confirm,
                CancelText = AppResources.Dialog_cancel
            });

            if (!res) return;

            var data = new
            {
                Email = App.user.Email
            };

            UserDialogs.Instance.ShowLoading(title: AppResources.Dialog_loading);
            ServerResponse<object> resp = await NetworkUtils.PostRequest<object>("api/Account/ForgotPassword", JsonConvert.SerializeObject(data));
            UserDialogs.Instance.HideLoading();

			if(resp.Response != null && resp.Response.IsSuccessStatusCode)
            {
                UserDialogs.Instance.Alert(AppResources.Settings_resetPasswordSuccess);
                App.SignOut();
            }
            else if(resp.BadRequest != null)
            {
                UserDialogs.Instance.Alert(resp.BadRequest.Message);
            }
            else
            {
                UserDialogs.Instance.Alert(AppResources.Settings_resetPasswordFailure);
            }
        }

        private async void AddNewPIN()
        {
            MessagingCenter.Subscribe<UnlockPage, KeyValuePair<int, string>>(this, "Unlock-Condition", (senderPage, vals) => {
                // Remove PIN after double checking details
                if (vals.Key != 987654321)
                {
                    return;
                }

                MessagingCenter.Unsubscribe<UnlockPage, KeyValuePair<int, string>>(this, "Unlock-Condition");

                UserDialogs.Instance.ShowSuccess(AppResources.Settings_addPINSuccess);
                SetContent();
            });

			try
			{
				UnlockPage newPage = new UnlockPage(true, true, null, 987654321)
				{
					Title = AppResources.Settings_addPINEnter
				};

				await Navigation.PushAsync(newPage);
			}
			catch(Exception e) 
			{
				Title = e.ToString ();
			}
            
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            SetContent();
        }

        private async void RemovePIN()
        {
            MessagingCenter.Subscribe<UnlockPage, KeyValuePair<int, string>>(this, "Unlock-Condition", (senderPage, vals) => {
                // Remove PIN after double checking details
                if (vals.Key != 123456789 || vals.Value != App.user.AppCode)
                {
                    return;
                }

                MessagingCenter.Unsubscribe<UnlockPage, KeyValuePair<int, string>>(this, "Unlock-Condition");

                App.user.AppCode = null;
                App.db.AddUser(App.user);

                UserDialogs.Instance.ShowSuccess(AppResources.Settings_removePINSuccess);
                SetContent();
            });

            await App.Homepage.Navigation.PushAsync(new UnlockPage(false, true, App.user.AppCode, 123456789)
            {
                Title = AppResources.Settings_removePINEnter
            });
            return;
        }

        private async void DeleteAccount()
        {
            PromptResult res = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                Title = AppResources.Settings_warning,
                InputType = InputType.Default,
                Message = string.Format(AppResources.Settings_deleteAccWarning, AppResources.Settings_deleteAccSafeWord),
                OkText = AppResources.Dialog_confirm
            });

            if (!res.Ok) return;

            if (res.Text.ToUpper() == AppResources.Settings_deleteAccSafeWord.ToUpper())
            {
                bool success = await NetworkUtils.DeleteRequest("api/User/Delete");

                if(success)
                {
                    bool finalSuccess = await NetworkUtils.DeleteRequest("api/Account/Delete/?email=" + App.user.Email);
                    if (finalSuccess)
                    {
                        UserDialogs.Instance.Alert(AppResources.Settings_deleteAccSuccess, AppResources.Dialog_success);
                        App.SignOut();
                        return;
                    }
                }

                UserDialogs.Instance.Alert(AppResources.Settings_deleteAccFailure, AppResources.Error_delete, AppResources.Dialog_understand);
            }
            else
            {
                await DisplayAlert(
                    AppResources.Dialog_cancelled,
                    string.Format(AppResources.Settings_deleteAccSafeWordMismatch, AppResources.Settings_deleteAccSafeWord),
                    AppResources.Dialog_understand
                 );
            }
        }

    }
}
