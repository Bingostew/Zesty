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

        public void ToMainSelectionPage()
        {
            Content = ContentManager.selectionPage.Content;
        }

        public void ToSingleSelectionPage()
        {
            //ContentManager.singleSelectionPage.SetView();
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                Content = new SingleSelectionPage(LocalStorageController.DeleteCabinet, FireBaseController.DeleteCabinet).Content;
            }
            else
            {
                Content = new SingleSelectionPage(LocalStorageController.DeleteFridge, FireBaseController.DeleteFridge).Content;
            }
        }
        public void ToUnplacedPage()
        {
            Content = new UnplacedPage(FireBaseController.SaveItem, LocalStorageController.AddItem,
                LocalStorageController.DeleteItem, FireBaseController.DeleteItem).Content;
        }
        
        public void ToAddItemPage(string name)
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.fridge)
            {
                Content = new CabinetAddPage(name).Content;
            }
            else
            {
                Content = new CabinetAddPage(name).Content;
            }
        }


        public void ToViewItemPage(string name, int directSelectIndex = -1, string directSelectStorageType = "")
        {
            Content = new CabinetViewPage(name, LocalStorageController.DeleteItem, FireBaseController.DeleteItem,
                LocalStorageController.UpdateItem, FireBaseController.SaveItem, directSelectIndex, directSelectStorageType).Content;
        }
        public void ToStorageCreationPage(bool newShelf, string name = "")
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                Content = new CabinetEditPage(newShelf, LocalStorageController.AddCabinet, FireBaseController.SaveCabinet, name).Content;
            }
            else
            {
                Content = new FridgeEditPage(newShelf, LocalStorageController.AddFridge, FireBaseController.SaveFridge, name).Content;
            }
        }

        public void ToInfoPage(InfoPage infoPage)
        {
            Content = infoPage.Content;
        }
    }
}