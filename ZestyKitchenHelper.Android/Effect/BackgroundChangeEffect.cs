using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ZestyKitchenHelper.Droid;
using Xamarin.Forms;
using ZestyKitchenHelper.Droid.Effects;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("Zesty")]
[assembly: ExportEffect(typeof(BackgroundChangeEffect), "BackgroundChangeEvent")]
namespace ZestyKitchenHelper.Droid.Effects
{
    public class BackgroundChangeEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var control = Control as TextView;
            var effect = (BackgroundChange)Element.Effects.FirstOrDefault((e) => e is BackgroundChange);
            control.SetBackgroundColor(effect.color.ToAndroid());
        }
        protected override void OnDetached()
        {
            throw new NotImplementedException();
        }
    }
}