using Acr.UserDialogs;
using Newtonsoft.Json;
using SkinSelfie.AppModels;
using SkinSelfie.Cells;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class UploadsQueuePage : ContentPage
    {
        StackLayout nonePendingLayout;
        ScrollView scroller;

        private List<WrappedCell<PendingPhotoUpload>> WrappedItems = new List<WrappedCell<PendingPhotoUpload>>();
        public static Func<UploadCell, bool> OnCellTapped;

        public UploadsQueuePage()
        {
            nonePendingLayout = new StackLayout
            {
                Padding = new Thickness(25),
                Spacing = 35,
                Children = {
                    new Label { Text = AppResources.Uploads_noneTitle, FontAttributes= FontAttributes.Bold, FontSize=28},
                    new Label { Text = AppResources.Uploads_noneMessage, FontSize = 18}
                }
            };

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

            OnCellTapped = StageUpload;

            layout.Children.Add(scroller);

            Content = layout;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Title = AppResources.Uploads_title;
            SetContent();
        }

        private void SetContent()
        {
            List<PendingPhotoUpload> inDb = (List<PendingPhotoUpload>)App.db.GetUploads();

            bool welcomeOnly = (inDb == null || inDb.Count == 0);
            nonePendingLayout.IsVisible = welcomeOnly;
            nonePendingLayout.IsEnabled = welcomeOnly;

            if (welcomeOnly)
            {
                scroller.Content = nonePendingLayout;
                return;
            }

            WrappedItems = inDb.Select(item => new WrappedCell<PendingPhotoUpload>() { Item = item, IsSelected = false }).ToList();

            ListView listView = new ListView();
            listView.ItemsSource = WrappedItems;
            listView.HasUnevenRows = true;
            listView.VerticalOptions = LayoutOptions.FillAndExpand;
            listView.ItemTemplate = new DataTemplate(typeof(UploadCell));

            scroller.VerticalOptions = LayoutOptions.FillAndExpand;
            scroller.Content = listView;

            UpdateToolbar(0);
        }

		private bool StageUpload(UploadCell tappedCell)
		{
			bool highlighted = ((WrappedCell<PendingPhotoUpload>)tappedCell.BindingContext).IsSelected;

			List<PendingPhotoUpload> hlts = WrappedItems.Where(item => item.IsSelected).Select(wrappedItem => wrappedItem.Item).ToList();

			if (highlighted)
			{
				UpdateToolbar(hlts.Count - 1);
				return false;
			}
			else
			{
				UpdateToolbar(hlts.Count + 1);
				return true;
			}
		}

        private void UpdateToolbar(int numHighlights)
        {
            ToolbarItems.Clear();

            if(WrappedItems == null || WrappedItems.Count == 0)
            {
                return;
            }

            ToolbarItem uploadBtn = new ToolbarItem
            {
                Order = ToolbarItemOrder.Primary,
                Command = new Command(StartUpload),
            };

            uploadBtn.Text = (numHighlights > 0) ? string.Format(AppResources.Uploads_upSelectionBtn, numHighlights) : 
                                                   AppResources.Uploads_allBtn;

            ToolbarItems.Add(uploadBtn);
        }

        private async void StartUpload()
        {
            bool upload = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
            {
                Message = AppResources.Uploads_dataWarning,
                OkText = AppResources.Uploads_dataWarningContinue,
                CancelText = AppResources.Dialog_cancel
            });

            if (!upload) return;

            List<PendingPhotoUpload> selected = WrappedItems.Where(item => item.IsSelected)
                .Select(wrappedItem => wrappedItem.Item).ToList();

            // If none are selected, upload all
            if(selected == null || selected.Count == 0)
            {
                selected = WrappedItems.Select(wrappedItem => wrappedItem.Item).ToList();
            }

            UserDialogs.Instance.ShowLoading();

            foreach(PendingPhotoUpload ph in selected)
            {
                byte[] data = DependencyService.Get<ISaveAndLoad>().LoadFromFile(ph.LocalPhotoLoc);

                UserCondition thisCond = JsonConvert.DeserializeObject<UserCondition>(ph.UserConditionJson);

                ServerResponse<string[]> resp = await NetworkUtils.UploadFile(data, string.Format("{0}/{1}/", App.user.Id, thisCond.Id), Guid.NewGuid() + ".jpg");

                if (resp.Response == null || !resp.Response.IsSuccessStatusCode)
                {
                    UserDialogs.Instance.HideLoading();
                    UserDialogs.Instance.Alert(AppResources.PhotoEdit_uploadErr, AppResources.Dialog_error);
                    return;
                }

                Photo toPost = new Photo
                {
                    Url = resp.Data[0],
                    ThumbUrl = resp.Data[1],
                    CreatedAt = ph.CreatedAt,
                    Notes = ph.Notes,
                    PhotoDescription = ph.PhotoDescription,
                    Treatment = ph.Treatment,
                    Rating = ph.Rating,
                    UserCondition = thisCond
                };

                // post photo
                ServerResponse<Photo> photoResp = await NetworkUtils.PostRequest<Photo>("api/Photo", JsonConvert.SerializeObject(toPost));

                if (photoResp.Response == null || !photoResp.Response.IsSuccessStatusCode)
                {
                    UserDialogs.Instance.HideLoading();
                    UserDialogs.Instance.Alert(AppResources.PhotoEdit_uploadErr, AppResources.Dialog_error);
                    return;
                }
                else
                {
                    DependencyService.Get<ISaveAndLoad>().DeleteLocalCopy(ph.LocalPhotoLoc);
                    App.db.DeleteUpload(ph.Id);
                    SetContent();
                }
            }

            UserDialogs.Instance.HideLoading();
            App.ShouldUpdate = true;
            GalleryPage.NeedsUpdate = true;

            UserDialogs.Instance.ShowSuccess(AppResources.ChangePass_successTitle);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            UpdateToolbar(0);
        }

    }
}
