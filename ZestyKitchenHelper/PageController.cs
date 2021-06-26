using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;
using Utility;

namespace ZestyKitchenHelper
{
    public class PageController : NavigationPage
    {

        public async void InitializePageSequence() {
            ContentManager.selectionPage = new SelectionPage();
            await PushAsync(ContentManager.selectionPage);
            SetHasNavigationBar(RootPage, false);
        }

        public async void ToMainSelectionPage()
        {
            await PushAsync(ContentManager.selectionPage);
        }

        public async void ToSingleSelectionPage(bool animated)
        {
            //ContentManager.singleSelectionPage.SetView();
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                await PushAsync(new SingleSelectionPage(LocalStorageController.DeleteCabinet, FireBaseController.DeleteCabinet), animated);
            }
            else
            {
                await PushAsync(new SingleSelectionPage(LocalStorageController.DeleteFridge, FireBaseController.DeleteFridge), animated);
            }
        }
        public async void ToUnplacedPage()
        {
             await PushAsync(new UnplacedPage(FireBaseController.SaveItem, LocalStorageController.AddItem,
                LocalStorageController.DeleteItem, FireBaseController.DeleteItem));
        }
        
        public async void ToAddItemPage(string name)
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.fridge)
            {
                await PushAsync(new CabinetAddPage(name));
            }
            else
            {
                await PushAsync(new CabinetAddPage(name));
            }
        }


        public async void ToViewItemPage(string name, int directSelectIndex = -1, string directSelectStorageType = "")
        {
            await PushAsync(new CabinetViewPage(name, LocalStorageController.DeleteItem, FireBaseController.DeleteItem,
                LocalStorageController.UpdateItem, FireBaseController.SaveItem, directSelectIndex, directSelectStorageType));
        }
        public async void ToStorageCreationPage(bool newShelf, string name = "")
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                await PushAsync(new CabinetEditPage(newShelf, LocalStorageController.AddCabinet, FireBaseController.SaveCabinet, name));
            }
            else
            {
                await PushAsync(new FridgeEditPage(newShelf, LocalStorageController.AddFridge, FireBaseController.SaveFridge, name));
            }
        }

        public async void ToInfoPage(InfoPage infoPage)
        {
            await PushAsync(infoPage);
        }

        public async void ToAddView(AddView addview)
        {
            await PushAsync(addview);
        }
        public async void ReturnToPrevious()
        {
            await PopAsync();
        }
    }
}