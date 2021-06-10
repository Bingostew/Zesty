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

        public void InitializePageSequence() {
            ContentManager.selectionPage = new SelectionPage();
            Content = ContentManager.selectionPage.Content;
        }
        public void InitializeSingleSelectionPage(ContentManager.StorageSelection selection)
        {
            ContentManager.storageSelection = selection;
            if (selection == ContentManager.StorageSelection.cabinet) {
                Content = new SingleSelectionPage(LocalStorageController.DeleteFridgeAsync, FireBaseMediator.DeleteCabinet).Content;
            }
            else
            {
                Content = new SingleSelectionPage(LocalStorageController.DeleteCabinetAsync, FireBaseMediator.DeleteFridge).Content;
            }
        }

        public void ToMainSelectionPage()
        {
            Content = ContentManager.selectionPage.Content;
        }

        public void ToSingleSelectionPage()
        {
            //ContentManager.singleSelectionPage.SetView();
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                Content = new SingleSelectionPage(LocalStorageController.DeleteCabinetAsync, FireBaseMediator.DeleteCabinet).Content;
            }
            else
            {
                Content = new SingleSelectionPage(LocalStorageController.DeleteFridgeAsync, FireBaseMediator.DeleteFridge).Content;
            }
        }
        public void ToUnplacedPage()
        {
            Content = new UnplacedPage(FireBaseMediator.PutItem, LocalStorageController.SaveItemAsync,
                LocalStorageController.DeleteItemAsync, FireBaseMediator.DeleteItem).Content;
        }
        public void ToAddItemPage(string name)
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.fridge)
            {
                Content = new CabinetAddPage(name, LocalStorageController.SaveItemAsync, FireBaseMediator.PutItem,
                LocalStorageController.SaveFridgeLocal, FireBaseMediator.SaveFridge).Content;
            }
            else
            {
                Content = new CabinetAddPage(name, LocalStorageController.SaveItemAsync, FireBaseMediator.PutItem,
                LocalStorageController.SaveCabinetLocal, FireBaseMediator.SaveCabinet).Content;
            }
        }

        public void ToViewItemPage(string name)
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                Content = new CabinetViewPage(name, LocalStorageController.DeleteItemAsync, FireBaseMediator.DeleteItem,
                    LocalStorageController.UpdateItemAsync, FireBaseMediator.UpdateItem,
                    LocalStorageController.SaveCabinetLocal, FireBaseMediator.SaveCabinet).Content;
            }
            else
            {
                Content = new CabinetViewPage(name, LocalStorageController.DeleteItemAsync, FireBaseMediator.DeleteItem,
                    LocalStorageController.UpdateItemAsync, FireBaseMediator.UpdateItem,
                    LocalStorageController.SaveFridgeLocal, FireBaseMediator.SaveFridge).Content;
            }
        }
        public void ToStorageCreationPage(bool newShelf, string name = "")
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                Content = new CabinetEditPage(newShelf, LocalStorageController.SaveCabinetLocal, FireBaseMediator.SaveCabinet, name).Content;
            }
            else
            {
                Content = new FridgeEditPage(newShelf, LocalStorageController.SaveFridgeLocal, FireBaseMediator.SaveFridge, name).Content;
            }
        }

        public void ToInfoPage(InfoPage infoPage)
        {
            Content = infoPage.Content;
        }
    }
}