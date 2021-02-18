using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Text.Method;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using ZestyKitchenHelper.Droid.Effects;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AndroidX;
using Utility;

[assembly: ExportEffect(typeof(TouchEffect), "ScreenTouchEvent")]
namespace ZestyKitchenHelper.Droid.Effects
{
    public class TouchEffect : PlatformEffect
    {
        public static Activity activity;
        private int[] location = new int[2];
        Func<double, double> fromPixels;
        Point pressPoint;
        ScreenTouch touchEffect;
        private bool isInContact;
        private bool capture;
        private Point viewPoint = new Point();
        protected override void OnAttached()
        {
            Console.WriteLine("touch attached exe wow!");
            touchEffect = (ScreenTouch)Element.Effects.FirstOrDefault(e => e is ScreenTouch);
            capture = touchEffect.Capture;
            if(Control != null && touchEffect != null)
            {
              //  viewDictionary.Add(Control, this);

                Control.Touch += OnTouch;

                fromPixels = Control.Context.FromPixels;
            }

        }

       
        private void OnTouch(object obj, Android.Views.View.TouchEventArgs args)
        {
            var sender = obj as Android.Views.View;
            MotionEvent motionEvent = args.Event;
 
            int pointerIndex = motionEvent.ActionIndex;
            int id = motionEvent.GetPointerId(pointerIndex);

            sender.GetLocationOnScreen(location);
            pressPoint = new Point(location[0], location[1]);

            Point coordinate = new Point(location[0] + motionEvent.GetX(pointerIndex), location[1] + motionEvent.GetY(pointerIndex));

            switch (args.Event.ActionMasked)
            {
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    Console.WriteLine("down");

                    capture = touchEffect.Capture;
                    break;
                    
                case MotionEventActions.Move:
                    for (pointerIndex = 0; pointerIndex < motionEvent.PointerCount; pointerIndex++)
                    {

                        sender.GetLocationOnScreen(location);

                        coordinate = new Point(motionEvent.GetX(pointerIndex),
                                                       motionEvent.GetY(pointerIndex));
                        if (!capture) { CheckCollision(touchEffect.ContactViews, coordinate, TouchActionEventArgs.TouchActionType.Moved); }
                        else { CheckOverlay(touchEffect.ContactViews, touchEffect.ContactÍnitiators, coordinate, TouchActionEventArgs.TouchActionType.Moved); }

                    }
                    break;
                    
                case MotionEventActions.Up:
                case MotionEventActions.Pointer1Up:
                    Console.WriteLine("done");
                    coordinate = new Point(motionEvent.GetX(pointerIndex),
                                                     motionEvent.GetY(pointerIndex));
                    if (!capture) { CheckCollision(touchEffect.ContactViews, coordinate, TouchActionEventArgs.TouchActionType.Released); }
                    else { CheckOverlay(touchEffect.ContactViews, touchEffect.ContactÍnitiators, coordinate, TouchActionEventArgs.TouchActionType.Released); }
                    break;
            }
        }

        Vector2D GetAbsolutePositionAndroid(Android.Views.View view)
        {
            var x = view.GetX();
            var y = view.GetY();
            var parent = (Android.Views.View)view.Parent;
            while(parent != null && typeof(Android.Views.View).IsAssignableFrom(parent.Parent.GetType())) { x += parent.GetX(); y += parent.GetY(); parent = (Android.Views.View)parent.Parent; }
            return new Vector2D(x, y);
        }

        int GetStatusBarHeight()
        {
            int statusBarHeight = -1;
            int resourceId = activity.Resources.GetIdentifier("status_bar_height", "dimen", "android");
            if (resourceId > 0)
            {
                statusBarHeight = activity.Resources.GetDimensionPixelSize(resourceId);
            }
            return statusBarHeight;
        }

        void CheckOverlay(Dictionary<int, List<Xamarin.Forms.ImageButton>> contactBase, List<Xamarin.Forms.ImageButton> contactInitiators, Point coordinate, TouchActionEventArgs.TouchActionType type)
        {
            Console.WriteLine("check" + Control);
            var viewXL = new Rectangle(contactInitiators[0].GetAbsolutePosition().X, contactInitiators[0].GetAbsolutePosition().Y, contactInitiators[0].Width, contactInitiators[0].Height);
            var viewXR = new Rectangle(contactInitiators[1].GetAbsolutePosition().X, contactInitiators[1].GetAbsolutePosition().Y, contactInitiators[1].Width, contactInitiators[1].Height);
            var viewYT = new Rectangle(contactInitiators[2].GetAbsolutePosition().X, contactInitiators[2].GetAbsolutePosition().Y, contactInitiators[2].Width, contactInitiators[2].Height);
            var viewYB = new Rectangle(contactInitiators[3].GetAbsolutePosition().X, contactInitiators[3].GetAbsolutePosition().Y, contactInitiators[3].Width, contactInitiators[3].Height);
            foreach(var i in contactInitiators)
            {
               // Console.WriteLine(" x " + i.GetAbsolutePosition().X + " y " + i.GetAbsolutePosition().Y);
            }

            // top buffer, top buffer is wrong
            isInContact = false;
            List<int> contactIndexes = new List<int>();
            List<Xamarin.Forms.View> contactViews = new List<Xamarin.Forms.View>();
            foreach (var index in contactBase.Keys)
            {
                foreach (var button in contactBase[index])
                {
                    var bound = new Rectangle(button.GetAbsolutePosition().X, button.GetAbsolutePosition().Y, button.Width, button.Height);
                    
                    Console.WriteLine(" top " + viewYT.Top);
                    /*
                    Console.WriteLine("viewxl " + viewXL.IntersectsWith(bound) +
                       " xr " + viewXR.IntersectsWith(bound) + " yt " + viewYT.IntersectsWith(bound)
                       + " yb " + viewYB.IntersectsWith(bound));*/
                    
                    if (viewXL.IntersectsWith(bound) || viewXR.IntersectsWith(bound) || viewYT.IntersectsWith(bound) || viewYB.IntersectsWith(bound))
                    {
                        isInContact = true;
                        contactIndexes.Add(index);
                        contactViews.Add(button);
                    }
                }
            }
            if (isInContact)
            {
                FireEvent(this, type, coordinate, contactIndexes, contactViews);
            }
            else
            {
                FireEvent(this, type, coordinate, null, null);
            }
        }

        void CheckCollision(Dictionary<int, List<Xamarin.Forms.ImageButton>> contactBase, Point coordinate, TouchActionEventArgs.TouchActionType type)
        {
            List<int> contactIndexes = new List<int>();
            List<Xamarin.Forms.View> contactViews = new List<Xamarin.Forms.View>();
            if (contactBase != null)
            {
                var controlRect = new Rectangle(fromPixels(GetAbsolutePositionAndroid(Control).X + (Control.Width / 2)) - 10, 
                    fromPixels(GetAbsolutePositionAndroid(Control).Y - GetStatusBarHeight() + (Control.Height / 2)) - 10,
                    20, 20);

                foreach (var index in contactBase.Keys)
                {
                    foreach (var button in contactBase[index])
                    {
                        var contactRect = new Rectangle(button.GetAbsolutePosition().X, button.GetAbsolutePosition().Y, button.Width, button.Height);
                        if (contactRect.IntersectsWith(controlRect))
                        {
                            isInContact = true;
                            contactIndexes.Add(index);
                            contactViews.Add(button);
                        }
                    }
                }
            }
            if (isInContact)
            {
                FireEvent(this, type, coordinate, contactIndexes, contactViews);
            }
            else
            {
                FireEvent(this, type, coordinate, null, null);
            }
        }


        void FireEvent(TouchEffect touchEffect, TouchActionEventArgs.TouchActionType actionType, Point pointerLocation, List<int> contactIndexes, List<Xamarin.Forms.View> contactView)
        {
            Console.WriteLine("contact " + isInContact);
            // Get the method to call for firing events
            Action<Element, TouchActionEventArgs> onTouchAction = touchEffect.touchEffect.OnTouchAction;

            // Get the location of the pointer within the view
            touchEffect.Control.GetLocationOnScreen(location);
            var x = fromPixels(pointerLocation.X - touchEffect.Control.Width / 2);
            var y = fromPixels(pointerLocation.Y - touchEffect.Control.Height / 2);

            Point point = new Point(fromPixels(x), fromPixels(y));

            // Call the method
            onTouchAction(touchEffect.Element,
                new TouchActionEventArgs(actionType, point, pressPoint, isInContact, contactIndexes, contactView));
            isInContact = false;
        }

        protected override void OnDetached()
        {
            ///Control.Touch -= OnTouch;
        }
    }
}
