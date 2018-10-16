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
    public class ContactListPage : ContentPage
    {
        private List<WrappedCell<EmailContact>> WrappedItems = new List<WrappedCell<EmailContact>>();
        private ScrollView scroller;
        private Action<string> onChosen;

        public ContactListPage(List<EmailContact> given, Action<string> onChosen)
        {
            Title = AppResources.ContactList_title;
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

            WrappedItems = given.Select(item => new WrappedCell<EmailContact>() { Item = item, IsSelected = false }).ToList();

            ListView listView = new ListView();
            listView.ItemsSource = WrappedItems;
            listView.HasUnevenRows = true;
            listView.VerticalOptions = LayoutOptions.FillAndExpand;
            listView.ItemTemplate = new DataTemplate(typeof(ContactCell));
            listView.ItemSelected += ListView_ItemSelected;

            scroller.VerticalOptions = LayoutOptions.FillAndExpand;
            scroller.Content = listView;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            ((ListView)sender).SelectedItem = null;
            onChosen(((WrappedCell<EmailContact>)e.SelectedItem).Item.Email);
        }
    }
}