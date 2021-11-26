using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using ZestyKitchenHelper.Droid.Renderer;

[assembly: ExportRenderer(typeof(TabbedPage), typeof(CustomTabbedPageRenderer))]
namespace ZestyKitchenHelper.Droid.Renderer
{
    public class CustomTabbedPageRenderer : TabbedPageRenderer
    {
        private TabLayout layout;
        public CustomTabbedPageRenderer(IntPtr hande, JniHandleOwnership transfer) { }
        public CustomTabbedPageRenderer()  { }
        public CustomTabbedPageRenderer(Context context) : base(context)
        { }
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            ResizeIcons();
        }

        protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
        {
            base.OnElementChanged(e);
            if(Element != null)
            {
                layout = (TabLayout)ViewGroup.GetChildAt(1);
                ((PageController)Element).resizeIconAction = ResizeIcons;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            ((PageController)Element).resizeIconAction = ResizeIcons;
            if (layout == null && e.PropertyName == "Renderer")
            {
                layout = (TabLayout)ViewGroup.GetChildAt(1);
            }
        }

        public void ResizeIcons()
        {
            if (layout == null || layout.TabCount == 0)
            { 
                return;
            }

            var expiredCabinets = new List<string>();
            var expiredFridges = new List<string>();
            var expiredItems = new List<int>();
            ContentManager.GetItemExpirationInfo(expiredCabinets, expiredFridges, expiredItems);

            if (expiredCabinets.Count > 0)
            {
                layout.GetTabAt(0).SetIcon(MaxResizeImage(Resources.GetDrawable(Resources.GetIdentifier("pantry_warning", "drawable", ContentManager.package_name), null), ContentManager.tab_icon_image_size, ContentManager.tab_icon_image_size));
            }
            else
            {
                var drawable = Resources.GetDrawable(Resources.GetIdentifier("pantry", "drawable", ContentManager.package_name), null);
                var image = MaxResizeImage(drawable, ContentManager.tab_icon_image_size, ContentManager.tab_icon_image_size);
                var tab = layout.GetTabAt(0);
                Console.WriteLine("CustomTabbedPageRenderer 75 tab count " + layout.TabCount);
                tab.SetIcon(image);

            }
            if (expiredFridges.Count > 0) 
            {
                layout.GetTabAt(2).SetIcon(MaxResizeImage(Resources.GetDrawable(Resources.GetIdentifier("fridge_warning", "drawable", ContentManager.package_name), null), ContentManager.tab_icon_image_size, ContentManager.tab_icon_image_size));
            }
            else
            {
                layout.GetTabAt(2).SetIcon(MaxResizeImage(Resources.GetDrawable(Resources.GetIdentifier("fridge", "drawable", ContentManager.package_name), null), ContentManager.tab_icon_image_size, ContentManager.tab_icon_image_size));
            }
            if (expiredItems.Count > 0)
            {
                layout.GetTabAt(1).SetIcon(MaxResizeImage(Resources.GetDrawable(Resources.GetIdentifier("all_items_warning", "drawable", ContentManager.package_name), null),ContentManager.tab_icon_image_size, ContentManager.tab_icon_image_size));
            }
            else
            {
                layout.GetTabAt(1).SetIcon(MaxResizeImage(Resources.GetDrawable(Resources.GetIdentifier("all_items", "drawable", ContentManager.package_name), null), ContentManager.tab_icon_image_size, ContentManager.tab_icon_image_size));
            }
        }

        private Android.Graphics.Drawables.Drawable MaxResizeImage(Android.Graphics.Drawables.Drawable image, float maxWidth, float maxHeight)
        {
            var originalSize = image.Bounds;
            var maxSizeFactor = Math.Min(maxWidth / originalSize.Width(), maxHeight / originalSize.Height());

            if (maxSizeFactor > 1)
                return image;

            var width = originalSize.Width() * maxSizeFactor;
            var height = originalSize.Height() * maxSizeFactor;

            image.Bounds = new Android.Graphics.Rect(0, 0, (int)width, (int)height);
            return image;
        }
    }
}