using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class TermsOfUsePage : ContentPage
    {
        public TermsOfUsePage()
        {
            Title = AppResources.Terms_Title;

            ScrollView scroller = new ScrollView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Content = new StackLayout
                {
                    Padding = new Thickness(20),
                    Spacing = 30,
                    Children = {
                        new Image { Source = "icon_badge.png", HorizontalOptions = LayoutOptions.Center, WidthRequest = 120},

                        new Label { Text = AppResources.Terms_Title, FontAttributes= FontAttributes.Bold, FontSize=25},
                        new Label { Text = AppResources.Terms_Intro, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_UseTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_UseBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_ProhibitedTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_ProhibitedBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_ContentTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_ContentBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_BehaviourTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_BehaviourBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_MonitoringTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_MonitoringBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_TerminationTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_TerminationBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_LinksTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_LinksBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_RightsTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_RightsBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_WarrantiesTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_WarrantiesBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_IndeminityTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_IndemnityBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_JurisdictionTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_JurisdictionBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_SeverabilityTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_SeverabilityBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_WaiverTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_WaiverBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_NoticesTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_NoticesBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 },

                        new Label { Text = AppResources.Terms_ContactTitle,  FontAttributes= FontAttributes.Bold,  FontSize = 20},
                        new Label { Text = AppResources.Terms_ContactBody, HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 16 }
                    }
                }
            };

            Content = scroller;
        }
    }
}
