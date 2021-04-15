using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Utility;
using Xamarin.Forms.Internals;


namespace ZestyKitchenHelper
{
    public abstract class EditPage : ContentPage
    {
        protected Dictionary<ImageButton, Image> dividerHelper = new Dictionary<ImageButton, Image>();

        protected ImageSource cabinetCellImage = ContentManager.cabinetIcon;
        protected ImageSource cabinetDividerLeft = "cabinet_divider_left.png";
        protected ImageSource cabinetDividerMiddle = "cabinet_divider_middle.png";
        protected ImageSource cabinetDividerRight = "cabinet_divider_right.png";
        protected ImageSource subdivideIcon = "subdivide.png";
        protected ImageSource mergeIcon = "merge.png";
        protected ImageSource countArrow = "small_arrow.png";

        protected ImageButton addRowButton = new ImageButton() { Source = ContentManager.addItemIcon, WidthRequest = 50, HeightRequest = 50, HorizontalOptions = LayoutOptions.CenterAndExpand };
        protected Grid assetGrid;

        protected StackLayout pageContent = new StackLayout() { BackgroundColor = Color.Wheat };
        protected AbsoluteLayout cellContainer = new AbsoluteLayout() { HorizontalOptions = LayoutOptions.Center};
        protected Dictionary<int, AbsoluteLayout> preSaveState = new Dictionary<int, AbsoluteLayout>();
        protected Action<string, string, string> storageSaveLocalEvent, storageSaveBaseEvent;
        protected int selectedCellIndex = -1;
        protected ImageButton selectedButton;
        protected int subdivideAmount = 2;
        protected string nameLegacy;
        protected string name;
        protected double screenWidth = Application.Current.MainPage.Width;
        protected double screenHeight = Application.Current.MainPage.Width;

        protected double initialItemX = 0;
        protected int indexer = 0;

        protected const double bufferWidth = 15;
        protected const string outlineTag = "outline";
        protected const string movedTag = "move";
        protected abstract string cellImageSource { get; }
        public EditPage(bool newShelf, string storageName = "")
        {
            name = storageName;
            nameLegacy = storageName;
            var nameEntry = new Entry() { Text = "untitled" };
            nameEntry.TextChanged += (obj, args) => name = args.NewTextValue;
            nameEntry.Completed += (obj, arg) => {
                StoreCabinetInfo();
            };


            SetNewShelf(newShelf);

            var saveGrid = new Grid()
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand,
                ColumnDefinitions =
                {
                    new ColumnDefinition(){ Width = 80 },
                    new ColumnDefinition(){ Width = 80 },
                    new ColumnDefinition(){Width = 80 }
                }
            };

            var saveButton = new Button() { Text = "Save", BackgroundColor = Color.Blue };
            var cancelButton = new Button() { Text = "Exit", BackgroundColor = Color.Red };
            saveButton.Clicked += (obj, args) => ConfirmationSaveEvent(ContentManager.pageController.ToSingleSelectionPage);
            cancelButton.Clicked += (obj, args) => ConfirmationCancelEvent(ContentManager.pageController.ToSingleSelectionPage);

            saveGrid.Children.Add(saveButton, 0, 0);
            saveGrid.Children.Add(cancelButton, 2, 0);

            pageContent.Children.Add(nameEntry);

            if (!newShelf) 
            {
                preSaveState = ContentManager.GetInfoBase()[nameLegacy];
                var itemMaxIndex = ContentManager.cabinetItemBase[nameLegacy].Keys.Any() ?  ContentManager.cabinetItemBase[nameLegacy].Keys.Max() : 0;
                var infoMaxIndex = ContentManager.cabinetInfo[nameLegacy].Keys.Any() ? ContentManager.cabinetInfo[nameLegacy].Keys.Max() : 0;
                while (indexer <= itemMaxIndex || indexer <= infoMaxIndex) { indexer++; }
            }
            SetBasicView(newShelf);

            pageContent.Children.Add(saveGrid);

            Content = pageContent;
        }

        protected abstract void SetNewShelf(bool newShelf);
        
        /*
        private void SetAdvancedView()
        {
            int currentIndex = 0;
            Dictionary<int, List<ImageButton>> contactViews = new Dictionary<int, List<ImageButton>>();

            assetGrid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition(){Height = 50},
                    new RowDefinition(){Height = 20}
                },
                ColumnDefinitions =
                {
                     new ColumnDefinition(),
                     new ColumnDefinition(),
                     new ColumnDefinition(),
                     new ColumnDefinition()
                }
            };


            int selectedIndex = 0;
            AbsoluteLayout selectedView = null;
            List<ImageButton> selectedInitiators = null;
            View selectedControl = null;

            var cellAsset = new ImageButton() { Source = ContentManager.addIcon, Aspect = Aspect.Fill };
            var cellLabel = new Label() { Text = "Insert Cell", HorizontalTextAlignment = TextAlignment.Center };
            var deleteButton = new ImageButton() { Source = ContentManager.addIcon, Aspect = Aspect.Fill };
            var deleteLabel = new Label() { Text = "Delete", HorizontalTextAlignment = TextAlignment.Center };
            var moveButton = new ImageButton() { Source = ContentManager.addIcon, Aspect = Aspect.Fill };
            var moveLabel = new Label() { Text = "Move", HorizontalTextAlignment = TextAlignment.Center };
            var transformButton = new ImageButton() { Source = ContentManager.addIcon, Aspect = Aspect.Fill };
            var transformLabel = new Label() { Text = "Transform", HorizontalTextAlignment = TextAlignment.Center };
            assetGrid.OrganizeGrid(new List<View>()
                { cellAsset, cellLabel, deleteButton, deleteLabel, moveButton, moveLabel, transformButton, transformLabel }, GridOrganizer.OrganizeMode.TwoRowSpanLeft);
            initialItemX = cellAsset.GetAbsolutePosition().X;

            var cabinetBackground = new ImageButton() { Source = ContentManager.cabinetIcon, Aspect = Aspect.Fill };
            AbsoluteLayout.SetLayoutBounds(cabinetBackground, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(cabinetBackground, AbsoluteLayoutFlags.All);

            var cabinetContainer = new AbsoluteLayout()
            {
                WidthRequest = screenWidth,
                BackgroundColor = Color.Wheat,
                HeightRequest = 7 * screenHeight / 10,
                Children =
                {
                    cabinetBackground
                }
            };

            cellAsset.Clicked += (obj, args) =>
            {
                var imageInstance = new ImageButton() { Source = ContentManager.cabinetIcon, Aspect = Aspect.Fill };
                var button = new ImageButton() { Source = ContentManager.transIcon };
                button.SetElementTag(movedTag);
                var outline = new Button() { BorderColor = Color.Blue, BorderWidth = 3, IsVisible = false };
                outline.SetElementTag(outlineTag);
                var container = new AbsoluteLayout()
                {
                    WidthRequest = 100,
                    HeightRequest = 100,
                };
                container.Children.Add(imageInstance, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                var contactViewInstance = new Dictionary<int, List<ImageButton>>(contactViews);
                var contactInitiators = new List<ImageButton>();
                AddBuffer(container, indexer, contactInitiators);
                container.Children.Add(outline, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                container.Children.Add(button);
                ContentManager.cabinetInfo[nameLegacy].Add(indexer, container);
                ContentManager.cabinetItemBase[nameLegacy].Add(indexer, new Dictionary<ImageButton, List<ItemLayout>>() { { button, new List<ItemLayout>() } });
                var index = indexer;
                button.Clicked += (objct, argument) =>
                {
                    foreach (var outlineContainer in ContentManager.cabinetInfo[nameLegacy].Values)
                    {
                        outlineContainer.Children.FirstOrDefault(e => e.GetElementTag() == outlineTag).IsVisible = false;
                        outlineContainer.Children.FirstOrDefault(e => e.GetElementTag() == movedTag).RemoveEffect(typeof(ImageTint));
                    }
                    button.AddEffect(new ImageTint() { tint = Color.FromRgba(100, 50, 50, 50) });
                    if (selectedView != container) { selectedView = container; selectedInitiators = contactInitiators; selectedControl = button; selectedIndex = index; }
                    outline.IsVisible = true;
                };
                indexer++;


                ScreenTouch touchEvent = new ScreenTouch() { Capture = true, ContactViews = contactViewInstance, ContactÍnitiators = contactInitiators };
                touchEvent.OnTouchEvent += (objct, arguments) => OnTouch(objct, arguments, container, button);
                button.Effects.Add(touchEvent);
                cabinetContainer.Children.Add(container, new Rectangle(0, 0, container.WidthRequest / cabinetContainer.Width, container.HeightRequest / cabinetContainer.Height), AbsoluteLayoutFlags.All);
            };
            moveButton.Clicked += (obj, args) =>
            {
                if (selectedView != null)
                {
                    Console.WriteLine("select " + selectedIndex);
                    var contactViewsInstance = new Dictionary<int, List<ImageButton>>(contactViews);
                    contactViewsInstance.Remove(selectedIndex);
                    ScreenTouch touchEvent = new ScreenTouch() { Capture = true, ContactViews = contactViewsInstance, ContactÍnitiators = selectedInitiators };
                    touchEvent.OnTouchEvent += (objct, arguments) => OnTouch(objct, arguments, selectedView, selectedControl);
                    selectedControl.AddEffect(touchEvent);
                }
            };
            deleteButton.Clicked += (obj, args) =>
            {
                if (selectedView != null)
                {
                    ContentManager.cabinetItemBase[name].Remove(selectedIndex);
                    cabinetContainer.Children.Remove(selectedView);
                }
            };
            transformButton.Clicked += (obj, args) =>
            {
                if (selectedView != null)
                {
                    selectedView.Children.Add(GetTransformationOverlay(selectedView, cabinetContainer), new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                }
            };

            void AddBuffer(Layout<View> parent, int index, List<ImageButton> bufferList = null)
            {
                var bufferRectLeft = new ImageButton() { BackgroundColor = Color.Red };
                bufferRectLeft.SetElementTag("left");
                var bufferRectTop = new ImageButton() { BackgroundColor = Color.Red };
                bufferRectTop.SetElementTag("top");
                var bufferRectRight = new ImageButton() { BackgroundColor = Color.Red };
                bufferRectRight.SetElementTag("right");
                var bufferRectBottom = new ImageButton() { BackgroundColor = Color.Red };
                bufferRectBottom.SetElementTag("bottom");
                var parentWidth = parent.WidthRequest;
                var parentHeight = parent.HeightRequest;
                var bufferWidthProportionalX = bufferWidth / parentWidth;
                var bufferWidthProportionalY = bufferWidth / parentHeight;
                AbsoluteLayout.SetLayoutBounds(bufferRectLeft, new Rectangle(0, .5, bufferWidthProportionalX, (parentHeight - bufferWidth * 2) / parentHeight));
                AbsoluteLayout.SetLayoutFlags(bufferRectLeft, AbsoluteLayoutFlags.All);
                AbsoluteLayout.SetLayoutBounds(bufferRectRight, new Rectangle(1, .5, bufferWidthProportionalX, (parent.HeightRequest - bufferWidth * 2) / parentHeight));
                AbsoluteLayout.SetLayoutFlags(bufferRectRight, AbsoluteLayoutFlags.All);
                AbsoluteLayout.SetLayoutBounds(bufferRectTop, new Rectangle(.5, 0, (parent.WidthRequest - (bufferWidth * 2)) / parentWidth, bufferWidthProportionalY));
                AbsoluteLayout.SetLayoutFlags(bufferRectTop, AbsoluteLayoutFlags.All);
                AbsoluteLayout.SetLayoutBounds(bufferRectBottom, new Rectangle(.5, 1, (parent.WidthRequest - (bufferWidth * 2)) / parentWidth, bufferWidthProportionalY));
                AbsoluteLayout.SetLayoutFlags(bufferRectBottom, AbsoluteLayoutFlags.All);
                parent.Children.Add(bufferRectLeft);
                parent.Children.Add(bufferRectRight);
                parent.Children.Add(bufferRectTop);
                parent.Children.Add(bufferRectBottom);
                if (bufferList != null)
                { bufferList.Add(bufferRectLeft); bufferList.Add(bufferRectRight); bufferList.Add(bufferRectTop); bufferList.Add(bufferRectBottom); }
                contactViews.Add(index, new List<ImageButton>() { bufferRectLeft, bufferRectRight, bufferRectTop, bufferRectBottom });
            }

            void OnTouch(object obj, TouchActionEventArgs args, View movedElement, View control)
            {
                movedElement.TranslationX += args.Location.X;
                movedElement.TranslationY += args.Location.Y;
                var pos = movedElement.GetAbsolutePosition();
                var containerPos = cabinetContainer.GetAbsolutePosition();
                if (args.IsInContact)
                {
                    if (args.Type == TouchActionEventArgs.TouchActionType.Released)
                    {
                        movedElement.RemoveEffect(typeof(ScreenTouch));
                        var checker = args.ContactView[0].Width;
                        if (args.ContactView.Count > 1 && args.ContactView[1].Width != checker)
                        {
                            SetBufferOffset(movedElement, args.ContactView[0]);
                            SetBufferOffset(movedElement, args.ContactView[1]);
                        }
                        else
                        {
                            SetBufferOffset(movedElement, args.ContactView[0]);
                            Console.WriteLine("movexed " + movedElement.X);
                        }
                    }
                }
                if (args.Type == TouchActionEventArgs.TouchActionType.Released)
                {
                    control.RemoveEffect(typeof(ScreenTouch));
                    if (pos.X < containerPos.X) { movedElement.TranslationX -= pos.X; }
                    else if (pos.X + movedElement.Width > containerPos.X + cabinetContainer.Width) { movedElement.TranslationX -= pos.X + movedElement.Width - (containerPos.X + cabinetContainer.Width); }
                    if (pos.Y < containerPos.Y) { movedElement.TranslationY += containerPos.Y - pos.Y; }
                    else if (pos.Y + movedElement.Height > containerPos.Y + cabinetContainer.Height) { movedElement.TranslationY -= pos.Y + movedElement.Height - (containerPos.Y + cabinetContainer.Height); }
                    if (!args.IsInContact)
                    {
                        AbsoluteLayout.SetLayoutBounds(movedElement,
                            new Rectangle(pos.X / (cabinetContainer.Width - movedElement.Width), (pos.Y - containerPos.Y) / (cabinetContainer.Height - movedElement.Height),
                            movedElement.Width / cabinetContainer.Width, movedElement.Height / cabinetContainer.Height));
                        AbsoluteLayout.SetLayoutFlags(movedElement, AbsoluteLayoutFlags.All);
                    }
                    movedElement.TranslationX = 0;
                    movedElement.TranslationY = 0;
                }
            }

            void SetBufferOffset(View view, View buffer)
            {
                // formula for x position of view in absolute layout : P = a(x-w) and P = a(y-h)
                //where P is the x position and a is the proprotional factor (0-1). x is the parent width, y is the parent height
                var tag = buffer.GetElementTag();
                var bufferPos = buffer.GetAbsolutePosition();
                var viewPos = view.GetAbsolutePosition(0);
                var containerPos = cabinetContainer.GetAbsolutePosition();
                var containerWidth = cabinetContainer.Width - view.Width;
                var containerHeight = cabinetContainer.Height - view.Height;
                switch (tag)
                {
                    case "right":
                        AbsoluteLayout.SetLayoutBounds(view, new Rectangle((bufferPos.X + bufferWidth) / containerWidth, viewPos.Y / containerHeight,
                          view.Width / cabinetContainer.Width, view.Height / cabinetContainer.Height));
                        AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.All);
                        break;
                    case "left":
                        AbsoluteLayout.SetLayoutBounds(view, new Rectangle((bufferPos.X - view.Width) / containerWidth, viewPos.Y / containerHeight,
                         view.Width / cabinetContainer.Width, view.Height / cabinetContainer.Height));
                        AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.All);
                        break;
                    case "bottom":
                        AbsoluteLayout.SetLayoutBounds(view, new Rectangle(viewPos.X / containerWidth, (bufferPos.Y - containerPos.Y + bufferWidth) / containerHeight,
                          view.Width / cabinetContainer.Width, view.Height / cabinetContainer.Height));
                        AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.All);
                        break;
                    case "top":
                        AbsoluteLayout.SetLayoutBounds(view, new Rectangle(viewPos.X / containerWidth, (bufferPos.Y - containerPos.Y - view.Height) / containerHeight,
                        view.Width / cabinetContainer.Width, view.Height / cabinetContainer.Height));
                        AbsoluteLayout.SetLayoutFlags(view, AbsoluteLayoutFlags.All);
                        break;
                }
            }
            pageContent.Children.Add(cabinetContainer);
            pageContent.Children.Add(assetGrid);
        }

        
        private AbsoluteLayout GetTransformationOverlay(AbsoluteLayout overlayedView, Layout<View> parent)
        {
            var buttonSize = 0.2;
            double[] xCycle = new double[8] { 0, 0, 0, .5, .5, 1, 1, 1 };
            double[] yCycle = new double[8] { 0, .5, 1, 0, 1, 0, .5, 1 };
            AbsoluteLayout overlay = new AbsoluteLayout();
            for (int i = 0; i < 8; i++)
            {
                var scaleButton = new Button();
                ScreenTouch touchEvent = new ScreenTouch();
                int index = i;
                var width = overlayedView.Width;
                var height = overlayedView.Height;
                var originX = overlayedView.X;
                var originY = overlayedView.Y;
                touchEvent.OnTouchEvent += (obj, args) =>
                {
                    Console.WriteLine("transformed touched " + overlayedView.WidthRequest);
                    overlayedView.AnchorY = yCycle[index] == 0 ? 1 : 0; overlayedView.AnchorX = xCycle[index] == 0 ? 1 : 0;
                    if (yCycle[index] == 0.5) { overlayedView.ScaleX += args.Location.X / width; }
                    else if (xCycle[index] == 0.5) { overlayedView.ScaleY += args.Location.Y / height; }
                    else { var scale = args.Location.X > args.Location.Y ? args.Location.X / width : args.Location.Y / height; overlayedView.Scale += scale; }

                };
                scaleButton.Effects.Add(touchEvent);
                overlay.Children.Add(scaleButton, new Rectangle(xCycle[i], yCycle[i], buttonSize, buttonSize), AbsoluteLayoutFlags.All);
            }

            return overlay;
        }
        */
        protected virtual void SetBasicView(bool newShelf)
        {
            cellContainer.WidthRequest = screenWidth;
            cellContainer.HeightRequest = 7 * screenHeight / 8;
            if(newShelf) AddCellRow();

            addRowButton.Clicked += (object obj, EventArgs args) => AddCellRow();

            pageContent.Children.Add(GetAssetGrid());
            pageContent.Children.Add(cellContainer);
            pageContent.Children.Add(addRowButton);
        }

        protected abstract Grid GetAssetGrid();
        protected abstract void StoreCabinetInfo(); 

        
        public void DeleteCabinet()
        {
            ContentManager.cabinetInfo.Remove(name);
        }

        protected virtual void AddCellStorage(View parent, ImageButton button) { }
        protected virtual void AddCellStorage(View parent, List<ImageButton> button) { }
        protected abstract void AddCellRow();

        protected abstract void ConfirmationSaveEvent(Action finishEditEvent);
        protected abstract void ConfirmationCancelEvent(Action finishEditEvent);
        protected abstract void SubdivideCell(int amount);

        protected abstract void DeleteCell();
        protected abstract void MergeCell();
        protected abstract bool CanTransform();
    }



    public class CabinetEditPage : EditPage
    {
        public const int cabinet_height = 50;
        protected override string cellImageSource => ContentManager.cabinetIcon;
        public CabinetEditPage(bool newShelf, Action<string, string, string> saveCabinetLocalEvent, Action<string, string, string> saveCabinetBaseEvent, string storageName = "") 
            : base(newShelf, storageName)
        {
            storageSaveLocalEvent = saveCabinetLocalEvent;
            storageSaveBaseEvent = saveCabinetBaseEvent;
        }
        protected override void SetNewShelf(bool newShelf)
        {
            if (newShelf)
            {
                int identifier = 1;
                while (ContentManager.cabinetInfo.ContainsKey("untitled shelf" + identifier) || ContentManager.cabinetItemBase.ContainsKey("untitled shelf" + identifier))
                {
                    identifier++;
                }
                name = "untitled shelf" + identifier;
                nameLegacy = name;
                ContentManager.cabinetInfo.Add(name, new Dictionary<int, AbsoluteLayout>());
                ContentManager.cabinetItemBase.Add(name, new Dictionary<int, Dictionary<ImageButton, List<ItemLayout>>>());
            }
            else
            {
                cellContainer.Children.Add(ContentManager.GetStorageView(name));
            }
        }

        protected override Grid GetAssetGrid()
        {
            Grid assetGrid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition() {Height = 50 },
                    new RowDefinition() {Height = 20}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition(){Width = 30 },
                    new ColumnDefinition(){Width = 10 },
                    new ColumnDefinition(){Width = 30 },
                    new ColumnDefinition(){Width = 3 },
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                }
            };

            var dividerImage = new Image() { BackgroundColor = Color.Gray };
            var subdivideButton = new ImageButton() { Source = subdivideIcon };
            subdivideButton.Clicked += (object obj, EventArgs args) => { if (CanTransform()) { SubdivideCell(subdivideAmount); } };
            var subdivideLabel = new Label() { Text = "Subdivide", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var numLabel = new Label() { Text = "2", HorizontalOptions = LayoutOptions.Center, FontSize = 20, VerticalOptions = LayoutOptions.Center, TextColor = Color.Black };
            var minusButton = new ImageButton() { Source = countArrow };
            minusButton.Clicked += (obj, args) =>
            {
                if (subdivideAmount > 2) { subdivideAmount--; numLabel.Text = subdivideAmount.ToString(); }
            };
            var plusButton = new ImageButton() { Source = countArrow, Rotation = 180 };
            plusButton.Clicked += (obj, args) => {
                if (subdivideAmount < 9) { subdivideAmount++; numLabel.Text = subdivideAmount.ToString(); }
            };
            var mergeRowButton = new ImageButton() { Source = mergeIcon };
            mergeRowButton.Clicked += (obj, args) => { if (CanTransform()) MergeCell(); };
            var mergeRowLabel = new Label() { Text = "Merge Row", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var deleteButton = new ImageButton() { Source = ContentManager.deleteCellIcon };
            deleteButton.Clicked += (obj, args) => { if (CanTransform()) DeleteCell(); };
            var deleteLabel = new Label() { Text = "Delete", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };


            assetGrid.Children.Add(subdivideButton, 0, 0);
            assetGrid.Children.Add(subdivideLabel, 0, 1);
            assetGrid.Children.Add(minusButton, 1, 0);
            assetGrid.Children.Add(numLabel, 2, 0);
            assetGrid.Children.Add(plusButton, 3, 0);
            Grid.SetColumn(dividerImage, 4);
            Grid.SetRowSpan(dividerImage, 2);
            assetGrid.Children.Add(dividerImage, 4, 0);
            assetGrid.Children.Add(mergeRowButton, 5, 0);
            assetGrid.Children.Add(mergeRowLabel, 5, 1);
            assetGrid.Children.Add(deleteButton, 6, 0);
            assetGrid.Children.Add(deleteLabel, 6, 1);
            return assetGrid;
        }

        protected override void AddCellStorage(View parent, ImageButton button)
        {
            var index = indexer;
            while (ContentManager.cabinetInfo[nameLegacy].ContainsKey(index))
            {
                index++;
            }
            ContentManager.cabinetInfo[nameLegacy].Add(index, parent as AbsoluteLayout);
            ContentManager.cabinetItemBase[nameLegacy].Add(index, new Dictionary<ImageButton, List<ItemLayout>>());
            ContentManager.cabinetItemBase[nameLegacy][index].Add(button, new List<ItemLayout>());
        }

        private void ReCalculateCellBounds()
        {
            for (int i = 0; i < cellContainer.Children.Count; i++)
            {
                AbsoluteLayout.SetLayoutBounds(cellContainer.Children[i], new Rectangle(0, cabinet_height / cellContainer.HeightRequest * i, 1, cabinet_height / cellContainer.HeightRequest));
            }
        }
        protected override async void AddCellRow()
        {
            if (cellContainer.Children.Count < 10)
            {
                var rowContainer = new AbsoluteLayout() { WidthRequest = cellContainer.WidthRequest, HorizontalOptions = LayoutOptions.Center };
                var newCabinetCell = new Image() { Source = cellImageSource, Aspect = Aspect.Fill, WidthRequest = cellContainer.WidthRequest, HeightRequest = rowContainer.HeightRequest };
                var button = new ImageButton() { Source = ContentManager.transIcon, Aspect = Aspect.Fill };
                AbsoluteLayout.SetLayoutBounds(button, new Rectangle(.5, 0, 1, 1));
                AbsoluteLayout.SetLayoutFlags(button, AbsoluteLayoutFlags.All);

                if (cellContainer.Children.Count > 0)
                {
                    AbsoluteLayout.SetLayoutBounds(rowContainer, new Rectangle(0, cabinet_height / cellContainer.HeightRequest * cellContainer.Children.Count, 1, cabinet_height / cellContainer.HeightRequest));
                }
                else
                {
                    AbsoluteLayout.SetLayoutBounds(rowContainer, new Rectangle(0, 0, 1, cabinet_height / cellContainer.HeightRequest));
                }

                AbsoluteLayout.SetLayoutFlags(rowContainer, AbsoluteLayoutFlags.All);

                rowContainer.Children.Add(newCabinetCell);
                rowContainer.Children.Add(button);
                cellContainer.Children.Insert(0, rowContainer);

                var index = indexer;
                AddCellStorage(rowContainer, button);

                button.Clicked += (o, a) =>
                {
                    foreach (Layout<View> element in cellContainer.Children)
                    {
                        element.Children.RemoveEffects(typeof(ImageTint));
                    }
                    button.ToggleEffects(new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }, null);
                    selectedCellIndex = index;
                    selectedButton = button;
                };

                indexer++;
                AbsoluteLayout.SetLayoutBounds(newCabinetCell, new Rectangle(.5, .5, 1, 1));
                AbsoluteLayout.SetLayoutFlags(newCabinetCell, AbsoluteLayoutFlags.All);
            }
            else { await DisplayAlert("Unable to add new row", "Maximun number of rows reached", "cancel"); }
        }

        const string Divider_Tag = "divider";
        protected override void SubdivideCell(int amount)
        {
            var parent = selectedButton;
            var ancestorWidth = ContentManager.cabinetInfo[nameLegacy][selectedCellIndex].Children[0].Width;
            var parentWidth = parent.Width;
            var parentHeight = parent.Height;
            var parentLeftOffset = parent.TranslationX / ancestorWidth;
            var index = selectedCellIndex;
            var indexUnderParent = ContentManager.cabinetInfo[nameLegacy][selectedCellIndex].Children.IndexOf(selectedButton);

            ContentManager.cabinetInfo[nameLegacy][selectedCellIndex].Children.Remove(selectedButton);
            ContentManager.cabinetItemBase[nameLegacy][selectedCellIndex].Remove(selectedButton);
            double offset = 0;
            for (int i = 0; i <amount; i ++)
            {
                var lastIndex = ContentManager.cabinetInfo[nameLegacy][selectedCellIndex].Children.Count - 1;
                var button = new ImageButton() { Source = ContentManager.transIcon, Aspect = Aspect.Fill, WidthRequest = parentWidth / amount, HeightRequest = parentHeight };

                Image divider = new Image() { Aspect = Aspect.Fill };
                if (i != 0)
                {
                    offset = parentWidth / amount * i;
                    var parentLeft = parent.Bounds.Left;
                    var dividerX = parentWidth / amount * i + parent.GetAbsolutePosition().X;
                    ContentManager.cabinetInfo[nameLegacy][selectedCellIndex].Children.Add(divider);

                    AbsoluteLayout.SetLayoutBounds(divider, new Rectangle(dividerX / ancestorWidth, 0, .05, 1));
                    AbsoluteLayout.SetLayoutFlags(divider, AbsoluteLayoutFlags.All);
                    if (amount % 2 == 0)
                    {
                        divider.Source = dividerX < ancestorWidth / 2 ? cabinetDividerLeft :
                            dividerX == ancestorWidth / 2 ? cabinetDividerMiddle : cabinetDividerRight;
                    }
                    else
                    {
                        divider.Source = dividerX < ancestorWidth / 2 ? cabinetDividerLeft : cabinetDividerRight;
                    }


                    if (divider.Source != cabinetDividerMiddle)
                    {
                        divider.ScaleX = Math.Abs(ancestorWidth / 2 - (dividerX)) / (ancestorWidth / 2);
                        if (divider.ScaleX < 0.2) { divider.ScaleX = 0.2; }
                    }
                    else { divider.ScaleX = .3; }
                }

                dividerHelper.Add(button, divider);
                AbsoluteLayout.SetLayoutBounds(button, new Rectangle((parent.GetAbsolutePosition().X + offset) / (ancestorWidth - button.WidthRequest), 0, button.WidthRequest / ancestorWidth, 1));
                AbsoluteLayout.SetLayoutFlags(button, AbsoluteLayoutFlags.All);
                ContentManager.cabinetInfo[nameLegacy][selectedCellIndex].Children.Insert(indexUnderParent + i, button);
                ContentManager.cabinetItemBase[nameLegacy][selectedCellIndex].Add(button, new List<ItemLayout>());
                button.Clicked += (o, a) => {
                    foreach (Layout<View> element in cellContainer.Children)
                    {
                        element.Children.RemoveEffects(typeof(ImageTint));
                    }
                    button.ToggleEffects((new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }), null);
                    selectedCellIndex = index;
                    selectedButton = button;
                };
            }
            selectedCellIndex = -1;
        }

        protected override void MergeCell()
        {
            var itemBase = ContentManager.cabinetItemBase[name][selectedCellIndex];
            var infoBase = ContentManager.cabinetInfo[nameLegacy][selectedCellIndex];

            var currentButtonIndex = infoBase.Children.IndexOf(selectedButton);
            var buttonList = infoBase.Children.Where(e => e.GetType() == typeof(ImageButton)).Cast<ImageButton>().ToList();
            var selectedButtonListIndex = buttonList.IndexOf(selectedButton); 
            var mergeButtonIndex = buttonList.Count > selectedButtonListIndex + 1 ? selectedButtonListIndex + 1 : -1;
            Console.WriteLine("ultra merger " + selectedButtonListIndex);
            ImageButton mergeButton = mergeButtonIndex >= 0 ? buttonList[mergeButtonIndex] : null;
            var leftBound = selectedButton.GetAbsolutePosition(0).X;
            var totalWidth = mergeButton != null ? mergeButton.Width + selectedButton.Width : selectedButton.Width;

            infoBase.Children.Remove(selectedButton);
            itemBase.Remove(selectedButton);
            if (mergeButton != null)
            {
                itemBase.Remove(mergeButton);
                infoBase.Children.Remove(mergeButton);
                infoBase.Children.Remove(dividerHelper[mergeButton]);
            }
            var index = selectedCellIndex;
            var buttonIndex = ContentManager.cabinetInfo[nameLegacy][index].Children.Count - 1;
            var button = new ImageButton() { Source = ContentManager.transIcon, Aspect = Aspect.Fill };
            var ancestorWidth = ContentManager.cabinetInfo[nameLegacy][selectedCellIndex].Children[0].Width;
            var widthDifference = ancestorWidth - totalWidth <= 0 ? 0 : ancestorWidth - totalWidth;
            var buttonX = leftBound == 0 ? 0 : leftBound / widthDifference;
            infoBase.Children.Insert(currentButtonIndex, button);
            AbsoluteLayout.SetLayoutBounds(button, new Rectangle(buttonX, 0, totalWidth / ancestorWidth, 1));
            AbsoluteLayout.SetLayoutFlags(button, AbsoluteLayoutFlags.All);

            itemBase.Add(button, new List<ItemLayout>());
            button.Clicked += (o, a) =>
            {
                foreach (Layout<View> element in cellContainer.Children)
                {
                    element.Children.RemoveEffects(typeof(ImageTint));
                }
                button.ToggleEffects((new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }), null);
                selectedCellIndex = index;
                selectedButton = button;
            };
            Image divider = dividerHelper.ContainsKey(selectedButton) ? dividerHelper[selectedButton] : null;
            if(divider != null) dividerHelper.Add(button, divider);
            if(mergeButton != null)dividerHelper.Remove(mergeButton);
        }

        protected override void DeleteCell()
        {
            var index = selectedCellIndex;
            ContentManager.cabinetItemBase[nameLegacy].Remove(selectedCellIndex);
            cellContainer.Children.Remove(ContentManager.cabinetInfo[nameLegacy][selectedCellIndex]);
            ContentManager.cabinetInfo[nameLegacy].Remove(index);
            ReCalculateCellBounds();
        }

        protected override async void ConfirmationCancelEvent(Action finishEvent)
        {
            bool cancelConfirmed = await ContentManager.pageController.DisplayAlert("Confirmation", "Do you want to discard all current changes?", "Discard", "Cancel");
            if (cancelConfirmed) { ContentManager.cabinetInfo[nameLegacy] = preSaveState; finishEvent.Invoke(); }
        }

        protected async override void ConfirmationSaveEvent(Action finishEvent)
        {
            bool saveConfirmed = await ContentManager.pageController.DisplayAlert("Confirmation", "Do you want to save all changes?", "Save", "Cancel");
            if (saveConfirmed)
            {
                foreach (var layout in ContentManager.cabinetInfo[nameLegacy].Values)
                {
                    layout.Children.RemoveEffects(typeof(ImageTint));
                }
                cellContainer.Children.Remove(addRowButton);
                StoreCabinetInfo();
                finishEvent.Invoke();
            }

        }

        protected override void StoreCabinetInfo()
        {
            var content = ContentManager.cabinetInfo[nameLegacy];
            var itemContent = ContentManager.cabinetItemBase[nameLegacy];
            if (nameLegacy != name)
            {
                if (ContentManager.cabinetInfo.ContainsKey(nameLegacy)) ContentManager.cabinetInfo.Remove(nameLegacy);
                if (ContentManager.cabinetItemBase.ContainsKey(nameLegacy)) ContentManager.cabinetItemBase.Remove(nameLegacy);
                ContentManager.cabinetInfo.Add(name, content);
                ContentManager.cabinetItemBase.Add(name, itemContent);
            }
            nameLegacy = name;

            string rowInfo, itemInfo;
            ContentManager.SetLocalCabinet(name, out rowInfo, out itemInfo);
            storageSaveLocalEvent(name, rowInfo, itemInfo);
            storageSaveBaseEvent(name, rowInfo, itemInfo);
        }
        protected override bool CanTransform()
        {
            return ContentManager.cabinetInfo[nameLegacy].ContainsKey(selectedCellIndex) && selectedCellIndex >= 0; 
    }
    }

    // FRIDGE MARKER
    public class FridgeEditPage : EditPage
    {
        const string untitledName = "untitled fridge ";
        public const string Left_Cell_Tag = "sideLeft";
        public const string Right_Cell_Tag = "sideRight";
        public const double Main_Cell_Width_Div =2;
        public const double Side_Cell_Width_Div = 5;
        public const double fridge_height = 50;
        protected override string cellImageSource => ContentManager.fridgeIcon; 
        public FridgeEditPage(bool newShelf, Action<string, string, string> _saveFridgeLocalEvent, Action<string, string, string> _saveFridgeBaseEvent, string storageName = "") 
            : base(newShelf, storageName)
        {
            storageSaveBaseEvent = _saveFridgeBaseEvent;
            storageSaveLocalEvent = _saveFridgeLocalEvent;
        }

        protected override void SetNewShelf(bool newShelf)
        {
            if (newShelf)
            {
                int identifier = 1;
                while (ContentManager.fridgeInfo.ContainsKey(untitledName + identifier) || ContentManager.fridgeItemBase.ContainsKey(untitledName + identifier))
                {
                    identifier++;
                }
                name = untitledName + identifier;
                nameLegacy = name;
                ContentManager.fridgeInfo.Add(nameLegacy, new Dictionary<int, AbsoluteLayout>());
                ContentManager.fridgeItemBase.Add(nameLegacy, new Dictionary<int, Dictionary<ImageButton, List<ItemLayout>>>());
            }
        }

        protected override Grid GetAssetGrid()
        {
            Grid assetGrid = new Grid()
            {
                HeightRequest = 70,
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = screenWidth / 10 },
                    new ColumnDefinition(){Width = screenWidth / 10 },
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition(){Width = screenWidth / 32 },
                    new ColumnDefinition(){Width = screenWidth / 24 },
                    new ColumnDefinition(){Width = screenWidth / 32 },
                    new ColumnDefinition(){Width = screenWidth / 10 },
                    new ColumnDefinition(){Width = screenWidth / 10 },
                },
                RowDefinitions =
                {
                    new RowDefinition(){Height = 20},
                    new RowDefinition(){Height = 50 }
                }
            };
            var addLeftCellButton = new ImageButton() { Source = countArrow, Rotation = -90, Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            var addLeftLabel = new Label() { Text = "+", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 20 };
            var deleteLeftCellButton = new ImageButton() { Source = countArrow, Rotation = 90, Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            var deleteLeftLabel = new Label() { Text = "-", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 20 };
            addLeftCellButton.Clicked += (obj, args) => ChangeSideCell(true, true);
            deleteLeftCellButton.Clicked += (obj, args) => ChangeSideCell(true, false);
            var addRightCellButton = new ImageButton() { Source = countArrow, Rotation = -90, Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            var addRightLabel = new Label() { Text = "+", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 20 };
            var deleteRightCellButton = new ImageButton() { Source = countArrow, Rotation = 90, Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            var deleteRightLabel = new Label() { Text = "-", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 20 };
            addRightCellButton.Clicked += (obj, args) => ChangeSideCell(false, true);
            deleteRightCellButton.Clicked += (obj, args) => ChangeSideCell(false, false);
            var subdivideButton = new ImageButton() { Source = subdivideIcon, Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            subdivideButton.Clicked += (object obj, EventArgs args) => { if (CanTransform()) { SubdivideCell(subdivideAmount); } };
            var subdivideLabel = new Label() { Text = "Div", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var mergeButton = new ImageButton() { Source = mergeIcon, Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            mergeButton.Clicked += (obj, args) => { if (CanTransform()) { MergeCell(); } };
            var mergeLabel = new Label() { Text = "Merge", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var deleteButton = new ImageButton() { Source = ContentManager.deleteCellIcon, Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            deleteButton.Clicked += (obj, args) => { if (CanTransform()) { DeleteCell(); } };
            var deleteLabel = new Label() { Text = "Del", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var numLabel = new Label() { Text = "2", HorizontalOptions = LayoutOptions.Center, FontSize = 20, VerticalOptions = LayoutOptions.Center, TextColor = Color.Black };
            var minusButton = new ImageButton() { Source = countArrow, Rotation = 180 };
            minusButton.Clicked += (obj, args) =>
            {
                if (subdivideAmount > 2) { subdivideAmount--; numLabel.Text = subdivideAmount.ToString(); }
            };
            var plusButton = new ImageButton() { Source = countArrow };
            plusButton.Clicked += (obj, args) => {
                if (subdivideAmount < 9) { subdivideAmount++; numLabel.Text = subdivideAmount.ToString(); }
            };
            assetGrid.Children.Add(addLeftCellButton, 0, 1);
            assetGrid.Children.Add(deleteLeftCellButton, 1, 1);
            assetGrid.Children.Add(deleteButton, 2,1);
            assetGrid.Children.Add(mergeButton, 3, 1);
            assetGrid.Children.Add(subdivideButton, 4, 1);
            assetGrid.Children.Add(minusButton, 5, 1);
            assetGrid.Children.Add(numLabel, 6, 1);
            assetGrid.Children.Add(plusButton, 7, 1);
            assetGrid.Children.Add(addRightCellButton, 8, 1);
            assetGrid.Children.Add(deleteRightCellButton, 9, 1);
            assetGrid.Children.Add(addLeftLabel, 0, 0);
            assetGrid.Children.Add(deleteLeftLabel, 1, 0);
            assetGrid.Children.Add(deleteLabel, 2, 0);
            assetGrid.Children.Add(mergeLabel, 3, 0);
            assetGrid.Children.Add(subdivideLabel, 4, 0);
            assetGrid.Children.Add(addRightLabel, 8, 0);
            assetGrid.Children.Add(deleteRightLabel, 9, 0);
            return assetGrid;
        }

        private void ReCalculateCellBounds()
        {
            for (int i = 2; i < cellContainer.Children.Count; i++)
            {
                var y = i == 2 ? 0 : fridge_height / (cellContainer.HeightRequest - fridge_height) * (i - 2);
                if (cellContainer.Children[i].GetElementTag() != Left_Cell_Tag || cellContainer.Children[i].GetElementTag() != Right_Cell_Tag)
                AbsoluteLayout.SetLayoutBounds(cellContainer.Children[i], new Rectangle(.5, y, 1/Main_Cell_Width_Div, fridge_height / cellContainer.HeightRequest));
            }
        }

        protected override void AddCellStorage(View parent, List<ImageButton> buttons)
        {
            var index = indexer;
            ContentManager.fridgeInfo[nameLegacy].Add(index, parent as AbsoluteLayout);
            ContentManager.fridgeItemBase[nameLegacy].Add(index, new Dictionary<ImageButton, List<ItemLayout>>());
            foreach (var button in buttons)
            {
                ContentManager.fridgeItemBase[nameLegacy][index].Add(button, new List<ItemLayout>());
            }
        }

        AbsoluteLayout leftCellContainer = new AbsoluteLayout();
        AbsoluteLayout rightCellContainer = new AbsoluteLayout();
        protected async override void AddCellRow()
        {
            if (cellContainer.Children.Count < 10)
            {
                var rowContainer = new AbsoluteLayout() { WidthRequest = cellContainer.WidthRequest, HeightRequest = fridge_height, HorizontalOptions = LayoutOptions.Center };
                var fridgeCell = new Image() { Source = cellImageSource, Aspect = Aspect.Fill };

                // formula for setting relative pos: Pr = a(Wp - Wc) / Wp where a is proprotional value (from 0 to 1), Pr is desired relative value (0 to 1), w is width(parent & child)
                var cellWidth = rowContainer.WidthRequest; var cellHeight = fridge_height / cellContainer.Height;
                rowContainer.Children.Add(fridgeCell, new Rectangle(.5, 0, 1, 1), AbsoluteLayoutFlags.All);

                if (cellContainer.Children.Count == 0)
                {
                    leftCellContainer.SetElementTag(Left_Cell_Tag);
                    rightCellContainer.SetElementTag(Right_Cell_Tag);
                    var fridgeSideLeft = new Image()
                    {
                        Source = ContentManager.fridgeSideIcon,
                        Aspect = Aspect.Fill,
                        WidthRequest = cellContainer.WidthRequest / Side_Cell_Width_Div,
                        HeightRequest = cellHeight
                    };
                    fridgeSideLeft.SetElementTag(Left_Cell_Tag);
                    var fridgeSideRight = new Image()
                    {
                        Source = ContentManager.fridgeSideIcon,
                        Aspect = Aspect.Fill,
                        WidthRequest = cellContainer.WidthRequest / Side_Cell_Width_Div,
                        HeightRequest = cellHeight
                    };
                    fridgeSideRight.SetElementTag(Right_Cell_Tag);

                    leftCellContainer.Children.Add(fridgeSideLeft, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                    rightCellContainer.Children.Add(fridgeSideRight, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                    cellContainer.Children.Add(leftCellContainer, new Rectangle(0, 0, cellContainer.WidthRequest / Side_Cell_Width_Div / cellContainer.WidthRequest, fridge_height / cellContainer.HeightRequest), AbsoluteLayoutFlags.All);
                    cellContainer.Children.Add(rightCellContainer, new Rectangle(1, 0, cellContainer.WidthRequest / Side_Cell_Width_Div / cellContainer.WidthRequest, fridge_height / cellContainer.HeightRequest), AbsoluteLayoutFlags.All);
                    ContentManager.fridgeInfo[nameLegacy].Add(indexer, leftCellContainer);
                    ContentManager.fridgeItemBase[nameLegacy].Add(indexer, new Dictionary<ImageButton, List<ItemLayout>>()); indexer++;
                    ContentManager.fridgeInfo[nameLegacy].Add(indexer, rightCellContainer);
                    ContentManager.fridgeItemBase[nameLegacy].Add(indexer, new Dictionary<ImageButton, List<ItemLayout>>()); indexer++;
                    ChangeSideCell(true, true);
                    ChangeSideCell(false, true);
                }
                List<ImageButton> buttonList = new List<ImageButton>();
                foreach (var cell in rowContainer.Children)
                {
                    var index = indexer;
                    var button = new ImageButton() { Source = ContentManager.transIcon, Aspect = Aspect.Fill };
                    button.Clicked += (o, a) =>
                    {
                        foreach (Layout<View> element in cellContainer.Children)
                        {
                            element.Children.RemoveEffects(typeof(ImageTint));
                        }
                        button.ToggleEffects(new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }, null);
                        selectedCellIndex = index;
                        selectedButton = button;
                    };
                    buttonList.Add(button);
                    if (cell.GetElementTag() == Left_Cell_Tag)
                    {
                        button.SetElementTag(Left_Cell_Tag);
                        leftCellContainer.Children.Add(button, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                    }
                    else if (cell.GetElementTag() == Right_Cell_Tag)
                    {
                        button.SetElementTag(Right_Cell_Tag);
                        rightCellContainer.Children.Add(button, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
                    }
                    AbsoluteLayout.SetLayoutBounds(button, AbsoluteLayout.GetLayoutBounds(cell));
                    AbsoluteLayout.SetLayoutFlags(button, AbsoluteLayout.GetLayoutFlags(cell));
                }
                foreach (var button in buttonList) { if (button.GetElementTag() == string.Empty) rowContainer.Children.Add(button); }
                if (cellContainer.Children.Count > 0)
                {
                    AbsoluteLayout.SetLayoutBounds(rowContainer, new Rectangle(.5, fridge_height * (cellContainer.Children.Count - 2) / (cellContainer.HeightRequest - fridge_height),
                        .5, fridge_height / cellContainer.HeightRequest));
                }
                else
                {
                    AbsoluteLayout.SetLayoutBounds(rowContainer, new Rectangle(.5, 0, .5, fridge_height / cellContainer.HeightRequest));
                }
                AbsoluteLayout.SetLayoutFlags(rowContainer, AbsoluteLayoutFlags.All);
                cellContainer.Children.Add(rowContainer);
                AddCellStorage(rowContainer, buttonList);
                if (cellContainer.Children.Count > 0)
                {
                    ConfigureSideCells();
                }
                indexer++;
            }
            else { await DisplayAlert("Unable to add new row", "Maximun number of rows reached", "cancel"); }
        }

        private void ConfigureSideCells()
        {
            var height = cellContainer.Children.Count * fridge_height;
            var width = cellContainer.WidthRequest;
            AbsoluteLayout.SetLayoutBounds(leftCellContainer, new Rectangle(0, 0, width / Side_Cell_Width_Div / cellContainer.WidthRequest, fridge_height * (cellContainer.Children.Count - 2) / cellContainer.HeightRequest));
            AbsoluteLayout.SetLayoutBounds(rightCellContainer, new Rectangle(1, 0, width / Side_Cell_Width_Div / cellContainer.WidthRequest, fridge_height * (cellContainer.Children.Count - 2) / cellContainer.HeightRequest));
        }
        protected void ChangeSideCell(bool left, bool add)
        {
            var container = left ? leftCellContainer : rightCellContainer;
            var itemBase = left ? ContentManager.fridgeItemBase[nameLegacy][0] : ContentManager.fridgeItemBase[nameLegacy][1];
            var tag = left ? Left_Cell_Tag : Right_Cell_Tag;
            var childList = container.Children;
            int preCount = childList.Count / 2;
            var count = add ? preCount + 1 : preCount - 1;
            var parentHeight = container.Height;
            var parentWidth = container.Width;
            var height = parentHeight / (count);

            itemBase.Clear();
            container.Children.Clear();
            List<ImageButton> buttonList = new List<ImageButton>();

            for (int i = 0; i < count; i++)
            {
                var cell = new Image() { Source = ContentManager.fridgeSideIcon, Aspect = Aspect.Fill };
                var button = new ImageButton() { Source = ContentManager.transIcon, Aspect = Aspect.Fill,BorderColor = Color.Blue, BorderWidth = 5 };
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
                var y = i == 0 ? 0 : parentHeight / count * i / (parentHeight - height);
                container.Children.Add(cell, new Rectangle(0, y, 1, height / parentHeight), AbsoluteLayoutFlags.All);
                container.Children.Add(button, new Rectangle(0, y, 1, height / parentHeight), AbsoluteLayoutFlags.All);
                itemBase.Add(button, new List<ItemLayout>());
            }
            indexer++;
        }
        protected override void SubdivideCell(int amount)
        {
            ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children.Remove(selectedButton);
            ContentManager.fridgeItemBase[nameLegacy][selectedCellIndex].Remove(selectedButton);

            var parent = selectedButton;
            var ancestorWidth = ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children[0].Width;
            var parentWidth = parent.Width;
            var parentHeight = parent.Height;
            var parentLeftOffset = parent.TranslationX / ancestorWidth;
            var index = selectedCellIndex;

            for (int i = 0; i < amount; i++)
            {
                var lastIndex = ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children.Count - 1;
                var button = new ImageButton() { Source = ContentManager.transIcon, Aspect = Aspect.Fill, WidthRequest = parentWidth / amount };
                Image divider = new Image() { Aspect = Aspect.Fill };

                double offset = 0;
                if (i != 0)
                {
                    offset = parentWidth / amount * i;
                    var parentLeft = parent.Bounds.Left;
                    var dividerX = parentWidth / amount * i + parent.GetAbsolutePosition().X;
                    ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children.Add(divider);

                    AbsoluteLayout.SetLayoutBounds(divider, new Rectangle(dividerX / ancestorWidth, 0, .05, 1));
                    AbsoluteLayout.SetLayoutFlags(divider, AbsoluteLayoutFlags.All);
                    divider.Source = ContentManager.fridgeDividerIcon;

                    divider.ScaleX = .5;
                    dividerHelper.Add(button, divider);
                }

                ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children.Add(button);
                AbsoluteLayout.SetLayoutBounds(button, new Rectangle((parent.GetAbsolutePosition().X + offset) / (ancestorWidth - button.WidthRequest), 0, button.WidthRequest / ancestorWidth, 1));
                AbsoluteLayout.SetLayoutFlags(button, AbsoluteLayoutFlags.All);
                ContentManager.fridgeItemBase[nameLegacy][selectedCellIndex].Add(button, new List<ItemLayout>());
                var buttonIndex = ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children.Count - 1;
                button.Clicked += (o, a) =>
                {
                    foreach (Layout<View> element in cellContainer.Children)
                    {
                        element.Children.RemoveEffects(typeof(ImageTint));
                    }
                    button.ToggleEffects((new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }), null);
                    selectedCellIndex = index;
                    selectedButton = button;
                };
            }
            selectedCellIndex = -1;
        }
        protected override void MergeCell()
        {
            for (int i = ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children.Count - 1; i >= 1; i--)
            {
                ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children.RemoveAt(i);
            }
            ContentManager.fridgeItemBase[nameLegacy].Remove(selectedCellIndex);

            var index = selectedCellIndex;
            var buttonIndex = ContentManager.fridgeInfo[nameLegacy][index].Children.Count - 1;
            var button = new ImageButton() { Source = ContentManager.transIcon, Aspect = Aspect.Fill };

            ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children.Add(button);
            ContentManager.fridgeItemBase[nameLegacy].Add(index, new Dictionary<ImageButton, List<ItemLayout>>());
            ContentManager.fridgeItemBase[nameLegacy][index].Add(button, new List<ItemLayout>());

            AbsoluteLayout.SetLayoutBounds(button, new Rectangle(.5, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(button, AbsoluteLayoutFlags.All);
            button.Clicked += (o, a) =>
            {
                foreach (Layout<View> element in cellContainer.Children)
                {
                    element.Children.RemoveEffects(typeof(ImageTint));
                }
                button.ToggleEffects((new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }), null);
                selectedCellIndex = index;
                selectedButton = button;
            };
        }
        protected override void DeleteCell()
        {
            var index = selectedCellIndex;
            ContentManager.fridgeItemBase[nameLegacy].Remove(selectedCellIndex);
            cellContainer.Children.Remove(ContentManager.fridgeInfo[nameLegacy][selectedCellIndex]);
            ContentManager.fridgeInfo[nameLegacy].Remove(index);
            ConfigureSideCells();
            ReCalculateCellBounds();
        }

        protected override async void ConfirmationCancelEvent(Action finishEvent)
        {
            bool cancelConfirmed = await ContentManager.pageController.DisplayAlert("Confirmation", "Do you want to discard all current changes?", "Discard", "Cancel");
            if (cancelConfirmed) 
            {
                if(ContentManager.fridgeInfo[nameLegacy].Count == 0) { ContentManager.fridgeInfo.Remove(nameLegacy); }
                ContentManager.fridgeInfo[nameLegacy] = preSaveState; finishEvent.Invoke(); 
            }
        }

        protected override async void ConfirmationSaveEvent(Action finishEvent)
        {
            bool saveConfirmed = await ContentManager.pageController.DisplayAlert("Confirmation", "Do you want to save all changes?", "Save", "Cancel");
            if (saveConfirmed)
            {
                foreach (var layout in ContentManager.fridgeInfo[nameLegacy].Values)
                {
                    layout.Children.RemoveEffects(typeof(ImageTint));
                }
                StoreCabinetInfo();
                finishEvent?.Invoke();
            }
        }
        
        protected override void StoreCabinetInfo()
        {
            var content = ContentManager.fridgeInfo[nameLegacy];
            var itemContent = ContentManager.fridgeItemBase[nameLegacy];
            if (nameLegacy != name)
            {
                if (ContentManager.fridgeInfo.ContainsKey(nameLegacy)) ContentManager.fridgeInfo.Remove(nameLegacy);
                if (ContentManager.fridgeItemBase.ContainsKey(nameLegacy)) ContentManager.fridgeItemBase.Remove(nameLegacy);
                ContentManager.fridgeInfo.Add(name, content);
                ContentManager.fridgeItemBase.Add(name, itemContent);
            }
            nameLegacy = name;
            string rowInfo, itemInfo;
            ContentManager.SetLocalFridge(name, out rowInfo, out itemInfo);
            storageSaveLocalEvent(name, rowInfo, itemInfo);
            storageSaveBaseEvent(name, rowInfo, itemInfo);
        }

        protected override bool CanTransform()
        {
            return ContentManager.fridgeInfo[nameLegacy].ContainsKey(selectedCellIndex) && selectedCellIndex > 0;
        }

    }
}
