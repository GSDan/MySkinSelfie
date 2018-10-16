using Acr.UserDialogs;
using Newtonsoft.Json;
using SkinSelfie.AppModels;
using SkinSelfie.Interfaces;
using System;

using Xamarin.Forms;
using System.Threading.Tasks;

namespace SkinSelfie.Pages
{
    public class ConditionEditPage : ContentPage
    {
        BodyPart selectedBp;
        SkinRegion selectedSkinRegion;
        UserCondition editingCondition;

        Picker bodyPartPicker;
        Picker regionPicker;
        DatePicker startDatePicker;
        Entry conditionEntry;
        Switch passcodeSwitch;
        Switch activeSwitch;
        Picker reminderTypePicker;

        string code = "";

        public ConditionEditPage(UserCondition existing = null)
        {
            editingCondition = existing;

            Title = (existing == null) ? 
                AppResources.ConditionEdit_titleNew : 
                string.Format(AppResources.ConditionEdit_titleEdit, existing.Condition);

            ToolbarItems.Add(new ToolbarItem
            {
                Text = AppResources.ConditionEdit_finished,
                Icon = "ic_done_white_24dp.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => CommitChanges(existing))
            });

			Label blurb = new Label
			{
				Text = AppResources.ConditionEdit_blurbNew,
				FontSize = 16,
				FontAttributes = FontAttributes.Bold
			};

            if (existing != null)
            {
				ToolbarItem deleteItem = new ToolbarItem {
					Text = AppResources.ConditionEdit_delete,
					Order = ToolbarItemOrder.Secondary,
					Command = new Command (() => DeleteCondition (existing))
				};

				Device.OnPlatform (
					iOS: () => {
						deleteItem.Icon = "ic_delete.png";
					});

				ToolbarItems.Add(deleteItem);

                blurb.Text = AppResources.ConditionEdit_blurbEdit;
            }
				
            bodyPartPicker = new Picker
            {
                Title = AppResources.ConditionEdit_bodyPart
            };
            bodyPartPicker.SelectedIndexChanged += BodyPartPicker_SelectedIndexChanged;

            regionPicker = new Picker
            {
                Title = AppResources.ConditionEdit_skinRegion
            };

            for (int i = 0; i < App.bodyParts.Count; i++)
            {
                bodyPartPicker.Items.Add(App.bodyParts[i].DisplayName);

                if (existing != null)
                {
                    if (existing.SkinRegion.BodyPart.Id == App.bodyParts[i].Id)
                    {
                        selectedBp = App.bodyParts[i];
                        bodyPartPicker.SelectedIndex = i;
                    }
                }
                else if (i == 0)
                {
                    selectedBp = App.bodyParts[0];
                    bodyPartPicker.SelectedIndex = i;
                }
            }

            conditionEntry = new Entry
            {
                Placeholder = AppResources.ConditionEdit_conditionName
            };

			if (existing != null) 
			{
				conditionEntry.Text = existing.Condition;
				code = (existing.Passcode != null)? existing.Passcode.ToString() : "";
			}

            startDatePicker = new DatePicker
            {
                Format = "dd MMMM yyyy",
                MaximumDate = DateTime.Now + TimeSpan.FromDays(1),
                MinimumDate = App.user.BirthDate + TimeSpan.FromDays(1),
                Date = (existing == null) ? App.user.BirthDate : existing.StartDate
            };

            passcodeSwitch = new Switch
            {
                IsToggled = (existing == null) ? false : existing.Passcode != null,
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End
            };

            passcodeSwitch.Toggled += PasscodeSwitch_OnChanged;

            StackLayout passLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new Label { Text = AppResources.ConditionEdit_passcode, FontSize = 16, HorizontalOptions = LayoutOptions.StartAndExpand },
                    passcodeSwitch
                }
            };

            StackLayout activeLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };

            if (existing != null)
            {
                activeSwitch = new Switch
                {
                    IsToggled = (existing == null) ? false : existing.Finished,
                    VerticalOptions = LayoutOptions.Start,
                    HorizontalOptions = LayoutOptions.End
                };

                passcodeSwitch.Toggled += PasscodeSwitch_OnChanged;

                activeLayout.Children.Add(new Label { Text = AppResources.ConditionEdit_markFinished, FontSize = 16, HorizontalOptions = LayoutOptions.StartAndExpand });
                activeLayout.Children.Add(activeSwitch);
            }
            

            reminderTypePicker = new Picker
            {
                Title = AppResources.ConditionEdit_reminderFreq
            };

            reminderTypePicker.Items.Add(UserCondition.ReminderType.None.ToString());
            reminderTypePicker.Items.Add(UserCondition.ReminderType.Daily.ToString());
            reminderTypePicker.Items.Add(UserCondition.ReminderType.Weekly.ToString());
            reminderTypePicker.Items.Add(UserCondition.ReminderType.Monthly.ToString());
            reminderTypePicker.SelectedIndex = (existing == null) ? 0 : (int)existing.reminder;

            StackLayout reminderLayout = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new Label { Text = AppResources.ConditionEdit_reminderEnabled, FontSize = 16, HorizontalOptions = LayoutOptions.StartAndExpand },
                    reminderTypePicker
                }
            };

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Spacing = 15,
                    Padding = new Thickness(30),
                    Children =
                    {
                        blurb,
                        new Label {Text = AppResources.ConditionEdit_bodyPartHead },
                        bodyPartPicker,
                        new Label {Text = AppResources.ConditionEdit_skinRegionHead },
                        regionPicker,
                        new Label {Text = AppResources.ConditionEdit_conditionNameHead },
                        conditionEntry,
                        new Label {Text = AppResources.ConditionEdit_startDate },
                        startDatePicker,
                        activeLayout,
                        passLayout,
                        reminderLayout
                    }
                }
            };
        }

        private void PasscodeSwitch_OnChanged(object sender, ToggledEventArgs e)
        {
            if (passcodeSwitch.IsToggled) GetNewPassword();
        }

        private async void GetNewPassword()
        {
            PromptResult result = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                IsCancellable = true,
                InputType = InputType.NumericPassword,
                Message = AppResources.Settings_addPINEnter,
                Placeholder = code
            });

            if (result.Ok && !string.IsNullOrWhiteSpace(result.Text))
            {
                code = result.Text;
            }
            else
            {
                passcodeSwitch.IsToggled = false;
            }
        }

        private void BodyPartPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (bodyPartPicker.SelectedIndex == -1)
            {
                bodyPartPicker.SelectedIndex = 0;
            }

            selectedBp = GetBPFromName(bodyPartPicker.Items[bodyPartPicker.SelectedIndex]);
            UpdateRegionPicker();
        }

        private void UpdateRegionPicker()
        {
            regionPicker.Items.Clear();

            for (int i = 0; i < selectedBp.GetSkinRegions().Count; i++)
            {
                regionPicker.Items.Add(selectedBp.GetSkinRegions()[i].DisplayName);

                if (editingCondition != null && selectedBp.Id == editingCondition.SkinRegion.BodyPart.Id)
                {
                    if (selectedBp.GetSkinRegions()[i].Id == editingCondition.SkinRegion.Id)
                    {
                        selectedSkinRegion = selectedBp.GetSkinRegions()[i];
                        regionPicker.SelectedIndex = i;
                    }
                }
                else if (i == 0)
                {
                    selectedSkinRegion = selectedBp.GetSkinRegions()[i];
                    regionPicker.SelectedIndex = 0;
                }
            }
        }

        private async void CommitChanges(UserCondition existing)
        {
            if (string.IsNullOrWhiteSpace(conditionEntry.Text))
            {
                UserDialogs.Instance.Alert(AppResources.ConditionEdit_conditionNameError);
                return;
            }

            selectedSkinRegion = GetSRFromName(regionPicker.Items[regionPicker.SelectedIndex]);
            selectedSkinRegion.BodyPart = selectedBp;

            UserCondition newCondition = new UserCondition
            {
                Owner = App.user,
                StartDate = startDatePicker.Date,
                SkinRegion = selectedSkinRegion,
                Condition = conditionEntry.Text
            };

            if (passcodeSwitch.IsToggled)
            {
                newCondition.Passcode = int.Parse(code);
            }

            if (existing != null)
            {
                if (activeSwitch != null)
                {
                    newCondition.Finished = activeSwitch.IsToggled;
                }

                newCondition.Id = existing.Id;
                newCondition.Photos = existing.Photos;
                newCondition.Owner = existing.Owner;

                UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);

                string json = JsonConvert.SerializeObject(newCondition,
                    new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                ServerResponse<Photo> photoResp = await NetworkUtils.PutRequest<Photo>("api/UserConditions/" + existing.Id, json);
                UserDialogs.Instance.HideLoading();

				if (photoResp.Response == null || !photoResp.Response.IsSuccessStatusCode)
                {
                    UserDialogs.Instance.Alert(AppResources.ConditionEdit_updateError, AppResources.Error_update);
                    return;
                }

                newCondition.reminder = (UserCondition.ReminderType)reminderTypePicker.SelectedIndex;
                UpdateLocalDB(newCondition);

                UserDialogs.Instance.HideLoading();
                App.ShouldUpdate = true;

				await UserDialogs.Instance.AlertAsync(AppResources.ConditionEdit_updateSuccess, AppResources.Dialog_success);

				await App.Homepage.Navigation.PopAsync();
				App.Homepage.Navigation.PopAsync();
                
            }
            else
            {
                string message = string.Format(AppResources.ConditionEdit_confirm, conditionEntry.Text, selectedSkinRegion.DisplayName);

                if (!await UserDialogs.Instance.ConfirmAsync(message, AppResources.ConditionEdit_confirmTitle, AppResources.Dialog_confirm, AppResources.Dialog_cancel)) return;

				UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);

                ServerResponse<UserCondition> userResp = await NetworkUtils.PostRequest<UserCondition>("api/UserConditions",
                JsonConvert.SerializeObject(newCondition, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

				UserDialogs.Instance.HideLoading();

				if (userResp.Response == null || !userResp.Response.IsSuccessStatusCode)
                {
                    await UserDialogs.Instance.AlertAsync(AppResources.ConditionEdit_submitError + userResp.Response.ReasonPhrase, AppResources.Dialog_error);
                    return;
                }

                newCondition.reminder = (UserCondition.ReminderType)reminderTypePicker.SelectedIndex;
                UpdateLocalDB(newCondition);

                App.ShouldUpdate = true;

                App.Homepage.Navigation.PopAsync();

            }
        }

        private BodyPart GetBPFromName(string name)
        {
            foreach (BodyPart bp in App.bodyParts)
            {
                if (bp.DisplayName == name) return bp;
            }
            return null;
        }

        private SkinRegion GetSRFromName(string name)
        {
            foreach (SkinRegion sr in selectedBp.GetSkinRegions())
            {
                if (sr.DisplayName == name) return sr;
            }
            return null;
        }

        private void UpdateLocalDB(UserCondition condition)
        {
            IReminder reminderManager = DependencyService.Get<IReminder>();
            if (condition.reminder == UserCondition.ReminderType.None)
            {
                reminderManager.CancelAlarm(condition);
            }
            else
            {
                reminderManager.SetAlarm(condition); //TODO
            }

            App.db.AddOrUpdateReminder(new ReminderData
            {
                UserConditionId = condition.Id,
                ReminderType = reminderTypePicker.SelectedIndex
            });

            App.db.AddOrUpdateCondition(new ConditionData(condition));
        }

        private async void DeleteCondition(UserCondition existing)
        {
            if (existing == null) return;

            bool delete = await UserDialogs.Instance.ConfirmAsync(
                AppResources.ConditionEdit_deleteWarningMessage, 
                AppResources.ConditionEdit_deleteWarningTitle, 
                AppResources.Dialog_delete, 
                AppResources.Dialog_cancel
            );

            if (!delete) return;

            UserDialogs.Instance.ShowLoading(AppResources.Dialog_loading);
            bool deletionSuccess = await NetworkUtils.DeleteRequest("api/UserConditions/" + existing.Id);
            UserDialogs.Instance.HideLoading();

            if (!deletionSuccess)
            {
                UserDialogs.Instance.Alert(AppResources.ConditionEdit_deleteError, AppResources.Error_delete);
                return;
            }
            else
            {
                App.ShouldUpdate = true;

                App.db.DeleteReminder(existing.Id);
                App.db.DeleteCondition(existing.Id);
                App.db.DeleteUploadsForCondition(existing.Id);

                await UserDialogs.Instance.AlertAsync(
					AppResources.ConditionEdit_deleteSuccessMessage,
                    AppResources.ConditionEdit_deleteSuccessTitle
                );

				await App.Homepage.Navigation.PopAsync();
				await App.Homepage.Navigation.PopAsync();
            }
        }

    }
}
