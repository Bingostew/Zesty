using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using Auth0.OidcClient;
using IdentityModel.OidcClient.Browser;
using Android.Support.V7.App;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Essentials;
using Javax.Security.Auth.Login;
using Android.Graphics;
using AndroidX.Work;
using Utility;
using Android.Webkit;

namespace ZestyKitchenHelper.Droid
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MainTheme", LaunchMode = LaunchMode.SingleTask)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] {Intent.CategoryDefault, Intent.CategoryBrowsable},
       DataScheme = "com.companyname.zestykitchenhelper", DataHost = "dev-4l7acohw.auth0.com", DataPathPrefix = "/android/com.companyname.zestykitchenhelper/callback")]
    public class MainActivity : Auth0ClientActivity
    {
        public static Auth0Client client;
        protected Android.Widget.Button loginButton;
        protected TextView skipLoginField;
        protected TextView loadingOverlay;
        protected TextView loadingText;
        private UserProfile userProfile;
        private const string expiration_work_id = "expWork";
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            ActivityMediator.Instance.Send(intent.DataString);
        }
        protected override void OnResume()
        {
            base.OnResume();
            Platform.OnResume();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            client = new Auth0Client(new Auth0ClientOptions()
            { Domain = "dev-4l7acohw.auth0.com", ClientId = "Srn3fq8ccb7dnBmskN5VNGG2A4A0XKz4" }) ;
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LoginPage);
            loadingOverlay = FindViewById<TextView>(Resource.Id.loadingOverlay);
            skipLoginField = FindViewById<TextView>(Resource.Id.skipLoginButton);
            loadingText = FindViewById<TextView>(Resource.Id.loadingText);
            loginButton = FindViewById<Android.Widget.Button>(Resource.Id.loginButton);

            RemoveLoadingPage();
            loginButton.Click += (obj, args) => { Login(); loginButton.Enabled = false; };
            skipLoginField.Click += (obj, args) => 
            {
                Android.App.AlertDialog.Builder dialogBuilder = new Android.App.AlertDialog.Builder(this);
                Android.App.AlertDialog alert = dialogBuilder.Create();
                alert.SetTitle("Skip Log In?");
                alert.SetButton2("Skip", (o,a) => ToSelectionActivity());
                alert.SetButton("Cancel", (o, a) => alert.Hide());
                alert.SetMessage("Logging in allows the same information to be edited on multiple devices.");
                alert.Show();
            };
        }
        private void StartBackgroundCheck()
        {
            // Minimum time for periodic work is 15 min
            PeriodicWorkRequest periodicWork = PeriodicWorkRequest.Builder.From<NotificationWorker>(1, Java.Util.Concurrent.TimeUnit.Seconds).Build();

            WorkManager.GetInstance(this).EnqueueUniquePeriodicWork(expiration_work_id, ExistingPeriodicWorkPolicy.Replace, periodicWork);
        }


        private async Task LoginAsync()
        {
            LoadingPage();
            await client.LogoutAsync();
            var loginResult = await client.LoginAsync();
            if (!loginResult.IsError)
            {
                var name = loginResult.User.FindFirst(c => c.Type == "name")?.Value;
                var email = loginResult.User.FindFirst(c => c.Type == "email")?.Value;
                var image = loginResult.User.FindFirst(c => c.Type == "picture")?.Value;

                userProfile = new UserProfile()
                {
                    Email = email,
                    Name = name,
                };
                
                
                if (!await FireBaseController.HasUser(email))
                {
                    ContentManager.isUserNew = true;
                    await FireBaseController.AddUser(name, email);
                }

              //  var serializedLoginResponse = JsonConvert.SerializeObject(userProfile);
                ContentManager.sessionUserProfile = userProfile;
                Console.WriteLine("REEEEE");
            }
            else
            {
                Console.WriteLine("failure");
            }
        }
        protected void LoadingPage(/*Android.Views.View view*/)
        {
            loadingOverlay.ScaleX = 1;
            loadingOverlay.ScaleY = 1;
            loadingText.ScaleX = 1;
            loadingText.ScaleY = 1;
            loginButton.Enabled = false;
        }
        
        protected void RemoveLoadingPage()
        {
            loadingOverlay.ScaleX = 0;
            loadingOverlay.ScaleY = 0;
            loadingText.ScaleX = 0;
            loadingText.ScaleY = 0;
            loginButton.Enabled = true;
        }

        private async void Login()
        {
            await LoginAsync();
            if (!await FireBaseController.HasUser(userProfile.Email))
            {
                await FireBaseController.AddUser(userProfile.Name, userProfile.Email);
            }
            ToSelectionActivity();
        }

        private void ToSelectionActivity()
        {
            ContentManager.InitializeApp();
            LoadingPage();
            StartBackgroundCheck();
            StartActivity(new Intent(this, typeof(SelectionActivity)));
        }

        protected async Task<BrowserResultType> Logout()
        {
            return await client.LogoutAsync();
        }
    }
    
}