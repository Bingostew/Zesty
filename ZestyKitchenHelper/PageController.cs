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
        private const string unplaced_page_name = "unplaced page";
        private const string view_page_name = "view page";
        private const string add_page_name = "add page";
        private const string single_selection_name = "single selection page";
        private const string edit_page_name = "edit page";
        private const string info_page_name = "info page";
        private const string selection_name = "selection page";
        private const string add_view_name = "add view";
        private const string scan_page_name = "scan page";
        private List<string> navigationStack = new List<string>();
        private List<List<object>> navigationParams = new List<List<object>>();
        public void InitializePageSequence()
        {
            ContentManager.selectionPage = new SelectionPage();
            Content = ContentManager.selectionPage.Content;
            navigationStack.Add(selection_name);
            navigationParams.Add(new List<object>() { });
        }

        public void ToMainSelectionPage()
        {
            Content = ContentManager.selectionPage.Content;
            navigationStack.Add(selection_name);
            navigationParams.Add(new List<object>() { });
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
            navigationStack.Add(single_selection_name);
            navigationParams.Add(new List<object>() { });
        }
        public void ToUnplacedPage()
        {
            Content = new UnplacedPage(FireBaseController.SaveItem, LocalStorageController.AddItem,
               LocalStorageController.DeleteItem, FireBaseController.DeleteItem).Content;

            navigationStack.Add(unplaced_page_name);
            navigationParams.Add(new List<object>() { });
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
            navigationStack.Add(add_page_name);
            navigationParams.Add(new List<object>() { name });
        }


        public void ToViewItemPage(string name, int directSelectIndex = -1, string directSelectStorageType = "")
        {
            Content = new CabinetViewPage(name, LocalStorageController.DeleteItem, FireBaseController.DeleteItem,
                LocalStorageController.UpdateItem, FireBaseController.SaveItem, directSelectIndex, directSelectStorageType).Content;

            navigationStack.Add(view_page_name);
            navigationParams.Add(new List<object>() { name, directSelectIndex, directSelectStorageType });
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
            navigationStack.Add(edit_page_name);
            navigationParams.Add(new List<object>() { newShelf, name });
        }

        public void ToInfoPage(InfoPage infoPage)
        {
            Content = infoPage.Content;
            navigationStack.Add(info_page_name);
            navigationParams.Add(new List<object>() { infoPage });
        }

        public void ToAddView(AddView addview)
        {
            Content = addview.Content;
            navigationStack.Add(add_view_name);
            navigationParams.Add(new List<object>() { addview });
        }

        public void ToScanPage(AddView addview)
        {
            var scannerPage = new BarcodeScannerPage(addview);
            Content = scannerPage.Content;
            scannerPage.StartScanning();
            navigationStack.Add(scan_page_name);
            navigationParams.Add(new List<object>() { });
        }

        public void ReturnToPrevious()
        {
            var contentString = navigationStack[navigationStack.Count - 2];
            var parameters = navigationParams[navigationParams.Count - 2];

            navigationStack.RemoveAt(navigationStack.Count - 1);
            navigationParams.RemoveAt(navigationParams.Count - 1);

            switch (contentString)
            {
                case unplaced_page_name:
                    ToUnplacedPage();
                    break;

                case add_page_name:
                    ToAddItemPage((string)parameters[0]);
                    break;

                case view_page_name:
                    ToViewItemPage((string)parameters[0], (int)parameters[1], (string)parameters[2]);
                    break;

                case edit_page_name:
                    ToStorageCreationPage((bool)parameters[0], (string)parameters[1]);
                    break;

                case single_selection_name:
                    ToSingleSelectionPage();
                    break;

                case info_page_name:
                    ToInfoPage((InfoPage)parameters[0]);
                    break;

                case selection_name:
                    ToMainSelectionPage();
                    break;

                case add_view_name:
                    ToAddView((AddView)parameters[0]);
                    break;
            }
            navigationStack.Remove(contentString);
            navigationParams.Remove(parameters);
        }
    }
}