using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;

namespace ZestyKitchenHelper
{
    public delegate void TouchActionEventHandler(object sender, TouchActionEventArgs args);
    public class TouchActionEventArgs : EventArgs
    {
        public enum TouchActionType
        {
            Entered,
            Pressed,
            Moved,
            Released,
            Exited,
            Cancelled
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Type of action registered</param>
        /// <param name="location">Location of point</param>
        /// <param name="originLocation"></param>
        /// <param name="isInContact"></param>
        /// <param name="contactIndex">The ID of each StorageCells in contact</param>
        /// <param name="view"></param>
        public TouchActionEventArgs(TouchActionType type, Point location, Point originLocation, bool isInContact, List<int> contactIndex, List<View> view)
        {
            Type = type;
            Location = location;
            OldLocation = originLocation;
            IsInContact = isInContact;
            ContactIndex = contactIndex;
            ContactView = view;
        }

        public long Id { get; set; }
        public List<int> ContactIndex { get; set; }

        public List<View> ContactView { get; set; }
         public TouchActionType Type { get; set; }
        public Point Location { get; set; }
        public Point OldLocation { get; set; }
        public bool IsInContact { get; set; }
    }
    public class BackgroundChange : RoutingEffect
    {
        public Color color { get; set; }
        public BackgroundChange() : base("App.BackgroundChangeEvent")
        {
        }
    }

    public class ImageTint : RoutingEffect
    {
        public Color tint { get; set; }
        public Image Image { get; set; }
        public ImageTint() : base("App.ImageTintEvent") { }
    }

    public class ScreenTouch : RoutingEffect
    { 
        public ScreenTouch() : base("App.ScreenTouchEvent"){}
        public bool Capture = false;
        public Utility.IStorage ContactView { get; set; }

        public event TouchActionEventHandler OnTouchEvent;
        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            OnTouchEvent?.Invoke(element, args);
        }
    }

}
