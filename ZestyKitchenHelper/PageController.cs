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
    public class PageController : ContentPage
    {
        private void SetContent(View page)
        {
            Content = page;
        }

        public void InitializePageSequence() {
            ContentManager.selectionPage = new SelectionPage();
            SetContent(ContentManager.selectionPage.Content);
        }
        public void InitializeSingleSelectionPage(ContentManager.StorageSelection selection)
        {
            ContentManager.storageSelection = selection;
            if (selection == ContentManager.StorageSelection.cabinet) {
                SetContent(new SingleSelectionPage(LocalStorageController.DeleteFridgeAsync, FireBaseMediator.DeleteCabinet).Content);
            }
            else
            {
                SetContent(new SingleSelectionPage(LocalStorageController.DeleteCabinetAsync, FireBaseMediator.DeleteFridge).Content);
            }
        }

        public void ToMainSelectionPage()
        {
            SetContent(ContentManager.selectionPage.Content);
        }

        public void ToSingleSelectionPage()
        {
            //ContentManager.singleSelectionPage.SetView();
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                SetContent(new SingleSelectionPage(LocalStorageController.DeleteCabinetAsync, FireBaseMediator.DeleteCabinet).Content);
            }
            else
            {
                SetContent(new SingleSelectionPage(LocalStorageController.DeleteFridgeAsync, FireBaseMediator.DeleteFridge).Content);
            }
        }
        public void ToUnplacedPage()
        {
            SetContent(new UnplacedPage(FireBaseMediator.PutItem, LocalStorageController.SaveItemAsync,
                LocalStorageController.DeleteItemAsync, FireBaseMediator.DeleteItem).Content);
        }
        public void ToAddItemPage(string name)
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.fridge)
            {
                SetContent(new CabinetAddPage(name, LocalStorageController.SaveItemAsync, FireBaseMediator.PutItem,
                LocalStorageController.SaveFridgeLocal, FireBaseMediator.SaveFridge).Content);
            }
            else
            {
                SetContent(new CabinetAddPage(name, LocalStorageController.SaveItemAsync, FireBaseMediator.PutItem,
                LocalStorageController.SaveCabinetLocal, FireBaseMediator.SaveCabinet).Content);
            }
        }

        public void ToViewItemPage(string name)
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                SetContent(new CabinetViewPage(name, LocalStorageController.DeleteItemAsync, FireBaseMediator.DeleteItem,
                    LocalStorageController.UpdateItemAsync, FireBaseMediator.UpdateItem,
                    LocalStorageController.SaveCabinetLocal, FireBaseMediator.SaveCabinet).Content);
            }
            else
            {
                SetContent(new CabinetViewPage(name, LocalStorageController.DeleteItemAsync, FireBaseMediator.DeleteItem,
                    LocalStorageController.UpdateItemAsync, FireBaseMediator.UpdateItem,
                    LocalStorageController.SaveFridgeLocal, FireBaseMediator.SaveFridge).Content);
            }
        }
        public void ToStorageCreationPage(bool newShelf, string name = "")
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                SetContent(new CabinetEditPage(newShelf, LocalStorageController.SaveCabinetLocal, FireBaseMediator.SaveCabinet, name).Content);
            }
            else
            {
                SetContent(new FridgeEditPage(newShelf, LocalStorageController.SaveFridgeLocal, FireBaseMediator.SaveFridge, name).Content);
            }
        }

        public void ToInfoPage(InfoPage infoPage)
        {
            SetContent(infoPage.Content);
        }
    }
}