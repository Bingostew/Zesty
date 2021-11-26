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
        const int layout_margin = 5;
        private const double storage_width_proportional_cabinet = 0.5;
        private const double storage_width_proportional_fridge = 0.5;
        private const double tool_grid_height_proportional = 0.05;
        private const double storage_height_proportional = 0.3;
        private const int main_font_size = 15;
        const string expIndicatorString = "Expiration Date";
        const string alphaIndicatorString = "Alphabetical";

        private AbsoluteLayout viewOverlay;
        private IStorage storage;
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

            var titleGrid = new TopPage(name, extraReturnAction: () =>
           {
               foreach (var cell in storage.GetGridCells())
               {
                   cell.GetButton().RemoveEffect(typeof(ImageTint));
               }
           }).GetGrid();
            titleGrid.HeightRequest = ContentManager.screenHeight * TopPage.top_bar_height_proportional;

            var backgroundImage = storageSelection == ContentManager.StorageSelection.fridge ? ContentManager.fridgeIcon : ContentManager.cabinetCellIcon;
            Image backgroundCell = new Image()
            { Source = backgroundImage, Aspect = Aspect.Fill, WidthRequest = ContentManager.screenWidth - (layout_margin * 2)};
            ImageButton backButton = new ImageButton() { Source = ContentManager.backButton, BackgroundColor = Color.Transparent, WidthRequest = 100, HeightRequest = 100 };
            backButton.Clicked += (obj, args) =>
            {
                viewOverlay.IsVisible = false;
            };

            // searching and sorting 
            var sortSelectorIcon = new ImageButton()
            {
                Source = ContentManager.sortIcon,
                BackgroundColor = Color.Transparent
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
            var toolGrid = new Grid()
            {
                Margin = new Thickness(layout_margin, 0),
                RowDefinitions =
                {
                    new RowDefinition(){Height = ContentManager.screenHeight *tool_grid_height_proportional }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = new GridLength(5, GridUnitType.Star)},  new ColumnDefinition(){Width = new GridLength(1, GridUnitType.Star)}
                }
            };
            toolGrid.Children.Add(searchBar, 0, 0);
            toolGrid.Children.Add(sortSelectorIcon, 1, 0);
            toolGrid.Children.Add(sortSelector, 1, 0);

            viewOverlay = new AbsoluteLayout()
            {
                IsVisible = false,
                BackgroundColor = ContentManager.ThemeColor,
                WidthRequest = ContentManager.screenWidth - (layout_margin * 2),
                HeightRequest = ContentManager.screenHeight * 0.4,
                Margin = new Thickness(layout_margin, 0, layout_margin, layout_margin),
                Children =
                {
                    backgroundCell
                }
            };
            ContentManager.AddOnBackgroundChangeListener(c => viewOverlay.BackgroundColor = c);

            ScrollView gridContainer = new ScrollView() { WidthRequest = ContentManager.screenWidth };
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

            // storage model
            var storageView = ContentManager.GetStorageView(storageSelection, name);
            storageView.HeightRequest = ContentManager.screenHeight * storage_height_proportional;
            storage = ContentManager.GetSelectedStorage(storageSelection, name);
            var storageViewWidth = storageSelection == ContentManager.StorageSelection.cabinet ? storage_width_proportional_cabinet : storage_width_proportional_fridge;

            // expiration info grid
            var totalItemIcon = new ImageButton() { Source = ContentManager.allItemIcon, BackgroundColor = Color.Transparent };
            var totalItemLabel = new Label() { TextColor = Color.Black, FontFamily = "oswald_regular", VerticalTextAlignment = TextAlignment.Center, FontSize = main_font_size };
            var expiredIcon = new ImageButton() { Source = ContentManager.expWarningIcon, BackgroundColor = Color.Transparent };
            var expiredAmountLabel = new Label() { TextColor = Color.Black, FontFamily = "oswald_regular", VerticalTextAlignment = TextAlignment.Center, FontSize = main_font_size };
            var almostExpiredIcon = new ImageButton() { Source = ContentManager.expWarningIcon, BackgroundColor = Color.Transparent };
            var almostExpiredAmountLabel = new Label() { TextColor = Color.Black, FontFamily = "oswald_regular", VerticalTextAlignment = TextAlignment.Center, FontSize = main_font_size };
            var expInfoGrid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition(),
                    new RowDefinition(),
                    new RowDefinition()
                },
                ColumnDefinitions = {
                    new ColumnDefinition(){Width = new GridLength(1, GridUnitType.Star) }, new ColumnDefinition(){Width = new GridLength(2, GridUnitType.Star)}
                }
            };
            GridManager.AddGridItem(expInfoGrid, new List<View>() { totalItemIcon, totalItemLabel, expiredIcon, expiredAmountLabel, almostExpiredIcon, almostExpiredAmountLabel }, true);

            var storageViewAndExpGrid = GridManager.InitializeGrid(1, 2, GridLength.Star, GridLength.Star);
            storageViewAndExpGrid.HeightRequest = ContentManager.screenHeight * storage_height_proportional;
            GridManager.AddGridItem(storageViewAndExpGrid, new List<View>() { storageView, expInfoGrid }, true);

            // sets the text info for all item amount, expired item amount, and almost expired item amount
            void calculateExpirationAmount(Grid itemLayoutGrid)
            {
                int expiredItemCount = 0;
                int almostExpiredItemCount = 0;
                foreach (ItemLayout item in itemLayoutGrid.Children)
                {
                    if (item.ItemData.daysUntilExp == 0)
                    {
                        expiredItemCount++;
                    }
                    else if (item.ItemData.daysUntilExp <= 7 && item.ItemData.daysUntilExp > 0)
                    {
                        almostExpiredItemCount++;
                    }
                }
                totalItemLabel.Text = "Items: " + itemLayoutGrid.Children.Count;
                expiredAmountLabel.Text = "Expired: " + expiredItemCount;
                almostExpiredAmountLabel.Text = "Almost Expired: " + almostExpiredItemCount;
            }
            foreach (var cell in storage.GetGridCells())
            {
                // Set up listener to show overlay
                ImageButton button = cell.GetButton();
                var grid = cell.GetItemGrid();
                currentGrid = grid;
                grid.ChildRemoved += (o, a) => { calculateExpirationAmount(grid); };
                grid.WidthRequest = WidthRequest = ContentManager.screenWidth - (layout_margin * 2);
                grid.Margin = new Thickness(layout_margin,0 );

                button.Clicked += async (obj, args) =>
                {
                    viewOverlay.IsVisible = true;
                    gridContainer.Content = grid;
                    var viewOverlayXOffset = ContentManager.screenWidth * 0.75;
                    await viewOverlay.LinearInterpolator(viewOverlayXOffset, 200, t => viewOverlay.TranslationX = viewOverlayXOffset - t);
                    calculateExpirationAmount(grid);
                    grid.IsVisible = true;
                };

                foreach (var child in grid.Children)
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
                var cell = storage.GetGridCell(directSelectIndex);
                var grid = cell.GetItemGrid();
                gridContainer.Content = grid;
                grid.IsVisible = true;
                cell.GetButton().AddEffect(new ImageTint() { tint = ContentManager.button_tint_color });
            }

            Content = new StackLayout()
            {
                HeightRequest = ContentManager.screenHeight,
                WidthRequest = ContentManager.screenWidth,
                Children = {
                            titleGrid,
                            storageViewAndExpGrid,
                            toolGrid,
                            viewOverlay
                }
            };

        }
    }
}
