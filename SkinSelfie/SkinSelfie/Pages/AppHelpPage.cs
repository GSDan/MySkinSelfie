using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class AppHelpPage : ContentPage
    {
        public AppHelpPage()
        {
            Title = AppResources.Help_title;

            ScrollView scroller = new ScrollView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Content = new StackLayout
                    {
                        Padding = new Thickness(25),
                        Spacing = 30,
                        Children = {
                        new Image { Source = "icon_badge.png", HorizontalOptions = LayoutOptions.Center, WidthRequest = 120},
                        new Label { Text = AppResources.Help_heading, FontAttributes= FontAttributes.Bold, FontSize=28},                        
                        new Label { Text = AppResources.Help_funcHeading,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Help_funcBody, FontSize = 18 },
                        new Label { Text = AppResources.Help_securityHeading,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Help_securityBody, FontSize = 18 },
                        new Label { Text = AppResources.Help_controlHeading,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Help_controlBody, FontSize = 18 },
                        new Label { Text = AppResources.Help_shareHeading,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Help_shareBody, FontSize = 18 },
                    }
                }
            };

            Content = scroller;
        }
    }
}
