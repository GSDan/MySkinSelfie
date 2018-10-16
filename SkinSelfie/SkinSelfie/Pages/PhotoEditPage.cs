using Acr.UserDialogs;
using FFImageLoading.Forms;
using Newtonsoft.Json;
using SkinSelfie.AppModels;
using SkinSelfie.Interfaces;
using System;
using Xamarin.Forms;
using System.IO;

namespace SkinSelfie.Pages
{
    public class PhotoEditPage : ContentPage
    {
        Photo photo;
        Editor treatment;
        Editor notes;
        Editor description;

        public PhotoEditPage(Photo photo)
        {
            this.photo = photo;

            ToolbarItems.Add(new ToolbarItem
            {
                Text = AppResources.PhotoEdit_finished,
                Icon = "ic_done_white_24dp.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => CommitChanges())
            });

            if (!string.IsNullOrWhiteSpace(photo.Url))
            {
                ToolbarItem deleteItem = new ToolbarItem
                {
                    Text = AppResources.PhotoEdit_delete,
                    Order = ToolbarItemOrder.Secondary,
                    Command = new Command(() => DeletePhoto())
                };

                Device.OnPlatform(
                    iOS: () => {
                        deleteItem.Icon = "ic_delete.png";
                    });

                ToolbarItems.Add(deleteItem);
            }

            Title = Utils.GetDateString(photo.CreatedAt);

            StackLayout layout = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            ScrollView scroller = new ScrollView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            RelativeLayout content = new RelativeLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            CachedImage theImage = new CachedImage()
            {
                Aspect = Aspect.AspectFill,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                HeightRequest = 500,
                TransparencyEnabled = false,
                DownsampleToViewSize = true,
                CacheDuration = TimeSpan.FromDays(30)
            };

			if (photo.InMemory != null)
			{
				theImage.Source = ImageSource.FromStream(() => new MemoryStream(photo.InMemory));
			}
			else
			{
				theImage.Source = photo.GetReqUrl(false);
			}

            content.Children.Add(theImage, Constraint.Constant(0), Constraint.Constant(0), Constraint.RelativeToParent((parent) => {
                return (parent.Width);
            }));

            treatment = new Editor
            {
                HorizontalOptions = LayoutOptions.Fill
            };

            if (!string.IsNullOrWhiteSpace(photo.Treatment)) treatment.Text = photo.Treatment;

            notes = new Editor
            {
                HorizontalOptions = LayoutOptions.Fill
            };

            if (!string.IsNullOrWhiteSpace(photo.Notes)) notes.Text = photo.Notes;

            description = new Editor
            {
                HorizontalOptions = LayoutOptions.Fill
            };
            if (!string.IsNullOrWhiteSpace(photo.PhotoDescription)) description.Text = photo.PhotoDescription;

            StackLayout details = new StackLayout
            {
                Padding = 20,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Fill,
                Spacing = 10,
                Children =
                {
                    new Label { Text = Utils.GetDateString(photo.CreatedAt), FontAttributes = FontAttributes.Bold, FontSize = 24},
                    new Label { Text = AppResources.PhotoEdit_treatment, FontAttributes = FontAttributes.Bold, FontSize = 20 },
                    treatment,
                    new Label { Text = AppResources.PhotoEdit_notes, FontAttributes = FontAttributes.Bold, FontSize = 20 },
                    notes,
                    new Label { Text = AppResources.PhotoEdit_shotConditions, FontAttributes = FontAttributes.Bold, FontSize = 20 },
                    description
                }
            };
            content.Children.Add(details, Constraint.Constant(0),
                Constraint.RelativeToView(theImage, (parent, view) =>
                {
                    return view.Y + view.Height + 10;
                }));

            scroller.Content = content;

            // Windows phone doesn't have an action bar, so we need to add the page title as a label
            if (Device.OS == TargetPlatform.WinPhone)
            {
                ContentView titleView = new ContentView { Padding = 15 };
                titleView.Content = new Label { Text = Title };
                layout.Children.Add(titleView);
            }

            layout.Children.Add(scroller);

            Content = layout;
        }

        private async void UploadNewPhoto()
        {
            UserDialogs.Instance.ShowLoading();

            ServerResponse<string[]> resp = await NetworkUtils.UploadFile(photo.InMemory, string.Format("{0}/{1}/", App.user.Id, photo.UserCondition.Id), Guid.NewGuid() + ".jpg");

            if (resp.Response == null || !resp.Response.IsSuccessStatusCode)
            {
                UserDialogs.Instance.HideLoading();
                UserDialogs.Instance.Alert(AppResources.PhotoEdit_uploadErr, AppResources.Dialog_error);
                return;
            }

            photo.Url = resp.Data[0];
            photo.ThumbUrl = resp.Data[1];

            // post photo
            ServerResponse<Photo> photoResp = await NetworkUtils.PostRequest<Photo>("api/Photo", JsonConvert.SerializeObject(photo));

            UserDialogs.Instance.HideLoading();

            if (photoResp.Response == null || !photoResp.Response.IsSuccessStatusCode)
            {
                UserDialogs.Instance.Alert(AppResources.PhotoEdit_uploadErr, AppResources.Dialog_error);
                return;
            }

            DependencyService.Get<IReminder>().SetAlarm(photo.UserCondition);

            App.ShouldUpdate = true;
            GalleryPage.NeedsUpdate = true;

            await UserDialogs.Instance.AlertAsync(AppResources.PhotoEdit_uploadSuccess, AppResources.ChangePass_successTitle);

            try
            {
                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                {
                    await App.Homepage.Navigation.PopAsync();
                });
            }
            catch (Exception e)
            {
                UserDialogs.Instance.Alert(e.Message, AppResources.Error_update);
            }
        }

        private async void CommitChanges()
        {
            photo.Treatment = treatment.Text;
            photo.Notes = notes.Text;
            photo.PhotoDescription = description.Text;

            if (photo.Url == null && photo.InMemory != null)
            {
                // Photo is new, hasn't been uploaded yet. Prompt to see if should upload now
                // or add to the upload queue for later

                bool shouldUploadNow = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                {
                    Title = AppResources.PhotoEdit_QueueOrUpload_title,
                    Message = AppResources.PhotoEdit_QueueOrUpload_message,
                    OkText = AppResources.PhotoEdit_QueueOrUpload_now,
                    CancelText = AppResources.PhotoEdit_QueueOrUpload_queue,
                });

                if (shouldUploadNow)
                {
                    UploadNewPhoto();
                }
                else
                {
                    // Add to pending uploads in DB
                    string filename = DependencyService.Get<ISaveAndLoad>().SaveLocalCopy(photo.InMemory, DateTime.Now.ToString("yyyyMMddHHmmss"));

                    App.db.AddUpload(new PendingPhotoUpload
                    {
                        Id = new Random().Next(),
                        CreatedAt = DateTime.Now,
                        LocalPhotoLoc = filename,
                        Notes = photo.Notes,
                        PhotoDescription = photo.PhotoDescription,
                        Rating = photo.Rating,
                        Treatment = photo.Treatment,
                        UserConditionId = photo.UserCondition.Id,
                        UserConditionJson = JsonConvert.SerializeObject(photo.UserCondition, new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
                    });

                    await UserDialogs.Instance.AlertAsync(AppResources.PhotoEdit_AddedToQueue, AppResources.ChangePass_successTitle);
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await App.Homepage.Navigation.PopAsync();
                    });
                }
            }
            else
            {
                // update photo
                UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);
                ServerResponse<Photo> photoResp = await NetworkUtils.PutRequest<Photo>("api/Photo/" + photo.Id, JsonConvert.SerializeObject(photo));
                UserDialogs.Instance.HideLoading();

                if (photoResp.Response == null || !photoResp.Response.IsSuccessStatusCode)
                {
                    UserDialogs.Instance.Alert(AppResources.PhotoEdit_updateErr, AppResources.Error_update);
                    return;
                }

                App.ShouldUpdate = true;

                await UserDialogs.Instance.AlertAsync(AppResources.PhotoEdit_updateSuccess, AppResources.Dialog_success);

                try
                {
                    Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                    {
                        await App.Homepage.Navigation.PopAsync();
                        await App.Homepage.Navigation.PopAsync();
                    });
                }
                catch (Exception e)
                {
                    UserDialogs.Instance.Alert(e.Message, AppResources.Error_update);
                }
            }
        }

        private async void DeletePhoto()
        {
            bool delete = await UserDialogs.Instance.ConfirmAsync(
                AppResources.PhotoEdit_deleteConfirmMessage,
                AppResources.PhotoEdit_deleteConfirmTitle,
                AppResources.PhotoEdit_deleteConfirmYes,
                AppResources.Dialog_cancel
            );

            if (!delete) return;

            UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);
            bool deletionSuccess = await NetworkUtils.DeleteRequest("api/Photo/" + photo.Id);
            UserDialogs.Instance.HideLoading();

            if (!deletionSuccess)
            {
                UserDialogs.Instance.Alert(AppResources.PhotoEdit_deleteErr, AppResources.Error_delete);
                return;
            }
            else
            {
                App.ShouldUpdate = true;
                GalleryPage.NeedsUpdate = true;

                await UserDialogs.Instance.AlertAsync(AppResources.PhotoEdit_deleteSuccess, AppResources.Dialog_success);

                Xamarin.Forms.Device.BeginInvokeOnMainThread(async () =>
                {
                    await App.Homepage.Navigation.PopAsync();
                    await App.Homepage.Navigation.PopAsync();
                });
            }
        }
    }
}
