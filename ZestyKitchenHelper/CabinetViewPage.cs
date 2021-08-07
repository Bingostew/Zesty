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
        const int item_grid_margin = 10;
        const int layout_margin = 5;
        private const double storage_width_proportional_cabinet = 0.5;
        private const double storage_width_proportional_fridge = 0.5;
        const string expIndicatorString = "Expiration Date";
        const string alphaIndicatorString = "Alphabetical";

        private AbsoluteLayout viewOverlay;
        private string storageName;
        private Dictionary<int, Grid> expandedViews = new Dictionary<int, Grid>();
        private Grid currentGrid;
        Action<Item> deleteItemLocalEvent, deleteItemBaseEvent, updateItemLocalEvent, updateItemBaseEvent;
        
        // If directSelectIndex is > -1, then the cell with this index will be displayed immediately after user enters the view.
        public CabinetViewPage(string name, Action<Item> deleteItemLocal, Action<Item> deleteItemBase, Action<Item> updateItemLocal, Action<Item> updateItemBase,
            ContentManager.StorageSelection storageSelection, int directSelectIndex = -1)
        {
            updateItemLocalEvent = updateItemLocal;
            updateItemBaseEvent = updateItemBase;
            deleteItemBaseEvent = deleteItemBase;
            deleteItemLocalEvent = deleteItemLocal;

            var titleGrid = new TopPage(name).GetGrid();
            titleGrid.HeightRequest = ContentManager.screenHeight * TopPage.top_bar_height_proportional;

            var backgroundImage = storageSelection == ContentManager.StorageSelection.fridge ? ContentManager.fridgeIcon : ContentManager.cabinetCellIcon;
            storageName = name;
            Image backgroundCell = new Image()
            { Source = backgroundImage, Aspect = Aspect.Fill };
            ImageButton backButton = new ImageButton() { Source = ContentManager.backButton, BackgroundColor = Color.Transparent, WidthRequest = 100, HeightRequest = 100 };
            backButton.Clicked += (obj, args) =>
            {
                viewOverlay.IsVisible = false;
            };

            var sortSelector = new Picker()
            {
                Margin = new Thickness(layout_margin),
                ItemsSource = new List<string>() { expIndicatorString, alphaIndicatorString },
                Title = "Sort Order",
            };
            var searchBar = new SearchBar()
            {
                Margin = new Thickness(layout_margin),
                Placeholder = "Search"
            };

            AbsoluteLayout.SetLayoutBounds(sortSelector, new Rectangle(1, 0, 0.5, 0.1));
            AbsoluteLayout.SetLayoutFlags(sortSelector, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(searchBar, new Rectangle(1, 0.15, 0.5, 0.1));
            AbsoluteLayout.SetLayoutFlags(searchBar, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(backgroundCell, new Rectangle(0, 1, 1, .9));
            AbsoluteLayout.SetLayoutFlags(backgroundCell, AbsoluteLayoutFlags.All);
            viewOverlay = new AbsoluteLayout()
            {
                IsVisible = false,
                Margin = new Thickness(item_grid_margin),
                BackgroundColor = ContentManager.ThemeColor,
                WidthRequest = ContentManager.screenWidth,
                HeightRequest = ContentManager.screenHeight,
                Children =
                {
                    backgroundCell
                }
            };
            AbsoluteLayout.SetLayoutBounds(backgroundCell, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(backgroundCell, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(viewOverlay, new Rectangle(1, 1, ContentManager.screenWidth, ContentManager.screenHeight * 0.6));
            AbsoluteLayout.SetLayoutFlags(viewOverlay, AbsoluteLayoutFlags.PositionProportional);
            ContentManager.AddOnBackgroundChangeListener(c => viewOverlay.BackgroundColor = c);

            ScrollView gridContainer = new ScrollView();
            gridContainer.Scrolled += (o, a) => Console.WriteLine("CabinetView 74 gridcontainer scrolled");
            viewOverlay.Children.Add(gridContainer, AbsoluteLayout.GetLayoutBounds(backgroundCell), AbsoluteLayout.GetLayoutFlags(backgroundCell));

            sortSelector.SelectedIndexChanged += (obj, args) =>
            {
                if (currentGrid != null)
                {
                    switch (sortSelector.SelectedItem)
                    {
                        case expIndicatorString: GridOrganizer.SortItemGrid(currentGrid, GridOrganizer.ItemSortingMode.Expiration_Close); break;
                        case alphaIndicatorString: GridOrganizer.SortItemGrid(currentGrid, GridOrganizer.ItemSortingMode.A_Z); break;
                    }
                }
            };

            searchBar.Unfocused += (o, a) =>
            {
                var currentGridChildren = currentGrid.Children.Cast<ItemLayout>();
                Grid filteredGrid = GridManager.InitializeGrid(1, 4, new GridLength(ContentManager.item_layout_size, GridUnitType.Absolute), GridLength.Star);
                GridManager.FilterItemGrid(currentGridChildren, filteredGrid, searchBar.Text);
                gridContainer.Content = filteredGrid;
            };

            var storageView = ContentManager.GetStorageView(storageSelection, name);
            var storage = ContentManager.GetSelectedStorage(storageSelection, name);
            var storageViewWidth = storageSelection == ContentManager.StorageSelection.cabinet ? storage_width_proportional_cabinet : storage_width_proportional_fridge;

            AbsoluteLayout.SetLayoutBounds(storageView, new Rectangle(0, 0, storageViewWidth, 0.3));
            AbsoluteLayout.SetLayoutFlags(storageView, AbsoluteLayoutFlags.All);

            foreach (var cell in storage.GetGridCells())
            {
                // Set up listener to show overlay
                ImageButton button = cell.GetButton();
                var grid = cell.GetItemGrid();
                currentGrid = grid;
                Console.WriteLine("CabinetView 92 item grid length " + grid.Children.Count);

                button.Clicked += async (obj, args) =>
                {
                    viewOverlay.IsVisible = true;
                    gridContainer.Content = grid;
                    var viewOverlayXOffset = ContentManager.screenWidth * 0.75;
                    await viewOverlay.LinearInterpolator(viewOverlayXOffset, 200, t => viewOverlay.TranslationX = viewOverlayXOffset - t);
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
                var grid = storage.GetGridCell(directSelectIndex).GetItemGrid();
                gridContainer.Content = grid;
                grid.IsVisible = true;

            }

            Content = new AbsoluteLayout()
            {
                Children =
                {
                    new StackLayout()
                    {
                        HeightRequest = ContentManager.screenHeight,
                        WidthRequest = ContentManager.screenWidth,
                        Children =
                        {
                            titleGrid,
                            new AbsoluteLayout()
                            {
                                WidthRequest = ContentManager.screenWidth,
                                HeightRequest = ContentManager.screenHeight * 0.9,
                                Children = {storageView, viewOverlay, sortSelector, searchBar}
                            }
                        }
                    },
                }
            };
        }
    }
}
