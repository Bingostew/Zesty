﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using System.Linq;
using Xamarin.Forms.PlatformConfiguration;
using Utility;
using System.Globalization;
using Xamarin.Essentials;
using Firebase.Database;

namespace ZestyKitchenHelper
{
    public class ContentManager
    {
        public static UserProfile sessionUserProfile;
        public static bool isUserNew;

        public const string cabinetCellIcon = "cabinet_cell.png";
        public const string cabinetLeftIcon = "cabinet_divider_left.png";
        public const string cabinetMiddleIcon = "cabinet_divider_middle.png";
        public const string cabinetRightIcon = "cabinet_divider_right.png";
        public const string safeIcon = "swan.JPG";
        public const string addIcon = "add_new_content.png";
        public const string addItemIcon = "add_new_item.png";
        public const string subdivideIcon = "swan.JPG";
        public const string transIcon = "transparent.png";
        public const string fridgeIcon = "fridge_cell.png";
        public const string fridgeSideIcon = "fridge_side_cell.png";
        public const string fridgeDividerIcon = "fridge_cell_divider.png";
        public const string deleteCellIcon = "delete_cell_button.png";
        public const string backButton = "back_arrow.png";
        public const string pantryIcon = "pantry.png";
        public const string refridgeIcon = "fridge.png";
        public const string countIcon = "small_arrow.png";

        public const string defaultSearchAllBarText = "Search item...";
        
        public const string cabinetStorageType = "Cabinet";
        public const string fridgeStorageType = "Fridge";
        public const string metaGridName = "Meta Grid";
        public const string unplacedGridName = "Unplaced Grid";
        public const string pUnplacedGridName = "Partial Unplaced Grid";

        public const string itemStorageIdGenerator = "ItemId";
        public const string storageCellIdGenerator = "StorageCell";
        public const string cabinetEditIdGenerator = "Cabinet";
        public const string fridgeEditIdGenerator = "Fridge";

        public static PageController pageController = new PageController();
        public static SelectionPage selectionPage;

        public static Dictionary<string, Cabinet> CabinetMetaBase = new Dictionary<string, Cabinet>();
        public static Dictionary<string, Fridge> FridgeMetaBase = new Dictionary<string, Fridge>();
        public static async void InitializeApp()
        {


            // Initialize ID Groups
            IDGenerator.InitializeIDGroup(itemStorageIdGenerator);
            IDGenerator.InitializeIDGroup(cabinetEditIdGenerator);
            IDGenerator.InitializeIDGroup(fridgeEditIdGenerator);
            IDGenerator.InitializeIDGroup(storageCellIdGenerator);
            LocalStorageController.ResetDatabase(); // WARNING: FOR TESTING PURPOSES ONLY
            LocalStorageController.InitializeLocalDataBase();

            // Initialize Important Grids
            GridManager.InitializeGrid(metaGridName, 9, 4, GridLength.Auto, GridLength.Star);
            GridManager.InitializeGrid(unplacedGridName, 0, 4, GridLength.Star, GridLength.Star);


            // Load saved data
            if (!isUserNew)
            {
                List<Cabinet> localCabinets = await LocalStorageController.GetTableListAsync<Cabinet>();
                List<Fridge> localFridges = await LocalStorageController.GetTableListAsync<Fridge>();
                List<StorageCell> localStorageCells = await LocalStorageController.GetTableListAsync<StorageCell>();
                List<Item> localItems = await LocalStorageController.GetTableListAsync<Item>();
                List<Cabinet> baseCabinets = new List<Cabinet>();
                List<Fridge> baseFridges = new List<Fridge>();
                List<StorageCell> baseStorageCells = new List<StorageCell>();
                List<Item> baseItems = new List<Item>();

                if (sessionUserProfile != null)
                {
                    // Populating list with firebase data 
                    baseCabinets = (await FireBaseController.GetCabinets()).ToList().ConvertAll(o => o.Object);
                    baseFridges = (await FireBaseController.GetFridges()).ToList().ConvertAll(o => o.Object);
                    baseItems = (await FireBaseController.GetItems()).ToList().ConvertAll(o => o.Object);
                    baseStorageCells = (await FireBaseController.GetStorageCells()).ToList().ConvertAll(o => o.Object);

                    // Load with cloud data
                    ContentLoader.LoadItems(baseItems);
                    ContentLoader.LoadCabinets(baseCabinets, baseStorageCells, baseItems);
                    ContentLoader.LoadFridges(baseFridges, baseStorageCells, baseItems);
                }
                else
                {
                    // Load with local data
                    ContentLoader.LoadItems(localItems);
                    ContentLoader.LoadCabinets(localCabinets, localStorageCells, localItems);
                    ContentLoader.LoadFridges(localFridges, localStorageCells, localItems);
                }

                if (sessionUserProfile != null)
                {
                    // Updating local data with firebase data
                    // -----Logic: In a firebase list, select all item that does not exist in the local list. 
                    // -----Hence check for ID similarities in both list, if the given item ID in firebase does not match any item ID in local list, add to listDiff
                    List<Item> itemListDiff = baseItems.Where(i => !localItems.Any(j => i.ID == j.ID)).ToList();
                    List<Cabinet> cabinetListDiff = baseCabinets.Where(i => !localCabinets.Any(j => i.ID == j.ID)).ToList();
                    List<Fridge> fridgeListDiff = baseFridges.Where(i => !localFridges.Any(j => i.ID == j.ID)).ToList();
                    // List<StorageCell> storageCellListDiff = baseStorageCells.Where(i => !localStorageCells.Any(j => i.MetaID == j.MetaID)).ToList();

                    //actually updating local list.
                    cabinetListDiff.ForEach(c => LocalStorageController.AddCabinet(c.Name));
                    fridgeListDiff.ForEach(f => LocalStorageController.AddFridge(f.Name));
                    itemListDiff.ForEach(i => LocalStorageController.AddItem(i));

                    baseItems.ForEach(i => LocalStorageController.UpdateItem(i));
                }
            }
        }
        /// <summary>
        /// Returns the string that represents the storage the current user is in.
        /// </summary>
        /// <returns></returns>
        public static string GetStorageType()
        {
            return storageSelection == StorageSelection.cabinet ? cabinetStorageType : fridgeStorageType;
        }
        /// <summary>
        /// Calls GetFridgeView or GetCabinetView depending on whether the user entered the cabinet or fridge selection page.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Layout<View> GetStorageView(string name)
        {
            if (storageSelection == StorageSelection.cabinet)
                return GetCabinetView(name);
            else
                return GetFridgeView(name);
        }
        /// <summary>
        /// Retrieves the grids that make up the fridge's layout
        /// </summary>
        /// <param name="name">name of fridge.</param>
        /// <returns></returns>
        public static Layout<View> GetFridgeView(string name)
        {
            Grid gridContainer = new Grid()
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = GridLength.Star },
                    new ColumnDefinition(){Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition(){Width = GridLength.Star }
                }
            };
            Fridge fridge = FridgeMetaBase[name];

            gridContainer.Children.Add(fridge.LeftGrid, 0, 0);
            gridContainer.Children.Add(fridge.MainGrid, 1, 0);
            gridContainer.Children.Add(fridge.RightGrid, 2, 0);

            return gridContainer;
        }
        /// <summary>
        /// Retrieves the grid that makes up the cabinet layout.
        /// </summary>
        /// <param name="name">name of cabinet.</param>
        /// <returns></returns>
        public static Layout<View> GetCabinetView(string name)
        {
            return CabinetMetaBase[name].MainGrid;
        }
        /// <summary>
        /// Retreives the IStorage storage by name, depending on whether user selected to enter the cabinet or fridge selection pages.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IStorage GetSelectedStorage(string name)
        {
            if(storageSelection == StorageSelection.cabinet && CabinetMetaBase.ContainsKey(name))
            {
                return CabinetMetaBase[name];
            }
            else if (storageSelection == StorageSelection.fridge && FridgeMetaBase.ContainsKey(name))
            {
                return FridgeMetaBase[name];
            }

            return null;
        }

        public static void AddSelectedStorage(string name, IStorage storage)
        {
            if (storageSelection == StorageSelection.cabinet)
            {
                CabinetMetaBase.Add(name, (Cabinet)storage);
            }
            else
            {
                FridgeMetaBase.Add(name, (Fridge)storage);
            }
        }
        /*
        public static void SetLocalCabinet(string name, out string rowInfo, out string itemInfo)
        {
            var itemContent = cabinetItemBase[name];
            rowInfo = "";
            itemInfo = "";

            var rowList = cabinetInfo[name].Keys.ToList();
            for (int i = 0; i < rowList.Count; i++)
            {
                var index = rowList[i];
                var buttonList = itemContent[index].Keys.ToArray();
                var buttonCount = itemContent[index].Values.Count;
                rowInfo += "(";
                itemInfo += i.ToString() + "(";
                for (int j = 0; j < buttonCount; j++)
                {
                    rowInfo += Math.Round(buttonList[j].GetAbsolutePosition().X / (buttonList[j].Parent as View).Width, 2);
                    if (j != buttonCount - 1) rowInfo += "+";
                    for (int k = 0; k < itemContent[index][buttonList[j]].Count; k++)
                    {
                        itemInfo += itemContent[index][buttonList[j]][k].ItemData.ID + "+";
                    }
                }
                 itemInfo += ")";
                rowInfo += ")";
                if (i != rowList.Count - 1) { rowInfo += ","; itemInfo += ","; }
            }
        }
        public static void SetLocalFridge(string name, out string rowInfo, out string itemInfo)
        {
            var itemContent = fridgeItemBase[name];
            rowInfo = "";
            itemInfo = "";

            var rowList = fridgeInfo[name].Keys.ToList();
            rowInfo += "(" + fridgeInfo[name][0].Children.Where(e => e.GetType() == typeof(ImageButton)).Count() + "),";
            itemInfo += "(";
            foreach (var side in fridgeInfo[name][0].Children)
            {
                if (side.GetType() == typeof(ItemLayout))
                    itemInfo += (side as ItemLayout).ItemData.ID + "+";
            }
            itemInfo += "),";
            rowInfo += "(" + fridgeInfo[name][1].Children.Where(e => e.GetType() == typeof(ImageButton)).Count() + "),";
            itemInfo += "(";
            foreach (var side in fridgeInfo[name][1].Children)
            {
                if (side.GetType() == typeof(ItemLayout))
                    itemInfo += (side as ItemLayout).ItemData.ID + "+";
            }
            itemInfo += "),";
            for (int i = 2; i < rowList.Count; i++)
            {
                var index = rowList[i];
                var buttonList = itemContent[index].Keys.ToArray();
                var buttonCount = itemContent[index].Values.Count;
                rowInfo += "(";
                itemInfo += i.ToString() + "(";
                for (int j = 0; j < buttonCount; j++)
                {
                    rowInfo += Math.Round((buttonList[j].GetAbsolutePosition().X - buttonList[j].Width / 2) / (buttonList[j].Parent as View).Width, 2);
                    if (j != buttonCount - 1) rowInfo += "+";
                    for (int k = 0; k < itemContent[index][buttonList[j]].Count; k++)
                    {
                        itemInfo += itemContent[index][buttonList[j]][k].ItemData.ID + "+";
                    }
                }
                itemInfo += ")";
                rowInfo += ")";
                if (i != rowList.Count - 1) { rowInfo += ","; itemInfo += ","; }
            }
        }

        public static void ParseLocalItems(List<Item> itemList)
        {
            foreach (var item in itemList)
            {
                if (!MetaItemBase.ContainsKey(item.ID))
                {
                    ItemLayout itemLayout = new ItemLayout(50, 50, item)
                                .AddMainImage()
                                .AddAmountMark()
                                .AddExpirationMark()
                                .AddTitle()
                                .AddInfoIcon();
                    MetaItemBase.Add(item.ID, itemLayout);
                    if (!item.stored) UnplacedItemBase.Add(item.ID, itemLayout);
                }
            }
        }

        public static void ParseLocalCabinets(List<Cabinet> cabinetList, List<Item> itemList)
        {
            foreach (var cabinet in cabinetList)
            {
               if (cabinet == null || cabinet.Name == null || cabinetInfo.ContainsKey(cabinet.Name)) { break; }
                AbsoluteLayout cabinetLayout = new AbsoluteLayout() { HeightRequest = 400 };
                cabinetLayout.WidthRequest = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                var cellIndexer = 0;
                var name = cabinet.Name;
                string[] cabinetRows = cabinet.RowInfo.Split(',');
                string[] itemString = cabinet.RowItems.Split(',');
                cabinetInfo.Add(name, new Dictionary<int, AbsoluteLayout>());
                cabinetItemBase.Add(name, new Dictionary<int, Dictionary<ImageButton, List<ItemLayout>>>());
                
                foreach (var row in cabinetRows)
                {

                    string[] rowParsable = row.TrimStart('(').TrimEnd(')').Split('+');
                    var background = new Image() { Source = cabinetIcon, Aspect = Aspect.Fill };
                    int amount = rowParsable.Length;

                    cabinetItemBase[name].Add(cellIndexer, new Dictionary<ImageButton, List<ItemLayout>>());
                    AbsoluteLayout rowLayout = new AbsoluteLayout() { HeightRequest = 50 };
                    cabinetLayout.Children.Insert(0, rowLayout);
                    if (cabinetLayout.Children.Count > 0)
                    {
                        AbsoluteLayout.SetLayoutBounds(rowLayout, new Rectangle(0, CabinetEditPage.cabinet_height / cabinetLayout.HeightRequest * cabinetLayout.Children.Count,
                            1, CabinetEditPage.cabinet_height / cabinetLayout.HeightRequest));
                    }
                    else
                    {
                        AbsoluteLayout.SetLayoutBounds(rowLayout, new Rectangle(0, 0, 1, CabinetEditPage.cabinet_height / cabinetLayout.HeightRequest));
                    }
                    AbsoluteLayout.SetLayoutFlags(rowLayout, AbsoluteLayoutFlags.All);
                    cabinetInfo[name].Add(cellIndexer, rowLayout);
                    rowLayout.Children.Add(background, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                    for (int i = 0; i < amount; i++)
                    {
                        double offset = double.Parse(rowParsable[i], CultureInfo.InvariantCulture); 
                        var cellButton = new ImageButton() { Source = transIcon, Aspect = Aspect.Fill };
                        var button = new ImageButton() { Source = transIcon, Aspect = Aspect.Fill };
                        button.AnchorX = .5;
                        Image divider = new Image() { Aspect = Aspect.Fill };
                        var lastButtonWidth = 1 - double.Parse(rowParsable[rowParsable.Length - 1]);
                        var buttonWidth = i < rowParsable.Length - 1 ? double.Parse(rowParsable[i + 1]) - double.Parse(rowParsable[i]) : i == rowParsable.Length - 1 ? lastButtonWidth: 0;
                        double buttonX = i == 0 ? 0 : i == rowParsable.Length - 1 ? 1 : offset / (1 - buttonWidth);
                        AbsoluteLayout.SetLayoutBounds(button, new Rectangle(buttonX, 0, buttonWidth, 1));
                        AbsoluteLayout.SetLayoutFlags(button, AbsoluteLayoutFlags.All);
                        rowLayout.Children.Add(button);
                        cabinetItemBase[name][cellIndexer].Add(button, new List<ItemLayout>());

                        if (i != 0)
                        {
                            rowLayout.Children.Add(divider);
                            var dividerX = button.GetAbsolutePosition().X;
                            if (amount % 2 == 0)
                            {
                                divider.Source = offset < .5 ? cabinetLeftIcon : offset == .5 ? cabinetMiddleIcon : cabinetRightIcon;
                            }
                            else
                            {
                                divider.Source = offset < .5 ? cabinetLeftIcon : cabinetRightIcon;
                            }

                            if (divider.Source.ToString() != cabinetMiddleIcon)
                            {
                                divider.ScaleX = Math.Abs((.5 - offset) / .5);
                                if (divider.ScaleX < 0.2) { divider.ScaleX = 0.2; }
                            }
                            else { divider.ScaleX = 0.3; }
                            AbsoluteLayout.SetLayoutBounds(divider, new Rectangle(offset, 0, .05, 1));
                            AbsoluteLayout.SetLayoutFlags(divider, AbsoluteLayoutFlags.All);
                        }
                        button.Clicked += (o, a) =>
                        {
                            foreach (Layout<View> element in cabinetLayout.Children)
                            {
                                 element.Children.RemoveEffects(typeof(ImageTint));
                            }
                            button.ToggleEffects((new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }), null);
                        };

                        var itemParsable = "";
                        if (itemString != null) itemParsable = itemString[cellIndexer].Length > 2 ? itemString[cellIndexer].TrimEnd(')').Substring(2) : "";
                        var itemIterator = 0;
                        
                        if (itemParsable.Length > 2)
                            foreach (var itemID in itemParsable.Split('+'))
                            {
                                if (itemID.Length > 0)
                                {
                                    int id = int.Parse(itemID);
                                    var item = itemList.Where(t => t.ID == id).FirstOrDefault();
                                    if (item != null)
                                    {
                                        ItemLayout itemLayout = new ItemLayout(50, 50, item)
                                        .AddMainImage()
                                        .AddAmountMark()
                                        .AddExpirationMark()
                                        .AddTitle();
                                        itemLayout.BindCabinetInfo(button.GetAbsolutePosition().X, cellIndexer, button, name, GetCabinetView);
                                        itemLayout.StorageName = name;
                                        itemLayout.RecalculateDate();
                                        itemLayout.AddInfoIcon();
                                        cabinetItemBase[name][cellIndexer][button].Add(itemLayout);
                                        UnplacedItemBase.Remove(item.ID);
                                        itemIterator++;
                                    }
                                }
                            }
                    }
                    cellIndexer++;
                }
            }
        }
        public static void ParseLocalFridge(List<Fridge> fridgeList, List<Item> itemList)
        {
             foreach (var fridge in fridgeList)
            {
                if (fridge == null || fridge.Name == null || fridgeInfo.ContainsKey(fridge.Name)) { break; }
                AbsoluteLayout fridgeLayout = new AbsoluteLayout() { HeightRequest = 400 };
                fridgeLayout.WidthRequest = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                var cellIndexer = 0;
                var name = fridge.Name;
                string[] fridgeRows = fridge.RowInfo.Split(',');
                string[] itemString = fridge.RowItems.Split(',');
                fridgeInfo.Add(name, new Dictionary<int, AbsoluteLayout>());
                fridgeItemBase.Add(name, new Dictionary<int, Dictionary<ImageButton, List<ItemLayout>>>());
                fridgeLayout.HeightRequest = 400;
                var height = (fridgeRows.Length - 2) * FridgeEditPage.fridge_height / fridgeLayout.HeightRequest;
                var leftRowLayout = new AbsoluteLayout() { HeightRequest = height };
                var rightRowLayout = new AbsoluteLayout() { HeightRequest = height };
                fridgeLayout.Children.Add(leftRowLayout, new Rectangle(0, 0, 1 / FridgeEditPage.Side_Cell_Width_Div, height), AbsoluteLayoutFlags.All);
                fridgeLayout.Children.Add(rightRowLayout, new Rectangle(1, 0, 1 / FridgeEditPage.Side_Cell_Width_Div, height), AbsoluteLayoutFlags.All);
                setSideShelf(true); setSideShelf(false);
                void setSideShelf(bool left)
                {
                    var container = leftRowLayout;
                    var tag =  FridgeEditPage.Left_Cell_Tag;
                    var count = double.Parse(fridgeRows[0].TrimStart('(').TrimEnd(')'));
                    if (!left)
                    {
                        container = rightRowLayout; 
                        tag = FridgeEditPage.Right_Cell_Tag;
                        count = double.Parse(fridgeRows[1].TrimStart('(').TrimEnd(')'));
                    }
                    var parentHeight = container.HeightRequest;
                    var parentWidth = container.Width;
                    fridgeInfo[name].Add(cellIndexer, container);

                    List<ImageButton> buttonList = new List<ImageButton>();
                    fridgeItemBase[name].Add(cellIndexer, new Dictionary<ImageButton, List<ItemLayout>>());
                    for (int j = 0; j < count; j++)
                    {
                        var cell = new Image() { Source = fridgeSideIcon, Aspect = Aspect.Fill };
                        var button = new ImageButton() { Source = transIcon, Aspect = Aspect.Fill };
                        buttonList.Add(button);
                        button.Clicked += (obj, args) =>
                        {
                            foreach (View element in container.Children)
                            {
                                if (element.GetType() == typeof(ImageButton) && element != button) element.RemoveEffect(typeof(ImageTint));
                            }
                            button.ToggleEffects(new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }, null);
                        };
                        cell.SetElementTag(tag);
                        button.SetElementTag(tag);
                        var y = j == 0 ? 0 : parentHeight / count * j / (parentHeight - (parentHeight / count));
                        container.Children.Add(cell, new Rectangle(0, y, 1, 1/count), AbsoluteLayoutFlags.All);
                        container.Children.Add(button, new Rectangle(0, y, 1, 1/count), AbsoluteLayoutFlags.All);
                        fridgeItemBase[name][cellIndexer].Add(button, new List<ItemLayout>());
                    }
                    cellIndexer++;
                }

                for (int k = 2; k < fridgeRows.Length; k ++)
                {
                    string[] rowParsable = fridgeRows[k].TrimStart('(').TrimEnd(')').Split('+');
                    var background = new Image() { Source = fridgeIcon, Aspect = Aspect.Fill };
                    int amount = rowParsable.Length;

                    fridgeItemBase[name].Add(cellIndexer, new Dictionary<ImageButton, List<ItemLayout>>());
                    AbsoluteLayout rowLayout = new AbsoluteLayout() { HeightRequest = 50 };

                    if (fridgeLayout.Children.Count > 2)
                    {
                        AbsoluteLayout.SetLayoutBounds(rowLayout, new Rectangle(.5, FridgeEditPage.fridge_height / fridgeLayout.HeightRequest * (fridgeLayout.Children.Count - 2),
                            1 / FridgeEditPage.Main_Cell_Width_Div, FridgeEditPage.fridge_height / fridgeLayout.HeightRequest));
                    }
                    else
                    {
                        AbsoluteLayout.SetLayoutBounds(rowLayout, new Rectangle(.5, 0,1 / FridgeEditPage.Main_Cell_Width_Div, FridgeEditPage.fridge_height / fridgeLayout.HeightRequest));
                    }
                    fridgeLayout.Children.Insert(2, rowLayout);
                    AbsoluteLayout.SetLayoutFlags(rowLayout, AbsoluteLayoutFlags.All);
                    fridgeInfo[name].Add(cellIndexer, rowLayout);
                    rowLayout.Children.Add(background, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                    for (int i = 0; i < amount; i++)
                    {
                        double offset = double.Parse(rowParsable[i], CultureInfo.InvariantCulture); 
                        var button = new ImageButton() { Source = transIcon, Aspect = Aspect.Fill };
                        Image divider = new Image() { Aspect = Aspect.Fill };
                        var lastButtonWidth = 1 - double.Parse(rowParsable[rowParsable.Length - 1]);

                        var buttonWidth = i < amount - 1 ? double.Parse(rowParsable[i + 1]) - double.Parse(rowParsable[i]) : i == amount - 1 ? lastButtonWidth: 0;
                        double buttonX = i == amount - 1 ? 0 : offset / (1 - lastButtonWidth);
                        AbsoluteLayout.SetLayoutBounds(button, new Rectangle(buttonX, 0, buttonWidth, 1));
                        AbsoluteLayout.SetLayoutFlags(button, AbsoluteLayoutFlags.All);
                        rowLayout.Children.Add(button);
                        fridgeItemBase[name][cellIndexer].Add(button, new List<ItemLayout>());


                      //  Console.WriteLine("parse num " + buttonWidth + " offset "+ buttonX);
                      
                        if (i != 0)
                        {
                            rowLayout.Children.Add(divider);
                            var dividerX = button.GetAbsolutePosition().X;
                            if (amount % 2 == 0)
                            {
                                divider.Source = offset < .5 ? cabinetLeftIcon : offset == .5 ? cabinetMiddleIcon : cabinetRightIcon;
                            }
                            else
                            {
                                divider.Source = offset < .5 ? cabinetLeftIcon : cabinetRightIcon;
                            }

                            if (divider.Source.ToString() != cabinetMiddleIcon)
                            {
                                divider.ScaleX = Math.Abs((.5 - offset) / .5);
                                if (divider.ScaleX < 0.2) { divider.ScaleX = 0.2; }
                            }
                            else { divider.ScaleX = 0.3; }
                            AbsoluteLayout.SetLayoutBounds(divider, new Rectangle(offset, 0, .05, 1));
                            AbsoluteLayout.SetLayoutFlags(divider, AbsoluteLayoutFlags.All);
                        }
                        button.Clicked += (o, a) =>
                        {
                            foreach (Layout<View> element in fridgeLayout.Children)
                            {
                                element.Children.RemoveEffects(typeof(ImageTint));
                            }
                            button.ToggleEffects((new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }), null);
                        };

                        

                        var itemParsable = "";
                        if (itemString != null) itemParsable = itemString[cellIndexer].Length > 2 ? itemString[cellIndexer].TrimEnd(')').Substring(2) : "";
                        var itemIterator = 0;

                        if (itemParsable.Length > 2)
                            foreach (var itemID in itemParsable.Split('+'))
                            {
                                if (itemID.Length > 0)
                                {
                                    int id = int.Parse(itemID);
                                    var item = itemList.Where(t => t.ID == id).FirstOrDefault();
                                    if (item != null)
                                    {
                                        ItemLayout itemLayout = new ItemLayout(50, 50, item)
                                            .AddMainImage()
                                            .AddAmountMark()
                                            .AddExpirationMark()
                                            .AddTitle();
                                        item.ID = id;
                                        itemLayout.BindCabinetInfo(button.GetAbsolutePosition().X, cellIndexer, button, name, GetFridgeView);
                                        itemLayout.StorageName = name;
                                        itemLayout.RecalculateDate();
                                        itemLayout.AddInfoIcon();
                                        fridgeItemBase[name][cellIndexer][button].Add(itemLayout);
                                        UnplacedItemBase.Remove(item.ID);
                                        itemIterator++;
                                    }
                                }
                            }
                    }
                    cellIndexer++;
                }
            }
        }

        */

        /*
        public static Dictionary<int, List<ImageButton>> GetContactViews(string cabinetName)
        {
            Dictionary<int, List<ImageButton>> dict = new Dictionary<int, List<ImageButton>>();
            foreach (int index in GetItemBase()[cabinetName].Keys)
            {
                dict.Add(index, GetItemBase()[cabinetName][index].Keys.ToList());
            }
            return dict;
        }*/
       

        public static readonly Dictionary<string, IconLayout> PresetIcons = new Dictionary<string, IconLayout>()
        { 
            {"carrot", new IconLayout("carrot.png", "carrot") },
            {"apple",  new IconLayout("apple.png", "apple") },
            {"blueberry", new IconLayout("blueberry.png", "blueberry") },
            {"cookie",  new IconLayout("cookie.png", "cookie") },
            {"chocolate",  new IconLayout("chocolate.png", "chocolate") },
            {"cucumber",  new IconLayout("cucumber.png", "cucumber") },
            {"egg",  new IconLayout("egg.png", "egg") },
            {"grape",  new IconLayout("grape.png", "grape") },
            {"lemon",  new IconLayout("lemon.png", "lemon") },
            {"meat",  new IconLayout("meat.png", "meat") },
           // {"meat",  new IconLayout("meat.png", "meat") },
            {"orange",  new IconLayout("orange.png", "orange") },
            {"watermelon",  new IconLayout("watermelon.png", "watermelon") },
           // {"meat",  new IconLayout("meat.png", "meat") },
            {"potato", new IconLayout("potato.png", "potato") }
        };
        public static readonly Dictionary<string, IconLayout> DefaultIcons = new Dictionary<string, IconLayout>()
        {
            {"product",  new IconLayout("carrot.png", "carrot") }
        };

        public static readonly Dictionary<string, string> PresetExpirationBase = new Dictionary<string, string>()
        {
            {"carrot", $"Whole carrots last 4 weeks (until {DateCalculator.GetDateIn(28)}). Baby carrots last 3 weeks (until {DateCalculator.GetDateIn(21)})."},
            {"apple", $"Apples last 1 week in pantry (until {DateCalculator.GetDateIn(7)}) and 5 weeks in fridge (until {DateCalculator.GetDateIn(35)})." },
            {"blueberry", $"Blueberries last 3 days in pantry (until {DateCalculator.GetDateIn(3)}) and 10 days in fridge (until {DateCalculator.GetDateIn(10)})." },
            {"blueberrie", $"Blueberries last 3 days in pantry (until {DateCalculator.GetDateIn(3)}) and 10 days in fridge (until {DateCalculator.GetDateIn(10)})." },
            {"cucumber", $"Cucumbers last 1 week in fridge (until {DateCalculator.GetDateIn(7)})." },
            {"grape", $"Grapes last 4 days in pantry (until {DateCalculator.GetDateIn(4)}) and 8 days in fridge (until {DateCalculator.GetDateIn(8)})." },
            {"lemon", $"Lemons last 3 weeks in pantry (until {DateCalculator.GetDateIn(21)}) and 8 weeks in fridge (until {DateCalculator.GetDateIn(56)})." },
            {"orange", $"Oranges last 2 weeks in pantry (until {DateCalculator.GetDateIn(14)}) and a month in fridge (until {DateCalculator.GetDateIn(30)})." },
            {"watermelon", $"Watermelons last 1 week in pantry (until {DateCalculator.GetDateIn(7)}) and 2 weeks in fridge (until {DateCalculator.GetDateIn(14)})." },
            {"potato", $"Potatoes last 3 weeks in pantry (until {DateCalculator.GetDateIn(21)}) and 2 months in fridge (until {DateCalculator.GetDateIn(60)})." },
            {"potatoe", $"Potatoes last 3 weeks in pantry (until {DateCalculator.GetDateIn(21)}) and 2 months in fridge (until {DateCalculator.GetDateIn(60)})." },
        };


        public static Dictionary<int, ItemLayout> UnplacedItemBase = new Dictionary<int, ItemLayout>();
        public static Dictionary<int, ItemLayout> MetaItemBase = new Dictionary<int, ItemLayout>();

        public enum StorageSelection
        {
            cabinet,
            fridge
        }
        public static StorageSelection storageSelection;
    }
}
