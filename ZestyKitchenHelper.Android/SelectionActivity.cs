using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Content.PM;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using IdentityModel.OidcClient.Browser;
using Java.Lang;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Android.Hardware.Camera2;
using ZestyKitchenHelper.Droid.Effects;
using AndroidX.Work;
using Utility;
using System.Runtime.Remoting.Messaging;
using AndroidX.Fragment.App;
using Javax.Crypto.Spec;

namespace ZestyKitchenHelper.Droid
{
    [Activity()]
    public class SelectionActivity :  AndroidX.Fragment.App.FragmentActivity
    {
        List<ContentPage> navigationStack = new List<ContentPage>();
        private UserProfile userProfile;
        public CameraDevice cam;
        public CameraCaptureSession camCapture;
        public CaptureRequest camCaptureRequest;

        Dictionary<string,  AndroidX.Fragment.App.Fragment> selectionFragments = new Dictionary<string,  AndroidX.Fragment.App.Fragment>();
         AndroidX.Fragment.App.Fragment mainSelectionFrag;
         AndroidX.Fragment.App.Fragment selectionFrag;
        AndroidX.Fragment.App.Fragment viewFrag;
        AndroidX.Fragment.App.Fragment unplacedFrag;

         AndroidX.Fragment.App.FragmentManager manager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            manager = SupportFragmentManager;
            base.OnCreate(savedInstanceState);
            Console.WriteLine("oncreate");
            mainSelectionFrag = ContentManager.pageController.CreateSupportFragment(this);
            manager.BeginTransaction().Replace(Android.Resource.Id.Content, mainSelectionFrag).Commit();

            ActionBar?.Hide();
            TouchEffect.activity = this;
            ContentManager.pageController.InitializePageSequence();

            GetLoginResult(savedInstanceState);

           // UpdateUserEvent();
           // GetUserEvent();
        }
        protected override void OnStart()
        {
            base.OnStart();
        }
        protected override void OnResume()
        {
            base.OnResume();
            if(ContentManager.singleSelectionPage != null) ContentManager.singleSelectionPage.SetView();
        }
        protected override void OnStop()
        {
            base.OnStop();
        }
        protected override void OnPause()
        {
            base.OnPause();
        }


        private async void GetUserEvent()
        {
            var user = await FireBaseMediator.fireBaseController.GetUser(ContentManager.sessionUserName);
            if (user != null)
            {

            }
        }
        private void GetLoginResult(Bundle saveInstanceState)
        {
            string loginResultAsJson = Intent.GetStringExtra("LoginResult") ?? string.Empty;
            Console.WriteLine("result length : " + loginResultAsJson);
            userProfile = JsonConvert.DeserializeObject<UserProfile>(loginResultAsJson);   
        }
    }
}