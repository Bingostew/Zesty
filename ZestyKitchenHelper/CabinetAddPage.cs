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
        private const int storage_margin = 10;

        AbsoluteLayout pageContainer;
        AddView addView;
        static View storageView;
        private static string storageName;
        const int unplacedGridRows = 2;
        const int unplacedGridColumns = 4;
        private static int gridFootIndex;
        private static Grid unplacedGrid, partialUnplacedGrid;

        public CabinetAddPage(string _storageName)
        {
            storageName = _storageName;

            //-- set up storage view
            var name = new Label() { Text = _storageName, FontSize = 30, TextColor = Color.Black, Margin = new Thickness(0, storage_margin, 0, 0)};
            storageView = ContentManager.GetStorageView(storageName);
            storageView.WidthRequest = ContentManager.screenWidth;
            storageView.HeightRequest = ContentManager.screenHeight * .75;
            storageView.VerticalOptions = LayoutOptions.EndAndExpand;
            storageView.Margin = new Thickness(storage_margin);

            //-- set up unplaced grid
            unplacedGrid = GridManager.GetGrid(ContentManager.unplacedGridName);
            partialUnplacedGrid = GridManager.InitializeGrid(ContentManager.pUnplacedGridName, 2, 4, GridLength.Star, GridLength.Star);
            // add listener to set TouchEffect for each new item added. If grid already exist
            partialUnplacedGrid.ChildAdded += (o, v) =>
            {
                EffectManager.UpdateScreenTouchBounds(v.Element as ItemLayout, storageName, UpdateShelf);
            };
            Console.WriteLine("CabinetAddPage 42 Unplaced children length " + unplacedGrid.Children.Count);
            // initialize grid by constraining UnplacedGrid
            partialUnplacedGrid = GridManager.ConstrainGrid(GridManager.GetGrid(ContentManager.unplacedGridName), 0, 8, partialUnplacedGrid, (v) =>
            {
                return new ItemLayout(50, 50, (v as ItemLayout).ItemData)
                                .AddMainImage()
                                .AddAmountMark()
                                .AddExpirationMark()
                                .AddTitle()
                                .AddInfoIcon();
            }
            , true);
            addView = new AddView(LocalStorageController.AddItem, FireBaseController.SaveItem, storageName, true, partialUnplacedGrid);

            // title grid
            var titleGrid = new TopPage(() =>
            {
                foreach (ItemLayout child in partialUnplacedGrid.Children)
                {
                    child.iconImage.RemoveEffect(typeof(ScreenTouch));
                }
                foreach (StorageCell child in ContentManager.GetSelectedStorage(storageName).GetGridCells())
                {
                    child.GetButton().RemoveEffect(typeof(ImageTint));
                }
                GridManager.RemoveGrid(ContentManager.pUnplacedGridName);
            }).GetGrid();
          

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
            addNewButton.Clicked += (obj, args) => { ContentManager.pageController.ToAddView(addView); };
            var searchBar = new SearchBar() { Text = ContentManager.defaultSearchAllBarText, MinimumWidthRequest = 300 };
            searchBar.Focused += (obj, args) => searchBar.Text = "";
            searchBar.Unfocused += (obj, args) => { if (searchBar.Text.Length == 0) searchBar.Text = ContentManager.defaultSearchAllBarText; };
            searchBar.TextChanged += (obj, args) => OnSearch(args.NewTextValue);

            void OnSearch(string text)
            {
                partialUnplacedGrid.Children.Clear();
                foreach (ItemLayout item in partialUnplacedGrid.Children)
                {
                    var match = 0;
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (i < item.ItemData.Name.Length && string.Equals(text[i].ToString(), item.ItemData.Name[i].ToString(), StringComparison.OrdinalIgnoreCase))
                        {
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

            pageContainer = new AbsoluteLayout();
            pageContainer.BackgroundColor = Color.Wheat;
            pageContainer.Children.Add(titleGrid, new Rectangle(0, 0, 1, TopPage.top_bar_height_proportional), AbsoluteLayoutFlags.All);
            pageContainer.Children.Add(name, new Rectangle(0, .1, 1, .1), AbsoluteLayoutFlags.All);
            pageContainer.Children.Add(storageView, new Rectangle(1, 1, 1, .7), AbsoluteLayoutFlags.All);
            pageContainer.Children.Add(gridPageSelectGrid, new Rectangle(0, .175, 1, .1), AbsoluteLayoutFlags.All);
            pageContainer.Children.Add(partialUnplacedGrid, new Rectangle(0, .3, 1, .3), AbsoluteLayoutFlags.All);

            Content = new AbsoluteLayout()
            {
                Children =
                {
                    pageContainer
                }
            };
            AbsoluteLayout.SetLayoutBounds(pageContainer, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(pageContainer, AbsoluteLayoutFlags.All);
        }


        private async void UpdateShelf(string name, ItemLayout itemLayout, int cellIndex)
        { 
            // Update copies: Meta Item Base contains copies of items that should be updated
            ItemLayout metaItemLayout = ContentManager.MetaItemBase[itemLayout.ItemData.ID];
            ItemLayout unplacedItemLayout = ContentManager.UnplacedItemBase[itemLayout.ItemData.ID];

            itemLayout.IsVisible = false;

            Console.WriteLine("Cabinet Add 165 cell index " + cellIndex);
            metaItemLayout.ItemData.SetStorage(name, cellIndex, ContentManager.GetStorageType());
            itemLayout.ItemData.SetStorage(name, cellIndex, ContentManager.GetStorageType());

            itemLayout.SetMarkingVisibility(false);
            ContentManager.UnplacedItemBase.Remove(itemLayout.ItemData.ID);
            GridManager.RemoveGridItem(ContentManager.unplacedGridName, unplacedItemLayout);

            // Weird fact: the animation actually allows the touchEffect cycle to complete without complaining that the item is disposed.
            var storage = ContentManager.GetSelectedStorage(name);
            var cellBackground = storage.GetGridCell(cellIndex).GetBackground();
            await ViewExtensions.QuadraticInterpolator(cellBackground, .5, 250, d => cellBackground.Scale = d, null);

            GridManager.RemoveGridItem(partialUnplacedGrid, itemLayout);
            //    GridManager.RemoveGridItem(unplacedGrid, itemLayout);
            //    removeUnplacedItemsEvent.Invoke();

            Console.WriteLine("CabinetAddPage 180 Unplaced children length " + unplacedGrid.Children.Count);

            storage.AddGridItems(cellIndex, new List<View>() { itemLayout });

            LocalStorageController.UpdateItem(itemLayout.ItemData);
            FireBaseController.SaveItem(itemLayout.ItemData);
        }
    }
}
