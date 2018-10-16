using Acr.UserDialogs;
using SkinSelfie.AppModels;
using SkinSelfie.Cells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class ManageSharesPage : ContentPage
    {
        private List<WrappedCell<Share>> WrappedItems = new List<WrappedCell<Share>>();
        private ScrollView scroller;

        public ManageSharesPage()
        {
            Title = AppResources.ManageSharesPage_title;

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
            ServerResponse<List<Share>> returned = await NetworkUtils.GetRequest<List<Share>>("api/shares/MyShares");
            UserDialogs.Instance.HideLoading();

            if (returned == null || returned.Data == null)
            {
                UserDialogs.Instance.Alert(AppResources.Main_connectionErr);
                Navigation.PopAsync();
            }
            else if(returned.Data.Count == 0)
            {
                UserDialogs.Instance.Alert(AppResources.ManageSharesPage_noShares);
                Navigation.PopAsync();
            }
            else
            {
                WrappedItems = returned.Data.Select(item => new WrappedCell<Share>()
                    { Item = item, IsSelected = false }).ToList();

                ListView listView = new ListView();
                listView.ItemsSource = WrappedItems;
                listView.HasUnevenRows = true;
                listView.VerticalOptions = LayoutOptions.FillAndExpand;
                listView.ItemTemplate = new DataTemplate(typeof(ShareCell));
                listView.ItemSelected += ListView_ItemSelected;

                scroller.VerticalOptions = LayoutOptions.FillAndExpand;
                scroller.Content = listView;
            }
        }

        private async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            ((ListView)sender).SelectedItem = null;

            Share chosen = ((WrappedCell<Share>)e.SelectedItem).Item;

            bool delete = await UserDialogs.Instance.ConfirmAsync(
                string.Format(AppResources.ManageSharesPage_confirmRevokeMessage, 
                    chosen.SharedEmail, chosen.UserCondition.Condition),
                AppResources.ManageSharesPage_deleteTitle,
                AppResources.ManageSharesPage_deleteConfirm,
                AppResources.Dialog_cancel
            );

            if (!delete) return;

            UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);
            bool deletionSuccess = await NetworkUtils.DeleteRequest("api/Shares/" + chosen.Id);
            UserDialogs.Instance.HideLoading();

            if (!deletionSuccess)
            {
                await UserDialogs.Instance.AlertAsync(AppResources.ManageSharesPage_deleteErr, AppResources.Error_delete);
                return;
            }
            else
            {
                await UserDialogs.Instance.AlertAsync(AppResources.ManageSharesPage_deleteSuccess, 
                    AppResources.ConditionEdit_deleteSuccessTitle);
                LoadData();
            }
        }
    }
}