using Acr.UserDialogs;
using Newtonsoft.Json;
using SkinSelfie.AppModels;
using SkinSelfie.Cells;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace SkinSelfie.Pages
{
    public class MainPage : ContentPage
    {
        List<UserCondition> Conditions;
		List<UserCondition> FinishedConditions;
        StackLayout welcomeLayout;
        ScrollView scroller;

        public MainPage()
        {
            // Show welcome message if no regions present
            welcomeLayout = new StackLayout
            {
                Padding = new Thickness(25),
                Spacing = 35,
                Children = {
                    new Label { Text = AppResources.Main_welcome1, FontAttributes= FontAttributes.Bold, FontSize=28},
                    new Label { Text = AppResources.Main_welcome2, FontSize = 18},
                    new Label { Text = AppResources.Main_welcome3, FontSize = 18 },
                    new Label { Text = AppResources.Main_welcome4, FontSize = 18, FontAttributes = FontAttributes.Bold }
                }
            };
            welcomeLayout.IsVisible = false;
            welcomeLayout.IsEnabled = false;

			Conditions = new List<UserCondition> ();
			FinishedConditions = new List<UserCondition> ();

            StackLayout layout = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.Fill
            };

            scroller = new ScrollView
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.Fill,
                Padding = new Thickness(0, 0, 0, 15)
            };

            layout.Children.Add(scroller);

            Content = layout;

			ToolbarItems.Add(new ToolbarItem
			{
				Text = AppResources.Main_add,
				Icon = "ic_create_new_folder_white_24dp.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command(() => Navigation.PushAsync(new ConditionEditPage()))
			});
		}

        private void OpenQuestionnaire(bool confirm)
        {
            if(confirm)
            Device.OpenUri(new Uri("http://goo.gl/forms/NefvZC4vxM"));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (App.ShouldUpdate)
            {
                PrepareData();
            }
        }

        private Grid PrepareGrid(List<UserCondition> toDisplay)
        {
            int colCount = (int)(App.screenWidthInches * 0.87);

            Grid contentGrid = new Grid
            {
                VerticalOptions = LayoutOptions.Fill,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                RowDefinitions = new RowDefinitionCollection(),
                ColumnDefinitions = new ColumnDefinitionCollection(),
                RowSpacing = 20,
                Padding = new Thickness(0, 0, 0, 10)
            };

            for (int i = 0; i < colCount; i++)
            {
                contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200, GridUnitType.Star) });
            }

            for (int i = 0; i < toDisplay.Count; i++)
            {
                int currentCol = i % colCount;

                // Add a row to the grid if needed
                if (currentCol == 0)
                {
                    contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                int currentRow = contentGrid.RowDefinitions.Count - 1;

                UserConditionCell thisView = new UserConditionCell(toDisplay[i]);
                contentGrid.Children.Add(thisView, currentCol, currentRow);
            }

            return contentGrid;
        }

        private void UpdateGrids()
        {
            bool welcomeOnly = (Conditions.Count == 0 && FinishedConditions.Count == 0);
            welcomeLayout.IsVisible = welcomeOnly;
            welcomeLayout.IsEnabled = welcomeOnly;

            if (welcomeOnly)
            {
                scroller.Content = welcomeLayout;
                return;
            }

			StackLayout gridStack = new StackLayout ();
            Grid unfinishedGrid;
            Grid finishedGrid;

            if (Conditions.Count > 0)
            {
                unfinishedGrid = PrepareGrid(Conditions);
                gridStack.Children.Add(new ContentView
                {
                    Content = new Label
                    {
                        Text = AppResources.Main_activeCollections,
                        FontSize = 21,
                        FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center
                    },
                    Padding = new Thickness(10)
                });
                gridStack.Children.Add(unfinishedGrid);
            }

            if (FinishedConditions.Count > 0)
            {
                finishedGrid = PrepareGrid(FinishedConditions);
                gridStack.Children.Add(new ContentView
                {
                    Content = new Label
                    {
                        Text = AppResources.Main_finishedCollections,
                        FontSize = 21,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center
                    },
					Padding = new Thickness(10)
                });
                gridStack.Children.Add(finishedGrid);
            }

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() => {
                scroller.Content = gridStack;
            });
        }

        public async void PrepareData()
        {
            if (App.bodyParts == null || App.bodyParts.Count == 0)
            {
                UserDialogs.Instance.ShowLoading(title: AppResources.Main_loadingAppData);
                await App.FetchBodyParts();
                UserDialogs.Instance.HideLoading();
            }

            List<ConditionData> cachedConditions = (List<ConditionData>)App.db.GetConditions();
            if (cachedConditions == null) cachedConditions = new List<ConditionData>();


            UserDialogs.Instance.ShowLoading(title: AppResources.Main_loadingUserContent);
            ServerResponse<List<UserCondition>> returned = await NetworkUtils.GetRequest<List<UserCondition>>("api/UserConditions");
            if (returned == null || returned.Data == null)
            {
                UserDialogs.Instance.HideLoading();

                if (returned != null && 
                    returned.Response != null && 
                    returned.Response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    UserDialogs.Instance.Alert(AppResources.Main_authErr);
                    App.SignOut();
                    return;
                }

                UserDialogs.Instance.ShowError(AppResources.OfflineModeWarning, 3000);
                returned = new ServerResponse<List<UserCondition>>();
                returned.Data = new List<UserCondition>();

                foreach (ConditionData d in cachedConditions)
                {
                    returned.Data.Add(d.GetCondition());
                }
            }

			Conditions = new List<UserCondition>();
			FinishedConditions = new List<UserCondition>();

            foreach (UserCondition cond in returned.Data)
            {
                cond.Owner = App.user;

                ReminderData data = App.db.GetReminder(cond.Id);

                if (data != null)
                {
                    cond.reminder = (UserCondition.ReminderType)data.ReminderType;
                }

                if (cond.Finished)
                {
					FinishedConditions.Add(cond);
                }
                else
                {
					Conditions.Add(cond);
                }

                App.db.AddOrUpdateCondition(new ConditionData(cond));
            }
				
            App.ShouldUpdate = false;
            UserDialogs.Instance.HideLoading();

            UpdateGrids();            
        }
    }
}
