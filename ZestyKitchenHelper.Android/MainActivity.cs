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
        protected Android.Widget.Button cloudLoginButton;
        protected Android.Widget.Button localLoginButton;
        protected TextView helpText;
        protected TextView loadingOverlay;
        protected TextView loadingText;
        private UserProfile userProfile;
        private const string expiration_work_id = "expWork";
        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            ActivityMediator.Instance.Send(intent.DataString);
        }
        protected async override void OnResume()
        {
            base.OnResume();
            Platform.OnResume();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            client = new Auth0Client(new Auth0ClientOptions()
            { Domain = "dev-4l7acohw.auth0.com", ClientId = "Srn3fq8ccb7dnBmskN5VNGG2A4A0XKz4" }) ;

            base.OnCreate(savedInstanceState);

            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            var mainPage = new MainPage();
            mainPage.InitializeLogin(LoginLocal, LoginCloud);
            SetNativeView(mainPage);
            int uiOptions = (int)Window.DecorView.SystemUiVisibility;
            uiOptions |= (int)SystemUiFlags.LowProfile;
            uiOptions |= (int)SystemUiFlags.HideNavigation;
            uiOptions |= (int)SystemUiFlags.Fullscreen;
            uiOptions |= (int)SystemUiFlags.ImmersiveSticky;
            Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
        }
        private void StartBackgroundCheck()
        {
            // Minimum time for periodic work is 15 min
            PeriodicWorkRequest periodicWork = PeriodicWorkRequest.Builder.From<NotificationWorker>(1, Java.Util.Concurrent.TimeUnit.Seconds).Build();

            WorkManager.GetInstance(this).EnqueueUniquePeriodicWork(expiration_work_id, ExistingPeriodicWorkPolicy.Replace, periodicWork);
        }


        private async Task LoginAsync()
        {
            //await client.LogoutAsync();
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
                    IconImage = ContentManager.ProfileIcons[0]
                };


                if (!await FireBaseController.HasUser(email))
                {
                    ContentManager.isUserNew = true;
                    ContentManager.sessionUserProfile = userProfile;
                }
                else
                {
                    ContentManager.sessionUserProfile = await FireBaseController.GetUser(email);
                }

                //  var serializedLoginResponse = JsonConvert.SerializeObject(userProfile);
                Console.WriteLine("REEEEE");
            }
            else
            {
                Console.WriteLine("failure");
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {

            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async void LoginCloud()
        {
            await LoginAsync();
            ContentManager.isLocal = false;
            ToSelectionActivity();
        }

        private void LoginLocal()
        {
            ContentManager.isLocal = true;
            ToSelectionActivity();
        }

        private void ToSelectionActivity()
        {
            StartBackgroundCheck();
            StartActivity(new Intent(this, typeof(SelectionActivity)));
        }

        protected async Task<BrowserResultType> Logout()
        {
            return await client.LogoutAsync();
        }

        private void SetNativeView(Xamarin.Forms.VisualElement view)
        {
            var renderer = Xamarin.Forms.Platform.Android.Platform.CreateRendererWithContext(view, this);
            renderer.Element.Layout(new Rectangle(0, 0, ContentManager.screenWidth, ContentManager.screenHeight));
            SetContentView(renderer.View);
        }
    }
    
}