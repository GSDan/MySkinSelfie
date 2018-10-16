using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SkinSelfie.Cells
{
    public class ButtonCell : ViewCell
    {
        Action OnClick;

        public ButtonCell(string label, Action _onClick)
        {
            this.OnClick = _onClick;

            View = new StackLayout
            {
                Padding = new Thickness(15, 10),
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    new Label
                    {
                        Text = label,
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Center
                    }
                }
            };

            this.Tapped += ButtonCell_Tapped; ;
        }

        private void ButtonCell_Tapped(object sender, EventArgs e)
        {
            OnClick();
        }
    }
}
