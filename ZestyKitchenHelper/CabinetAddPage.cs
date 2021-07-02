using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xamarin.Forms;
using Utility;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ZestyKitchenHelper
{
    public class CabinetAddPage : ContentPage
    {
        private const int storage_margin = 10;
        private const double animation_offestX = 1000;

        Converter<View, ItemLayout> itemLayoutCopier;
        AbsoluteLayout pageContainer;
        AddView addView;
        static View storageView;
        private static string storageName;
        const int unplacedGridRows = 2;
        const int unplacedGridColumns = 4;
        private static int gridFootIndex;
        private static Grid unplacedGrid, partialUnplacedGrid;

        // DirectGridFootIndex: when page loads, partial grids directly goes to the set of items with the first item being this index.
        public CabinetAddPage(string _storageName, int directGridFootIndex = 0)
        {
            storageName = _storageName;

            storageView = ContentManager.GetStorageView(storageName);
            storageView.WidthRequest = ContentManager.screenWidth;
            storageView.HeightRequest = ContentManager.screenHeight * .75;
            storageView.VerticalOptions = LayoutOptions.EndAndExpand;
            storageView.Margin = new Thickness(storage_margin);
            animateStorage();

            async void animateStorage()
            {
                await storageView.LinearInterpolator(animation_offestX, 500, (d) => { storageView.TranslationY = animation_offestX - d; });
            }

            //-- set up unplaced grid
            unplacedGrid = GridManager.GetGrid(ContentManager.unplacedGridName);
            partialUnplacedGrid = GridManager.InitializeGrid(ContentManager.pUnplacedGridName, 2, 4, GridLength.Star, GridLength.Star);
            // add listener to set TouchEffect for each new item added. If grid already exist
            partialUnplacedGrid.ChildAdded += (o, v) =>
            {
                EffectManager.UpdateScreenTouchBounds(v.Element as ItemLayout, storageName, UpdateShelf);
            };
            // Add listener to remove partial unplaced grid child
            unplacedGrid.ChildRemoved += (o, a) =>
            {
                foreach (ItemLayout child in partialUnplacedGrid.Children)
                {
                    if ((a.Element as ItemLayout).ItemData.ID == child.ItemData.ID)
                    {
                        partialUnplacedGrid = GridManager.ConstrainGrid(unplacedGrid, gridFootIndex, gridFootIndex + 7, partialUnplacedGrid, itemLayoutCopier, true);
                        break;
                    }
                }
            };
            Console.WriteLine("CabinetAddPage 42 Unplaced children length " + unplacedGrid.Children.Count);
            // initialize grid by constraining UnplacedGrid and Converter
            itemLayoutCopier = (v) =>
            {
                return new ItemLayout(50, 50, (v as ItemLayout).ItemData)
                                .AddMainImage()
                                .AddAmountMark()
                                .AddExpirationMark()
                                .AddTitle()
                                .AddInfoIcon();
            };
            partialUnplacedGrid = GridManager.ConstrainGrid(GridManager.GetGrid(ContentManager.unplacedGridName), directGridFootIndex, directGridFootIndex + 7, partialUnplacedGrid, itemLayoutCopier, true);
            addView = new AddView(LocalStorageController.AddItem, FireBaseController.SaveItem, storageName, true, partialUnplacedGrid);

            // title grid
            var titleGrid = new TopPage(_storageName, () =>
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


            gridFootIndex = directGridFootIndex;
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
                Console.WriteLine("CabinetAddPage grid foot index " + gridFootIndex);
                gridFootIndex += 7;
                partialUnplacedGrid = GridManager.ConstrainGrid(unplacedGrid, gridFootIndex, gridFootIndex + 7, partialUnplacedGrid, itemLayoutCopier, true);
            };
            var lastPage = new ImageButton() { Source = ContentManager.countIcon, Rotation = 180 };
            lastPage.Clicked += (obj, args) =>
            {
                Console.WriteLine("CabinetAddPage grid foot index " + gridFootIndex);
                gridFootIndex = gridFootIndex - 7 < 0 ? 0 : gridFootIndex - 7;
                partialUnplacedGrid = GridManager.ConstrainGrid(unplacedGrid, gridFootIndex, gridFootIndex + 7, partialUnplacedGrid, itemLayoutCopier, true);
            };
            var addNewButton = new ImageButton() { Source = ContentManager.addIcon, BackgroundColor = Color.Transparent };
            addNewButton.Clicked += (obj, args) => { addView.ResetForm(); ContentManager.pageController.ToAddView(addView); };
            var searchBar = new SearchBar() { Text = ContentManager.defaultSearchAllBarText, MinimumWidthRequest = 300 };
            searchBar.Focused += (obj, args) => searchBar.Text = "";
            searchBar.Unfocused += (obj, args) =>
            {
                if (searchBar.Text.Length == 0)
                    searchBar.Text = ContentManager.defaultSearchAllBarText;
                GridManager.FilterItemGrid(ContentManager.UnplacedItemBase.Values, partialUnplacedGrid, searchBar.Text);
            };

            gridPageSelectGrid.Children.Add(lastPage, 1, 0);
            gridPageSelectGrid.Children.Add(nextPage, 2, 0);
            gridPageSelectGrid.Children.Add(addNewButton, 0, 0);
            gridPageSelectGrid.Children.Add(searchBar, 3, 0);

            pageContainer = new AbsoluteLayout();
            pageContainer.BackgroundColor = Color.Wheat;
            pageContainer.Children.Add(titleGrid, new Rectangle(0, 0, 1, TopPage.top_bar_height_proportional), AbsoluteLayoutFlags.All);
            pageContainer.Children.Add(storageView, new Rectangle(1, 1, 1, .5), AbsoluteLayoutFlags.All);
            pageContainer.Children.Add(gridPageSelectGrid, new Rectangle(0, .12, 1, .1), AbsoluteLayoutFlags.All);
            pageContainer.Children.Add(partialUnplacedGrid, new Rectangle(0, .275, 1, .3), AbsoluteLayoutFlags.All);

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
            itemLayout.iconImage.RemoveEffect(typeof(ScreenTouch));
            itemLayout.IsVisible = false;

            Console.WriteLine("Cabinet Add 165 cell index " + cellIndex);
            metaItemLayout.ItemData.SetStorage(name, cellIndex, ContentManager.GetStorageType());
            itemLayout.ItemData.SetStorage(name, cellIndex, ContentManager.GetStorageType());

            ContentManager.UnplacedItemBase.Remove(itemLayout.ItemData.ID);
            GridManager.RemoveGridItem(ContentManager.unplacedGridName, unplacedItemLayout);

            // Weird fact: the animation actually allows the touchEffect cycle to complete without complaining that the item is disposed.
            var storage = ContentManager.GetSelectedStorage(name);
            var cellBackground = storage.GetGridCell(cellIndex).GetBackground();
            await ViewExtensions.QuadraticInterpolator(cellBackground, .5, 250, d => cellBackground.Scale = d, null);

            GridManager.RemoveGridItem(partialUnplacedGrid, itemLayout);

            storage.AddGridItems(cellIndex, new List<View>() { itemLayout });

            LocalStorageController.UpdateItem(itemLayout.ItemData);
            FireBaseController.SaveItem(itemLayout.ItemData);
        }
    }
}
