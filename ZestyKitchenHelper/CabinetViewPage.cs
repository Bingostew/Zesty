using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Utility;

namespace ZestyKitchenHelper
{
    public class CabinetViewPage : ContentPage
    {
        const int max_grid_count = 20;
        private const int tool_grid_margin = 10;

        private Grid currentGrid;
        private AbsoluteLayout viewOverlay;
        private string storageName;
        private Dictionary<int, Grid> expandedViews = new Dictionary<int, Grid>();
        Action<Item> deleteItemLocalEvent, deleteItemBaseEvent, updateItemLocalEvent, updateItemBaseEvent;
        
        // If directSelectIndex is > -1, then the cell with this index will be displayed immediately after user enters the view.
        // DirectSelectStorageType is either "Cabinet" or "Fridge".
        public CabinetViewPage(string name, Action<Item> deleteItemLocal, Action<Item> deleteItemBase, Action<Item> updateItemLocal, Action<Item> updateItemBase, 
            int directSelectIndex = -1, string directSelectStorageType = "")
        {
            updateItemLocalEvent = updateItemLocal;
            updateItemBaseEvent = updateItemBase;
            deleteItemBaseEvent = deleteItemBase;
            deleteItemLocalEvent = deleteItemLocal;

            var titleGrid = new TopPage(name).GetGrid();
            titleGrid.HeightRequest = ContentManager.screenHeight * TopPage.top_bar_height_proportional;

            var backgroundImage = ContentManager.storageSelection == ContentManager.StorageSelection.fridge ? ContentManager.fridgeIcon : ContentManager.cabinetCellIcon;
            storageName = name;
            Image backgroundCell = new Image()
            { Source = backgroundImage, Aspect = Aspect.Fill };
            ImageButton backButton = new ImageButton() { Source = ContentManager.backButton, BackgroundColor = Color.Transparent, WidthRequest = 100, HeightRequest = 100 };
            backButton.Clicked += (obj, args) =>
            {
                if (currentGrid != null) { currentGrid.IsVisible = false; }
                viewOverlay.IsVisible = false;
            };
            const string expIndicatorString = "Expiration Date";
            const string alphaIndicatorString = "Alphabetical";
            var sortSelector = new Picker()
            {
                ItemsSource = new List<string>() { expIndicatorString, alphaIndicatorString },
                Title = "Sort Order",
            };
            sortSelector.SelectedIndexChanged += (obj, args) =>
            {
                switch (sortSelector.SelectedItem)
                {
                    case expIndicatorString: GridOrganizer.SortItemGrid(currentGrid, GridOrganizer.ItemSortingMode.Expiration_Close); break;
                    case alphaIndicatorString: GridOrganizer.SortItemGrid(currentGrid, GridOrganizer.ItemSortingMode.A_Z); break;
                }

            };

            AbsoluteLayout.SetLayoutBounds(sortSelector, new Rectangle(1, 0, 0.75, 0.1));
            AbsoluteLayout.SetLayoutFlags(sortSelector, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(backgroundCell, new Rectangle(0, 1, 1, .9));
            AbsoluteLayout.SetLayoutFlags(backgroundCell, AbsoluteLayoutFlags.All);
            viewOverlay = new AbsoluteLayout()
            {
                IsVisible = false,
                BackgroundColor = ContentManager.ThemeColor,
                WidthRequest = ContentManager.screenWidth,
                HeightRequest = ContentManager.screenHeight,
                Children =
                {
                    backgroundCell,
                    backButton
                }
            };
            ContentManager.AddOnBackgroundChangeListener(c => viewOverlay.BackgroundColor = c);

            var storageLabel = new Label() { Text = name, FontSize = 40, TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center };
            var storageView = ContentManager.GetStorageView(name);

            storageView.HorizontalOptions = LayoutOptions.CenterAndExpand;
            storageView.WidthRequest = Application.Current.MainPage.Width * .8;
            storageView.HeightRequest = 7 * Application.Current.MainPage.Height / 8;
            var storageViewFrame = new Frame() { BorderColor = Color.Black, Content = storageView };
            foreach (var cell in ContentManager.GetSelectedStorage(name).GetGridCells())
            {
                // Set up listener to show overlay
                ImageButton button = cell.GetButton();
                var grid = cell.GetItemGrid();
                ScrollView gridContainer = new ScrollView() { Content = grid };
                viewOverlay.Children.Add(gridContainer, AbsoluteLayout.GetLayoutBounds(backgroundCell), AbsoluteLayout.GetLayoutFlags(backgroundCell));
                
                button.Clicked += (obj, args) =>
                    {
                        viewOverlay.IsVisible = true;
                        currentGrid = grid;
                        grid.IsVisible = true;
                    };
  
                foreach(var child in grid.Children)
                {
                    child.IsVisible = true;
                }
            }
            Console.WriteLine("Cabinet View 136 " + directSelectIndex);
            // Set direct view of cell
            if (directSelectIndex >= 0)
            {
               Console.WriteLine("CabinetView 140 View item grid children: overlayed");
                viewOverlay.IsVisible = true;
                IStorage storage = directSelectStorageType == ContentManager.cabinetStorageType ? ContentManager.CabinetMetaBase[name] : (IStorage)ContentManager.FridgeMetaBase[name];
                currentGrid = storage.GetGridCell(directSelectIndex).GetItemGrid();
                storage.GetGridCell(directSelectIndex).GetItemGrid().IsVisible = true;

            }

            Content = new AbsoluteLayout()
            {
                Children =
                {
                    new StackLayout()
                    {
                        WidthRequest = Application.Current.MainPage.Width,
                        Children =
                        {
                            titleGrid,
                            storageLabel,
                            storageViewFrame
                        }
                    },
                    viewOverlay
                }
            };
        }
   }
}
