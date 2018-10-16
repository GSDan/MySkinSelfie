using SkinSelfie.AppModels;
using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class PhotoCarouselPage : CarouselPage
    {
        public static Photo currentPhoto;
        private int initialOpen = 0;
        public int selectedIndex = 0;

        public PhotoCarouselPage(Photo current)
        {
            ToolbarItems.Add(new ToolbarItem
            {
                Text = AppResources.PhotoCarousel_Edit,
                Icon = "ic_create_white_24dp.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => EditPhoto())
            });


            CurrentPageChanged += PhotoCarouselPage_CurrentPageChanged;

            ItemsSource = App.SelectedCondition.Photos;
            ItemTemplate = new DataTemplate(() =>
            {
                return new PhotoPage(this);
            });

            for(int i = 0; i < App.SelectedCondition.Photos.Count; i++)
            {
                if(App.SelectedCondition.Photos[i].Id == current.Id)
                {
                    initialOpen = i;
                    break;
                }
            }

            SelectedItem = ((List<Photo>)ItemsSource)[initialOpen];
        }

        private void PhotoCarouselPage_CurrentPageChanged(object sender, System.EventArgs e)
        {
            selectedIndex = Children.IndexOf(CurrentPage);
            currentPhoto = ((List<Photo>)ItemsSource)[selectedIndex];
            Title = Utils.GetDateString(currentPhoto.CreatedAt);
        }

        private void EditPhoto()
        {
            Navigation.PushAsync(new PhotoEditPage(currentPhoto));
        }
    }
}
