using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZestyKitchenHelper;
using Xamarin.Forms;

using Foundation;
using UIKit;
using Utility;
using CoreGraphics;

namespace ZestyKitchenHelper.iOS.Effect
{
    public class TouchRecognizer : UIGestureRecognizer
    {
        Element element;
        UIView view;
        ScreenTouch touchEffect;

        bool isInContact;

        static Dictionary<UIView, TouchRecognizer> viewDictionary = new Dictionary<UIView, TouchRecognizer>();
        static Dictionary<long, TouchRecognizer> idToTouchDictionary = new Dictionary<long, TouchRecognizer>();

        public TouchRecognizer(Element element, UIView view, ScreenTouch touchEffect)
        {
            this.element = element;
            this.view = view;
            this.touchEffect = touchEffect;

            viewDictionary.Add(view, this);
        }

        public void Detach()
        {
            viewDictionary.Remove(view);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            foreach(UITouch touch in touches.Cast<UITouch>())
            {
                long id = touch.Handle.ToInt64();
                if (!idToTouchDictionary.ContainsKey(id))
                {
                    idToTouchDictionary.Add(id, this);
                }
                CheckCollision(touch, idToTouchDictionary[id].touchEffect.ContactView, TouchActionEventArgs.TouchActionType.Pressed);

            }
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = touch.Handle.ToInt64();
                CheckCollision(touch, idToTouchDictionary[id].touchEffect.ContactView, TouchActionEventArgs.TouchActionType.Moved);
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = touch.Handle.ToInt64();
                CheckCollision(touch, idToTouchDictionary[id].touchEffect.ContactView, TouchActionEventArgs.TouchActionType.Released);
                idToTouchDictionary.Remove(id);
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);

            foreach (UITouch touch in touches.Cast<UITouch>())
            {
                long id = touch.Handle.ToInt64();
                CheckCollision(touch, idToTouchDictionary[id].touchEffect.ContactView, TouchActionEventArgs.TouchActionType.Cancelled);
                idToTouchDictionary.Remove(id);
            }
        }

        void CheckCollision(UITouch touch, IStorage contactBase, TouchActionEventArgs.TouchActionType type)
        {
            long id = touch.Handle.ToInt64();
            List<int> contactIndexes = new List<int>();
            List<Xamarin.Forms.View> contactViews = new List<Xamarin.Forms.View>();
            isInContact = false;
            if (contactBase != null)
            {
                var coordinate = touch.LocationInView(null);


                foreach (var cell in contactBase.GetGridCells())
                {
                    var child = cell.GetButton();

                    var contactRect = new CGRect(child.GetAbsolutePosition().X, child.GetAbsolutePosition().Y, child.Width, child.Height);
                    if (contactRect.Contains(coordinate))
                    {
                        isInContact = true;

                        contactIndexes.Add(cell.Index);
                        contactViews.Add(child);
                    }
                }
            }
            if (isInContact)
            {
                FireEvent(idToTouchDictionary[id], id, type, touch, true, contactIndexes, contactViews);
            }
            else
            {
                FireEvent(idToTouchDictionary[id], id, type, touch, false, null, null);
            } 
        }

        void FireEvent(TouchRecognizer recognizer, long id, TouchActionEventArgs.TouchActionType actionType, UITouch touch, bool isInContact, List<int> contactIndexes, List<Xamarin.Forms.View> contactView)
        {
            Console.WriteLine("contact " + isInContact);
            // Get the method to call for firing events
            Action<Element, TouchActionEventArgs> onTouchAction = recognizer.touchEffect.OnTouchAction;

            // Get the location within the view
            var cgPoint = touch.LocationInView(view);
            Point point = new Point(cgPoint.X - view.Bounds.Width / 2, cgPoint.Y - view.Bounds.Height / 2);

            // Call the method
            onTouchAction(touchEffect.Element,
                new TouchActionEventArgs(actionType, point, isInContact, contactIndexes, contactView));
            isInContact = false;
        }
    }
}