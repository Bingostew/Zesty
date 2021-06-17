using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Platform.iOS;
using CoreGraphics;


using Foundation;
using UIKit;

namespace ZestyKitchenHelper.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public static UIView ConvertFormsToNative(Xamarin.Forms.VisualElement view, CGRect size)
        {
            var renderer = Platform.CreateRenderer(view);

            renderer.NativeView.Frame = size;

            renderer.NativeView.AutoresizingMask = UIViewAutoresizing.All;
            renderer.NativeView.ContentMode = UIViewContentMode.ScaleToFill;

            renderer.Element.Layout(size.ToRectangle());

            var nativeView = renderer.NativeView;

            nativeView.SetNeedsLayout();
            
            return nativeView;
        }
        public UIWindow window { get; set; }
        private UIViewController mainView;
        private UIView selectionPage, unplacedPage;
        public static UIView currentView;
        private CGRect screensize;
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            UIApplication.SharedApplication.StatusBarHidden = false;
            
            return base.FinishedLaunching(app, options);
        }
        /*
        public void GotoUnplacedPage(object obj, EventArgs args)
        {
            if (unplacedPage != null)
            {
                currentView = unplacedPage;
                mainView.View = unplacedPage;
            }
            else { throw new NullReferenceException(); }
        }
        public void GotoSelectionPage(object obj, EventArgs args)
        {
            if (selectionPage != null)
            {
                currentView = selectionPage;
                mainView.View = selectionPage;
            }
            else { throw new NullReferenceException(); }
        }

        public void GotoAddPage(string name)
        {
            /*
            if (ContentManager.storageSelection == ContentManager.StorageSelection.fridge)
            {

            }
            else
            {
                var cabinetAddPage = ConvertFormsToNative(new CabinetAddPage(name, (obj, args) => NavigateToSelection(ContentManager.StorageSelection.cabinet), 
                    new Action<Utility.Item>((i) => { })),need change screensize);
                currentView = cabinetAddPage;
                mainView.View = cabinetAddPage;
            }
        }

        public void GotoViewPage(string name)
        {
            
            if (ContentManager.storageSelection == ContentManager.StorageSelection.fridge)
            {

            }
            else
            {
                var cabinetViewPage = ConvertFormsToNative(new CabinetViewPage(name, (obj, args) => NavigateToSelection(ContentManager.StorageSelection.cabinet)), screensize);
                currentView = cabinetViewPage;
                mainView.View = cabinetViewPage;
            }
        }

        private delegate void OnDestroyAction();
        private event OnDestroyAction selectionBackPressedEvent;

        public void GotoStorageCreationPage(object obj, EventArgs args)
        {
            
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                var cabinetEditPage = ConvertFormsToNative(new CabinetEditPage(true, () => NavigateToSelection(ContentManager.StorageSelection.cabinet)), screensize); ;
                currentView = cabinetEditPage;
                mainView.View = cabinetEditPage;
            }
            else
            {
                
            }
    // ActionBar.Hide();
        }
        private void AddNavigation(Xamarin.Forms.ImageButton fridge, Xamarin.Forms.ImageButton cabinet)
        {
            fridge.Clicked += (obj, args) => NavigateToSelection(ContentManager.StorageSelection.fridge);
            cabinet.Clicked += (obj, args) => NavigateToSelection(ContentManager.StorageSelection.cabinet);
        }
        private void NavigateToSelection(ContentManager.StorageSelection selection)
        {
            ContentManager.storageSelection = selection;
            ContentManager.singleSelectionPage = new SingleSelectionPage(GotoAddPage, GotoViewPage, GotoStorageCreationPage, GotoSelectionPage);
        }
    */
    }
}
