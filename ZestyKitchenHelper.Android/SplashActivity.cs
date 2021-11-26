using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Utility;
using Xamarin.Forms.Platform.Android;

namespace ZestyKitchenHelper.Droid
{
    [Activity(MainLauncher = true, NoHistory = true, Theme = "@style/MainTheme.Splash")]
    public class SplashActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
        }

        protected override void OnResume()
        {
            base.OnResume();
            LoadApplication(new App());
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
        }


    }
}