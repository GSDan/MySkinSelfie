using Acr.UserDialogs;
using Newtonsoft.Json;
using SkinSelfie.AppModels;
using SkinSelfie.Pages;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Platform.Device;
using System.Net.Http;
using System.Net.Http.Headers;
using FFImageLoading;
using SkinSelfie.Interfaces;
using System.Linq;

namespace SkinSelfie
{
    public class App : Application
    {
		public static App SkinSelfie;
        public static DatabaseManager db;
        public static User user;
        public static RangeEnabledObservableCollection<BodyPart> bodyParts;
        public static NavigationPage Homepage;
        public static bool Locked;
        public static Random rand = new Random();
        public static IDisplay display;
        public static double screenHeightInches;
        public static double screenWidthInches;
        public static double screenSizeInches;
        public static UserCondition SelectedCondition { get; set; }
        
        public static bool ShouldUpdate = true;

        public App()
        {
			SkinSelfie = this;

            if (Device.OS == TargetPlatform.iOS || Device.OS == TargetPlatform.Android)
            {
                var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
                AppResources.Culture = ci; // set the RESX for resource localization
                DependencyService.Get<ILocalize>().SetLocale(ci); // set the Thread for locale-aware methods
            }

            display = Resolver.Resolve<IDevice>().Display;
			screenHeightInches = display.ScreenHeightInches ();
			screenWidthInches = display.ScreenWidthInches ();
			screenSizeInches = display.ScreenSizeInches ();

			// The scale property doesn't seem very reliable
			// Only apply it if the device is pretending to be bigger than the resolution suggests
			// (Tested on GS3 Mini, Nexus 6p and Nexus 7)
			if (display.Xdpi < 200) {
				screenHeightInches /= display.Scale;
				screenWidthInches /= display.Scale;
				screenSizeInches /= display.Scale;
			}

            ShouldUpdate = true;
            db = new DatabaseManager();
            bodyParts = new RangeEnabledObservableCollection<BodyPart>();

            user = db.GetUser();

            if (user == null)
            {
                OpenWelcomePage();
            }
            else
            {
                user.Token = db.GetToken();

                if (!user.GDPRConsent || user.Token == null || user.Token.expires < DateTime.Now)
                {
                    OpenWelcomePage();
                }
                else
                {
					InitializeImageLoading();
                    Homepage = new NavigationPage(new MainTabPage());
                    MainPage = Homepage;
                }
            }
        }

		private static void InitializeImageLoading()
		{
			// Initialize image loader
			HttpClient client = new HttpClient();
			client.BaseAddress = new Uri(ConfidentialData.SiteUrl);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.user.Token.access_token);
			ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
			{
				HttpClient = client
			});
		}

        private void OpenWelcomePage()
        {
            Homepage = new NavigationPage(new WelcomePage())
            {
                BarBackgroundColor = Color.FromHex("00BCD4"),
                BarTextColor = Color.White
            };
            MainPage = Homepage;
        }

        private static ServerResponse<T> GenServerResp<T>(bool success)
        {
            return new ServerResponse<T>
            {
                Response = new System.Net.Http.HttpResponseMessage { StatusCode = (success)? HttpStatusCode.OK : HttpStatusCode.BadRequest },
                BadRequest = (success)? null : new BadRequest { Message = "Bad Request", ModelState = new Dictionary<string, string[]>()}
            };
        }

        public static async Task<ServerResponse<object>> CreateAccount(UserCreate data, User account)
        {
            if (data.Password != data.ConfirmPassword)
            {
                ServerResponse<object> failResp = GenServerResp<object>(false);
                failResp.BadRequest.ModelState.Add(AppResources.App_passwordFailTitle, new string[] { AppResources.App_passwordFailMessage });
                return failResp;
            }
                
            ServerResponse<object> resp = await NetworkUtils.PostRequest<object>("api/Account/Register", JsonConvert.SerializeObject(data));

			if (resp.Response == null || !resp.Response.IsSuccessStatusCode)
            {
                return resp;
            }

            // Account created, now we need to get a token
            ServerResponse<AccessToken> token = await FetchToken(account.Email, data.Password);
			if (token.Response == null || !token.Response.IsSuccessStatusCode)
            {
                ServerResponse<object> failResp = GenServerResp<object>(false);
                failResp.BadRequest.ModelState.Add(AppResources.App_loginError, new string[] { AppResources.Error_connection });
                return failResp;
            }

            ServerResponse<User> userResp = await NetworkUtils.PostRequest<User>("api/User", JsonConvert.SerializeObject(account));

			if (userResp.Response == null || !userResp.Response.IsSuccessStatusCode)
            {
                ServerResponse<object> failResp = GenServerResp<object>(false);
                failResp.BadRequest = userResp.BadRequest;
                return failResp;
            }

            App.user = userResp.Data;
            App.user.Token = token.Data;
            App.user.FirstStart = DateTime.Now;
            App.user.CameraTipsSeen = 0;
            App.user.GDPRConsent = true;

            db.AddUser(App.user);

            ServerResponse<object> final = GenServerResp<object>(true);
            final.Data = App.user;
            return final;
        }

        public static async Task<ServerResponse<AccessToken>> FetchToken(string email, string password)
        {
            UserCreate data = new UserCreate
            {
                Username = email,
                Password = password,
                Grant_type = "password"
            };

            ServerResponse<AccessToken> tokenResp = await NetworkUtils.FetchToken(data);

			if(tokenResp.Response != null && tokenResp.Response.IsSuccessStatusCode)
            {
                if(user == null)
                {
                    user = new User
                    {
                        Email = email
                    };
                }

                user.Token = tokenResp.Data;
                db.AddToken(tokenResp.Data);
				InitializeImageLoading();
            }

            return tokenResp;
        }

        public static async Task<ServerResponse<User>> FetchAccount(string email)
        {
            ServerResponse<User> returned = await NetworkUtils.GetRequest<User>("api/User/?email=" + email);

            if(returned.Response.IsSuccessStatusCode)
            {
                AccessToken token = user.Token;
                user = returned.Data;
                user.Token = token;
                user.FirstStart = DateTime.Now;
                user.GDPRConsent = true;
                db.AddUser(user);
            }

            return returned;
        }

        public static async void SignOut()
        {
			if (user != null) {
				db.DeleteUser (user.Id);
				db.DeleteToken (user.Token.Id);
				user = null;
			}

            SkinSelfie.OpenWelcomePage();
            await Homepage.PopToRootAsync();
            ShouldUpdate = true;
        }

        public static async Task FetchBodyParts()
        {
            ServerResponse<List<BodyPart>> returned = await NetworkUtils.GetRequest<List<BodyPart>>("api/bodyparts");
            if (returned.Data == null)
            {
                List<BodyPart> fromDb = (List<BodyPart>)db.GetBodyParts();
                if(fromDb == null || fromDb.Count == 0)
                {
                    UserDialogs.Instance.ShowError(AppResources.Error_connection);
                    App.SignOut();
                    return;
                }

                returned.Data = fromDb;
            }
            else
            {
                db.UpdateBodyParts(returned.Data);
            }

            bodyParts.Clear();

            TranslationDict trans = JsonConvert.DeserializeObject<TranslationDict>(AppResources.Trans_BodyPartsSkinRegions);

            for (int i =0; i < returned.Data.Count; i++)
            {
                returned.Data[i].Translate(trans.Translations);
            }

            bodyParts.InsertRange(returned.Data);
        }

        public static async Task ResetNavStackToMain()
        {
            try
            {
                App.Homepage = new NavigationPage(new MainTabPage());
				SkinSelfie.MainPage = App.Homepage;
				await App.Homepage.PopToRootAsync();
            }
            catch
            {
                ShouldUpdate = true;
            }
        }

        public static async void AddPinIfWanted()
        {
            bool res = await UserDialogs.Instance.ConfirmAsync(
                AppResources.App_pinPromptMessage,
                AppResources.App_pinPromptTitle,
                AppResources.App_pinPromptYes,
                AppResources.App_pinPromptNo);
			if(res)
            {
               await App.Homepage.Navigation.PushAsync(new UnlockPage(true, true));
            }

			if (user != null && db != null) 
			{
				user.CodePrompted = true;
				db.AddUser(user);
			}
        }

        protected override void OnStart()
        {
			if (user == null || db == null) return;

            if(!user.CodePrompted)
            {
                AddPinIfWanted();
            }
            else if(!string.IsNullOrWhiteSpace(user.AppCode))
            {
                MainPage.Navigation.PushModalAsync(new UnlockPage());
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            if (user != null && !string.IsNullOrWhiteSpace(user.AppCode) && !App.Locked)
            {
                MainPage.Navigation.PushModalAsync(new UnlockPage());
            }
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        
    }
}
