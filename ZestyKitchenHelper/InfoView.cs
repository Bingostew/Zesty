using System;
using System.Collections.Generic;
using System.Text;
using Utility;
using Xamarin.Forms;
using System.Linq;

namespace ZestyKitchenHelper
{
    public class InfoView
    {
        public const double info_view_height_proportional = 0.75;
        public const double info_view_width_proportional = 0.75;
        private const int grid_font_size = 14;
        private const int side_margin = 10;
        private const int vertical_margin = 5;
        private const int close_button_size = 30;
        private double button_width;


        StackLayout pageContainer;

        public InfoView(Item item)
        {
            // Calculating sizes
            button_width = ContentManager.screenWidth * info_view_width_proportional / 3;
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
            closeButton.Clicked += (o, a) => ContentManager.pageController.RemoveInfoView(this);
            var itemName = new Label() { Text = item.Name, TextColor = Color.Black, FontSize = 25, HorizontalTextAlignment = TextAlignment.Center };
            var itemNameDivider = new BoxView() { HeightRequest = 1, WidthRequest = ContentManager.screenWidth * 0.5, Color = Color.Gray };
            var itemImage = new Image() { Source = item.Icon.Substring(6), Aspect = Aspect.Fill, HorizontalOptions = LayoutOptions.Center };

            var expirationDateTitle = new Label() { Text = "Expiration Date:", TextColor = Color.Black, FontSize = grid_font_size };
            var expDateText = item.daysUntilExp < 0 ? "?" : item.expMonth + "/" + item.expDay + "/" + item.expYear;
            var expirationDateLabel = new Label() { Text = expDateText, TextColor = Color.Black, FontSize = grid_font_size, HorizontalTextAlignment = TextAlignment.End };
            var expirationDateDivider = new BoxView() { HeightRequest = 1, WidthRequest = ContentManager.screenWidth * 0.5, Color = Color.Gray };

            var daysToExpString = item.daysUntilExp < 0 ? "?" : item.daysUntilExp.ToString();
            var daysToExpirationTitle = new Label() { Text = "Days Until Expiration:", TextColor = Color.Black, FontSize = grid_font_size };
            var daysToExpirationLabel = new Label() { Text = daysToExpString, TextColor = Color.Black, FontSize = grid_font_size, HorizontalTextAlignment = TextAlignment.End };
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
            mainGrid.Children.Add(amountDivider, 0, 5);
            Grid.SetColumnSpan(amountDivider, 2);
            mainGrid.Children.Add(locationTitle, 0, 6);
            mainGrid.Children.Add(locationLabel, 1, 6);

            var toStorageViewButton = new Button() { BackgroundColor = Color.FromRgba(0, 100, 20, 80), Text = "View In Storage", TextColor = Color.Black,
                HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.EndAndExpand, Margin = new Thickness(0, vertical_margin) };
            var addButton = new Button()
            {
                BackgroundColor = Color.FromRgba(0, 100, 0, 80),
                Text = "Add",
                WidthRequest = button_width,
                TextColor = Color.Black,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand,
                Margin = new Thickness(0, 0, 0, vertical_margin)
            };
            var consumeButton = new Button()
            {
                BackgroundColor = Color.FromRgba(100, 20, 0, 80),
                Text = "Consume",
                WidthRequest = button_width,
                TextColor = Color.Black,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand,
                Margin = new Thickness(0, 0, 0, vertical_margin)
            };
            toStorageViewButton.BackgroundColor = item.Stored ? Color.FromRgba(0, 100, 20, 80) : Color.Gray;
            toStorageViewButton.Clicked += (obj, args) =>
            {
                if (item.Stored)
                {
                    ContentManager.pageController.RemoveInfoView(this);
                    if(!ContentManager.pageController.IsOnPage<CabinetViewPage>())
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



            void animateAmountChange(bool add)
            {
                // Get x, y, w, h in proprotional terms.
                var dhGradientLine = ContentManager.screenHeight - gradientLine.HeightRequest;
                var y = ContentManager.screenHeight * info_view_height_proportional / 2 + ContentManager.screenHeight / 2 - gradientLine.HeightRequest;
                y /= dhGradientLine;
                var h = gradientLine.HeightRequest / ContentManager.screenHeight;
                var gradientLineY = add ? y : 0;
                ContentManager.pageController.OverlayAnimation(gradientLine, new Rect(0.5, gradientLineY, info_view_width_proportional, h),
                            ViewExtensions.LinearInterpolator(gradientLine, ContentManager.screenHeight * info_view_height_proportional - gradientLine.HeightRequest, 500, t => gradientLine.TranslationY = add ? -t : t, Easing.CubicInOut));

                var amountLabelBounds = amountLabel.Bounds;
                var amountChangeLabel = new Label()
                { TextColor = Color.Gray, FontSize = 40, FontAttributes = FontAttributes.Bold, WidthRequest = 50, HeightRequest = 50, HorizontalTextAlignment = TextAlignment.End };
                amountChangeLabel.Text = add ? "+1" : "-1";
                var dwAmount = ContentManager.screenWidth - amountChangeLabel.WidthRequest;
                var dhAmount = ContentManager.screenHeight - amountChangeLabel.HeightRequest;
                ContentManager.pageController.OverlayAnimation(amountChangeLabel,
                    new Rect(amountLabel.GetAbsolutePosition().X / dwAmount, amountLabel.GetAbsolutePosition().Y / dhAmount, amountChangeLabel.WidthRequest / ContentManager.screenWidth, amountChangeLabel.HeightRequest / ContentManager.screenHeight),
                    amountChangeLabel.LinearInterpolator(80, 2000, t => { amountChangeLabel.TranslationY = -t; amountChangeLabel.Opacity = 1 - t / 100; }, Easing.CubicOut),
                    () => { amountChangeLabel.TranslationY = 0;});
            }
            addButton.Clicked += (obj, args) =>
            {
                // add amount
                item.Amount++;
                // animate 
                animateAmountChange(true);
                amountLabel.Text = item.Amount.ToString();

                // Save data locally or to cloud
                if (ContentManager.isLocal)
                    LocalStorageController.UpdateItem(item);
                else
                    FireBaseController.SaveItem(item);
            };
            consumeButton.Clicked += (obj, args) =>
            {
                // Subtract amount
                item.Amount--;
                // If not fully consumed, keep track of it
                if (item.Amount > 0)
                {
                    animateAmountChange(false);
                    amountLabel.Text = item.Amount.ToString();
                    // Save data locally or to cloud
                    if (ContentManager.isLocal)
                        LocalStorageController.UpdateItem(item);
                    else
                        FireBaseController.SaveItem(item);
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
                    if (ContentManager.isLocal)
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
                        List<View> childList = cellGrid.Children.ToList();
                        foreach (ItemLayout child in cellGrid.Children)
                        {
                            if (item.ID == child.ItemData.ID)
                            {
                                gridCell.RemoveItem(child);
                                break;
                            }
                        }
                        //Update storage cell children
                        //gridCell.AddItem(childList);
                    }
                }
            };

            pageContainer = new StackLayout()
            {
                BackgroundColor = Color.Beige,
                Children = { closeButton, itemName, itemNameDivider, itemImage, mainGrid, toStorageViewButton, addButton,
                    new StackLayout()
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children = {addButton, consumeButton}
                    }
                }
            };
        }

        public StackLayout GetView()
        {
            return pageContainer;
        }
    }
}
