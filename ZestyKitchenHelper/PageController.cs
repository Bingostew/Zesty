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
        private const string selection_name = "selection page";
        private const string add_view_name = "add view";
        private const string scan_page_name = "scan page";
        private const string preference_page_name = "preference page";
        private List<string> navigationStack = new List<string>();
        private List<List<object>> navigationParams = new List<List<object>>();

        AbsoluteLayout pageContainer;
        public async void InitializePageSequence()
        {
            Console.WriteLine("PageController 32 []]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]] " + ContentManager.isUserNew + ContentManager.isLocal);
            pageContainer = new AbsoluteLayout() { WidthRequest = ContentManager.screenWidth, HeightRequest = ContentManager.screenHeight, BackgroundColor = Color.Wheat };
            if(Device.RuntimePlatform == Device.iOS)
            {
                pageContainer.Effects.Add(new SafeAreaPadding());
            }
            ContentManager.AddOnBackgroundChangeListener(c => pageContainer.BackgroundColor = c);
            if (ContentManager.isUserNew)
            {
                SetUpPage setupPage = new SetUpPage();
                SetView(setupPage.Content);
            }
            else
            {
                if (ContentManager.isLocal)
                {
                    ContentManager.sessionUserProfile = await LocalStorageController.GetUserAsync();
                }
                ToMainSelectionPage();
            }
            Content = pageContainer;
        }

        public void SetView(View view)
        {
            pageContainer.Children.Clear();
            pageContainer.Children.Add(view, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
        }

        Image viewOverlaybackgroundTint = new Image() { BackgroundColor = Color.FromRgba(255, 255, 255, 80) };
        public void SetViewOverlay(View viewOverlay, double width, double height, double xProportional, double yProportional, AbsoluteLayoutFlags layoutFlags = AbsoluteLayoutFlags.All)
        {
            Console.WriteLine("PageController 60 View Overlay Set");

            pageContainer.Children.Add(viewOverlaybackgroundTint, new Rectangle(0, 0, 1, 1), layoutFlags);
            pageContainer.Children.Add(viewOverlay, new Rectangle(xProportional, yProportional, width, height), layoutFlags);
        }
        public void RemoveViewOverlay(View viewOverlay)
        {
            pageContainer.Children.Remove(viewOverlaybackgroundTint);
            pageContainer.Children.Remove(viewOverlay);
        }
        public void ShowAlert(string title, string body, string confirmString, string cancelString, Action onConfirmAction, Action onCancelAction)
        {
            Button confirmButton = new Button() { Text = confirmString, TextColor = Color.Black };
            Button cancelButton = new Button() { Text = cancelString, TextColor = Color.Black };
            Grid buttonGrid = GridManager.InitializeGrid(1, 2, GridLength.Star, GridLength.Star);
            buttonGrid.VerticalOptions = LayoutOptions.EndAndExpand;
            GridManager.AddGridItem(buttonGrid, new List<View>() { confirmButton, cancelButton }, false);

            StackLayout view = new StackLayout()
            {
                BackgroundColor = Color.WhiteSmoke,
                Children =
                {
                    new Label(){Text = title, TextColor = Color.Black, FontFamily = "Oswald-Medium", FontSize = 25, HorizontalTextAlignment = TextAlignment.Center},
                    new Label(){Text = body, TextColor = Color.Black, Margin = new Thickness(10) , FontFamily = "Raleway-Regular", FontSize = 15},
                   buttonGrid
                }
            };

            confirmButton.Clicked += (o, a) => { onConfirmAction?.Invoke(); RemoveViewOverlay(view); };
            cancelButton.Clicked += (o, a) => { onCancelAction?.Invoke(); RemoveViewOverlay(view); };

            view.AnchorX = 0.5;
            view.AnchorY = 0.5;

            SetViewOverlay(view, 300, 300, 0.5, 0.5, AbsoluteLayoutFlags.PositionProportional);
        }

        public void ToMainSelectionPage()
        {
            SetView(new SelectionPage().Content);
            navigationStack.Add(selection_name);
            navigationParams.Add(new List<object>() { });
        }

        public void ToSingleSelectionPage()
        {
            //ContentManager.singleSelectionPage.SetView();
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                SetView(new SingleSelectionPage(LocalStorageController.DeleteCabinetSynchronous, FireBaseController.DeleteCabinetSynchronous).Content);
            }
            else
            {

                SetView(new SingleSelectionPage(LocalStorageController.DeleteFridgeSynchronous, FireBaseController.DeleteFridgeSynchronous).Content);
            }
            navigationStack.Add(single_selection_name);
            navigationParams.Add(new List<object>() { });
        }
        public void ToUnplacedPage()
        {
            SetView(new UnplacedPage(LocalStorageController.AddItem, FireBaseController.SaveItem,
               LocalStorageController.DeleteItem, FireBaseController.DeleteItem).Content);

            navigationStack.Add(unplaced_page_name);
            navigationParams.Add(new List<object>() { });
        }

        public void ToAddItemPage(string name)
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.fridge)
            {
                SetView(new CabinetAddPage(name).Content);
            }
            else
            {
                SetView(new CabinetAddPage(name).Content);
            }
            navigationStack.Add(add_page_name);
            navigationParams.Add(new List<object>() { name });
        }


        public void ToViewItemPage(string name, int directSelectIndex = -1, string directSelectStorageType = null)
        {
            SetView(new CabinetViewPage(name, LocalStorageController.DeleteItem, FireBaseController.DeleteItem,
                LocalStorageController.UpdateItem, FireBaseController.SaveItem, 
                directSelectStorageType != null ? ContentManager.FromStorageType(directSelectStorageType) : ContentManager.storageSelection, 
                directSelectIndex).Content);

            navigationStack.Add(view_page_name);
            navigationParams.Add(new List<object>() { name, directSelectIndex, directSelectStorageType });
        }
        public void ToStorageCreationPage(bool newShelf, string name = "")
        {
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                SetView(new CabinetEditPage(newShelf, LocalStorageController.AddCabinet, FireBaseController.SaveCabinet, name).Content);
            }
            else
            {
                SetView(new FridgeEditPage(newShelf, LocalStorageController.AddFridge, FireBaseController.SaveFridge, name).Content);
            }
            navigationStack.Add(edit_page_name);
            navigationParams.Add(new List<object>() { newShelf, name });
        }

        public void ToAddView(AddView addview)
        {
            SetView(addview.Content);
            navigationStack.Add(add_view_name);
            navigationParams.Add(new List<object>() { addview });
        }

        public void ToScanPage(AddView addview)
        {
            var scannerPage = new BarcodeScannerPage(addview);
            SetView(scannerPage.Content);
            scannerPage.StartScanning();
            navigationStack.Add(scan_page_name);
            navigationParams.Add(new List<object>() { });
        }

        public void ToPreferencePage(PreferencePage preferencePage)
        {
            SetView(preferencePage.Content);
            navigationStack.Add(preference_page_name);
            navigationParams.Add(new List<object>() { preferencePage });
        }

        bool hasShownInfoView;
        public async void ShowInfoView(InfoView infoView)
        {
            if (hasShownInfoView)
                return;
            hasShownInfoView = true;
            SetViewOverlay(infoView.GetView(), InfoView.info_view_width_proportional, InfoView.info_view_height_proportional, 0.5, 0.5);
            await infoView.GetView().LinearInterpolator(1, 150, (d) => infoView.GetView().Scale = d);

        }

        public void RemoveInfoView(InfoView infoView)
        {
            hasShownInfoView = false;
            RemoveViewOverlay(infoView.GetView());
        }

        public async void OverlayAnimation(View animatedView, Rect bounds, Task<bool> animateAction, Action onFinishAction = null)
        {
            SetViewOverlay(animatedView, bounds.Width, bounds.Height, bounds.X, bounds.Y);
            await animateAction;
            RemoveViewOverlay(animatedView);
            onFinishAction?.Invoke();
        }

        public bool IsOnPage<T>()
        {
            var type = typeof(T);
            var currentPageString = navigationStack.Last();
            if ((type == typeof(CabinetViewPage) && currentPageString == view_page_name) ||
                (type == typeof(CabinetEditPage) && currentPageString == edit_page_name) ||
                (type == typeof(CabinetAddPage) && currentPageString == add_page_name) ||
                (type == typeof(UnplacedPage) && currentPageString == unplaced_page_name)||
                (type == typeof(SingleSelectionPage) && currentPageString == single_selection_name) ||
                (type == typeof(AddView) && currentPageString == add_view_name) ||
                (type == typeof(PreferencePage) && currentPageString == preference_page_name))
                return true;
            
            return false;
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

                case selection_name:
                    ToMainSelectionPage();
                    break;

                case add_view_name:
                    ToAddView((AddView)parameters[0]);
                    break;
                case preference_page_name:
                    ToPreferencePage((PreferencePage)parameters[0]);
                    break;
            }
            navigationStack.Remove(contentString);
            navigationParams.Remove(parameters);
        }
    }
}