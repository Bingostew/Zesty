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
        private static int gridWidth;
        private static int gridHeight;
        private static List<View> gridList = new List<View>();

        /// <summary>
        /// Creates a new grid with all unplaced items
        /// </summary>
        /// <param name="width"> number of columns. </param>
        /// <param name="height"> number of rows. If set to -1, row is added to fit </param>
        /// <param name="addButton"> the first child of grid, which is a button that sets the add form visiblity to true.</param>
        /// <returns></returns>
      /*
        public static void ChangePageUnplacedGrid(Grid grid, int startIndex, int gridCellAmount, string storageName)
        {
            Console.WriteLine("max " + grid.GetGridChilrenList().Count);
            gridList.Clear();
            var max = ContentManager.UnplacedItems.Count > gridCellAmount + startIndex ? gridCellAmount + startIndex : ContentManager.UnplacedItems.Count;
            var limited = true;
            if (gridCellAmount < 0)
            {
                max = grid.GetGridChilrenList().Count; limited = false;
            }

            for (int i = startIndex; i < max; i++)
            {
                AddUnplacedGrid(grid, storageName, null, grid.GetGridChilrenList()[i] as ItemLayout, limited);
            }
        }
        /*
        /// <summary>
        /// Add child to unplaced grid. If set _item to null, then itemLayout must not be null. Vice versa.
        /// </summary>
        /// <param name="grid"> the instance of the grid from InitializeNewGrid method</param>
        /// <param name="item"> the item data for the child </param>
        /// <param name="currentStorageName"> Optional: sets bounds for a rendered cabinet to activate the ScreenTouch effect </param>
        public static void AddUnplacedGrid(Grid grid, string currentStorageName, Item _item = null, ItemLayout itemLayout = null, bool limited = true)
        {
            var max = grid.RowDefinitions.Count * grid.ColumnDefinitions.Count;
            ItemLayout itemIcon;
            if (itemLayout == null)
            {
                itemIcon = new ItemLayout(60, 60, _item)
                    .AddMainImage()
                    .AddAmountMark()
                    .AddExpirationMark()
                    .AddTitle()
                    .AddInfoIcon();
            }
            else { itemIcon = itemLayout; }
            Item item = _item == null ? itemIcon.ItemData : _item;
            
            if (!ContentManager.MetaItemBase.ContainsKey(item.ID)) { ContentManager.MetaItemBase.Add(item.ID, itemIcon); }

            
            if (currentStorageName != null && currentStorageName.Length > 0)
            {

                ScreenTouch touchEvent = new ScreenTouch() { ContactViews = ContentManager.GetContactViews(currentStorageName) };
                touchEvent.OnTouchEvent += (obj, args) => OnContact(args, itemIcon, currentStorageName, item, grid);

                itemIcon.iconImage.Effects.Add(touchEvent);
            }
            gridList.Add(itemIcon);

            if (grid.Children.Count < max && limited) { grid.SetGridChildrenList(gridList); }
            else { UpdateUnplacedGrid(grid); }
        }*/
        
        static void OnContact(TouchActionEventArgs args, ItemLayout itemIcon, string currentStorageName, Item item, Grid grid)
        {
            var itemBase = ContentManager.GetItemBase();
            itemIcon.TranslationY += args.Location.Y; itemIcon.TranslationX += args.Location.X;
            if (args.Type == TouchActionEventArgs.TouchActionType.Released)
            {
                itemIcon.TranslationX = 0;
                itemIcon.TranslationY = 0;
            }

            if (args.IsInContact)
            {
                foreach (var value in itemBase[currentStorageName].Values)
                {
                    foreach (ImageButton button in value.Keys)
                    {
                        button.RemoveEffect(typeof(ImageTint));
                    }
                }
                args.ContactView[0].ToggleEffects(new ImageTint() { tint = Color.FromRgba(100, 30, 30, 70) }, null);
                if (args.Type == TouchActionEventArgs.TouchActionType.Released)
                {
                    itemIcon.iconImage.RemoveEffect(typeof(ScreenTouch));
                    args.ContactView[0].RemoveEffect(typeof(ImageTint));
                    itemBase[currentStorageName][args.ContactIndex[0]][args.ContactView[0] as ImageButton].Insert(0, itemIcon);
                    itemIcon.TranslationX += args.ContactView[0].TranslationX;
                    itemIcon.BindCabinetInfo(args.ContactView[0].TranslationX, args.ContactIndex[0], args.ContactView[0] as ImageButton, currentStorageName, ContentManager.GetStorageView);
                    CabinetAddPage.UpdateShelf(currentStorageName, itemIcon, args.ContactIndex[0], item); 
                    GridManager.RemoveGridItem(grid, itemIcon);
                }
            }
            else if(args.ContactView != null) { args.ContactView[0].ToggleEffects(new ImageTint(), null); }

        }
        public static void UpdateDragDropBounds(Grid grid, string storageName)
        {
            foreach (View child in grid.Children)
            {
                if (child.GetType() == typeof(ItemLayout)) {
                    Console.WriteLine("AddView 119: touch check 1");
                    var tryEffect = child.GetEffect(typeof(ScreenTouch)) as ScreenTouch;
                    if (tryEffect != null)
                    {
                        tryEffect.ContactViews = ContentManager.GetContactViews(storageName);
                    }
                    else
                    {
                        Console.WriteLine("AddView 127: touch effect added");
                        ScreenTouch touchEvent = new ScreenTouch() { ContactViews =ContentManager.GetContactViews(storageName) };
                        touchEvent.OnTouchEvent += (obj, args) => OnContact(args, child as ItemLayout, storageName, (child as ItemLayout).ItemData, grid);
                        (child as ItemLayout).iconImage.Effects.Add(touchEvent);
                    }
                }
            }
        }

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
            //Grid.IGridList<IconLayout> currentGridChildren = (Grid.IGridList <IconLayout> )currentGrid.Children;

            Vector2D<int> selectGridIndex = new Vector2D<int>(0, 6);

            Grid expirationGrid = GridManager.InitializeGrid("AddExpirationGrid", 2, 3);

            Grid iconGrid = GridManager.InitializeGrid("AddIconGrid", 2, 3);

            List<IconLayout> presetResult = new List<IconLayout>();
            List<int> presetResultSorter = new List<int>();
            Grid presetSelectGrid = GridManager.InitializeGrid("AddPresetGrid", 2, 3);

            Grid defaultSelectGrid = GridManager.InitializeGrid("AddDefaultGrid", 2, 3);

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
                RowDefinitions =
                {
                    new RowDefinition(){ Height = 40 },
                    new RowDefinition(){ Height = 40 },
                    new RowDefinition(){ Height = 40 },
                    new RowDefinition(){ Height = 30 },
                    new RowDefinition(){Height = GridLength.Star }
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
            var dateMonth = new Button() { BorderColor = Color.Black, BorderWidth = 2 };
            dateMonth.Clicked += (obj, arg) => { selectorIndex = 0; ClearText(dateMonth); };
            var dateDay = new Button() { BorderColor = Color.Black, BorderWidth = 2 };
            dateDay.Clicked += (obj, arg) => { selectorIndex = 1; ClearText(dateDay); };
            var dateYear = new Button() { BorderColor = Color.Black, BorderWidth = 2 };
            dateYear.Clicked += (obj, arg) => { selectorIndex = 2; ClearText(dateYear); };
            var amountInput = new Button() { BorderColor = Color.Black, BorderWidth = 2 };
            amountInput.Clicked += (obj, arg) => { selectorIndex = 3; ClearText(amountInput); };
            var lastPageButton = new ImageButton() { Source = ContentManager.countIcon, Rotation = 180, Aspect = Aspect.Fill, WidthRequest = 50 };
            lastPageButton.Clicked += (obj, args) =>
            {
                selectGridIndex = new Vector2D<int>(selectGridIndex.X - 6, selectGridIndex.Y - 6);
                GridManager.ConstrainGrid(presetSelectGrid, selectGridIndex.X, selectGridIndex.Y, false); 

               // var index = currentGridChildren.IndexOf(currentGrid.Children[0] as IconLayout);
               // if (index > 6) nextPresetPage(currentGridChildren.IndexOf(currentGrid.Children[0] as IconLayout) - 6);
               // else nextPresetPage(0);
            };
            var nextPageButton = new ImageButton() { Source = ContentManager.countIcon, Aspect = Aspect.Fill, WidthRequest = 50, TranslationX = 50 };
            nextPageButton.Clicked += (obj, args) =>
            {
                selectGridIndex = new Vector2D<int>(selectGridIndex.X + 6, selectGridIndex.Y + 6);
                GridManager.ConstrainGrid(presetSelectGrid, selectGridIndex.X, selectGridIndex.Y, false);
                //if (currentGrid.Children.Count >= 6) nextPresetPage(currentGridChildren.IndexOf(currentGrid.Children.Last() as IconLayout) + 1);
            };
            var pageToolGrid = new Grid() { ColumnDefinitions = { new ColumnDefinition() { Width = 50 }, new ColumnDefinition() { Width = 50 } } };
            pageToolGrid.Children.Add(lastPageButton, 0, 0); pageToolGrid.Children.Add(nextPageButton, 1, 0);
            var iconSelect1 = new Button() { HeightRequest = 10, CornerRadius = 2, BorderColor = Color.Wheat, BorderWidth = 3 };
            iconSelect1.Clicked += (obj, arg) =>
            {
                imageSelectorIndex = 0; toggleSelect(0, imageSelector, Color.Black, Color.Wheat);
                presetSelectGrid.IsVisible = true;
                defaultSelectGrid.IsVisible = false;
                currentGrid = presetSelectGrid;
               // currentGridChildren = presetResult;
                presetSelectGrid.Children.Clear();
                changeSelectedIcon();
            };
            var iconSelect2 = new Button() { HeightRequest = 10, CornerRadius = 2, BorderColor = Color.Wheat, BorderWidth = 3 };
            iconSelect2.Clicked += (obj, arg) =>
            {
                imageSelectorIndex = 1; toggleSelect(1, imageSelector, Color.Black, Color.Wheat);
                presetSelectGrid.IsVisible = false;
                defaultSelectGrid.IsVisible = true;
                //currentGridChildren = defaultSelectGrid.Children;
                currentGrid = defaultSelectGrid;
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
                            toggleIconSelect(button, presetSelectGrid); var _name = name;
                            item.icon = ContentManager.PresetIcons[_name].GetImageSource();
                        };
                        match = 0;
                    }
                }
                toggleIconSelect(null, presetSelectGrid);
                ListSorter.SortToListAscending(presetResultSorter, presetResult);
                nextPresetPage(0);
            }

            void nextPresetPage(int currentAmount)
            {
                presetSelectGrid.Children.Clear();
                //var currentAmount = presetResult.IndexOf(presetSelectGrid.Children.Last() as IconLayout);
                List<View> results = new List<View>();
                var max = 6 + currentAmount < presetResult.Count ? 6 + currentAmount : presetResult.Count;
                for (int i = currentAmount; i < max; i++)
                {
                    results.Add(presetResult[i]);
                }
                presetSelectGrid.OrganizeGrid(results, GridOrganizer.OrganizeMode.HorizontalLeft);
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
                ContentManager.UnplacedItems.Add(item);
                imageSelectorIndex = 0;
                selectorIndex = 0;
                nameInput.Text = "product";
                defaultSelectGrid.IsVisible = false;
                presetSelectGrid.IsVisible = false;
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
            Grid.SetRowSpan(presetSelectGrid, 3);
            Grid.SetRowSpan(defaultSelectGrid, 3);
            form.Children.Add(nameLabel, 0, 0);
            form.Children.Add(nameInput, 1, 0);
            form.Children.Add(dateLabel, 0, 1);
            form.Children.Add(expirationGrid, 1, 1);
            form.Children.Add(amountLabel, 0, 2);
            form.Children.Add(amountInput, 1, 2);
            form.Children.Add(iconLabel, 0, 3);
            form.Children.Add(iconGrid, 0, 4);
            form.Children.Add(pageToolGrid, 1, 3);
            form.Children.Add(presetSelectGrid, 1, 4);
            form.Children.Add(defaultSelectGrid, 1, 4);
            Grid.SetColumnSpan(iconGrid, 2);


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

            var nextButton = new Button() { IsVisible = false };
            var zeroButton = new Button() { Text = "0" };
            zeroButton.Clicked += (obj, arg) => changeText("0");
            var deleteButton = new Button() { IsVisible = false };

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
                item.SetDaysUntilExpiration();
                newItem.Add(item);
                List<ItemLayout> newItemLayouts = new List<ItemLayout>();
                foreach (Item _item in newItem)
                {
                    ItemLayout itemLayout = new ItemLayout(60, 60, _item)
                        .AddMainImage()
                        .AddAmountMark()
                        .AddExpirationMark()
                        .AddTitle()
                        .AddInfoIcon();

                    newItemLayouts.Add(itemLayout);
                    ContentManager.MetaItemBase.Add(_item.ID, itemLayout);
                }
                GridManager.AddGridItem(GridManager.GetGrid(ContentManager.unplacedGridName), newItemLayouts, false);
                if (partialUnplacedGrid != null)
                    GridManager.AddGridItem(partialUnplacedGrid, newItemLayouts, false);
                baseUnplacedEvent?.Invoke(item);
                localUnplacedEvent?.Invoke(item);
                resetForm();
                newItem.Clear();
                layout.IsVisible = false;
            };

            return layout;
        }
 
    }
}
