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
        private Grid currentGrid;
        private AbsoluteLayout viewOverlay;
        private string storageName;
        private Dictionary<int, Grid> expandedViews = new Dictionary<int, Grid>();
        Action<Item> deleteItemLocalEvent, deleteItemBaseEvent, updateItemLocalEvent, updateItemBaseEvent;
        Action<string, string, string> saveStorageLocalEvent, saveStorageBaseEvent;
        public CabinetViewPage(string name, Action<Item> deleteItemLocal, Action<Item> deleteItemBase, Action<Item> updateItemLocal, Action<Item> updateItemBase,
            Action<string, string, string> _saveStorageLocalEvent, Action<string, string, string> _saveStorageBaseEvent)
        {
            saveStorageBaseEvent = _saveStorageBaseEvent;
            saveStorageLocalEvent = _saveStorageLocalEvent;
            updateItemLocalEvent = updateItemLocal;
            updateItemBaseEvent = updateItemBase;
            deleteItemBaseEvent = deleteItemBase;
            deleteItemLocalEvent = deleteItemLocal;
            var backgroundImage = ContentManager.storageSelection == ContentManager.StorageSelection.fridge ? ContentManager.fridgeIcon : ContentManager.cabinetIcon;
            storageName = name;
            Image backgroundCell = new Image()
            { Source = backgroundImage, Aspect = Aspect.Fill };
            ImageButton backButton = new ImageButton() { Source = ContentManager.backButton, WidthRequest = 100, HeightRequest = 100 };
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
                    case expIndicatorString: GridOrganizer.SortItemGrid(currentGrid, GridOrganizer.SortingType.Expiration_Close); break;
                    case alphaIndicatorString: GridOrganizer.SortItemGrid(currentGrid, GridOrganizer.SortingType.A_Z); break;
                }

            };
            var lastPageButton = new ImageButton() { Source = ContentManager.countIcon, Rotation = 180, Aspect = Aspect.Fill, WidthRequest = 50 };
            lastPageButton.Clicked += (obj, args) =>
            {
                var index = currentGrid.GetGridChilrenList().IndexOf(currentGrid.Children[0]);
                if (index > max_grid_count) NextPresetPage(index - max_grid_count);
                else NextPresetPage(0);
            };
            var nextPageButton = new ImageButton() { Source = ContentManager.countIcon, Aspect = Aspect.Fill, WidthRequest = 50};
            nextPageButton.Clicked += (obj, args) =>
            {
                Console.WriteLine("children count " + currentGrid.GetGridChilrenList().IndexOf(currentGrid.Children[currentGrid.Children.Count - 1]));
                var index = currentGrid.GetGridChilrenList().IndexOf(currentGrid.Children[currentGrid.Children.Count - 1]);
                if (index < currentGrid.GetGridChilrenList().Count - 1) NextPresetPage(index + 1);
            };
            Grid toolGrid = new Grid()
            {
                ColumnSpacing = 20,
                RowDefinitions =
                {
                    new RowDefinition(){Height = 50}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = 40 },
                    new ColumnDefinition(){Width = 40 },
                    new ColumnDefinition()
                }
            };
            toolGrid.Children.Add(lastPageButton, 0, 0);
            toolGrid.Children.Add(nextPageButton, 1, 0);
            toolGrid.Children.Add(sortSelector, 2, 0);

            AbsoluteLayout.SetLayoutBounds(toolGrid, new Rectangle(150, 40, .6, 100));
            AbsoluteLayout.SetLayoutFlags(toolGrid, AbsoluteLayoutFlags.WidthProportional);
            AbsoluteLayout.SetLayoutBounds(backgroundCell, new Rectangle(0, 100, 1, Application.Current.MainPage.Height - 100));
            AbsoluteLayout.SetLayoutFlags(backgroundCell, AbsoluteLayoutFlags.WidthProportional);
            viewOverlay = new AbsoluteLayout()
            {
                IsVisible = false,
                BackgroundColor = Color.Wheat,
                WidthRequest = Application.Current.MainPage.Width,
                Children =
                {
                    backgroundCell,
                    backButton,
                    toolGrid
                }
            };
            viewOverlay.ChildAdded += (obj, args) => viewOverlay.ForceLayout();

            var storageLabel = new Label() { Text = name, FontSize = 40, TextColor = Color.Black, HorizontalTextAlignment = TextAlignment.Center };
            var returnButton = new ImageButton() { Source = ContentManager.backButton, WidthRequest = 100, HeightRequest = 100 };
            returnButton.Clicked += (o,a) => ContentManager.pageController.ToSingleSelectionPage();
            var storage = ContentManager.GetStorageView(name);
            storage.HorizontalOptions = LayoutOptions.CenterAndExpand;
            storage.WidthRequest = Application.Current.MainPage.Width * .8;
            storage.HeightRequest = 7 * Application.Current.MainPage.Height / 8;

            var itemBase = ContentManager.GetItemBase();
            foreach (int index in itemBase[name].Keys)
            {
                foreach (var button in itemBase[name][index].Keys)
                {
                    var grid = GetItemGrid(itemBase[name][index][button]);
                    ScrollView gridContainer = new ScrollView() { Content = grid };
                    viewOverlay.Children.Add(grid, AbsoluteLayout.GetLayoutBounds(backgroundCell), AbsoluteLayout.GetLayoutFlags(backgroundCell));
                    button.Clicked += (obj, args) =>
                        {
                            viewOverlay.IsVisible = true;
                            currentGrid = grid;
                            grid.IsVisible = true;
                        };
                }
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
                            returnButton,
                            storageLabel,
                            storage,
                        }
                    },
                    viewOverlay
                }
            };
        }
        private void NextPresetPage(int currentAmount)
        {
            currentGrid.Children.Clear();
            //var currentAmount = presetResult.IndexOf(presetSelectGrid.Children.Last() as IconLayout);
            List<View> results = new List<View>();
            var max = max_grid_count + currentAmount < currentGrid.GetGridChilrenList().Count ? max_grid_count + currentAmount : currentGrid.GetGridChilrenList().Count;
            for (int i = currentAmount; i < max; i++)
            {
                results.Add(currentGrid.GetGridChilrenList()[i] as View);
            }
            currentGrid.OrganizeGrid(results, GridOrganizer.OrganizeMode.HorizontalLeft);
        }

        private Grid GetItemGrid(List<ItemLayout> itemList)
        {
            List<ItemLayout> list = new List<ItemLayout>();
            List<ItemLayout> resultList = new List<ItemLayout>();
            Grid cellItemGrid = new Grid()
            {
                ColumnSpacing = 5,
                IsVisible = false,
                RowDefinitions =
                {
                    new RowDefinition(), new RowDefinition(), new RowDefinition(), new RowDefinition(), new RowDefinition()
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),  new ColumnDefinition(),  new ColumnDefinition(),  new ColumnDefinition()
                }
            };


            foreach (var item in itemList)
            {
                ItemLayout itemInstance = new ItemLayout();
                itemInstance.ItemData = item.ItemData;
                itemInstance.AddMainImage().AddAmountMark().AddExpirationMark().AddTitle();
                list.Add(itemInstance);
                if (list.Count <= max_grid_count) resultList.Add(itemInstance);

                itemInstance.AddDeleteButton();
                itemInstance.deleteButton.IsVisible = false;
                itemInstance.iconImage.Clicked += (obj, args) =>
                {
                    itemInstance.iconImage.ToggleEffects(
                        new ImageTint() { tint = Color.FromRgba(100, 50, 50, 80) }, new List<VisualElement>() { itemInstance.deleteButton });
                    Console.WriteLine("the icon image clicked");
                };

            
                itemInstance.deleteButton.Clicked += (obj, args) =>
                {
                    if (itemInstance.ItemData.amount > 1)
                    {
                        item.SubtractAmount();
                        itemInstance.amountLabel.Text = item.amountLabel.Text;
                        updateItemBaseEvent.Invoke(item.ItemData);
                        updateItemLocalEvent.Invoke(item.ItemData);
                    }
                    else
                    {
                        var index = cellItemGrid.GetGridChilrenList().IndexOf(cellItemGrid.Children[0]);
                        cellItemGrid.Children.Clear();
                        var removedList = cellItemGrid.GetGridChilrenList() as List<ItemLayout>;
                        removedList.Remove(itemInstance);
                        cellItemGrid.SetGridChildrenList(removedList);
                        NextPresetPage(index);
                        ContentManager.GetInfoBase()[storageName][item.ParentCellIndex].Children.Remove(item);
                        ContentManager.GetItemBase()[storageName][item.ParentCellIndex][item.ParentButton].Remove(item);
                        ContentManager.MetaItemBase.Remove(item.ItemData.ID);
                        string itemInfo, rowInfo;
                        ContentManager.SetLocalCabinet(item.StorageName, out rowInfo, out itemInfo);
                        saveStorageLocalEvent?.Invoke(item.StorageName, rowInfo, itemInfo);
                        saveStorageBaseEvent?.Invoke(item.StorageName, rowInfo, itemInfo);
                        deleteItemLocalEvent?.Invoke(item.ItemData); 
                        deleteItemBaseEvent?.Invoke(item.ItemData);
                        item.StorageName = string.Empty;
                    }
                };
            }

            cellItemGrid.SetGridChildrenList(list);
            cellItemGrid.OrganizeGrid(resultList, GridOrganizer.OrganizeMode.HorizontalLeft);
            AbsoluteLayout.SetLayoutBounds(cellItemGrid, new Rectangle(0, 100, 1, Application.Current.MainPage.Height - 100));
            AbsoluteLayout.SetLayoutFlags(cellItemGrid, AbsoluteLayoutFlags.WidthProportional); ;
            return cellItemGrid;
        }
   }
}
