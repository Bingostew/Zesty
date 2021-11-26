using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using ZestyKitchenHelper.iOS.Renderer;

[assembly: ExportRenderer(typeof(TabbedPage), typeof(CustomTabbedPageRenderer))]
namespace ZestyKitchenHelper.iOS.Renderer
{
    public class CustomTabbedPageRenderer : TabbedRenderer
    {
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            ResizeIcons();
        }

        public override void ItemSelected(UITabBar tabbar, UITabBarItem item)
        {
            ResizeIcons();
            item.SetTitleTextAttributes(new UITextAttributes() { Font = UIFont.PreferredCaption1 }, UIControlState.Normal);
        }

        public void ResizeIcons()
        {
            var tabPage = Element as TabbedPage;
            var tabbar = TabBar;

            var expiredCabinets = new List<string>();
            var expiredFridges = new List<string>();
            var expiredItems = new List<int>();
            ContentManager.GetItemExpirationInfo(expiredCabinets, expiredFridges, expiredItems);

            if (expiredCabinets.Count > 0)
            {
                tabbar.Items[0].Image = UIImage.FromFile(ContentManager.pantryWarningIcon);
            }
            else
            {
                tabbar.Items[0].Image = UIImage.FromFile(ContentManager.pantryIcon);
            }
            if (expiredFridges.Count > 0)
            {
                tabbar.Items[2].Image = UIImage.FromFile(ContentManager.fridgeWarningIcon);
            }
            else
            {
                tabbar.Items[2].Image = UIImage.FromFile(ContentManager.refridgeIcon);
            }
            if (expiredItems.Count > 0)
            {
                tabbar.Items[1].Image = UIImage.FromFile(ContentManager.allItemWarning);
            }
            else
            {
                tabbar.Items[1].Image = UIImage.FromFile(ContentManager.allItemIcon);
            }  
            if (TabBar?.Items == null)
                return;

            if(tabPage != null)
            {
                for(int i = 0; i < TabBar.Items.Length; i ++)
                {
                    UpdateIconSize(TabBar.Items[i]);
                }
            }
        }

        private void UpdateIconSize(UITabBarItem item)
        {
            if(item.Image != null)
                item.Image = MaxResizeImage(item.Image,ContentManager.tab_icon_image_size,ContentManager.tab_icon_image_size);
            if (item.SelectedImage != null)
                item.SelectedImage = MaxResizeImage(item.Image,ContentManager.tab_icon_image_size,ContentManager.tab_icon_image_size);
        }

        private UIImage MaxResizeImage(UIImage image, float maxWidth, float maxHeight)
        {
            var originalSize = image.Size;
            var maxSizeFactor = Math.Min(maxWidth / originalSize.Width, maxHeight / originalSize.Height);

            if (maxSizeFactor > 1)
                return image;

            var width = originalSize.Width * maxSizeFactor;
            var height = originalSize.Height * maxSizeFactor;
            UIGraphics.BeginImageContext(new CGSize(width, height));
            image.Draw(new CGRect(0, 0, width, height));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return resultImage;
        }
    }
}