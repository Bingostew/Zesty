using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Utility;
using System.Linq;
using ZXing.Net.Mobile.Forms;
using ZXing.Mobile;

namespace ZestyKitchenHelper
{
    public class AddView : ContentPage
    {
        private const double form_height_proportional = 0.45;
        private const int form_label_font_size = 20;
        private const int form_label_horizontal_margin = 10;
        private const int form_input_border_width = 2;
        private const int form_icon_select_border_radius = 2;
        private const int form_icon_margin = 5;
        private static readonly double formIconWidthHeight;
        private static readonly double formGridRowHeight;
        private const int form_grid_row_count = 5;
        private const int form_grid_spacing = 5;

        private const double numpad_height_proportional = 0.35;
        private static readonly Color numpadBackground = new Color(255, 255, 255, 80);
        private const int numpad_font_size = 20;
        private const int numpad_spacing = 5;

        int imageSelectorIndex = 0;
        int selectorIndex = 0;
        List<Button> formSelector = new List<Button>();
        List<Button> imageSelector = new List<Button>();
        Item item;
        Entry nameInput;
        Label autoDetectLabel;
        ScrollView presetScrollView, defaultScrollView;
        Grid presetSelectGrid;
        Grid defaultSelectGrid;
        List<IconLayout> presetResult = new List<IconLayout>();
        List<int> presetResultSorter = new List<int>();
        List<Item> newItem = new List<Item>();
        static AddView()
        {

            formGridRowHeight = (form_height_proportional * ContentManager.screenHeight / form_grid_row_count * 2 - form_grid_spacing * (form_grid_row_count - 1)) / 3;
            formIconWidthHeight = form_height_proportional * ContentManager.screenHeight / form_grid_row_count - form_icon_margin * 1.5 - form_grid_spacing * 2;
        }
        /// <summary>
        /// Initialized an instance of add form with a num pad.
        /// </summary>
        ///
        /// <returns></returns>
        public AddView(Action<Item> localUnplacedEvent, Action<Item> baseUnplacedEvent,
            string storageName = "", bool limited = true, Grid partialUnplacedGrid = null)
        {
            item = new Item().SetItem(-1, -1, -1, 1, "product", ContentManager.addIcon);
            Grid currentGrid = new Grid();

            Vector2D<int> selectGridIndex = new Vector2D<int>(0, 5);

            presetSelectGrid = GridManager.InitializeGrid("Partial Preset Grid", 2, 1, formIconWidthHeight, formIconWidthHeight);
            presetSelectGrid.RowSpacing = form_icon_margin; presetSelectGrid.ColumnSpacing = form_icon_margin;
            presetScrollView = new ScrollView() { Content = presetSelectGrid, Orientation = ScrollOrientation.Horizontal, Margin = new Thickness(0, form_grid_spacing) };
            defaultSelectGrid = GridManager.InitializeGrid("Partial Default Grid", 2, 1, formIconWidthHeight, formIconWidthHeight);
            defaultSelectGrid.RowSpacing = form_icon_margin; defaultSelectGrid.ColumnSpacing = form_icon_margin;
            defaultScrollView = new ScrollView() { Content = defaultSelectGrid, Orientation = ScrollOrientation.Horizontal, Margin = new Thickness(0, form_grid_spacing) };
            foreach (var name in ContentManager.DefaultIcons.Keys)
            {
                ContentManager.DefaultIcons[name].OnClickIconAction = (imageButton) =>
                {
                    toggleIconSelect(imageButton, defaultSelectGrid); item.Icon = ContentManager.DefaultIcons[name].GetImageSource();
                };
            }
            GridManager.AddGridItem(defaultSelectGrid, ContentManager.DefaultIcons.Values, true);

            autoDetectLabel = new Label() { FontSize = 15, TextColor = Color.Black, BackgroundColor = Color.White, IsVisible = false, AnchorX = 0 };

            var foregroundTint = new Image() { BackgroundColor = Color.FromRgba(0, 0, 0, 98), HeightRequest = ContentManager.screenHeight, WidthRequest = ContentManager.screenWidth };

            Grid form = new Grid()
            {
                BackgroundColor = Color.FromRgb(200, 200, 200),
                RowSpacing = form_grid_spacing,
                ColumnSpacing = form_grid_spacing,
                RowDefinitions =
                {
                    new RowDefinition(){ Height = GridLength.Star },
                    new RowDefinition(){ Height = GridLength.Star },
                    new RowDefinition(){ Height = GridLength.Star },
                    new RowDefinition(){ Height = GridLength.Star },
                    new RowDefinition(){ Height = GridLength.Star }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = 120 },
                    new ColumnDefinition()
                }
            };

            void ClearText(Button button)
            {
                button.Text = "";
                toggleSelect(selectorIndex, formSelector, Color.BlanchedAlmond, Color.Wheat);
            }

            nameInput = new Entry() { HeightRequest = form_height_proportional * ContentManager.screenHeight / form_grid_row_count, Placeholder = "Product" };
            nameInput.Unfocused += (obj, args) => { item.Name = nameInput.Text; autoDetectExpiration(nameInput.Text); if (imageSelectorIndex == 0) changeSelectedIcon(); };
            nameInput.Completed += (obj, args) => { item.Name = nameInput.Text; if (imageSelectorIndex == 0) changeSelectedIcon(); };
            var nameLabel = new Label() { Text = "Name: ", FontSize = form_label_font_size, TextColor = Color.Black, Margin = new Thickness(form_label_horizontal_margin, 0), VerticalTextAlignment = TextAlignment.Center };
            var dateLabel = new Label() { Text = "Exp. Date: ", FontSize = form_label_font_size, TextColor = Color.Black, Margin = new Thickness(form_label_horizontal_margin, 0), VerticalTextAlignment = TextAlignment.Center };
            var amountLabel = new Label() { Text = "Amount: ", FontSize = form_label_font_size, TextColor = Color.Black, Margin = new Thickness(form_label_horizontal_margin, 0), VerticalTextAlignment = TextAlignment.Center };
            var iconLabel = new Label() { Text = "Icon: ", FontSize = form_label_font_size, TextColor = Color.Black, HeightRequest = formGridRowHeight, VerticalTextAlignment = TextAlignment.Center };
            var dateMonth = new Button() { BorderColor = Color.Black, BorderWidth = form_input_border_width, BackgroundColor = Color.Transparent };
            dateMonth.Clicked += (obj, arg) => { selectorIndex = 0; ClearText(dateMonth); };
            var dateDay = new Button() { BorderColor = Color.Black, BorderWidth = form_input_border_width, BackgroundColor = Color.Transparent, };
            dateDay.Clicked += (obj, arg) => { selectorIndex = 1; ClearText(dateDay); };
            var dateYear = new Button() { BorderColor = Color.Black, BorderWidth = form_input_border_width };
            dateYear.Clicked += (obj, arg) => { selectorIndex = 2; ClearText(dateYear); };
            var amountInput = new Button() { BorderColor = Color.Black, BorderWidth = form_input_border_width, BackgroundColor = Color.Transparent, Margin = new Thickness(0, 0, form_label_horizontal_margin, 0) };
            amountInput.Clicked += (obj, arg) => { selectorIndex = 3; ClearText(amountInput); };
            Grid expGrid = GridManager.InitializeGrid(1, 3, GridLength.Star, GridLength.Star);
            expGrid.ColumnSpacing = form_grid_spacing;
            expGrid.Margin = new Thickness(0, 0, form_label_horizontal_margin, 0);
            GridManager.AddGridItem(expGrid, new List<View>() { dateMonth, dateDay, dateYear }, true);

            var iconSelect1 = new Button() { HeightRequest = formGridRowHeight, Text = "Preset", TextColor = Color.Black, CornerRadius = form_icon_select_border_radius, BorderColor = Color.Wheat, BorderWidth = 3 };
            iconSelect1.Clicked += (obj, arg) =>
            {
                imageSelectorIndex = 0; toggleSelect(0, imageSelector, Color.SaddleBrown, Color.Wheat);

                defaultScrollView.IsVisible = false;
                presetScrollView.IsVisible = true;
                changeSelectedIcon();
            };
            var iconSelect2 = new Button() { HeightRequest = formGridRowHeight, Text = "General", TextColor = Color.Black, CornerRadius = form_icon_select_border_radius, BorderColor = Color.Wheat, BorderWidth = 3 };
            iconSelect2.Clicked += (obj, arg) =>
            {
                imageSelectorIndex = 1; toggleSelect(1, imageSelector, Color.SaddleBrown, Color.Wheat);
                defaultScrollView.IsVisible = true;
                presetScrollView.IsVisible = false;
            };
            
            Grid iconLabelGrid = GridManager.InitializeGrid(3, 1, GridLength.Star, GridLength.Star);
            GridManager.AddGridItem(iconLabelGrid, new List<View>() { iconLabel, iconSelect1, iconSelect2 }, false);
            iconLabelGrid.Margin = new Thickness(form_label_horizontal_margin, form_grid_spacing);

            void toggleSelect(int index, List<Button> buttonList, Color colorTo, Color normal)
            {
                foreach (var button in buttonList)
                {
                    button.BackgroundColor = normal;
                }
                buttonList[index].BackgroundColor = colorTo;
            }

            formSelector.Add(dateMonth);
            formSelector.Add(dateDay);
            formSelector.Add(dateYear);
            formSelector.Add(amountInput);
            imageSelector.Add(iconSelect1);
            imageSelector.Add(iconSelect2);

            form.Children.Add(nameLabel, 0, 0);
            form.Children.Add(nameInput, 1, 0);
            form.Children.Add(dateLabel, 0, 1);
            form.Children.Add(expGrid, 1, 1);
            form.Children.Add(amountLabel, 0, 2);
            form.Children.Add(amountInput, 1, 2);
            form.Children.Add(presetScrollView, 1, 3);
            form.Children.Add(iconLabelGrid, 0, 3);
            Grid.SetRowSpan(presetScrollView, 2);
            Grid.SetRowSpan(iconLabelGrid, 2);

            void setItem(int newText, int index)
            {
                switch (selectorIndex)
                {
                    case 0: item.expMonth = newText; break;
                    case 1: item.expDay = newText; break;
                    case 2: item.expYear = newText; break;
                    case 3: item.Amount = newText; break;
                }
            }
            void changeText(string addedText)
            {
                // limiting digits: month / day: 2 digits. Year: 4 digits. Amount : 3 digits.
                int limit = selectorIndex == 2 ? 4 : selectorIndex == 3 ? 3 : 2;
                string oldText = formSelector[selectorIndex].Text ?? "";
                string newText = oldText;
                if (oldText.Length < limit)
                {
                    toggleSelect(selectorIndex, formSelector, Color.BlanchedAlmond, Color.Wheat);
                    newText = oldText + addedText;
                    formSelector[selectorIndex].Text = newText;
                    if (oldText.Length == limit - 1 && selectorIndex < formSelector.Count - 1)
                    {
                        var inputInt = int.Parse(newText);
                        switch (selectorIndex)
                        {
                            case 0:
                                if (inputInt > 12) newText = "12";
                                if (int.Parse(newText.ToCharArray()[0].ToString()) == 0) { newText.Remove(0, 1); }
                                if (inputInt <= 0) newText = "1"; break;
                            case 1:
                                var text = formSelector[0].Text ?? "1";
                                if (int.Parse(text.ToCharArray()[0].ToString()) == 0) { text.Remove(0, 1); }
                                var maxDay = DateCalculator.GetMonthList()[int.Parse(text) - 1];
                                if (inputInt > maxDay) newText = maxDay.ToString();
                                if (inputInt <= 0) newText = "1"; break;
                            case 3:
                                if (inputInt < 0) newText = "1"; break;
                        }
                        formSelector[selectorIndex].Text = newText;
                        setItem(int.Parse(newText), selectorIndex);
                        selectorIndex++;
                        toggleSelect(selectorIndex, formSelector, Color.BlanchedAlmond, Color.Wheat);
                    }
                    else
                    {
                        setItem(int.Parse(newText), selectorIndex);
                    }
                }
                else if (selectorIndex < formSelector.Count - 1)
                {
                    newText = oldText + addedText;
                    formSelector[selectorIndex].Text = newText;
                    setItem(int.Parse(newText), selectorIndex);
                    selectorIndex++; toggleSelect(selectorIndex, formSelector, Color.BlanchedAlmond, Color.Wheat);
                }
            }

            Grid numPadGrid = GridManager.InitializeGrid(4, 3, GridLength.Star, GridLength.Star);
            numPadGrid.Margin = new Thickness(form_label_horizontal_margin, 0);
            numPadGrid.RowSpacing = numpad_spacing;
            numPadGrid.ColumnSpacing = numpad_spacing;

            List<View> numPadList = new List<View>();
            for (int i = 1; i < 10; i++)
            {
                var button = new Button() { Text = i.ToString(), TextColor = Color.Black, Margin = new Thickness(0), BackgroundColor = numpadBackground, FontSize = numpad_font_size };
                int index = i;
                button.Clicked += (obj, arg) => changeText(index.ToString());
                numPadList.Add(button);
            }

            var numpadBottomLeftEmptyBuffer = new Button() { IsVisible = false };
            var zeroButton = new Button() { Text = "0", TextColor = Color.Black, FontSize = numpad_font_size, BackgroundColor = numpadBackground };
            zeroButton.Clicked += (obj, arg) => changeText("0");

            numPadList.Add(numpadBottomLeftEmptyBuffer);
            numPadList.Add(zeroButton);

            numPadGrid.OrganizeGrid(numPadList, GridOrganizer.OrganizeMode.HorizontalLeft);

            var scanButton = new Button() { Text = "Scan", TextColor = Color.Black, FontAttributes = FontAttributes.Bold, BackgroundColor = numpadBackground };
            var exitButton = new Button() { Text = "Exit", BackgroundColor = Color.Blue, TextColor = Color.Black };
            var newFormButton = new Button() { BackgroundColor = Color.ForestGreen, Text = "Add", TextColor = Color.Black };

            AbsoluteLayout manualEditLayout = new AbsoluteLayout()
            {
                HorizontalOptions = LayoutOptions.Center,
                HeightRequest = ContentManager.screenHeight,
                WidthRequest = ContentManager.screenWidth,
                Children =
                {
                    foregroundTint,
                    form,
                    autoDetectLabel,
                    numPadGrid,
                    scanButton,
                    newFormButton,
                    exitButton
                }
            };

            scanButton.Clicked += (o, a) =>
            {
                ContentManager.pageController.ToScanPage(this);
            };
            async void OnNewItemAdded(object obj, EventArgs args)
            {
                var animatedImage = new Image() { Source = item.Icon.Substring(6), Aspect = Aspect.Fill };
                manualEditLayout.Children.Add(animatedImage, new Rectangle(0.5, 0.9, 100, 100), AbsoluteLayoutFlags.PositionProportional);
                await animatedImage.QuadraticFlight(15, 75, -80, 30, (v) => { animatedImage.TranslationX = -v.X; animatedImage.TranslationY = -v.Y; }, 1500, easing: Easing.Linear);
            }

            newFormButton.Clicked += OnNewItemAdded;
            newFormButton.Clicked += (obj, args) =>
            {
                Item itemInstance = new Item().SetItem(item.expYear, item.expMonth, item.expDay, item.Amount, item.Name, item.Icon);
                Console.WriteLine("Addview 309 item icon " + itemInstance.Icon + " " + item.Icon);
                itemInstance.SetDaysUntilExpiration();
                newItem.Add(itemInstance);
                if (ContentManager.isLocal)
                {
                    localUnplacedEvent?.Invoke(itemInstance);
                }
                else
                {
                    Console.WriteLine("Addview 302 base unplace storage");
                    baseUnplacedEvent?.Invoke(itemInstance);
                }
                ResetForm();
            };

            AbsoluteLayout.SetLayoutBounds(foregroundTint, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(foregroundTint, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(autoDetectLabel, new Rectangle(0, 0, 1, .06));
            AbsoluteLayout.SetLayoutFlags(autoDetectLabel, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(form, new Rectangle(0, 0, 1, form_height_proportional));
            AbsoluteLayout.SetLayoutFlags(form, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(numPadGrid, new Rectangle(.5, .7, 1, numpad_height_proportional));
            AbsoluteLayout.SetLayoutFlags(numPadGrid, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(scanButton, new Rectangle(.5, .88, .5, .08));
            AbsoluteLayout.SetLayoutFlags(scanButton, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(newFormButton, new Rectangle(.25, .95, .25, .06));
            AbsoluteLayout.SetLayoutFlags(newFormButton, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(exitButton, new Rectangle(.75, .95, .25, .06));
            AbsoluteLayout.SetLayoutFlags(exitButton, AbsoluteLayoutFlags.All);

            exitButton.Clicked += (obj, args) =>
            {
                List<ItemLayout> newItemLayouts = new List<ItemLayout>();
                List<ItemLayout> newItemLayoutsCopy = new List<ItemLayout>();
                List<ItemLayout> newItemLayoutsCopy2 = new List<ItemLayout>();

                SaveInput(newItemLayouts, newItemLayoutsCopy, newItemLayoutsCopy2);

                if (partialUnplacedGrid != null)
                    GridManager.AddGridItem(partialUnplacedGrid, newItemLayoutsCopy2, false);

                newItemLayouts.Clear();
                newItemLayoutsCopy.Clear();
                newItemLayoutsCopy2.Clear();
                ResetForm();
                newItem.Clear();
                ContentManager.pageController.ReturnToPrevious();
            };

            Content = manualEditLayout;
        }

        public void SetProductName(string name)
        {
            nameInput.Text = name;
            item.Name = name;
            autoDetectExpiration(name);
            if (imageSelectorIndex == 0)
                changeSelectedIcon();
        }

        private void changeSelectedIcon()
        {
            presetResult.Clear(); presetResultSorter.Clear();
            foreach (string name in ContentManager.PresetIcons.Keys)
            {
                int match = 0;

                foreach (char n in name.ToCharArray())
                {
                    foreach (char i in item.Name.ToCharArray())
                    {
                        if (n == i || char.ToLower(n) == i || char.ToLower(i) == n)
                        {
                            match--;
                        }
                    }
                }
                presetResultSorter.Add(match);
                IconLayout iconLayout = ContentManager.PresetIcons[name];
                presetResult.Add(iconLayout);
                ContentManager.PresetIcons[name].OnClickIconAction += (button) =>
                {
                    toggleIconSelect(button, presetSelectGrid); var _name = name;
                    item.Icon = ContentManager.PresetIcons[_name].GetImageSource();
                };
                match = 0;

            } 
            toggleIconSelect(null, presetSelectGrid);
            ListSorter.SortToListAscending(presetResultSorter, presetResult);
            GridManager.AddGridItem(presetSelectGrid, presetResult, true, GridOrganizer.OrganizeMode.VerticalLeft);
            Console.WriteLine("AddView 177 preset children " + presetSelectGrid.Children.Count + " preset column count " + presetSelectGrid.ColumnDefinitions.Count);
        }

        private async void autoDetectExpiration(string name)
        {
            string[] names = name.Split(' ');
            bool detected = false;
            string key = "";
            string keyTrimmed = "";
            foreach (var n in names)
            {
                key = n.Trim().TrimStart(' ');
                keyTrimmed = key.TrimEnd('s');
                detected = ContentManager.PresetExpirationBase.ContainsKey(key) || ContentManager.PresetExpirationBase.ContainsKey(keyTrimmed);
                if (detected) break;
            }
            if (detected && !autoDetectLabel.IsVisible)
            {
                autoDetectLabel.ScaleX = 0;
                autoDetectLabel.IsVisible = true;
                autoDetectLabel.Text = ContentManager.PresetExpirationBase.ContainsKey(key) ? ContentManager.PresetExpirationBase[key] : ContentManager.PresetExpirationBase[keyTrimmed];
                await ViewExtensions.LinearInterpolator(autoDetectLabel, 1, 100, (v) => { Console.WriteLine("v " + v); autoDetectLabel.ScaleX = v; });
            }
            else if (!detected)
            {
                autoDetectLabel.IsVisible = false;
            }
        }

        private void toggleIconSelect(ImageButton selected, Grid selectedGrid)
        {
            foreach (IconLayout icon in selectedGrid.Children)
            {
                icon.imageButton.BorderWidth = 0;
            }
            if (selected != null)
            {
                selected.BorderWidth = 2;
                selected.BorderColor = Color.DarkGoldenrod;
            }
        }

        // First copy for unplacedGrid, second for metaGrid, third is optional for partial grid
        private void SaveInput(List<ItemLayout> newItemLayouts, List<ItemLayout> newItemLayoutsCopy, List<ItemLayout> newItemLayoutsCopy2 = null)
        {
            foreach (Item _item in newItem)
            {
                ItemLayout itemLayout = new ItemLayout(ContentManager.item_layout_size, ContentManager.item_layout_size, _item)
                    .AddMainImage()
                    .AddExpirationMark()
                    .AddTitle()
                    .AddInfoIcon();
                Console.WriteLine("AddView 443 item name " + _item.Name);
                ItemLayout itemLayoutCopy = new ItemLayout(ContentManager.item_layout_size, ContentManager.item_layout_size, _item)
                    .AddMainImage()
                    .AddExpirationMark()
                    .AddTitle()
                    .AddInfoIcon();

                ItemLayout itemLayoutCopy2 = new ItemLayout(ContentManager.item_layout_size, ContentManager.item_layout_size, _item)
                    .AddMainImage()
                    .AddExpirationMark()
                    .AddTitle()
                    .AddInfoIcon();

                newItemLayouts.Add(itemLayout);
                newItemLayoutsCopy.Add(itemLayoutCopy);
                if (newItemLayoutsCopy2 != null) newItemLayoutsCopy2.Add(itemLayoutCopy2);
                ContentManager.MetaItemBase.Add(_item.ID, itemLayout);
                ContentManager.UnplacedItemBase.Add(_item.ID, itemLayoutCopy);
            }


            GridManager.AddGridItem(GridManager.GetGrid(ContentManager.metaGridName), newItemLayouts, false);
            GridManager.AddGridItem(GridManager.GetGrid(ContentManager.unplacedGridName), newItemLayoutsCopy, false);
            Console.WriteLine("AddView 471 partial unplaced grid children count " + newItemLayoutsCopy2.Count);
        }

        public void ResetForm()
        {
            imageSelectorIndex = 0;
            selectorIndex = 0;
            presetScrollView.IsVisible = false;
            defaultSelectGrid.IsVisible = false;
            foreach (var button in imageSelector)
            {
                button.BackgroundColor = Color.Wheat;
            }
            foreach (var button in formSelector)
            {
                button.BackgroundColor = Color.BurlyWood;
                button.Text = "";
            }
            item = new Item().SetItem(-1, -1, -1, 1, "product", ContentManager.addIcon);
        }
     
    }
}
