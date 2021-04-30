using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xamarin.Forms;
using Utility;
using System.Runtime.CompilerServices;

namespace ZestyKitchenHelper
{
    public class CabinetAddPage : ContentPage
    {
        AbsoluteLayout pageContainer;
        AbsoluteLayout addForm;
        static AbsoluteLayout storage;
        private static string storageName;
        static Action<string, string, string> saveLocalItemToShelfEvent, saveBaseItemToShelfEvent;
        const int unplacedGridRows = 2;
        const int unplacedGridColumns = 4;
        private static int gridFootIndex;
        private static Grid unplacedGrid, partialUnplacedGrid;
        public CabinetAddPage(string _storageName, Action<Item> saveLocalItemEvent, Action<Item> saveBaseItemEvent,
            Action<string, string, string> _saveLocalItemToShelfEvent, Action<string, string, string> _saveBaseItemToShelfEvent)
        {
           
            storageName = _storageName;
            saveLocalItemToShelfEvent = _saveLocalItemToShelfEvent;
            saveBaseItemToShelfEvent = _saveBaseItemToShelfEvent;

            //-- set up storage view
            var name = new Label() { Text = _storageName, FontSize = 30, TextColor = Color.Black };
            storage = ContentManager.GetStorageView(storageName) as AbsoluteLayout;
            storage.WidthRequest = Application.Current.MainPage.Width * .8;
            storage.HeightRequest = 5 * Application.Current.MainPage.Height / 8;
            storage.VerticalOptions = LayoutOptions.EndAndExpand;

            //-- set up unplaced grid
            unplacedGrid = GridManager.GetGrid(ContentManager.unplacedGridName);
            partialUnplacedGrid = GridManager.InitializeGrid(ContentManager.pUnplacedGridName, 2, 4, GridLength.Star, GridLength.Star);
            // add listener to set TouchEffect for each new item added
            partialUnplacedGrid.ChildAdded += (o, v) => EffectManager.UpdateScreenTouchBounds(partialUnplacedGrid.Children.Last() as ItemLayout, storageName);
            // initialize grid by constraining UnplacedGrid
            partialUnplacedGrid = GridManager.ConstrainGrid(unplacedGrid, 0, 8, partialUnplacedGrid, null, true);
            addForm = AddView.GetAddForm(saveLocalItemEvent, saveBaseItemEvent, storageName, true, partialUnplacedGrid);

            var backButton = new ImageButton() { Source = ContentManager.backButton, Aspect = Aspect.Fill, BackgroundColor = Color.Transparent };
            backButton.Clicked += (o, a) =>
            {
                ContentManager.pageController.ToSingleSelectionPage();
                foreach (ItemLayout child in partialUnplacedGrid.Children)
                {
                    child.RemoveEffect(typeof(ScreenTouch));
                }
            };

            gridFootIndex = 0;
            var gridPageSelectGrid = new Grid()
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition() { Width = 50 },
                    new ColumnDefinition() { Width = 30 },
                    new ColumnDefinition() { Width = 30 },
                    new ColumnDefinition() { Width = GridLength.Auto}
                },
                HeightRequest = 50
            };
            var nextPage = new ImageButton() { Source = ContentManager.countIcon };
            nextPage.Clicked += (obj, args) =>
            {
                gridFootIndex += unplacedGridColumns * unplacedGridRows;
                if (ContentManager.UnplacedItemBase.Count > gridFootIndex + 1)
                {
                   // partialUnplacedGrid = GridManager.ConstrainGrid(unplacedGrid, gridFootIndex, unplacedGridColumns * unplacedGridRows, 
                     //   true, 2, 4, ContentManager.pUnplacedGridName);
                }
            };
            var lastPage = new ImageButton() { Source = ContentManager.countIcon, Rotation = 180 };
            lastPage.Clicked += (obj, args) =>
            {
                gridFootIndex -= unplacedGridColumns * unplacedGridRows;
                if (gridFootIndex > 0)
                {
                   // partialUnplacedGrid =  GridManager.ConstrainGrid(unplacedGrid, gridFootIndex, unplacedGridColumns * unplacedGridRows, 
                     //   true, 2, 4, ContentManager.pUnplacedGridName);
                }
            };
            var addNewButton = new ImageButton() { Source = ContentManager.addIcon, BackgroundColor = Color.Transparent };
            addNewButton.Clicked += (obj, args) => { addForm.IsVisible = true; };
            var searchBar = new SearchBar() { Text = ContentManager.defaultSearchAllBarText, MinimumWidthRequest = 300 };
            searchBar.Focused += (obj, args) => searchBar.Text = "";
            searchBar.Unfocused += (obj, args) => { if (searchBar.Text.Length == 0) searchBar.Text = ContentManager.defaultSearchAllBarText; };
            searchBar.TextChanged += (obj, args) => OnSearch(args.NewTextValue);

            void OnSearch(string text)
            {
                partialUnplacedGrid.Children.Clear();
                foreach (ItemLayout item in partialUnplacedGrid.Children) {
                    var match = 0;
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (i < item.ItemData.name.Length && string.Equals(text[i].ToString(), item.ItemData.name[i].ToString(), StringComparison.OrdinalIgnoreCase)){
                            match++;
                        }
                        else { match = 0; break; }
                    }
                    if ((match > 0 && !partialUnplacedGrid.Children.Contains(item)) || text == "" || text == ContentManager.defaultSearchAllBarText)
                    {
                        partialUnplacedGrid.Children.Add(item);
                    }
                }
            }
            gridPageSelectGrid.Children.Add(lastPage, 1, 0);
            gridPageSelectGrid.Children.Add(nextPage, 2, 0);
            gridPageSelectGrid.Children.Add(addNewButton, 0, 0);
            gridPageSelectGrid.Children.Add(searchBar, 3, 0);
            List<View> contactList = new List<View>();
            foreach (var dict in ContentManager.GetItemBase()[storageName].Values)
            {
                foreach (ImageButton button in dict.Keys)
                {
                    contactList.Add(button);
                }
            }

            var infoGrid = new Grid() { HeightRequest = 40, ColumnDefinitions = { new ColumnDefinition() { Width = 100 }, new ColumnDefinition() }};
            infoGrid.Children.Add(backButton, 0, 0);
            infoGrid.Children.Add(name, 1, 0);

            pageContainer = new AbsoluteLayout();
            pageContainer.BackgroundColor = Color.Wheat;
            pageContainer.Children.Add(infoGrid, new Rectangle(0,0,1, .1), AbsoluteLayoutFlags.All);
            pageContainer.Children.Add(storage, new Rectangle(.5, 1, .8, .5), AbsoluteLayoutFlags.All);
            pageContainer.Children.Add(gridPageSelectGrid, new Rectangle(0, .07, 1, .1), AbsoluteLayoutFlags.All);
            pageContainer.Children.Add(partialUnplacedGrid, new Rectangle(0, .25, 1, .3), AbsoluteLayoutFlags.All);

            Content = new AbsoluteLayout()
            {
                Children =
                {
                    pageContainer,
                    addForm
                }
            };
            AbsoluteLayout.SetLayoutBounds(pageContainer, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(pageContainer, AbsoluteLayoutFlags.All);
        }


        public static async void UpdateShelf(string name, ItemLayout item, int cellIndex,  Item itemStruct)
        {
            var itemInfo = ContentManager.GetInfoBase();
            item.IsVisible = false;
            item.ItemData.stored = true;
            AbsoluteLayout.SetLayoutBounds(item, new Rectangle(0, 0, 0, 0));
            AbsoluteLayout.SetLayoutFlags(item, AbsoluteLayoutFlags.None);
            var itemAmt = itemInfo[name][cellIndex].Children.Where(e => e.GetType() == typeof(ItemLayout) && e.TranslationX == item.TranslationX).ToList().Count;
            item.StorageName = name;
            item.StorageIndex = cellIndex;
            item.SetMarkingVisibility(false);
            ContentManager.UnplacedItemBase.Remove(itemStruct.ID);
            item.iconImage.ClearEffects();
            GridManager.RemoveGridItem(partialUnplacedGrid, item);
            GridManager.RemoveGridItem(GridManager.GetGrid(ContentManager.unplacedGridName), item);
            await ViewExtensions.QuadraticInterpolator(itemInfo[name][cellIndex], .5, 250, d => itemInfo[name][cellIndex].Scale = d, null);
            string rowInfo, rowItems;
            if(ContentManager.storageSelection == ContentManager.StorageSelection.fridge)
                ContentManager.SetLocalFridge(name, out rowInfo, out rowItems);
            else
                ContentManager.SetLocalCabinet(name, out rowInfo, out rowItems);
            saveLocalItemToShelfEvent(name, rowInfo, rowItems);
            saveBaseItemToShelfEvent(name, rowInfo, rowItems);
        }
    }
}
