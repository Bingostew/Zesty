using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using ZestyKitchenHelper.iOS.Effect;

[assembly:ExportEffect(typeof(SafeAreaPaddingEffect), "SafeAreaPaddingEvent")]
namespace ZestyKitchenHelper.iOS.Effect
{
    public class SafeAreaPaddingEffect : PlatformEffect
    {
        Thickness padding;
        protected override void OnAttached()
        {
            if(Element is Layout element)
            {
                if(UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    element.BackgroundColor = ContentManager.ThemeColor;
                    padding = element.Padding;
                    var inset = UIApplication.SharedApplication.Windows[0].SafeAreaInsets;
                    if(inset.Top > 0)
                    {
                        element.Padding = new Thickness(padding.Left + inset.Left, padding.Top + inset.Top, padding.Right + inset.Right, padding.Bottom + inset.Bottom);
                    }
                    else
                    {
                        element.Padding = new Thickness(padding.Left, padding.Top + 20, padding.Right, padding.Bottom);
                    }
                }
            }
        }

        protected override void OnDetached()
        {
            if(Element is Layout element)
            {
                element.Padding = padding;
            }
        }
    }
}