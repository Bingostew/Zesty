using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Utility;
using System.Linq;

namespace ZestyKitchenHelper
{
    public class AddView
    {
        static List<Item> newItem = new List<Item>();
        const int addFormTextFontSize = 20;

        /// <summary>
        /// Initialized an instance of add form with a num pad.
        /// </summary>
        ///
        /// <returns></returns>
        public static AbsoluteLayout GetAddForm(Action<Item> localUnplacedEvent, Action<Item> baseUnplacedEvent,
            string storageName = "", bool limited = true, Grid partialUnplacedGrid = null)
        {
            List<Button> formSelector = new List<Button>();
            List<Button> imageSelector = new List<Button>();
            int imageSelectorIndex = 0;
            int selectorIndex = 0;
            Item item = new Item().SetItem(2021, 1, 1, 1, "product", ContentManager.addIcon);
            Grid currentGrid = new Grid();

            Vector2D<int> selectGridIndex = new Vector2D<int>(0, 5);

            Grid expirationGrid = GridManager.InitializeGrid("AddExpirationGrid", 2, 3, GridLength.Auto, GridLength.Star);

            Grid iconGrid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition(){ Height = GridLength.Star },
                    new RowDefinition(){ Height = GridLength.Star }
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(){ Width = GridLength.Auto },
                    new ColumnDefinition(){ Width = GridLength.Star }
                }
            };

            List<IconLayout> presetResult = new List<IconLayout>();
            List<int> presetResultSorter = new List<int>();
            Grid partialPresetSelectGrid = GridManager.InitializeGrid("Partial Preset Grid", 2, 3, GridLength.Star, GridLength.Star);
            Grid defaultSelectGrid = GridManager.InitializeGrid("Add Default Grid", 2, 3, GridLength.Star, GridLength.Star);
            Grid partialDefaultSelectGrid = GridManager.InitializeGrid("Partial Default Grid", 2, 3, GridLength.Star, GridLength.Star);

            foreach (var name in ContentManager.DefaultIcons.Keys)
            {
                ContentManager.DefaultIcons[name].OnClickIconAction = (imageButton) =>
                {
                    toggleIconSelect(imageButton, defaultSelectGrid); item.icon = ContentManager.DefaultIcons[name].GetImageSource();
                };
            }
            //GridManager.AddGridItem(defaultSelectGrid, ContentManager.DefaultIcons.Values , true);

            var autoDetectLabel = new Label() { FontSize = 15, TextColor = Color.Black, BackgroundColor = Color.White, IsVisible = false, AnchorX = 0 };
            Entry nameInput = new Entry() { Text = "product" };
            nameInput.TextChanged += (obj, args) => { item.name = args.NewTextValue; autoDetectExpiration(args.NewTextValue); if (imageSelectorIndex == 0) changeSelectedIcon(); };
            nameInput.Completed += (obj, args) => { item.name = nameInput.Text; if (imageSelectorIndex == 0) changeSelectedIcon(); };
            var foregroundTint = new Image() { BackgroundColor = Color.FromRgba(0, 0, 0, 98), HeightRequest = Application.Current.MainPage.Height, WidthRequest = Application.Current.MainPage.Width };

            Grid form = new Grid()
            {
                BackgroundColor = Color.FromRgb(200, 200, 200),
                RowSpacing = 5,
                ColumnSpacing = 5,
                RowDefinitions =
                {
                    new RowDefinition(){ Height = GridLength.Star },
                    new RowDefinition(){ Height = GridLength.Star },
                    new RowDefinition(){ Height = GridLength.Star },
                    new RowDefinition(){ Height = GridLength.Star },
                    new RowDefinition(){Height = new GridLength(2, GridUnitType.Star) }
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

            var nameLabel = new Label() { Text = "Item Name: ", FontSize = addFormTextFontSize, TextColor = Color.Black };
            var dateLabel = new Label() { Text = "Exp. Date: ", FontSize = addFormTextFontSize, TextColor = Color.Black };
            var amountLabel = new Label() { Text = "Amount: ", FontSize = addFormTextFontSize, TextColor = Color.Black };
            var iconLabel = new Label() { Text = "Icon: ", FontSize = addFormTextFontSize, TextColor = Color.Black };
            var dateMonth = new Button() { BorderColor = Color.Black, BorderWidth = 2, BackgroundColor = Color.Transparent };
            dateMonth.Clicked += (obj, arg) => { selectorIndex = 0; ClearText(dateMonth); };
            var dateDay = new Button() { BorderColor = Color.Black, BorderWidth = 2, BackgroundColor = Color.Transparent, };
            dateDay.Clicked += (obj, arg) => { selectorIndex = 1; ClearText(dateDay); };
            var dateYear = new Button() { BorderColor = Color.Black, BorderWidth = 2 };
            dateYear.Clicked += (obj, arg) => { selectorIndex = 2; ClearText(dateYear); };
            var amountInput = new Button() { BorderColor = Color.Black, BorderWidth = 2, BackgroundColor = Color.Transparent, };
            amountInput.Clicked += (obj, arg) => { selectorIndex = 3; ClearText(amountInput); };
            var lastPageButton = new ImageButton() { Source = ContentManager.countIcon, Rotation = 180, 
                BackgroundColor = Color.Transparent, Aspect = Aspect.Fill, WidthRequest = 50 };
            lastPageButton.Clicked += (obj, args) =>
            {
                selectGridIndex = selectGridIndex.X < 6 ? new Vector2D<int>(0, 5)
                    : new Vector2D<int>(selectGridIndex.X - 6, selectGridIndex.Y - 6);
                Console.WriteLine("AddView 113: starting index: " + selectGridIndex.X + " end index: " + selectGridIndex.Y);
                GridManager.ConstrainGrid(presetResult, selectGridIndex.X, selectGridIndex.Y, partialPresetSelectGrid, null, true);
            };
            var nextPageButton = new ImageButton() { Source = ContentManager.countIcon, Aspect = Aspect.Fill, 
                BackgroundColor = Color.Transparent, WidthRequest = 50, TranslationX = 50 };
            nextPageButton.Clicked += (obj, args) =>
            {
                Console.WriteLine("AddView 121: starting index: " + selectGridIndex.X + " end index: " + selectGridIndex.Y);
                selectGridIndex = selectGridIndex.Y >= presetResult.Count - 6 ? new Vector2D<int>(presetResult.Count - 6, presetResult.Count - 1)
                    : new Vector2D<int>(selectGridIndex.X + 6, selectGridIndex.Y + 6);
                Console.WriteLine("AddView 124: starting index: " + selectGridIndex.X + " end index: " + selectGridIndex.Y);
                GridManager.ConstrainGrid(presetResult, selectGridIndex.X, selectGridIndex.Y, partialPresetSelectGrid, null, true);
            };
            var pageToolGrid = new Grid() { ColumnDefinitions = { new ColumnDefinition() { Width = 50 }, new ColumnDefinition() { Width = 50 } } };
            pageToolGrid.Children.Add(lastPageButton, 0, 0); pageToolGrid.Children.Add(nextPageButton, 1, 0);
            var iconSelect1 = new Button() { WidthRequest = 30, CornerRadius = 2, BorderColor = Color.Wheat, BorderWidth = 3 };
            iconSelect1.Clicked += (obj, arg) =>
            {
                imageSelectorIndex = 0; toggleSelect(0, imageSelector, Color.Black, Color.Wheat);
                partialPresetSelectGrid.IsVisible = true;
                partialDefaultSelectGrid.IsVisible = false;
                currentGrid = partialPresetSelectGrid;
                partialPresetSelectGrid.Children.Clear();
                changeSelectedIcon();
            };
            var iconSelect2 = new Button() { WidthRequest = 30, CornerRadius = 2, BorderColor = Color.Wheat, BorderWidth = 3 };
            iconSelect2.Clicked += (obj, arg) =>
            {
                imageSelectorIndex = 1; toggleSelect(1, imageSelector, Color.Black, Color.Wheat);
                partialPresetSelectGrid.IsVisible = false;
                partialDefaultSelectGrid.IsVisible = true;
                currentGrid = partialDefaultSelectGrid;
            };

            var iconLabel1 = new Label() { Text = "In-App Icons", TextColor = Color.Black, FontSize = 15, VerticalTextAlignment = TextAlignment.Center };
            var iconLabel2 = new Label() { Text = "General Icons", TextColor = Color.Black, FontSize = 15, VerticalTextAlignment = TextAlignment.Center };

            void changeSelectedIcon()
            {
                presetResult.Clear(); presetResultSorter.Clear();
                foreach (string name in ContentManager.PresetIcons.Keys)
                {
                    int match = 0;

                    foreach (char n in name.ToCharArray())
                    {
                        foreach (char i in item.name.ToCharArray())
                        {
                            if (n == i || char.ToLower(n) == i || char.ToLower(i) == n)
                            {
                                match--;
                            }
                        }
                    }
                    if (match != 0 || item.name == "" || item.name == "product")
                    {
                        presetResultSorter.Add(match);
                        presetResult.Add(ContentManager.PresetIcons[name]);
                        ContentManager.PresetIcons[name].OnClickIconAction += (button) =>
                        {
                            toggleIconSelect(button, partialPresetSelectGrid); var _name = name;
                            item.icon = ContentManager.PresetIcons[_name].GetImageSource();
                        };
                        match = 0;
                    }
                }
                toggleIconSelect(null, partialPresetSelectGrid);
                ListSorter.SortToListAscending(presetResultSorter, presetResult);
                GridManager.ConstrainGrid(presetResult, 0, 5, partialPresetSelectGrid, null);
            }

            async void autoDetectExpiration(string name)
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

            void toggleIconSelect(ImageButton selected, Grid selectedGrid)
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

            void toggleSelect(int index, List<Button> buttonList, Color colorTo, Color normal)
            {
                foreach (var button in buttonList)
                {
                    button.BackgroundColor = normal;
                }
                buttonList[index].BackgroundColor = colorTo;
            }

            void resetForm()
            {
                imageSelectorIndex = 0;
                selectorIndex = 0;
                nameInput.Text = "product";
                defaultSelectGrid.IsVisible = false;
                partialPresetSelectGrid.IsVisible = false;
                foreach (var button in imageSelector)
                {
                    button.BackgroundColor = Color.Wheat;
                }
                foreach (var button in formSelector)
                {
                    button.BackgroundColor = Color.BurlyWood;
                    button.Text = "";
                }
                item = new Item().SetItem(2021, 1, 1, 1, "product", ContentManager.addIcon);
            }

            formSelector.Add(dateMonth);
            formSelector.Add(dateDay);
            formSelector.Add(dateYear);
            formSelector.Add(amountInput);
            imageSelector.Add(iconSelect1);
            imageSelector.Add(iconSelect2);

            expirationGrid.Children.Add(dateMonth, 0, 0);
            expirationGrid.Children.Add(dateDay, 1, 0);
            expirationGrid.Children.Add(dateYear, 2, 0);
            iconGrid.Children.Add(iconSelect1, 0, 0);
            iconGrid.Children.Add(iconSelect2, 0, 1);
            iconGrid.Children.Add(iconLabel1, 1, 0);
            iconGrid.Children.Add(iconLabel2, 1, 1);
            Grid.SetRowSpan(partialPresetSelectGrid, 3);
            Grid.SetRowSpan(partialDefaultSelectGrid, 3);
            form.Children.Add(nameLabel, 0, 0);
            form.Children.Add(nameInput, 1, 0);
            form.Children.Add(dateLabel, 0, 1);
            form.Children.Add(expirationGrid, 1, 1);
            form.Children.Add(amountLabel, 0, 2);
            form.Children.Add(amountInput, 1, 2);
            form.Children.Add(iconLabel, 0, 3);
            form.Children.Add(iconGrid, 0, 4);
            form.Children.Add(pageToolGrid, 1, 3);
            form.Children.Add(partialPresetSelectGrid, 1, 4);
            form.Children.Add(partialDefaultSelectGrid, 1, 4);


            Grid numPadGrid = new Grid()
            {
                RowSpacing = 0,
                ColumnSpacing = 0,
                RowDefinitions =
                {
                    new RowDefinition(),
                    new RowDefinition(),
                    new RowDefinition(),
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };

            void setItem(int newText, int index)
            {
                switch (selectorIndex)
                {
                    case 0: item.expMonth = newText; break;
                    case 1: item.expDay = newText; break;
                    case 2: item.expYear = newText; break;
                    case 3: item.amount = newText; break;
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

            List<View> numPadList = new List<View>();
            for (int i = 1; i < 10; i++)
            {
                var button = new Button() { Text = i.ToString(), Margin = new Thickness(0) };
                int index = i;
                button.Clicked += (obj, arg) => changeText(index.ToString());
                numPadList.Add(button);
            }

            var nextButton = new Button() { IsVisible = false, BackgroundColor = Color.Transparent, };
            var zeroButton = new Button() { Text = "0"};
            zeroButton.Clicked += (obj, arg) => changeText("0");
            var deleteButton = new Button() { IsVisible = false, BackgroundColor = Color.Transparent, };

            numPadList.Add(nextButton);
            numPadList.Add(zeroButton);
            numPadList.Add(deleteButton);
            
            numPadGrid.OrganizeGrid(numPadList, GridOrganizer.OrganizeMode.HorizontalLeft);

            var saveButton = new Button() { Text = "Done", BackgroundColor = Color.Blue };
            var newFormButton = new Button() { BackgroundColor = Color.ForestGreen, Text = "Add" };

            AbsoluteLayout layout = new AbsoluteLayout()
            {
                HorizontalOptions = LayoutOptions.Center,
                HeightRequest = Application.Current.MainPage.Height,
                WidthRequest = Application.Current.MainPage.Width,
                IsVisible = false,
                Children =
                {
                    foregroundTint,
                    form,
                    autoDetectLabel,
                    numPadGrid,
                    newFormButton,
                    saveButton
                }
            };
           
            async void OnNewItemAdded(object obj, EventArgs args)
            {
                var animatedImage = new Image() { Source = item.icon.Substring(6), WidthRequest = 50, HeightRequest = 50, Aspect = Aspect.Fill };
                layout.Children.Add(animatedImage);
                AbsoluteLayout.SetLayoutBounds(animatedImage, AbsoluteLayout.GetLayoutBounds(newFormButton));
                AbsoluteLayout.SetLayoutFlags(animatedImage, AbsoluteLayoutFlags.All);
                await animatedImage.QuadraticFlight(15, 75, -80, 30, (v) => { animatedImage.TranslationX = -v.X; animatedImage.TranslationY = -v.Y; }, 1500, easing: Easing.Linear);
            }

            newFormButton.Clicked += OnNewItemAdded;
            newFormButton.Clicked += (obj, args) => 
            {  item.SetDaysUntilExpiration(); newItem.Add(item); baseUnplacedEvent?.Invoke(item); localUnplacedEvent?.Invoke(item);  resetForm(); };

            AbsoluteLayout.SetLayoutBounds(autoDetectLabel, new Rectangle(0, 0, 1, .06));
            AbsoluteLayout.SetLayoutFlags(autoDetectLabel, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(form, new Rectangle(.5, .1, .9, .4));
            AbsoluteLayout.SetLayoutFlags(form, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(numPadGrid, new Rectangle(.5, .95, .9, .5));
            AbsoluteLayout.SetLayoutFlags(numPadGrid, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(newFormButton, new Rectangle(.5, .9, .6, .06));
            AbsoluteLayout.SetLayoutFlags(newFormButton, AbsoluteLayoutFlags.All);
            AbsoluteLayout.SetLayoutBounds(saveButton, new Rectangle(.5, .97, .6, .06));
            AbsoluteLayout.SetLayoutFlags(saveButton, AbsoluteLayoutFlags.All);

            saveButton.Clicked += (obj, args) =>
            {
                List<ItemLayout> newItemLayouts = new List<ItemLayout>();
                List<ItemLayout> newItemLayoutsCopy = new List<ItemLayout>();

                SaveInput(item, newItemLayouts, newItemLayoutsCopy);

                if (partialUnplacedGrid != null)
                    GridManager.AddGridItem(partialUnplacedGrid, newItemLayoutsCopy, false);
                baseUnplacedEvent?.Invoke(item);
                localUnplacedEvent?.Invoke(item);
                resetForm();
                newItem.Clear();
                layout.IsVisible = false;
            };

            return layout;
        }
 

        private static void SaveInput(Item item, List<ItemLayout> newItemLayouts, List<ItemLayout> newItemLayoutsCopy)
        {
            item.SetDaysUntilExpiration();
            newItem.Add(item);

            foreach (Item _item in newItem)
            {
                ItemLayout itemLayout = new ItemLayout(100,100, _item)
                    .AddMainImage()
                    .AddAmountMark()
                    .AddExpirationMark()
                    .AddTitle()
                    .AddInfoIcon();

                ItemLayout itemLayoutCopy = new ItemLayout(100, 100, _item)
                    .AddMainImage()
                    .AddAmountMark()
                    .AddExpirationMark()
                    .AddTitle()
                    .AddInfoIcon();

                newItemLayouts.Add(itemLayout);
                newItemLayoutsCopy.Add(itemLayoutCopy);
                ContentManager.MetaItemBase.Add(_item.ID, itemLayout);
                ContentManager.UnplacedItemBase.Add(_item.ID, itemLayoutCopy);
            }
            Console.WriteLine("AddView 564: new item length = " + newItemLayouts.Count);


            GridManager.AddGridItem(GridManager.GetGrid(ContentManager.unplacedGridName), newItemLayoutsCopy, false);
            GridManager.AddGridItem(GridManager.GetGrid(ContentManager.metaGridName), newItemLayouts, false);       
        }
    }
}
