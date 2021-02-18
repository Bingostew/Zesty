using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ZestyKitchenHelper.Droid.Effects;
using Android.Widget;
using ZestyKitchenHelper.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;

[assembly: ExportEffect(typeof(ImageTintEffect), "ImageTintEvent")]
namespace ZestyKitchenHelper.Droid.Effects
{

    public class ImageTintEffect : PlatformEffect
    {
        ImageView control;
        protected override void OnAttached()
        {
            var effect = Element.Effects.FirstOrDefault(e => e is ImageTint) as ImageTint;
            control = Control as ImageView;
            var filter = new PorterDuffColorFilter(effect.tint.ToAndroid(), PorterDuff.Mode.SrcOver);

             control.SetColorFilter(filter);   
        }
        protected override void OnDetached()
        {
            try
            {
                control.ClearColorFilter();
            }
            catch { }

        }
    }
}