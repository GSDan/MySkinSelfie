using Acr.UserDialogs;
using SkinSelfie.AppModels;
using SkinSelfie.Cells;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using System;
using System.ComponentModel;
using System.Linq;
using SkinSelfie.Interfaces;

namespace SkinSelfie.Pages
{
    public class GalleryPage : ContentPage
    {
        public static bool NeedsUpdate = false;
        private bool Updating = false;
        UserCondition condition;
        StackLayout welcomeLayout;
        ScrollView scroller;

        public List<WrappedCell<Photo>> WrappedItems = new List<WrappedCell<Photo>>();
        public static Func<PhotoCell, bool> OnCellTapped;
        int selectionLimit = 2;

        public GalleryPage(UserCondition condition)
        {
            this.condition = condition;
            UpdateToolbar(0);

            welcomeLayout = new StackLayout
            {
                Padding = new Thickness(25),
                Spacing = 35,
                Children = {
                    new Label { Text = AppResources.Gallery_welcome1, FontAttributes= FontAttributes.Bold, FontSize=28},
                    new Label { Text = AppResources.Gallery_welcome2, FontSize = 18},
                    new Label { Text = AppResources.Gallery_welcome3, FontSize = 18}
                }
            };

            if (condition.Passcode == null)
            {
                welcomeLayout.Children.Add(new Label
                {
                    Text = AppResources.Gallery_noPasscode,
                    FontSize = 18
                });
            }

            if (Device.OS == TargetPlatform.WinPhone)
            {
                welcomeLayout.Children.Insert(0, new Label { Text = Title });
            }

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

            // Windows phone doesn't have an action bar, so we need to add the page title as a label
            if (Device.OS == TargetPlatform.WinPhone)
            {
                ContentView titleView = new ContentView { Padding = 15 };
                titleView.Content = new Label { Text = Title };
                layout.Children.Add(titleView);
            }

            OnCellTapped = StartCompare;

            layout.Children.Add(scroller);

            Content = layout;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Title = condition.Condition;

            if (NeedsUpdate && !Updating)
            {
                RefreshGallery();
            }
            else
            {
                SetContent();
            }
        }

        private async void RefreshGallery()
        {
            Updating = true;

            UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);
            ServerResponse<UserCondition> returned = await NetworkUtils.GetRequest<UserCondition>("api/UserConditions/" + condition.Id);
            UserDialogs.Instance.HideLoading();

            if (returned.Response == null || !returned.Response.IsSuccessStatusCode)
            {
                UserDialogs.Instance.Alert(AppResources.Error_connection, AppResources.Error_update);
                return;
            }
            condition = returned.Data;
            condition.Owner = App.user;
            App.SelectedCondition = condition;
            NeedsUpdate = false;
            Updating = false;

            SetContent();
        }

        private void SetContent()
        {
            bool welcomeOnly = (condition.Photos == null || condition.Photos.Count == 0);
            welcomeLayout.IsVisible = welcomeOnly;
            welcomeLayout.IsEnabled = welcomeOnly;

            if (welcomeOnly)
            {
                scroller.Content = welcomeLayout;
                return;
            }

            WrappedItems = condition.Photos.Select(item => new WrappedCell<Photo>() { Item = item, IsSelected = false }).ToList();

            ListView listView = new ListView();
            listView.ItemsSource = WrappedItems;
            listView.HasUnevenRows = true;
            listView.VerticalOptions = LayoutOptions.FillAndExpand;
            listView.ItemTemplate = new DataTemplate(typeof(PhotoCell));
            listView.ItemSelected += ListView_ItemSelected;

            scroller.VerticalOptions = LayoutOptions.FillAndExpand;
            scroller.Content = listView;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {

            if (e.SelectedItem == null) return;

            ((ListView)sender).SelectedItem = null; //disable the visual selection state.

            Device.BeginInvokeOnMainThread(() =>
            {
                Navigation.PushAsync(new PhotoCarouselPage(((WrappedCell<Photo>)e.SelectedItem).Item));
            });
        }


        private void UpdateToolbar(int numHighlights)
        {
            ToolbarItems.Clear();

            if (numHighlights > 0)
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = string.Format(AppResources.Gallery_compareCount, numHighlights, selectionLimit),
                    Order = ToolbarItemOrder.Primary,
                    Command = new Command(OpenCompare),
                });
            }
            else
            {
                ToolbarItems.Add(new ToolbarItem
                {
                    Text = AppResources.Gallery_takeNew,
                    Icon = "ic_camera_alt_white_24dp.png",
                    Order = ToolbarItemOrder.Primary,
                    Command = new Command(TryOpenCamera)
                });
            }
            ToolbarItem editItem = new ToolbarItem
            {
                Text = AppResources.Gallery_edit,
                Order = ToolbarItemOrder.Secondary,
                Command = new Command(() => Navigation.PushAsync(new ConditionEditPage(condition)))
            };

            Device.OnPlatform(
                iOS: () => {
                    editItem.Icon = "ic_mode_edit.png";
                });
            ToolbarItems.Add(editItem);
        }

        public async void TryOpenCamera()
        {
            bool hasPerm = await DependencyService.Get<IPermissions>().GetCameraPermissions();

            if (hasPerm)
            {
                await Navigation.PushAsync(new CameraPage());
            }
            else
            {
                UserDialogs.Instance.ShowError(AppResources.Gallery_needPerms);
            }
        }

        public bool StartCompare(PhotoCell tappedCell)
        {
            bool highlighted = ((WrappedCell<Photo>)tappedCell.BindingContext).IsSelected;

            List<Photo> hlts = GetSelection();

            if (highlighted)
            {
                UpdateToolbar(hlts.Count - 1);
                return false;
            }
            else if (hlts.Count < selectionLimit)
            {
                UpdateToolbar(hlts.Count + 1);
                return true;
            }
            else
            {
                UserDialogs.Instance.ShowError(
                    string.Format(AppResources.Gallery_compareTooManyTitle, selectionLimit)
                );
                return false;
            }
        }

        private async void OpenCompare()
        {
            List<Photo> hlts = GetSelection();

            if (hlts.Count < 2)
            {
                UserDialogs.Instance.Alert(
                    AppResources.Gallery_compareTooFewMessage,
                    AppResources.Gallery_compareTooFewTitle,
                    AppResources.Dialog_understand
                );
                return;
            }

            Navigation.PushAsync(new PhotoComparePage(hlts));
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            UpdateToolbar(0);
        }

        public List<Photo> GetSelection()
        {
            return WrappedItems.Where(item => item.IsSelected).Select(wrappedItem => wrappedItem.Item).ToList();
        }

    }
}
