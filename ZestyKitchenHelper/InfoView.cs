using System;
using System.Collections.Generic;
using System.Text;
using Utility;
using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class InfoView
    {
        public const double info_view_height_proportional = 0.75;
        public const double info_view_width_proportional = 0.75;
        private const int grid_font_size = 12;
        private const int side_margin = 10;
        private const int vertical_margin = 10;
        private const int close_button_size = 30;


        StackLayout pageContainer;

        public InfoView(Item item)
        {
            Grid mainGrid = new Grid()
            {
                HeightRequest = ContentManager.screenHeight * info_view_height_proportional * 0.75,
                Margin = new Thickness(side_margin, 0),
                RowDefinitions =
                {
                    new RowDefinition(){Height = GridLength.Star},
                    new RowDefinition(){Height = 1},
                    new RowDefinition(){Height = GridLength.Star},
                    new RowDefinition(){Height = 1},
                    new RowDefinition(){Height = GridLength.Star},
                    new RowDefinition(){Height = 1},
                    new RowDefinition(){Height = GridLength.Star},
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = GridLength.Star},
                    new ColumnDefinition(){Width = GridLength.Star},
                }
            };

            var closeButton = new Button() { BackgroundColor = Color.Gray, Text = "X", TextColor = Color.Black, HorizontalOptions = LayoutOptions.End, WidthRequest = close_button_size, HeightRequest = close_button_size };
            closeButton.Clicked += (o,a) => ContentManager.pageController.RemoveInfoView(this);
            var itemName = new Label() { Text = item.Name, TextColor = Color.Black, FontSize = 25, HorizontalTextAlignment = TextAlignment.Center };
            var itemNameDivider = new BoxView() { HeightRequest = 1, WidthRequest = ContentManager.screenWidth * 0.5, Color = Color.Gray };
            var itemImage = new Image() { Source = item.Icon.Substring(6), Aspect = Aspect.Fill, WidthRequest = 150, HeightRequest = 150, HorizontalOptions = LayoutOptions.Center };

            var expirationDateTitle = new Label() { Text = "Expiration Date:", TextColor = Color.Black, FontSize = grid_font_size };
            var expirationDateLabel = new Label() { Text =  item.expMonth + "/" + item.expDay + "/" + item.expYear, TextColor = Color.Black, FontSize = grid_font_size, HorizontalTextAlignment = TextAlignment.End };
            var expirationDateDivider = new BoxView() { HeightRequest = 1, WidthRequest = ContentManager.screenWidth * 0.5, Color = Color.Gray };

            var daysToExpirationTitle = new Label() { Text = "Days Until Expiration:", TextColor = Color.Black, FontSize = grid_font_size };
            var daysToExpirationLabel = new Label() { Text = item.daysUntilExp.ToString(), TextColor = Color.Black, FontSize = grid_font_size, HorizontalTextAlignment = TextAlignment.End };
            var expirationDayDivider = new BoxView() { HeightRequest = 1, WidthRequest = ContentManager.screenWidth * 0.5, Color = Color.Gray };

            var amountTitle = new Label() { Text = "Amount: ", TextColor = Color.Black, FontSize = grid_font_size };
            var amountLabel = new Label() { Text = item.Amount.ToString(), TextColor = Color.Black, FontSize = grid_font_size, WidthRequest = 30, HorizontalTextAlignment = TextAlignment.End };
            var amountDivider = new BoxView() { HeightRequest = 1, WidthRequest = ContentManager.screenWidth * 0.5, Color = Color.Gray };

            var locationTitle = new Label() { Text = "Location:", TextColor = Color.Black, FontSize = grid_font_size };
            var locationLabel = new Label() { TextColor = Color.Black, FontSize = grid_font_size, HorizontalTextAlignment = TextAlignment.End };
            locationLabel.Text = item.Stored ? item.StorageName : "Not Placed";

            mainGrid.Children.Add(expirationDateTitle, 0, 0);
            mainGrid.Children.Add(expirationDateLabel, 1, 0);
            mainGrid.Children.Add(expirationDateDivider, 0, 1);
            Grid.SetColumnSpan(expirationDateDivider, 2);
            mainGrid.Children.Add(daysToExpirationTitle, 0, 2);
            mainGrid.Children.Add(daysToExpirationLabel, 1, 2);
            mainGrid.Children.Add(expirationDayDivider, 0, 3);
            Grid.SetColumnSpan(expirationDayDivider, 2);
            mainGrid.Children.Add(amountTitle, 0, 4);
            mainGrid.Children.Add(amountLabel, 1, 4);
            mainGrid.Children.Add(amountDivider, 0,5);
            Grid.SetColumnSpan(amountDivider, 2);
            mainGrid.Children.Add(locationTitle, 0, 6);
            mainGrid.Children.Add(locationLabel, 1, 6);

            var toStorageViewButton = new Button() { BackgroundColor = Color.FromRgba(0, 100, 20, 80), Text = "View In Storage", TextColor = Color.Black, 
                HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.EndAndExpand, Margin = new Thickness(0, vertical_margin) };
            var consumeButton = new Button() { BackgroundColor = Color.FromRgba(100, 20, 0, 80), Text = "Consume", TextColor = Color.Black,
                HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.EndAndExpand, Margin = new Thickness(0, vertical_margin)  };
            toStorageViewButton.BackgroundColor = item.Stored ? Color.FromRgba(0, 100, 20, 80) : Color.Gray;
            toStorageViewButton.Clicked += (obj, args) =>
            {
                if (item.Stored)
                {
                    ContentManager.pageController.RemoveInfoView(this);
                    ContentManager.pageController.ToViewItemPage(item.StorageName, item.StorageCellIndex, item.StorageType);
                }
            };

            var gradientLineBrush = new LinearGradientBrush(new GradientStopCollection() {
                        new GradientStop(Color.Transparent, 0.1f),
                        new GradientStop(Color.FromRgba(0, 255, 0, 80), 0.5f),
                        new GradientStop(Color.Transparent, 1)
                  }, new Point(0, 0), new Point(0, 1));
            var gradientLine = new BoxView()
            {
                Background = gradientLineBrush,
                HeightRequest = 200,
            };
            var subtractionSymbol = new Label() { Text = "-1", TextColor = Color.Gray, FontSize = 40, FontAttributes = FontAttributes.Bold, WidthRequest = 50, HeightRequest = 50, HorizontalTextAlignment = TextAlignment.End};


            void animateConsumption()
            {
                // Get x, y, w, h in proprotional terms.
                consumeButton.IsEnabled = false;
                var dhGradientLine = ContentManager.screenHeight - gradientLine.HeightRequest;
                var y = ContentManager.screenHeight * info_view_height_proportional / 2 + ContentManager.screenHeight / 2 - gradientLine.HeightRequest;
                y /= dhGradientLine;
                var h = gradientLine.HeightRequest / ContentManager.screenHeight;
                ContentManager.pageController.OverlayAnimation(gradientLine, new Rect(0.5, y, info_view_width_proportional, h),
                            ViewExtensions.LinearInterpolator(gradientLine, ContentManager.screenHeight * info_view_height_proportional - gradientLine.HeightRequest, 500, t => gradientLine.TranslationY = -t, Easing.CubicInOut));

                var amountLabelBounds = amountLabel.Bounds;
                Console.WriteLine("InfoView 115 amt bounds " + amountLabelBounds.X + " " + amountLabelBounds.Y);
                var dwAmount = ContentManager.screenWidth - subtractionSymbol.WidthRequest;
                var dhAmount = ContentManager.screenHeight - subtractionSymbol.HeightRequest;
                ContentManager.pageController.OverlayAnimation(subtractionSymbol,
                    new Rect(amountLabelBounds.X / dwAmount, amountLabelBounds.Y / dhAmount, subtractionSymbol.WidthRequest / ContentManager.screenWidth, subtractionSymbol.HeightRequest / ContentManager.screenHeight),
                    subtractionSymbol.LinearInterpolator(100, 500, t => { subtractionSymbol.TranslationY = -t; subtractionSymbol.Opacity = 1 - t / 100; }, Easing.CubicOut), 
                    () => { subtractionSymbol.TranslationY = 0; consumeButton.IsEnabled = true; });
            }
            consumeButton.Clicked += (obj, args) =>
            {
                // Subtract amount
                item.Amount--;
                // If not fully consumed, keep track of it
                if (item.Amount > 0)
                {
                    animateConsumption();
                    amountLabel.Text = item.Amount.ToString();
                }
                // If fully consumed, remove it.
                else
                {
                    // If item not stored, remove it from unplaced grid
                    if (!item.Stored)
                    {
                        var itemLayoutUnplaced = ContentManager.UnplacedItemBase[item.ID];
                        var unplacedGrid = GridManager.GetGrid(ContentManager.unplacedGridName);
                        if (unplacedGrid.Children.Contains(itemLayoutUnplaced))
                            unplacedGrid.Children.Remove(itemLayoutUnplaced);
                    }
                    ContentManager.UnplacedItemBase.Remove(item.ID);

                    // Remove item from meta grid
                    var itemLayoutMeta = ContentManager.MetaItemBase[item.ID];
                    var metaGrid = GridManager.GetGrid(ContentManager.metaGridName);
                    ContentManager.MetaItemBase.Remove(item.ID);
                    GridManager.AddGridItem(metaGrid, ContentManager.MetaItemBase.Values, true);

                    // Exit out of infoView
                    ContentManager.pageController.RemoveInfoView(this);

                    // Save data locally and to cloud
                    if(ContentManager.isLocal)
                        LocalStorageController.DeleteItem(item);
                    else
                        FireBaseController.DeleteItem(item);

                    // If item is stored, delete it from storage
                    if (item.Stored)
                    {
                        // Delete item from storage cell
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
                        //Update storage cell children
                        GridManager.AddGridItem(cellGrid, childList, true);
                    }
                }
            };

            pageContainer = new StackLayout()
            {
                BackgroundColor = Color.Beige,
                Children = { closeButton, itemName, itemNameDivider, itemImage, mainGrid, toStorageViewButton, consumeButton }
            };
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
