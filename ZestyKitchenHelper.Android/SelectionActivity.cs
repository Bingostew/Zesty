using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Content.PM;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using IdentityModel.OidcClient.Browser;
using Java.Lang;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Android.Hardware.Camera2;
using ZestyKitchenHelper.Droid.Effects;
using AndroidX.Work;
using Utility;
using System.Runtime.Remoting.Messaging;
using AndroidX.Fragment.App;
using Javax.Crypto.Spec;

namespace ZestyKitchenHelper.Droid
{
    [Activity()]
    public class SelectionActivity :  AndroidX.Fragment.App.FragmentActivity
    {
        List<ContentPage> navigationStack = new List<ContentPage>();
        private UserProfile userProfile;
        public CameraDevice cam;
        public CameraCaptureSession camCapture;
        public CaptureRequest camCaptureRequest;

        Dictionary<string,  AndroidX.Fragment.App.Fragment> selectionFragments = new Dictionary<string,  AndroidX.Fragment.App.Fragment>();
         AndroidX.Fragment.App.Fragment mainSelectionFrag;
         AndroidX.Fragment.App.Fragment selectionFrag;
        AndroidX.Fragment.App.Fragment viewFrag;
        AndroidX.Fragment.App.Fragment unplacedFrag;

         AndroidX.Fragment.App.FragmentManager manager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            manager = SupportFragmentManager;
            base.OnCreate(savedInstanceState);
            Console.WriteLine("oncreate");
            ContentManager.selectionPage = new SelectionPage((obj, args) => UnplacedPage());
            mainSelectionFrag = ContentManager.selectionPage.CreateSupportFragment(this);
            manager.BeginTransaction().Replace(Android.Resource.Id.Content, mainSelectionFrag).Commit();
            AddNavigation(ContentManager.selectionPage.fridgeButton, ContentManager.selectionPage.cabinetButton);

            ActionBar?.Hide();
            TouchEffect.activity = this;

            ContentManager.navigateToInfoPageEvent = InformationPage;
            ContentManager.navigateToUnplacedPageEvent = UnplacedPage;

            GetLoginResult(savedInstanceState);

           // UpdateUserEvent();
           // GetUserEvent();
        }
        protected override void OnStart()
        {
            base.OnStart();
        }
        protected override void OnResume()
        {
            base.OnResume();
            if(ContentManager.singleSelectionPage != null) ContentManager.singleSelectionPage.SetView();
        }
        protected override void OnStop()
        {
            base.OnStop();
        }
        protected override void OnPause()
        {
            base.OnPause();
        }
        private void AddNavigation(Xamarin.Forms.ImageButton fridge, Xamarin.Forms.ImageButton cabinet)
        {
            fridge.Clicked += (obj, args) => NavigatePage(ContentManager.StorageSelection.fridge);
            cabinet.Clicked += (obj, args) => NavigatePage(ContentManager.StorageSelection.cabinet);
        }
        private void NavigatePage(ContentManager.StorageSelection selection)
        {
            ContentManager.storageSelection = selection;
            if (selection == ContentManager.StorageSelection.cabinet) {
                ContentManager.singleSelectionPage = new SingleSelectionPage(AddItemPage, ViewItemPage,
                    (n) => StorageCreationPage(false, n), () => StorageCreationPage(true), ReturnToMainSelection, DeleteCabinetLocal, FireBaseMediator.DeleteCabinet);
            }
            else
            {
                ContentManager.singleSelectionPage = new SingleSelectionPage(AddItemPage, ViewItemPage,
                    (n) => StorageCreationPage(false, n), () => StorageCreationPage(true), ReturnToMainSelection, DeleteCabinetLocal, FireBaseMediator.DeleteFridge);
            }
            selectionFrag = ContentManager.singleSelectionPage.CreateSupportFragment(this);
            navigationStack.Add(ContentManager.singleSelectionPage);
            manager.BeginTransaction().Replace(Android.Resource.Id.Content, selectionFrag).Commit();
        }

        private void ReturnToMainSelection(object obj, EventArgs args)
        {
            manager.BeginTransaction().Replace(Android.Resource.Id.Content, mainSelectionFrag).Commit();
        }

        private void ReturnToSelection()
        {
            ContentManager.singleSelectionPage.SetView();
            navigationStack.RemoveAt(navigationStack.Count - 1);
            manager.BeginTransaction().Replace(Android.Resource.Id.Content, selectionFrag).Commit();
        }
        private void UnplacedPage()
        {
            if(unplacedFrag == null)
            {
                UnplacedPage page = new UnplacedPage(ReturnToMainSelection, FireBaseMediator.PutItem, SaveItemsLocal, DeleteItemLocal, DeleteItemBase);
                unplacedFrag = page.CreateSupportFragment(this);
            }
            manager.BeginTransaction().Replace(Android.Resource.Id.Content, unplacedFrag).Commit();
        }
        private void AddItemPage(string name)
        {
             AndroidX.Fragment.App.Fragment frag = new  AndroidX.Fragment.App.Fragment();
            CabinetAddPage addPage = null;
            if (ContentManager.storageSelection == ContentManager.StorageSelection.fridge)
            {
                addPage = new CabinetAddPage(name, (obj, args) => ReturnToSelection(), SaveItemsLocal, FireBaseMediator.PutItem, InformationPage,
    ReturnToPreviousPage, SaveFridgeLocal, SaveFridgeBase);
            }
            else
            {
                addPage = new CabinetAddPage(name, (obj, args) => ReturnToSelection(), SaveItemsLocal, FireBaseMediator.PutItem, InformationPage,
    ReturnToPreviousPage, SaveCabinetLocal, SaveCabinetBase);
            }
            frag = addPage.CreateSupportFragment(this);
            navigationStack.Add(addPage);
            manager.BeginTransaction().Replace(Android.Resource.Id.Content, frag).Commit();
        }
        
        private void ViewItemPage(string name)
        {
            CabinetViewPage viewPage;
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                viewPage = new CabinetViewPage(name, (obj, args) => ReturnToSelection(), DeleteItemLocal, DeleteItemBase, UpdateItemLocal, FireBaseMediator.UpdateItem, SaveCabinetLocal, SaveCabinetBase);
            }
            else
            {
                viewPage = new CabinetViewPage(name, (obj, args) => ReturnToSelection(), DeleteItemLocal, DeleteItemBase, UpdateItemLocal, FireBaseMediator.UpdateItem, SaveFridgeLocal, SaveFridgeBase);
            }
            viewFrag = viewPage.CreateSupportFragment(this);
            navigationStack.Add(viewPage);
            manager.BeginTransaction().Replace(Android.Resource.Id.Content, viewFrag).Commit();
        }
        private void StorageCreationPage(bool newShelf, string name = "")
        {
             AndroidX.Fragment.App.Fragment fragment = null;
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                ContentManager.cabinetEditPage = new CabinetEditPage(newShelf, ReturnToSelection, SaveCabinetLocal, SaveCabinetBase, name);
                fragment = ContentManager.cabinetEditPage.CreateSupportFragment(this);
                navigationStack.Add(ContentManager.cabinetEditPage);
            }
            else
            {
                ContentManager.fridgeEditPage = new FridgeEditPage(newShelf, ReturnToSelection, SaveFridgeLocal, SaveFridgeBase, name);
                fragment = ContentManager.fridgeEditPage.CreateSupportFragment(this);
                navigationStack.Add(ContentManager.fridgeEditPage);
            }
            manager.BeginTransaction().Replace(Android.Resource.Id.Content, fragment).Commit();
            ActionBar?.Hide();
        }


        private async void SaveItemsLocal(Item item)
        { 
            MainActivity.localBase.SaveItemAsync(item);
            foreach(var stored in await MainActivity.localBase.GetItemListAsync())
            {
                Console.WriteLine("stored items brah " + stored.ID);
            }
        }

        private void DeleteItemLocal(Item item)
        {
            MainActivity.localBase.DeleteItemAsync(item);
        }

        private void DeleteItemBase(Item item)
        {
            FireBaseMediator.DeleteItem(item);
        }

        private void InformationPage(InfoPage infoPage)
        {
            var fragment = infoPage.CreateSupportFragment(this);
            manager.BeginTransaction().Replace(Android.Resource.Id.Content, fragment).Commit();
        }

        private void ReturnToPreviousPage()
        {
            if(navigationStack.Count > 1) 
            {
                (navigationStack[navigationStack.Count - 1] as INavigatablePage).SetView();
                var fragment = navigationStack[navigationStack.Count - 1].CreateSupportFragment(this);
                manager.BeginTransaction().Replace(Android.Resource.Id.Content, fragment).Commit();
            }
        }
        public static async void SaveCabinetLocal(string name, string cabinetRows, string rowItems)
        {
            var tryCabinet = await MainActivity.localBase.GetCabinetAsync(name);
            if (tryCabinet != null)
            {
                MainActivity.localBase.UpdateItemsAsync(new Cabinet().SetCabinet(cabinetRows, rowItems, name));
            }
            else
            {
                Cabinet cabinet = new Cabinet().SetCabinet(cabinetRows, rowItems, name);
                MainActivity.localBase.SaveItemAsync(cabinet);
            }
        }

        public static void SaveCabinetBase(string name, string cabinetRows, string rowItems)
        {
            if (ContentManager.sessionUserName != null)
            {
                FireBaseMediator.PutCabinet(new Cabinet().SetCabinet(cabinetRows, rowItems, name));
            }
        }
        public static async void SaveFridgeLocal(string name, string fridgeRows, string rowItems)
        {
            var tryFridge = await MainActivity.localBase.GetFridgeAsync(name);
            if (tryFridge != null)
            {
                MainActivity.localBase.UpdateItemsAsync(new Fridge().SetFridge(fridgeRows, rowItems, name));
            }
            else
            {
                Fridge fridge = new Fridge().SetFridge(fridgeRows, rowItems, name);
                MainActivity.localBase.SaveItemAsync(fridge);
            }
        }

        public static void SaveFridgeBase(string name, string fridgeRows, string rowItems)
        {
            if (ContentManager.sessionUserName != null)
            {
                FireBaseMediator.PutFridge(new Fridge().SetFridge(fridgeRows, rowItems, name));
            }
        }


        public static void DeleteCabinetLocal(string name)
        {
            MainActivity.localBase.DeleteCabinetAsync(name);
        }
        public static void DeleteFridgeLocal(string name)
        {
            MainActivity.localBase.DeleteFridgeAsync(name);
        }

        public static void UpdateItemLocal(Item item)
        {
            MainActivity.localBase.UpdateItemAsync(item);
        }
        private async void GetUserEvent()
        {
            var user = await MainActivity.dataBase.GetUser(ContentManager.sessionUserName);
            if (user != null)
            {

            }
        }
        private async void UpdateUserEvent()
        {
            await MainActivity.dataBase.UpdateUser(ContentManager.sessionUserName, new UserProfile() { Name = "ziming", Email = "cmail" });
        }
        private void GetLoginResult(Bundle saveInstanceState)
        {
            string loginResultAsJson = Intent.GetStringExtra("LoginResult") ?? string.Empty;
            Console.WriteLine("result length : " + loginResultAsJson);
            userProfile = JsonConvert.DeserializeObject<UserProfile>(loginResultAsJson);   
        }
    }
}