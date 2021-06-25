using System;
using System.Collections.Generic;
using System.Linq;
using Auth0.OidcClient;
using Xamarin.Forms.Platform.iOS;
using CoreGraphics;


using Foundation;
using UIKit;
using System.Threading.Tasks;

namespace ZestyKitchenHelper.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        private Auth0Client client;
        private UserProfile userProfile;
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
            global::ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            LoadApplication(new App());
            ContentManager.InitializeApp();

            client = new Auth0Client(new Auth0ClientOptions()
            {
                Domain = "dev-4l7acohw.auth0.com",
                ClientId = "Srn3fq8ccb7dnBmskN5VNGG2A4A0XKz4"
            });

            UIApplication.SharedApplication.StatusBarHidden = true;

            return base.FinishedLaunching(app, options);
        }

        public override UIWindow Window
        {
            get;set;
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            ActivityMediator.Instance.Send(url.AbsoluteString);
            return true;
        }

        private async Task LoginAsync()
        {
            var loginResult = await client.LoginAsync();

            if (!loginResult.IsError)
            {
                var name = loginResult.User.FindFirst(c => c.Type == "name")?.Value;
                var email = loginResult.User.FindFirst(c => c.Type == "email")?.Value;
                var image = loginResult.User.FindFirst(c => c.Type == "picture")?.Value;

                userProfile = new UserProfile()
                {
                    Email = email,
                    Name = name,
                };


                if (!await FireBaseController.HasUser(email))
                {
                    ContentManager.isUserNew = true;
                    await FireBaseController.AddUser(name, email);
                }

                //  var serializedLoginResponse = JsonConvert.SerializeObject(userProfile);
                ContentManager.sessionUserProfile = userProfile;
                Console.WriteLine("REEEEE");

            }
            else
            {
                Console.WriteLine("Failure");
            }
        }
       
        public async Task<IdentityModel.OidcClient.Browser.BrowserResultType> LogoutAsync()
        {
            return await client.LogoutAsync();
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
