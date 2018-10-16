using System;
using System.Collections.Generic;
using SkinSelfie.AppModels;
using SkinSelfie.Pages;
using Xamarin.Forms;

namespace SkinSelfie
{
    public class MainTabPage : TabbedPage
    {
        public MainTabPage()
        {
            Title = AppResources.Main_title;

            var mainPage = new MainPage
            {
                Title = "Home",
                Icon = "ic_home.png"
            };
            Children.Add(mainPage);

            var uploadsPage = new UploadsQueuePage
            {
                Title = AppResources.Uploads_btn,
                Icon = "ic_cloud_upload_white_24dp.png"
            };
            Children.Add(uploadsPage);

            var settingsPage = new AppSettingsPage
            {
                Title = AppResources.Main_settings,
                Icon = "ic_settings.png"
            };
            Children.Add(settingsPage);

            var helpPage = new AppHelpPage
            {
                Title = AppResources.Main_help,
                Icon = "ic_info_black_24dp.png"
            };
            Children.Add(helpPage);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var uploads = (List<PendingPhotoUpload>)App.db.GetUploads();

            Children[1].Icon = (uploads?.Count > 0) ? "ic_new_releases.png" : "ic_cloud_upload_white_24dp.png";
        }
    }
}
