using Acr.UserDialogs;
using SkinSelfie.AppModels;
using SkinSelfie.Cells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class StudyListPage : ContentPage
    {
        private List<WrappedCell<StudyEnrolment>> WrappedItems = new List<WrappedCell<StudyEnrolment>>();
        private ScrollView scroller;
        private Action<string> onChosen;

        public StudyListPage(Action<string> onChosen = null)
        {
            Title = AppResources.StudyListPage_title;
            this.onChosen = onChosen;

            StackLayout layout = new StackLayout
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };

            scroller = new ScrollView
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.Fill
            };

            layout.Children.Add(scroller);
            Content = layout;

            LoadData();
        }

        private async void LoadData()
        {
            UserDialogs.Instance.ShowLoading(title: AppResources.Main_loadingUserContent);
            ServerResponse<List<StudyEnrolment>> returned = 
                await NetworkUtils.GetRequest<List<StudyEnrolment>>("api/studies/myenrols");
            UserDialogs.Instance.HideLoading();

            if (returned == null || returned.Data == null)
            {
                UserDialogs.Instance.Alert(AppResources.StudyListPage_connectionErr);
                Navigation.PopAsync();
            }
            else 
            {
                if (returned.Data.Count == 0)
                {
                    UserDialogs.Instance.Alert(AppResources.StudyListPage_noStudies);
                }

                WrappedItems = returned.Data.Select(item => new WrappedCell<StudyEnrolment>()
                { Item = item, IsSelected = false }).ToList();

                ListView listView = new ListView();
                listView.ItemsSource = WrappedItems;
                listView.HasUnevenRows = true;
                listView.VerticalOptions = LayoutOptions.FillAndExpand;
                listView.ItemTemplate = new DataTemplate(typeof(StudyCell));
                listView.ItemSelected += ListView_ItemSelected;

                scroller.VerticalOptions = LayoutOptions.FillAndExpand;
                scroller.Content = listView;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ToolbarItems.Clear();

            ToolbarItems.Add(new ToolbarItem
            {
                Text = AppResources.StudyListPage_join,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => JoinStudy())
            });
        }

        private async void JoinStudy()
        {
            CodeEntry code = await Utils.InputBox(Navigation, AppResources.StudyListPage_codeTitle, AppResources.StudyListPage_codeMessage);

            if(code.Cancelled || string.IsNullOrWhiteSpace(code.Entered))
            {
                return;
            }

            UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);
            ServerResponse<StudyEnrolment> photoResp = 
                await NetworkUtils.PostRequest<StudyEnrolment>("api/studies/enrol?code="+code.Entered, "");
            UserDialogs.Instance.HideLoading();

            if (photoResp.Response.StatusCode == HttpStatusCode.NotFound)
            {
                await UserDialogs.Instance.AlertAsync(AppResources.StudyListPage_codeInvalid);
                return;
            }
            else if(!photoResp.Response.IsSuccessStatusCode)
            {
                await UserDialogs.Instance.AlertAsync(AppResources.Main_connectionErr);
                return;
            }
            else
            {
                await UserDialogs.Instance.AlertAsync(AppResources.StudyListPage_codeSuccess);
                LoadData();
            }
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            ((ListView)sender).SelectedItem = null;

            StudyEnrolment chosen = ((WrappedCell<StudyEnrolment>)e.SelectedItem).Item;

            if (onChosen != null)
            {
                onChosen(chosen.Study.Manager.Email);
                return;
            }

            bool delete = await UserDialogs.Instance.ConfirmAsync(
                string.Format(AppResources.StudyListPage_deleteConfirmMessage, chosen.Study.Name),
                AppResources.StudyListPage_deleteConfirmTitle,
                AppResources.StudyListPage_deleteConfirmBtn,
                AppResources.Dialog_cancel
            );

            if (!delete) return;

            UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);
            bool deletionSuccess = await NetworkUtils.DeleteRequest("api/studies/unenrol/?studyId=" + chosen.Study.Id);
            UserDialogs.Instance.HideLoading();

            if (!deletionSuccess)
            {
                await UserDialogs.Instance.AlertAsync(AppResources.StudyListPage_deleteErr, AppResources.Error_delete);
                return;
            }
            else
            {
                await UserDialogs.Instance.AlertAsync(AppResources.StudyListPage_deleteSuccess);
                LoadData();
            }
        }
    }
}