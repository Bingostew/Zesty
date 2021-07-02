using System;
using System.Collections.Generic;
using System.Text;
using Utility;
using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class InfoView
    {
        Func<string, Layout<View>> storageAction;
        string storageName;

        Label locationLabel;
        StackLayout pageContainer;
        public bool cabinetBinded;
        public InfoView(Item item)
        {
            var closeButton = new Button() { BackgroundColor = Color.Red, Text = "X", HorizontalOptions = LayoutOptions.End, WidthRequest = 100, HeightRequest = 100 };
            closeButton.Clicked += (o,a) => ContentManager.pageController.RemoveInfoView(this);
            var itemImage = new Image() { Source = item.Icon.Substring(6), Aspect = Aspect.Fill, WidthRequest = 150, HeightRequest = 150, HorizontalOptions = LayoutOptions.StartAndExpand };
            var expirationDateLabel = new Label() { Text = "Expiration Date: " + item.expMonth + "/" + item.expDay + "/" + item.expYear, TextColor = Color.Black, FontSize = 20 };
            var amountLabel = new Label() { Text = "Amount: " + item.Amount.ToString(), TextColor = Color.Black, FontSize = 20 };
            locationLabel = new Label() { TextColor = Color.Black, FontSize = 20 };
            locationLabel.Text = item.Stored ? "Location " + item.StorageName : "Location: This item has not been placed.";
            var toStorageViewButton = new Button() { BackgroundColor = Color.FromRgba(0, 100, 20, 80), Text = "View In Storage", TextColor = Color.Black, HorizontalOptions = LayoutOptions.CenterAndExpand };
            var consumeButton = new Button() { BackgroundColor = Color.FromRgba(100, 20, 0, 80), Text = "Consume", TextColor = Color.Black };
            toStorageViewButton.BackgroundColor = item.Stored ? Color.FromRgba(0, 100, 20, 80) : Color.Gray;
            toStorageViewButton.Clicked += (obj, args) =>
            {
                if (item.Stored)
                {
                    ContentManager.pageController.RemoveInfoView(this);
                    ContentManager.pageController.ToViewItemPage(item.StorageName, item.StorageCellIndex, item.StorageType);
                }
            };
            consumeButton.Clicked += (obj, args) =>
            {
                if (!item.Stored) {
                    var itemLayoutUnplaced = ContentManager.UnplacedItemBase[item.ID];
                    var unplacedGrid = GridManager.GetGrid(ContentManager.unplacedGridName);
                    if (unplacedGrid.Children.Contains(itemLayoutUnplaced))
                        unplacedGrid.Children.Remove(itemLayoutUnplaced);
                }
                var itemLayoutMeta = ContentManager.MetaItemBase[item.ID];
                var metaGrid = GridManager.GetGrid(ContentManager.metaGridName);
                if (metaGrid.Children.Contains(itemLayoutMeta))
                    metaGrid.Children.Remove(itemLayoutMeta);
                ContentManager.MetaItemBase.Remove(item.ID);
                ContentManager.UnplacedItemBase.Remove(item.ID);

                ContentManager.pageController.RemoveInfoView(this);

                LocalStorageController.DeleteItem(item);
                FireBaseController.DeleteItem(item);

                if (item.Stored)
                {
                    var gridCell = item.StorageType == ContentManager.fridgeStorageType ? ContentManager.FridgeMetaBase[item.StorageName].GetGridCell(item.StorageCellIndex) :
                        ContentManager.CabinetMetaBase[item.StorageName].GetGridCell(item.StorageCellIndex);
                    var cellGrid = gridCell.GetItemGrid();
                    var childList = cellGrid.Children;
                    foreach (ItemLayout child in cellGrid.Children)
                    {
                        if (item.ID == child.ItemData.ID)
                        {
                            childList.Remove(child);
                            break;
                        }
                    }
                    GridManager.AddGridItem(cellGrid, childList, true);
                }
                // TODO update cabinet or fridge
            };

            pageContainer = new StackLayout()
            {
                BackgroundColor = Color.FromRgba(0, 0, 0,90),
                Children = { closeButton, itemImage, expirationDateLabel, amountLabel, locationLabel, toStorageViewButton, consumeButton }
            };
        }

        public void BindCabinetInfo(Func<string, Layout<View>> getStorageAction, string _storageName)
        {
            storageAction = getStorageAction;
            storageName = _storageName;
            cabinetBinded = true;
        }

        public StackLayout GetView()
        {
            return pageContainer;
        }
        /*
        public void SetCabinetView()
        {
            if (storageAction != null)
            {
                locationLabel.Text = "Location: " + storageName;
                var storageView = storageAction(storageName);
                // ImageTint tintEffect = new ImageTint() { tint = Color.FromRgba(100, 20, 20, 90) };
                // parentButton.ToggleEffects(tintEffect, null);
                pageContainer.Children.Insert(pageContainer.Children.Count - 1, storageView);
            }

        }*/
    }
}
