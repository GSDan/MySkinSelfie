using SkinSelfie.AppModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkinSelfie.Cells
{
    public class ShareCell : ViewCell
    {
        public Label ConditionLabel;
        public Label EmailLabel;
        public Label DateLabel;

        public ShareCell()
        {
            ConditionLabel = new Label
            {
                FontSize = 15,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            };
            EmailLabel = new Label
            {
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.EndAndExpand
            };
            DateLabel = new Label
            {
                FontSize = 13,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                VerticalOptions = LayoutOptions.Center,
            };

            View = new StackLayout
            {
                Padding = new Thickness(10, 0),
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new StackLayout
                    {
                        Padding = new Thickness(0, 8),
                        Orientation = StackOrientation.Vertical,
                        Children =
                        {
                            ConditionLabel,
                            EmailLabel,
                        }
                    },                    
                    DateLabel
                }
            };
        }

        protected override void OnBindingContextChanged()
        {
            var c = (WrappedCell<Share>)BindingContext;
            if (c == null || c.Item == null) return;

            ConditionLabel.Text = c.Item.UserCondition.Condition;
            EmailLabel.Text = c.Item.SharedEmail;
            DateLabel.Text = c.Item.ExpireDate.ToString("MM/dd/yyyy");
            base.OnBindingContextChanged();
        }
    }
}
