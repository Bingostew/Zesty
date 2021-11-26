using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("Zesty")]
[assembly: ExportEffect(typeof(ZestyKitchenHelper.iOS.Effect.TouchEffect), "ScreenTouchEvent")]
namespace ZestyKitchenHelper.iOS.Effect
{
    public class TouchEffect : PlatformEffect
    {
        UIView view;
        TouchRecognizer recognizer;

        protected override void OnAttached()
        {
            view = Control == null ? Container : Control;


            ScreenTouch effect = (ScreenTouch)Element.Effects.FirstOrDefault(e => e is ScreenTouch);

            if(effect != null && view != null)
            {
                recognizer = new TouchRecognizer(Element, view, effect);
                view.AddGestureRecognizer(recognizer);
            }
        }

        protected override void OnDetached()
        {
            if(recognizer != null)
            {
                recognizer.Detach();
                view.RemoveGestureRecognizer(recognizer);
            }
        }

    }
}