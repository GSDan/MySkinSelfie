using SkinSelfie.AppModels;
using Xamarin.Forms;

namespace SkinSelfie.Cells
{
    public class ContactCell : ViewCell
    {
        public Label NameLabel;
        public Label EmailLabel;

        public ContactCell()
        {
            NameLabel = new Label
            {
                FontSize = 19,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };
            EmailLabel = new Label
            {
                FontSize = 15,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.EndAndExpand
            };

            View = new StackLayout
            {
                Padding = new Thickness(15, 10),
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    NameLabel,
                    EmailLabel
                }
            };
        }

        protected override void OnBindingContextChanged()
        {
            var c = (WrappedCell<EmailContact>)BindingContext;
            if (c == null || c.Item == null) return;

            NameLabel.Text = c.Item.Name;
            EmailLabel.Text = c.Item.Email;
            base.OnBindingContextChanged();
        }
    }
}
