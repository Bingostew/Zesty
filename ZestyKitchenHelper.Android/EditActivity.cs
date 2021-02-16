using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Org.Apache.Http.Impl.Client;
using Xamarin.Forms.Platform.Android;

namespace ZestyKitchenHelper.Droid
{
    [Activity(Label = "Activity1")]
    public class EditActivity : FragmentActivity
    {
        private delegate void OnDestroyAction();
        private event OnDestroyAction selectionBackPressedEvent;

        Android.Support.V4.App.FragmentManager manager;
        protected override void OnCreate(Bundle savedInstanceState)
        {
  
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            selectionBackPressedEvent?.Invoke();
        }
    }
}
