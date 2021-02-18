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
        /// <summary>
        /// For item buffers, must be in this order (Left, Right, Top, Bottom)
        /// </summary>
        public List<ImageButton> ContactÍnitiators { get; set; }
        public Dictionary<int, List<ImageButton>> ContactViews { get; set; }

        public event TouchActionEventHandler OnTouchEvent;
        public void OnTouchAction(Element element, TouchActionEventArgs args)
        {
            OnTouchEvent?.Invoke(element, args);
        }
    }

}
