﻿using System;
using System.Collections.Generic;
using System.Linq;
using Auth0.OidcClient;
using Xamarin.Forms.Platform.iOS;
using CoreGraphics;
using UserNotifications;


using Foundation;
using UIKit;
using Utility;
using System.Threading.Tasks;

namespace ZestyKitchenHelper.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public static Action ToPageControllerAction;

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

        public override UIWindow Window
        {
            get; set;
        }
        private UIStoryboard storyBoard = UIStoryboard.FromName("LoginStoryboard", null);
        private UIViewController initialViewController;
        public static UIView currentView;

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
            global::ZXing.Net.Mobile.Forms.iOS.Platform.Init();

            ToPageControllerAction = ToPageController;
            UIApplication.SharedApplication.StatusBarHidden = true;
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Badge | UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound, (a, e) => { Console.WriteLine("NOTIFICATION REGISTER COMPLETE: &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&" + a); });
                UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();
            }
            else
            {
                var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null);
                UIApplication.SharedApplication.RegisterUserNotificationSettings(notificationSettings);
            }
            
            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            NSTimer.CreateRepeatingScheduledTimer(10, t => Console.WriteLine($"AppDelegate 78 ping {t}"));
            LoadApplication(new App());
            ContentManager.InitializeApp();

            initialViewController = storyBoard.InstantiateViewController("LoginViewController");
            Window.RootViewController = initialViewController;
            Window.AddSubview(initialViewController.View);
            Window.MakeKeyAndVisible();

            return base.FinishedLaunching(app, options);
        }

        private void ToPageController()
        {
            ContentManager.pageController.InitializePageSequence();
            (Xamarin.Forms.Application.Current as App).SetMainPage();
            var renderer = Platform.CreateRenderer(ContentManager.pageController);

            renderer.NativeView.Frame = UIScreen.MainScreen.Bounds;

            renderer.NativeView.AutoresizingMask = UIViewAutoresizing.All;
            renderer.NativeView.ContentMode = UIViewContentMode.ScaleToFill;

            renderer.Element.Layout(UIScreen.MainScreen.Bounds.ToRectangle());

            Window.RootViewController = renderer.ViewController;
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            ActivityMediator.Instance.Send(url.AbsoluteString);
            return true;
        }

        public override async void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            base.PerformFetch(application, completionHandler);
            Console.WriteLine("APP DELEGATE 114 Perfor Fetch Called  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            var itemList = await LocalStorageController.GetTableListAsync<Item>();
           
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                foreach (var item in itemList)
                {
                    item.SetDaysUntilExpiration();
                    if (item.daysUntilExp < 1 && !item.oneDayWarning)
                    { NotifyUser(item, "Your " + item.Name + " expires in 1 day!"); item.oneDayWarning = true; }
                    else if (item.daysUntilExp < 3 && !item.threeDaysWarning)
                    { NotifyUser(item, "Your " + item.Name + " expires in 3 days!"); item.threeDaysWarning = true; }
                    else if (item.daysUntilExp < 7 && !item.weekWarning)
                    { NotifyUser(item, "Your " + item.Name + " expires in one week!"); item.weekWarning = true; }
                    LocalStorageController.UpdateItem(item);
                }
            }
            else
            {
                foreach (var item in itemList)
                {
                    item.SetDaysUntilExpiration();
                    if (item.daysUntilExp < 1 && !item.oneDayWarning)
                    { NotifyUserOld(item, "Your " + item.Name + " expires in 1 day!"); item.oneDayWarning = true; }
                    else if (item.daysUntilExp < 3 && !item.threeDaysWarning)
                    { NotifyUserOld(item, "Your " + item.Name + " expires in 3 days!"); item.threeDaysWarning = true; }
                    else if (item.daysUntilExp < 7 && !item.weekWarning)
                    { NotifyUserOld(item, "Your " + item.Name + " expires in one week!"); item.weekWarning = true; }
                    LocalStorageController.UpdateItem(item);
                }
            }
            completionHandler(UIBackgroundFetchResult.NewData);
        }

        private void NotifyUserOld(Utility.Item item, string alertString)
        {
            Console.WriteLine("APP DELEGATE 126: NOTIFICATION GOT IOS  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            UILocalNotification notification = new UILocalNotification();
            notification.FireDate = NSDate.Now;
            notification.AlertAction = ContentManager.exp_notification_title;
            notification.AlertBody = alertString;
            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }

        private async void NotifyUser(Utility.Item item, string alertString)
        {
            Console.WriteLine("APP DELEGATE 135: NOTIFICATION GOT IOS  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            var content = new UNMutableNotificationContent();
            content.Title = ContentManager.exp_notification_title;
            content.Body = alertString;
            content.Badge = 1;

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
            var requestID = item.ID.ToString();
            var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);
            UNUserNotificationCenter.Current.AddNotificationRequest(request, (e) => { Console.WriteLine("NOTIFICATION COMPLETE: &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&& FAILURE?" + (e != null)); });
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
