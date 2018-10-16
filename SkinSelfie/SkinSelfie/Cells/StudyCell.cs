using SkinSelfie.AppModels;
using Xamarin.Forms;

namespace SkinSelfie.Cells
{
    public class StudyCell : ViewCell
    {
        public Label NameLabel;
        public Label EmailLabel;

        public StudyCell()
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
                HorizontalOptions = LayoutOptions.FillAndExpand,
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
            var c = (WrappedCell<StudyEnrolment>)BindingContext;
            if (c == null || c.Item == null) return;

            NameLabel.Text = c.Item.Study.Name;
            EmailLabel.Text = c.Item.Study.Manager.Name;
            base.OnBindingContextChanged();
        }
    }
}
