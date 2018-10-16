using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SkinSelfie.AppModels;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkinSelfie
{
    public interface ISaveAndLoad
    {
        string SaveLocalCopy(byte[] data, string filename);
        byte[] LoadFromFile(string filename);
        void DeleteLocalCopy(string filename);
        long GetDirectorySize(string p);
    }

    public static class Utils
    {
        public static string GetDateString(DateTime date)
        {
            return string.Format("{0} {1}{2} {3}", date.ToString("HH:mm"),  date.Day, GetDaySuffix(date.Day), date.ToString("MMM yyyy"));
        }

        // http://stackoverflow.com/a/9130114
        public static string GetDaySuffix(int day)
        {
            switch (day)
            {
                case 1:
                case 21:
                case 31:
                    return "st";
                case 2:
                case 22:
                    return "nd";
                case 3:
                case 23:
                    return "rd";
                default:
                    return "th";
            }
        }

        public static string UppercaseFirst(string s, bool allOthersLower = false)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            if (allOthersLower) s = s.ToLower();

            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static Task<CodeEntry> InputBox(INavigation navigation, string title, string message)
        {
            // wait in this proc, until user did his input 
            var tcs = new TaskCompletionSource<CodeEntry>();

            var lblTitle = new Label { Text = title, HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
            var lblMessage = new Label { Text = message };
            var txtInput = new Entry { Text = "" };

            var btnOk = new Button
            {
                Text = AppResources.Dialog_confirm,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8),
            };
            btnOk.Clicked += async (s, e) =>
            {
                // close page
                var result = new CodeEntry { Entered = txtInput.Text, Cancelled = false };
                await navigation.PopModalAsync();
                // pass result
                tcs.SetResult(result);
            };

            var btnCancel = new Button
            {
                Text = AppResources.Dialog_cancel,
                HorizontalOptions = LayoutOptions.Fill,
                BackgroundColor = Color.FromRgb(0.8, 0.8, 0.8)
            };
            btnCancel.Clicked += async (s, e) =>
            {
                // close page
                var result = new CodeEntry { Entered = null, Cancelled = true };
                await navigation.PopModalAsync();
                // pass result
                tcs.SetResult(result);
            };

            var slButtons = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children = { btnOk, btnCancel },
            };

            var layout = new StackLayout
            {
                Padding = new Thickness(0, 40, 0, 0),
                VerticalOptions = LayoutOptions.StartAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Vertical,
                Children = { lblTitle, lblMessage, txtInput, slButtons },
            };

            // create and show page
            var page = new ContentPage();
            page.Content = layout;
            navigation.PushModalAsync(page);
            // open keyboard
            txtInput.Focus();

            // code is waiting her, until result is passed with tcs.SetResult() in btn-Clicked
            // then proc returns the result
            return tcs.Task;
        }
    }

    public class RangeEnabledObservableCollection<T> : ObservableCollection<T>
    {
        public void InsertRange(IEnumerable<T> items)
        {
            this.CheckReentrancy();
            foreach (var item in items)
                this.Items.Add(item);
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

}
