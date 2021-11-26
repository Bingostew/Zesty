using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


[assembly: ExportEffect(typeof(ZestyKitchenHelper.iOS.Effect.ImageTintEffect), "ImageTintEvent")]
namespace ZestyKitchenHelper.iOS.Effect
{
    class ImageTintEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var effect = Element.Effects.FirstOrDefault(e => e is ImageTint) as ImageTint;

            if (effect == null)
                return;

            if(Control is UIButton button)
            {
                button.SetBackgroundImage(new UIImage(effect.ImagePath), UIControlState.Normal);
            }
            else if (Control is UIImageView image)
            {
                image.Image = image.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                image.TintColor = UIColor.Clear;
                image.Opaque = false;
            }
        }
        protected override void OnDetached()
        {
            if (Control is UIImageView image)
            {
                image.TintColor = UIColor.Clear;
            }
            else if (Control is UIButton button)
            {
                Console.WriteLine("ImageTintEffect 43 image tint removed ");
                button.SetBackgroundImage(null, UIControlState.Normal);
            }
        }
    }
}