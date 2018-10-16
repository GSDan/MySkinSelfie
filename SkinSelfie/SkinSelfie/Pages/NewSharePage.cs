using Acr.UserDialogs;
using Newtonsoft.Json;
using SkinSelfie.AppModels;
using SkinSelfie.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class NewSharePage : ContentPage
    {
        UserCondition thisCondition;
        Entry emailField;
        Picker picker;

        // string, days valid
        Dictionary<string, int> times = new Dictionary<string, int>();

        public NewSharePage(UserCondition toShare)
        {
            thisCondition = toShare;

            Title = AppResources.NewSharePage_title;

            times.Add(AppResources.NewSharePage_time_forever, 3650); // 10 years
            times.Add(AppResources.NewSharePage_time_1Week, 7);
            times.Add(AppResources.NewSharePage_time_2Weeks, 14);
            times.Add(AppResources.NewSharePage_time_1Month, 30);
            times.Add(AppResources.NewSharePage_time_6Months, 182);
            times.Add(AppResources.NewSharePage_time_1Year, 365);

            emailField = new Entry
            {
                Placeholder = AppResources.CreateAcc_emailTemp,
                Keyboard = Keyboard.Email
            };

            picker = new Picker
            {
                Title = AppResources.NewSharePage_timePicker
            };

            foreach (KeyValuePair<string, int> p in times)
            {
                picker.Items.Add(p.Key);
            }

            picker.SelectedIndex = 0;

            Button contactButton = new Button()
            {
                Text = AppResources.NewSharePage_contactBtn,
                FontSize = 13,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            contactButton.Clicked += ContactButton_Clicked;

            Button studyButton = new Button()
            {
                Text = AppResources.NewSharePage_studyButton,
                FontSize = 13,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            studyButton.Clicked += StudyButton_Clicked;

            Button finishBtn = new Button()
            {
                Text = AppResources.NewSharePage_finishBtn,
                FontSize = 15,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Fill
            };
            finishBtn.Clicked += FinishBtn_Clicked;

            ScrollView scroller = new ScrollView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Content = new StackLayout
                {
                    Padding = new Thickness(25),
                    Spacing = 30,
                    Children = {
                        new Label { Text = string.Format(AppResources.NewSharePage_header, toShare.Condition),
                            FontAttributes = FontAttributes.Bold, FontSize=28},
                        new Label { Text = AppResources.NewSharePage_emailPrompt, FontSize = 16},
                        emailField,
                        new StackLayout()
                        {
                            Orientation = StackOrientation.Horizontal,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            Children = {
                                     contactButton,
                                     studyButton
                                }
                        },
                        new Label { Text = AppResources.NewSharePage_timePrompt, FontSize = 16},
                        picker,
                        new Label { Text = AppResources.NewSharePage_revokeReminder, FontSize = 16},
                        finishBtn
                    }
                }
            };

            Content = scroller;
        }

        private async void StudyButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new StudyListPage(
            (email) =>
            {
                emailField.Text = email;
                Navigation.PopAsync();
            }));
        }

        private async void ContactButton_Clicked(object sender, EventArgs e)
        {
            bool hasPerm = await DependencyService.Get<IPermissions>().GetContactsPermissions();
            if (hasPerm)
            {
                UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);
                List<EmailContact> res = (await DependencyService.Get<IContact>().GetEmailContacts()).OrderBy(contact => contact.Name).ToList();
                UserDialogs.Instance.HideLoading();

                await Navigation.PushAsync(new ContactListPage(res,
                (email) =>
                {
                    emailField.Text = email;
                    Navigation.PopAsync();
                }));
            }
        }

        private async void FinishBtn_Clicked(object sender, EventArgs e)
        {
            UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);

            Share thisShare = new Share()
            {
                CreatedAt = DateTime.Now,
                Owner = App.user,
                Updated = true,
                SharedEmail = emailField.Text,
                ExpireDate = DateTime.Now + new TimeSpan(times[picker.Items[picker.SelectedIndex]], 0, 0, 0),
                UserCondition = thisCondition
            };

            ServerResponse<UserCondition> shareResp = await NetworkUtils.PostRequest<UserCondition>("api/Shares",
            JsonConvert.SerializeObject(thisShare, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            UserDialogs.Instance.HideLoading();

            if (shareResp.Response == null || !shareResp.Response.IsSuccessStatusCode)
            {
                await UserDialogs.Instance.AlertAsync(AppResources.NewSharePage_error + " " + shareResp.Response.ReasonPhrase, AppResources.Dialog_error);
                return;
            }
            else
            {
                await UserDialogs.Instance.AlertAsync(AppResources.NewSharePage_success, AppResources.NewSharePage_successTitle);
                Navigation.PopToRootAsync();
            }
        }
    }
}