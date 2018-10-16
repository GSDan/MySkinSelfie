using Acr.UserDialogs;
using Newtonsoft.Json;
using SkinSelfie.AppModels;
using System;

using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class WelcomePage : ContentPage
    {
        public WelcomePage()
        {
            Title = AppResources.Welcome_title;

            Button createBtn = new Button { Text = AppResources.Welcome_btn_createAcc};
            createBtn.Clicked += CreateBtn_Clicked;
            Button loginBtn = new Button { Text = AppResources.Welcome_btn_signIn };
            loginBtn.Clicked += LoginBtn_Clicked;
            Button resetPassword = new Button { Text = AppResources.Welcome_btn_forgottenPassword };
            resetPassword.Clicked += ResetPassword_Clicked;
            Button aboutBtn = new Button { Text = AppResources.Main_help };
            aboutBtn.Clicked += AboutBtn_Clicked;

			// Scale the elements to fit screen sizes
			// Not a scrolling page, so need to fit everything in!
			double screenScale = App.screenHeightInches / 3.8;
            double maxTitle = 30;
            double maxText = 23;

            Device.OnPlatform (
				iOS: () => {
					createBtn.TextColor = Color.White;
                    createBtn.FontSize = Math.Min(18 * screenScale, maxText);
					loginBtn.TextColor = Color.White;
                    loginBtn.FontSize = Math.Min(18 * screenScale, maxText);
					resetPassword.TextColor = Color.White;
                    resetPassword.FontSize = Math.Min(18 * screenScale, maxText);
                    aboutBtn.TextColor = Color.White;
                    aboutBtn.FontSize = Math.Min(18 * screenScale, maxText);
                });
					
            Content = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Fill,
				Spacing = 12 * screenScale,
				Padding = new Thickness(0, 20 * screenScale),
                Children = {
                    new Image
                    {
                        Source = "icon_badge.png",
                        HorizontalOptions = LayoutOptions.Center,
						HeightRequest = App.display.HeightRequestInInches (App.screenHeightInches * 0.2)
                    },
                    new StackLayout
                    {
                        Padding = new Thickness(10),
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        BackgroundColor = new Color(0,0,0,0.2),
                        Children = {
                            new Label
                            {
                                Text = AppResources.Welcome_header,
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
								FontSize = Math.Min(25 * screenScale, maxTitle),
								HorizontalTextAlignment = TextAlignment.Center,
                                TextColor = Color.White
                            },
                            new Label {
                                Text = AppResources.Welcome_blurb,
								HorizontalTextAlignment = TextAlignment.Center,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
								FontSize = Math.Min(18 * screenScale, maxText),
                                TextColor = Color.White
                            },
                        }
                    },
                    new StackLayout
                    {
                        Padding = new Thickness(10,0),
                        HorizontalOptions = LayoutOptions.Center,
                        WidthRequest = 320,
                        Children = {
                            createBtn,
                            loginBtn,
                            resetPassword,
                            aboutBtn
                        }
                    }
                    
                }
            };

            BackgroundImage = "background.png";
            Padding = new Thickness(0);
        }

        private void AboutBtn_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AppHelpPage());
        }

        private void LoginBtn_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AccountLoginPage());
        }

        private void CreateBtn_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AccountCreationPage());
        }

        private async void ResetPassword_Clicked(object sender, EventArgs e)
        {
            PromptResult res = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                Title = AppResources.ResetPassword_title,
                Message = AppResources.ResetPassword_message,
                IsCancellable = true,
                Placeholder = AppResources.CreateAcc_emailTemp,
                InputType = InputType.Email
            });

            if (!res.Ok || string.IsNullOrWhiteSpace(res.Text)) return;

            var data = new
            {
                Email = res.Text
            };

            UserDialogs.Instance.ShowLoading(title: AppResources.Dialog_loading);
            ServerResponse<object> resp = await NetworkUtils.PostRequest<object>("api/Account/ForgotPassword", JsonConvert.SerializeObject(data));
            UserDialogs.Instance.HideLoading();

			if (resp.Response != null && resp.Response.IsSuccessStatusCode)
            {
                UserDialogs.Instance.Alert(AppResources.ResetPassword_success);
            }
            else if (resp.BadRequest != null)
            {
                UserDialogs.Instance.Alert(resp.BadRequest.Message);
            }
            else
            {
                UserDialogs.Instance.Alert(AppResources.ResetPassword_failure);
            }
        }
    }
}
