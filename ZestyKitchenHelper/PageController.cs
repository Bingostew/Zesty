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

    public class PageController : Xamarin.Forms.TabbedPage
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
        private const string main_page_name = "main";
        private Dictionary<ContentPage, List<string>> navigationStack = new Dictionary<ContentPage, List<string>>();
        private Dictionary<ContentPage, List<List<object>>> navigationParams = new Dictionary<ContentPage, List<List<object>>>();
        private ContentPage cabinetSelectPage, fridgeSelectPage, unplacedPage; // 3 main pages

        private ContentPage currentPageContainer;
        private AbsoluteLayout currentPageContent;

        public Action resizeIconAction;

        public async void InitializePageSequence()
        {
            Console.WriteLine("PageController 32 []]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]] " + ContentManager.isUserNew + ContentManager.isLocal);

            this.BarBackgroundColor = Color.WhiteSmoke;
            this.BarTextColor = Color.Black;
            this.UnselectedTabColor = Color.Gray;
            this.SelectedTabColor = ContentManager.ThemeColor;

            BackgroundColor = ContentManager.ThemeColor;
            ContentManager.AddOnBackgroundChangeListener(c => BackgroundColor = c);

            ContentManager.AddOnBackgroundChangeListener(c => currentPageContainer.BackgroundColor = c);
            if (ContentManager.isUserNew)
            {
                SetUpPage setupPage = new SetUpPage();
                setupPage.Title = "Zesty";
                Children.Add(setupPage);
                Console.WriteLine("PageCOntroller 53 is change native view action null " + (ContentManager.SetNativeViewFunction == null));
                ContentManager.SetNativeViewFunction(setupPage);
            }
            else
            {
                if (ContentManager.isLocal)
                {
                    ContentManager.sessionUserProfile = await LocalStorageController.GetUserAsync();
                }
                ToMainPage();
            }
        }

        public void SetView(View view)
        {
            currentPageContent.Children.Clear();
            currentPageContent.Children.Add(view, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
        }

        Image viewOverlaybackgroundTint = new Image() { BackgroundColor = Color.FromRgba(255, 255, 255, 80) };
        public void SetViewOverlay(View viewOverlay, double width, double height, double xProportional, double yProportional, AbsoluteLayoutFlags layoutFlags = AbsoluteLayoutFlags.All)
        {
            Console.WriteLine("PageController 60 View Overlay Set");

            // Bug: For cabinet edit page, currentpage is not part of tabbed page, thus overlay is not added to the correct page
            currentPageContent.Children.Add(viewOverlaybackgroundTint, new Rectangle(0, 0, 1, 1), layoutFlags);
            currentPageContent.Children.Add(viewOverlay, new Rectangle(xProportional, yProportional, width, height), layoutFlags);
        }
        public void RemoveViewOverlay(View viewOverlay)
        {
            currentPageContent.Children.Remove(viewOverlaybackgroundTint);
            currentPageContent.Children.Remove(viewOverlay);
        }
        public void ShowAlert(string title, string body, string confirmString, string cancelString, Action onConfirmAction, Action onCancelAction)
        {
            Button confirmButton = new Button() { Text = confirmString, TextColor = Color.Black, BackgroundColor = Color.AntiqueWhite, CornerRadius = 10 };
            Button cancelButton = new Button() { Text = cancelString, TextColor = Color.Black, BackgroundColor = Color.AntiqueWhite, CornerRadius = 10 };
            Grid buttonGrid = GridManager.InitializeGrid(1, 2, GridLength.Star, GridLength.Star);
            buttonGrid.VerticalOptions = LayoutOptions.EndAndExpand;
            GridManager.AddGridItem(buttonGrid, new List<View>() { confirmButton, cancelButton }, false);

            Frame frame = new Frame()
            {
                BackgroundColor = Color.Bisque,
                CornerRadius = 45,
                Content = new StackLayout()
                {
                    Children =
                {
                    new Label(){Text = title, TextColor = Color.Black, FontFamily = "Oswald-Medium", FontSize = 25, HorizontalTextAlignment = TextAlignment.Center},
                    new Label(){Text = body, TextColor = Color.Black, Margin = new Thickness(10) , FontFamily = "Raleway-Regular", FontSize = 15},
                   buttonGrid
                }
                }
            };
        
            confirmButton.Clicked += (o, a) => { onConfirmAction?.Invoke(); RemoveViewOverlay(frame); };
            cancelButton.Clicked += (o, a) => { onCancelAction?.Invoke(); RemoveViewOverlay(frame); };

            frame.AnchorX = 0.5;
            frame.AnchorY = 0.5;

            SetViewOverlay(frame, 300, 300, 0.5, 0.5, AbsoluteLayoutFlags.PositionProportional);
        }

        public void ToMainPage()
        {
            Children.Clear();

            cabinetSelectPage = new SingleSelectionPage(LocalStorageController.DeleteCabinetSynchronous, FireBaseController.DeleteCabinetSynchronous, ContentManager.StorageSelection.cabinet);
            fridgeSelectPage = new SingleSelectionPage(LocalStorageController.DeleteFridgeSynchronous, FireBaseController.DeleteFridgeSynchronous, ContentManager.StorageSelection.fridge);
            unplacedPage = new UnplacedPage(LocalStorageController.AddItem, FireBaseController.SaveItem, LocalStorageController.DeleteItem, FireBaseController.DeleteItem);

            // offset view from the top of IOS black box 
            if (Device.RuntimePlatform == Device.iOS)
            {
                cabinetSelectPage.Content.AddEffect(new SafeAreaPadding());
            }
            cabinetSelectPage.Title = "Pantry";
            fridgeSelectPage.Title = "Fridge";
            unplacedPage.Title = "My Items";

            Children.Add(cabinetSelectPage);
            Children.Add(unplacedPage);
            Children.Add(fridgeSelectPage);
            navigationStack.Add(cabinetSelectPage, new List<string>());
            navigationStack.Add(fridgeSelectPage, new List<string>());
            navigationStack.Add(unplacedPage, new List<string>());
            navigationParams.Add(cabinetSelectPage, new List<List<object>>());
            navigationParams.Add(fridgeSelectPage, new List<List<object>>());
            navigationParams.Add(unplacedPage, new List<List<object>>());
            navigationStack[cabinetSelectPage].Add(single_selection_name);
            navigationParams[cabinetSelectPage].Add(new List<object>() { });
            navigationStack[fridgeSelectPage].Add(single_selection_name);
            navigationParams[fridgeSelectPage].Add(new List<object>() { });
            navigationStack[unplacedPage].Add(unplaced_page_name);
            navigationParams[unplacedPage].Add(new List<object>() { });

            currentPageContainer = (ContentPage)CurrentPage;
            currentPageContent = ((IMainPage)CurrentPage).GetLayout();

            CurrentPageChanged += (o, a) =>
            {
                currentPageContainer = (ContentPage)CurrentPage;
                currentPageContent = ((IMainPage)CurrentPage).GetLayout();
                RecordTabbedNavigationInfo();

                // offset view from the top of IOS black box 
                if (Device.RuntimePlatform == Device.iOS)
                {
                    Console.WriteLine("PageController 157 inset set");
                    currentPageContent.AddEffect(new SafeAreaPadding());
                }

                // For Android only, resize and detect icon expiration
                resizeIconAction?.Invoke();
                Console.WriteLine("Pagecontroller 163 resize action is null " + (resizeIconAction == null));
            };
            OverwriteRootPage(this);
        }
        private void RecordTabbedNavigationInfo()
        {
            Console.WriteLine("PageController 159 storage type " + ContentManager.storageSelection);
            if (CurrentPage == cabinetSelectPage)
            {
                ContentManager.storageSelection = ContentManager.StorageSelection.cabinet;
              //  navigationStack[currentPageContainer].Add(single_selection_name);
               // navigationParams[currentPageContainer].Add(new List<object>() { });
            }
            else if (currentPageContainer == fridgeSelectPage)
            {
                ContentManager.storageSelection = ContentManager.StorageSelection.fridge;
             //   navigationStack[currentPageContainer].Add(single_selection_name);
              //  navigationParams[currentPageContainer].Add(new List<object>() { });
            }
            else
            {
             //   navigationStack[currentPageContainer].Add(unplaced_page_name);
             //   navigationParams[currentPageContainer].Add(new List<object>() { });
            }

        }
        public void ToMainSelectionPage()
        { 
            SetView(new SelectionPage().Content);
            navigationStack[currentPageContainer].Add(selection_name);
            navigationParams[currentPageContainer].Add(new List<object>() { });
        }

        public void ToSingleSelectionPage()
        {
            //ContentManager.singleSelectionPage.SetView();
            if (ContentManager.storageSelection == ContentManager.StorageSelection.cabinet)
            {
                // Populate page with new content to remove overlayed pages
                var cabinetPage = new SingleSelectionPage(LocalStorageController.DeleteCabinetSynchronous, FireBaseController.DeleteCabinetSynchronous, ContentManager.StorageSelection.cabinet);
                ReturnToBasePage(cabinetPage);

                currentPageContent = cabinetPage.GetLayout();
            }
            else
            {
                // Populate page with new content to remove overlayed pages
                var fridgePage = new SingleSelectionPage(LocalStorageController.DeleteCabinetSynchronous, FireBaseController.DeleteCabinetSynchronous, ContentManager.StorageSelection.fridge);
                ReturnToBasePage(fridgePage);

                currentPageContent = fridgePage.GetLayout();
            }
            navigationStack[currentPageContainer].Add(single_selection_name);
            navigationParams[currentPageContainer].Add(new List<object>() { });
        }
        public void ToUnplacedPage()
        {
            UnplacedPage unplacedPage = (UnplacedPage)Children[1];

            ReturnToBasePage(unplacedPage);
            /*
            OverwriteRootPage(this);
            this.CurrentPage = unplacedPage;
            currentPageContent = unplacedPage.GetLayout();*/
            unplacedPage.UpdateLayout();

            navigationStack[currentPageContainer].Add(unplaced_page_name);
            navigationParams[currentPageContainer].Add(new List<object>() { });
        }

        public void ToAddItemPage(string name)
        {
            OverwriteRootPage(new CabinetAddPage(name));
            navigationStack[currentPageContainer].Add(add_page_name);
            navigationParams[currentPageContainer].Add(new List<object>() { name });
        }


        public void ToViewItemPage(string name, int directSelectIndex = -1, string directSelectStorageType = null)
        {
            // Change page accordingly when user uses "ViewInStorage" button
            if (directSelectStorageType == ContentManager.cabinetStorageType)
            {
                CurrentPage = cabinetSelectPage;
            }
            else if (directSelectStorageType == ContentManager.fridgeStorageType)
            {
                CurrentPage = fridgeSelectPage;
            }
            
            SetView(new CabinetViewPage(name, LocalStorageController.DeleteItem, FireBaseController.DeleteItem,
                LocalStorageController.UpdateItem, FireBaseController.SaveItem, 
                directSelectStorageType != null ? ContentManager.FromStorageType(directSelectStorageType) : ContentManager.storageSelection, 
                directSelectIndex).Content);

            navigationStack[currentPageContainer].Add(view_page_name);
            navigationParams[currentPageContainer].Add(new List<object>() { name, directSelectIndex, directSelectStorageType });
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
            navigationStack[currentPageContainer].Add(edit_page_name);
            navigationParams[currentPageContainer].Add(new List<object>() { newShelf, name });
        }

        public void ToAddView(AddView addview)
        {
            OverwriteRootPage(addview);
           // currentPageContent = addview.GetLayout();
            navigationStack[currentPageContainer].Add(add_view_name);
            navigationParams[currentPageContainer].Add(new List<object>() { addview });
        }

        public void ToScanPage(AddView addview)
        {
            var scannerPage = new BarcodeScannerPage(addview);
            OverwriteRootPage(scannerPage);
            scannerPage.StartScanning();
            navigationStack[currentPageContainer].Add(scan_page_name);
            navigationParams[currentPageContainer].Add(new List<object>() { });
        }

        public void ToPreferencePage(PreferencePage preferencePage)
        {
            OverwriteRootPage(preferencePage);
            //SetView(preferencePage.Content);
            navigationStack[currentPageContainer].Add(preference_page_name);
            navigationParams[currentPageContainer].Add(new List<object>() { preferencePage });
        }

        // Preform different operations to return to SingleSelectPage and UnplacedPage for IOS/Android.
        private void ReturnToBasePage(IMainPage page)
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                ((IMainPage)currentPageContainer).SetLayout(page.GetLayout());
                currentPageContainer.BackgroundColor = ContentManager.ThemeColor;
                currentPageContainer.Content.AddEffect(new SafeAreaPadding());
                resizeIconAction?.Invoke();
            }
            else// No work
            {
                Console.WriteLine("Page Controller 322 Root Overwritten");
                ContentManager.SetNativeViewFunction(this);
              //  currentPageContainer.Content = null;
                ((IMainPage)currentPageContainer).SetLayout(page.GetLayout());
                /*

                var index = Children.IndexOf(currentPageContainer);
                Children.Remove(currentPageContainer);
                currentPageContainer = (ContentPage)page;
                currentPageContainer.Content = page.GetLayout();
                currentPageContent = page.GetLayout();
                Children.Insert(index, currentPageContainer);
                CurrentPage = currentPageContainer;
                navigationStack.Add((ContentPage)page, new List<string>());
                navigationParams.Add((ContentPage)page, new List<List<object>>());*/
            }
        }

        private void OverwriteRootPage(Page newPage)
        {
            // Check if a page is already overwritten into the native view. If it is, it must be removed for IOS to prevent going back to an older version of a view
            // (since new native views are added to subviews)
            if (IsOnNativeView(navigationStack[currentPageContainer].Last()))
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    PopRootPage();
                }
            }
            // offset view from the top of IOS black box 
            if (Device.RuntimePlatform == Device.iOS)
            {
                if(newPage is ContentPage)
                    ((ContentPage)newPage).Content.AddEffect(new SafeAreaPadding());
                else
                    newPage.AddEffect(new SafeAreaPadding());
            }

            ContentManager.SetNativeViewFunction(newPage);
        }

        private void PopRootPage()
        {
            ContentManager.PopNativeViewFunction();
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
            var currentPageString = navigationStack[currentPageContainer].Last();
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

        private bool IsOnNativeView(string contentString)
        {
            return contentString == preference_page_name || contentString == add_view_name || contentString == scan_page_name || contentString == add_page_name;
        }
        public void ReturnToPrevious()
        {
            // If current page overridden native view but next page does not, this page needs to be popped as the next page will not be automatically popped as it would
            // otherwise with OverwriteRootPage() function.
            if (IsOnNativeView(navigationStack[currentPageContainer].Last()) && !IsOnNativeView(navigationStack[currentPageContainer][navigationStack[currentPageContainer].Count - 2]))
            {
                if (Device.RuntimePlatform == Device.iOS)
                {
                    PopRootPage();
                }
            }
            var contentString = navigationStack[currentPageContainer][navigationStack[currentPageContainer].Count - 2];
            var parameters = navigationParams[currentPageContainer][navigationParams[currentPageContainer].Count - 2];

            Console.WriteLine("Page Controller 407 content string " + contentString);
            navigationStack[currentPageContainer].RemoveAt(navigationStack[currentPageContainer].Count - 1);
            navigationParams[currentPageContainer].RemoveAt(navigationParams[currentPageContainer].Count - 1);

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
            navigationStack[currentPageContainer].Remove(contentString);
            navigationParams[currentPageContainer].Remove(parameters);
        }
    }
}