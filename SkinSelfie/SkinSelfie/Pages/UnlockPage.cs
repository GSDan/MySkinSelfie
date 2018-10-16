using Acr.UserDialogs;
using SkinSelfie.AppModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class UnlockPage : ContentPage
    {
        public TaskCompletionSource<ImageSource> taskCompletionSource;
        Entry pinEntry;
        bool newCode;
        bool animating = false;
        string currentPin;
        int passedId = -1;
        bool canCancel;

        public UnlockPage(bool settingNew = false, bool optional = false, string checkAgainst = null, int? id = null)
        {
            if(id != null)
            {
                passedId = (int)id;
            }

			if (App.user == null) 
			{
				return;
			}

            canCancel = optional;
            currentPin = (checkAgainst == null)? App.user.AppCode : checkAgainst;

            newCode = settingNew;
            Title = (settingNew)? AppResources.Unlock_titleNew : AppResources.Unlock_titleUnlock;

            if(!settingNew) App.Locked = true;

            StackLayout layout = new StackLayout
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Padding = new Thickness(30, -20)
            };

            Label infoLabel = new Label
            {
                Text = (settingNew)? AppResources.Unlock_blurbNew : AppResources.Unlock_blurbUnlock,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold
            };
            layout.Children.Add(infoLabel);

            pinEntry = new Entry
            {
                IsPassword = true,
                Placeholder = "****",
                Keyboard = Keyboard.Numeric
            };
            pinEntry.TextChanged += PinEntry_TextChanged;
            layout.Children.Add(pinEntry);

            Button unlockButton = new Button
            {
                Text = (settingNew) ? AppResources.Unlock_btnNew : AppResources.Unlock_btnUnlock
            };
            unlockButton.Clicked += UnlockButton_Clicked;
            layout.Children.Add(unlockButton);

            if(settingNew)
            {
                Button cancelButton = new Button
                {
                    Text = AppResources.Dialog_cancel
                };
                cancelButton.Clicked += CancelButton_Clicked;
                layout.Children.Add(cancelButton);
            }
            else
            {
                Button forgotButton = new Button
                {
                    Text = AppResources.Unlock_btnForgot
                };
                forgotButton.Clicked += ForgotButton_Clicked; ;
                layout.Children.Add(forgotButton);
            }

            Content = layout;
        }

        private void ForgotButton_Clicked(object sender, EventArgs e)
        {
            if(passedId == -1)
            {
                UserDialogs.Instance.Confirm(new ConfirmConfig
                {
                    Title = AppResources.Unlock_forgotTitle,
                    Message = AppResources.Unlock_forgotMessage,
                    CancelText = AppResources.Dialog_cancel,
                    OkText = AppResources.Unlock_forgotConfirm,
                    OnAction = (bool confirmed) => {
                        if(confirmed)
                        {
                            App.SignOut();
                        }
                    }
                });
            }
            else
            {
                UserDialogs.Instance.Confirm(new ConfirmConfig
                {
                    Title = AppResources.Unlock_forgotTitle,
                    Message = string.Format(AppResources.Unlock_folderForgotMessage, App.user.Email),
                    CancelText = AppResources.Dialog_cancel,
                    OkText = AppResources.Unlock_folderForgotOk,
                    OnAction = async (bool confirmed) => {
                        if (confirmed)
                        {
                            UserDialogs.Instance.ShowLoading();
                            ServerResponse<UserCondition> returned = await NetworkUtils.PostRequest<UserCondition>("api/UserConditions/ResetPin/?id=" + passedId, "");
                            if (returned == null || returned.Data == null)
                            {
                                UserDialogs.Instance.HideLoading();

                                if (returned == null || returned.Response == null)
                                {
                                    UserDialogs.Instance.Alert(AppResources.Unlock_resetErr);
                                    return;
                                }

                                if (!returned.Response.IsSuccessStatusCode)
                                {
                                    UserDialogs.Instance.Alert(AppResources.Unlock_ohwtf);
                                    App.SignOut();
                                    return;
                                }
                            }

                            UserDialogs.Instance.Alert(new AlertConfig
                            {
                                Title = AppResources.Dialog_success,
                                Message = AppResources.Unlock_folderResetSuccess
                            });
                            App.ShouldUpdate = true;

                            if (canCancel)
                            {
                                // Allow for navbar
                                await App.Homepage.Navigation.PopAsync();
                            }
                            else
                            {
                                await App.Homepage.Navigation.PopModalAsync();
                            }
                        }
                    }
                });
                
            }
        }

        private void CancelButton_Clicked(object sender, EventArgs e)
        {
            if (canCancel)
            {
                // Allow for navbar
                Navigation.PopAsync();
            }
            else
            {
                Navigation.PopModalAsync();
            }
        }

        private async void AnimateError()
        {
            if(!animating)
            {
                animating = true;
                await pinEntry.ScaleTo(1.1, 40, Easing.CubicIn);
                await pinEntry.ScaleTo(1, 40, Easing.CubicOut);
                animating = false;
            }
            
        }

        private void PinEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            string val = pinEntry.Text;

            if (val.Length > 4)
            {
                val = val.Remove(val.Length - 1);// Remove Last character 
                pinEntry.Text = val; //Set the Old value
                AnimateError();
            }
        }

        private async void ConfirmCode()
        {
            PromptResult res = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                InputType = InputType.NumericPassword,
                Message = AppResources.Unlock_confirm,
            });

            if(res.Text == pinEntry.Text)
            {
                if (canCancel)
                {
                    // Allow for navbar
                    await Navigation.PopAsync();
                }
                else
                {
                    await Navigation.PopModalAsync();
                }

                App.Locked = false;
                App.user.AppCode = res.Text;
                App.db.AddUser(App.user);
                if (passedId != -1)
                {
                    MessagingCenter.Send<UnlockPage, KeyValuePair<int, string>>(this, "Unlock-Condition", new KeyValuePair<int, string>(passedId, currentPin));
                }
            }
            else
            {
                await DisplayAlert(AppResources.Dialog_error, AppResources.Unlock_confirmErr, AppResources.Dialog_understand);
            }
        }

        private async void UnlockButton_Clicked(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(pinEntry.Text))
            {
                AnimateError();
                pinEntry.Text = "";
                return;
            }

            if(newCode)
            {
                ConfirmCode();
            }
            else
            {
                if (pinEntry.Text == currentPin)
                {

                    if(canCancel)
                    {
                        // Allow for navbar
                        await App.Homepage.Navigation.PopAsync();
                    }
                    else
                    {
                        await App.Homepage.Navigation.PopModalAsync();
                    }
                    
                    App.Locked = false;
                    if (passedId != -1)
                    {
                        MessagingCenter.Send<UnlockPage, KeyValuePair<int, string>>(this, "Unlock-Condition", new KeyValuePair<int, string>(passedId, currentPin));
                    }
                    return;
                }

                await DisplayAlert(AppResources.Dialog_error, AppResources.Unlock_mismatch, AppResources.Dialog_understand);
                pinEntry.Text = "";
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return !canCancel;
        }
    }
}
