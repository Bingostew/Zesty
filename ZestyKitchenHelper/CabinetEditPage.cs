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

        protected Grid storageGrid;

        protected StackLayout pageContent = new StackLayout() { BackgroundColor = Color.Wheat };
        protected AbsoluteLayout cellContainer = new AbsoluteLayout() { HorizontalOptions = LayoutOptions.Center};
        protected Dictionary<int, AbsoluteLayout> preSaveState;
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
            IDGenerator.InitializeIDGroup(name);

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
            //   cellContainer.WidthRequest = screenWidth;
            // cellContainer.HeightRequest = 7 * screenHeight / 8;
            storageGrid.WidthRequest = screenWidth;
            storageGrid.HeightRequest = 7 * screenWidth / 8;
            storageGrid.BackgroundColor = Color.SaddleBrown;
            if(newShelf) AddCellRow();

            addRowButton.Clicked += (object obj, EventArgs args) => AddCellRow();

            pageContent.Children.Add(GetAssetGrid());
            pageContent.Children.Add(storageGrid);
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
        protected virtual void ConfirmationCancelEvent(Action finishEditEvent)
        {
            IDGenerator.DeleteIDGroup(name);
        }

        protected abstract void DeleteCell();
        protected bool CanTransform(){ return true; }
    }



    public class CabinetEditPage : EditPage
    {
        public const int cabinet_height = 50;
        protected override string cellImageSource => ContentManager.cabinetIcon;

        private Cabinet cabinet;
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
                storageGrid = GridManager.InitializeGrid("cabinet" + IDGenerator.GetID(ContentManager.cabinetEditIdGenerator), 0, 0, GridLength.Star, GridLength.Star);
                storageGrid.RowSpacing = 0;
                storageGrid.ColumnSpacing = 0;
                cabinet = new Cabinet(name, storageGrid);

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
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };

            var dividerImage = new Image() { BackgroundColor = Color.Gray };
            var subdivideButton = new ImageButton() { Source = subdivideIcon, BackgroundColor = Color.Transparent, };
            subdivideButton.Clicked += (object obj, EventArgs args) => { if (CanTransform()) { SubdivideCellHorizontal(); } };
            var subdivideLabel = new Label() { Text = "Subdivide Column", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var subdivideVerticalButton = new ImageButton() { Source = subdivideIcon, Rotation = 90, BackgroundColor = Color.Transparent, };
            subdivideVerticalButton.Clicked += (object obj, EventArgs args) => { if (CanTransform()) { SubdivideCellVertical(); } };
            var subdivideVerticalLabel = new Label() { Text = "Subdivide Row", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var mergeColumnButton = new ImageButton() { Source = mergeIcon, Rotation = 90, BackgroundColor = Color.Transparent, };
            mergeColumnButton.Clicked += (obk, args) => { if (CanTransform()) { MergeCellVertical(); } };
            var mergeColumnLabel = new Label() { Text = "Merge Column", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var mergeRowButton = new ImageButton() { Source = mergeIcon, BackgroundColor = Color.Transparent, };
            mergeRowButton.Clicked += (obj, args) => { if (CanTransform()) MergeCellHorizontal(); };
            var mergeRowLabel = new Label() { Text = "Merge Row", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var deleteButton = new ImageButton() { Source = ContentManager.deleteCellIcon, BackgroundColor = Color.Transparent, };
            deleteButton.Clicked += (obj, args) => { if (CanTransform()) DeleteCell(); };
            var deleteLabel = new Label() { Text = "Delete", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };


            assetGrid.Children.Add(subdivideButton, 0, 0);
            assetGrid.Children.Add(subdivideLabel, 0, 1);
            assetGrid.Children.Add(subdivideVerticalButton, 1, 0);
            assetGrid.Children.Add(subdivideVerticalLabel, 1, 1);
            assetGrid.Children.Add(mergeColumnButton, 2, 0);
            assetGrid.Children.Add(mergeColumnLabel, 2, 1);
            assetGrid.Children.Add(mergeRowButton, 3, 0);
            assetGrid.Children.Add(mergeRowLabel, 3, 1);
            assetGrid.Children.Add(deleteButton, 4, 0);
            assetGrid.Children.Add(deleteLabel, 4, 1);
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

        protected void AddCell(Vector2D<int> position, int columnSpan = 1, int rowSpan = 1)
        {
            // button for touch indication, image of cabinet
            Image cabinetCellBackground = new Image() { Source = cellImageSource, Aspect = Aspect.Fill };
            ImageButton transparentButton = new ImageButton() { Source = ContentManager.transIcon, BackgroundColor = Color.Transparent, Aspect = Aspect.Fill };

            // retrieve an ID for the new row
            int cellIndex = IDGenerator.GetID(name);

            // add items to the new position
            GridManager.AddGridItemAtPosition(storageGrid, new List<View>() { cabinetCellBackground, transparentButton }, position);

            // registering tint event on transparent button
            transparentButton.Clicked += (o, a) =>
            {
                foreach (View element in storageGrid.Children)
                {
                    // if given element is the transparent overlay, then remove all tints
                    if (element.GetType() == typeof(ImageButton))
                        element.RemoveEffect(typeof(ImageTint));
                }
                transparentButton.ToggleEffects(new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5) }, null);
                selectedCellIndex = cellIndex;
            };

            // register cipher of new cell
            StorageCell cell = new StorageCell(position, cellIndex, columnSpan, rowSpan);
            cabinet.AddGridCell(cellIndex, cell);
            // register children of new cell
            cabinet.AddGridCellUI(cellIndex, cabinetCellBackground, transparentButton);
            // set the column and row span of cell children
            columnSpan = columnSpan < 1 ? 1 : columnSpan;
            rowSpan = rowSpan < 1 ? 1 : rowSpan;
            cell.SetColumnSpan(columnSpan);
            cell.SetRowSpan(rowSpan);
        }
        protected override void AddCellRow()
        {
            AddCell(new Vector2D<int>(0, storageGrid.RowDefinitions.Count), storageGrid.ColumnDefinitions.Count);
        }

        /// <summary>
        /// Subdivide cell horizontally or vertically
        /// </summary>
        /// <param name="layerCount">Retrieves the row/column count. Column for horizontal; Row for vertical.</param>
        /// <param name="sameLayerPositionGetter">Get the position that will be checked to be equal. Y for horizontal; X for vertical.</param>
        /// <param name="compareLayerPositionGetter">Get the position that will be compared for subdivision. X for horizontal; X for vertical.</param>
        /// <param name="subdivideLayerSpanGetter">Get the layer span on the subdividing layer. Row span for horizontal; Column span for vertical.</param>
        /// <param name="addLayer">Function that adds a layer to the grid. Column for horizontal; Row for vertical.</param>
        /// <param name="setLayer">Function that sets layer span. Parameters: (cell, layer span). Column for horizontal; Row for vertical.</param>
        /// <param name="updateControlLayer"u>Function that updates the control layer span.</param>
        /// <param name="getNewPosition">Function that retrieves a cell position. Parameters: (x, y, change). </param>
        /// <param name="addCell">Function that adds the new celll after subdivision. Parameters: (position, layer span, cell).</param>
        protected void SubdivideCell(Func<int> layerCount, Func<Vector2D<int>, int> sameLayerPositionGetter, Func<Vector2D<int>, int> compareLayerPositionGetter,
            Func<StorageCell, int> subdivideLayerSpanGetter, Action addLayer, Action<StorageCell, int> setLayer, 
            Action<StorageCell> updateControlLayer, Func<int, int, int, Vector2D<int>> getNewPosition, Action<Vector2D<int>, int, StorageCell> addCell)
        {
            // if nothing is selected, then do not execute
            if (selectedCellIndex < 0)
                return;

            StorageCell selectedCell = cabinet.GetGridCell(selectedCellIndex);
            Vector2D<int> position = selectedCell.Position;

            // For horizontal, cycle through all cells and retrieve the number of columns for the given row
            // For vertical, cycle through all cells and retrieve the number row for each column
            int layerInLayerCount = 0;
            foreach (StorageCell cell in cabinet.GetGridCells())
            {
                Vector2D<int> pos = cell.Position;
                if (sameLayerPositionGetter(pos) == sameLayerPositionGetter(position))
                    layerInLayerCount++;
            }

            // retrieve the position in the middle of the cell. This will be the position of the new cell as well.
            double subdivideLine = (double)subdivideLayerSpanGetter(selectedCell) / 2;
            // retrieve the current row/column count and projected row/column count after subdivision
            int oldLayerCount = layerCount();

            // check if row/column needs to be added: if current row/column count is less than projected count, then don't add
            int newLayerCount = layerInLayerCount == oldLayerCount || subdivideLine % 1 != 0 ? oldLayerCount * 2 : oldLayerCount;
            // row/column span for the new cell made from subdivision
            int defaultLayerSpan = newLayerCount != oldLayerCount ? (int)(subdivideLine * 2) : (int)subdivideLine;


            // add new row/columns for subdivision
            while (layerCount() < newLayerCount)
            {
                addLayer();
            }

            foreach (int id in cabinet.GetGridIDs())
            {
                StorageCell cell = cabinet.GetGridCell(id);
                Vector2D<int> pos = cell.Position;
                bool isSelectedCell = pos.X == position.X && pos.Y == position.Y;

                // retrieve the children of a cell
                List<View> children = cell.Children;

                // calculate the new position of the cell due to change in column numbers. Formula: currentX / oldMaxX * newMaxX
                Vector2D<int> newPosition = getNewPosition(pos.X, pos.Y, (int)(compareLayerPositionGetter(pos) / (float)oldLayerCount * newLayerCount));

                // Explanation for horizontal: If subdivision causes column count to double, then all existing cells unaffected by the subdivison must have a doubled column span.  
                // Their positions must also be scaled accordingly. The selected cell for subdivision is halved, but since the column span is doubled, nothing is changed.
                // If subidvision keeps the column count, then existing cells unaffected by the subdivision are unaffected, no change required. 
                // The selected cell for subdivision is halved, thus its column span is halved.
                // (Logic is the same for vertical)
                if (newLayerCount != oldLayerCount)
                {
                    // scaling position accordingly
                    GridManager.AddGridItemAtPosition(storageGrid, children, newPosition);
                    cell.Position = newPosition;
                    // If unaffected by subdivision, then double rpw/column span. Else, maintain row/column span.
                    if (!isSelectedCell)
                        setLayer(cell, subdivideLayerSpanGetter(cell) * 2);
                    else
                        setLayer(cell, subdivideLayerSpanGetter(cell));
                }

                // If selected cell then half row/column span. Else, maintain row/column span.
                else if (isSelectedCell && subdivideLayerSpanGetter(cell) > 1)
                {
                    setLayer(cell, defaultLayerSpan);
                }

                // Reset cell's column/row span as changing grid's property changes its view.
                updateControlLayer(cell);
            }

            // add the new cell after subdivision
            Vector2D<int> newCellPosition = getNewPosition(position.X, position.Y,
                (int)(compareLayerPositionGetter(position) / (float)oldLayerCount * newLayerCount) + defaultLayerSpan);
            addCell(newCellPosition, defaultLayerSpan, selectedCell);
            // reset cell index
            selectedCellIndex = -1;
        }

        protected void SubdivideCellHorizontal()
        {
            SubdivideCell(() => storageGrid.ColumnDefinitions.Count, v => v.Y, v => v.X, c => c.GetColumnSpan(),
                () => storageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }), (c, i) => c.SetColumnSpan(i), c => c.SetRowSpan(c.GetRowSpan()),
                (x, y, p) => new Vector2D<int>(p, y),(v, s, c) => AddCell(v, s, c.GetRowSpan()));
        }

        protected void SubdivideCellVertical()
        {
            SubdivideCell(() => storageGrid.RowDefinitions.Count, v => v.X, v => v.Y, c => c.GetRowSpan(),
                () => storageGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }), (c, i) => c.SetRowSpan(i), c => c.SetColumnSpan(c.GetColumnSpan()),
                 (x, y, p) => new Vector2D<int>(x, p), (v, s, c) => AddCell(v, c.GetColumnSpan(), s));
        }

        private void ReCalculateCellBounds()
        {
            for (int i = 0; i < cellContainer.Children.Count; i++)
            {
                AbsoluteLayout.SetLayoutBounds(cellContainer.Children[i], new Rectangle(0, cabinet_height / cellContainer.HeightRequest * i, 1, cabinet_height / cellContainer.HeightRequest));
            }
        }

        /// <summary>
        /// Merges cells either horizontally or vertically
        /// </summary>
        /// <param name="layerCount"> Column count for horizontal. Row count for vertical.</param>
        /// <param name="sameLayerPositionGetter"> Position to check if cells are in the same position for control layer. Y for horizontal; X for vertical. </param>
        /// <param name="compareLayerPositionGetter"> Position in the merging layer. X for horizontal; Y for vertical.</param>
        /// <param name="sameLayerSpanGetter"> Layer span to check if both cells are the same length. Row span for horizontal; Column span for vertical. </param>
        /// <param name="mergeLayerSpanGetter"> Layer span to add when merging cells. Column span for horizontal; Row span for vertical </param>
        /// <param name="setLayerSpan"> Method that sets row/column span. Parameter: (cell, layer span). </param>
        protected void MergeCell(int layerCount, Func<Vector2D<int>, int> sameLayerPositionGetter, Func<Vector2D<int>, int> compareLayerPositionGetter, 
            Func<StorageCell, int> sameLayerSpanGetter, Func<StorageCell, int> mergeLayerSpanGetter, Action<StorageCell, int> setLayerSpan, Action<StorageCell> updateControlSpan)
        {
            // if nothing is selected, then do not execute
            if (selectedCellIndex < 0)
                return;
            StorageCell selectedCell = cabinet.GetGridCell(selectedCellIndex);
            StorageCell nextCell = null;
            int closestLayer = layerCount;

            foreach (int id in cabinet.GetGridIDs())
            {
                StorageCell cell = cabinet.GetGridCell(id);
                Vector2D<int> pos = cell.Position;

                // constrain: (If for horizontal) same row, column index must be greater than selected cell column index, row span of both cells are equal,
                // column index must be smaller than previous candidate's column index.
                // (Same logic for vertical)
                if (sameLayerPositionGetter(pos) == sameLayerPositionGetter(selectedCell.Position) && sameLayerSpanGetter(cell) == sameLayerSpanGetter(selectedCell) 
                    && compareLayerPositionGetter(pos) > compareLayerPositionGetter(selectedCell.Position) && compareLayerPositionGetter(pos) < closestLayer)
                {
                    closestLayer = compareLayerPositionGetter(pos);
                    nextCell = cell;
                }
            }

            // If no cell found, do not execute
            if (nextCell == null)
                return;

            // Remove children from cell, then remove cell from grid
            foreach (View child in nextCell.Children)
            {
                storageGrid.Children.Remove(child);
            }
            cabinet.RemoveGridCell(nextCell.Index);
            

            // add the column/row span of both cells to merge
            setLayerSpan(selectedCell, mergeLayerSpanGetter(selectedCell) + mergeLayerSpanGetter(nextCell));
            updateControlSpan(selectedCell);
            // reset cell index
            selectedCellIndex = -1;
        }

        protected void MergeCellHorizontal()
        {
            MergeCell(storageGrid.ColumnDefinitions.Count, v => v.Y, v => v.X, c => c.GetRowSpan(), c => c.GetColumnSpan(), (c, i) => c.SetColumnSpan(i), c => c.SetRowSpan(c.GetRowSpan()));
         
        }

        protected void MergeCellVertical()
        {
            MergeCell(storageGrid.RowDefinitions.Count, v => v.X, v => v.Y, c => c.GetColumnSpan(), c => c.GetRowSpan(), (c, i) => c.SetRowSpan(i), c => c.SetColumnSpan(c.GetColumnSpan()));
        }

        protected override void DeleteCell()
        {
            foreach (int id in cabinet.GetGridIDs())
            {
                foreach (View child in cabinet.GetGridCell(id).Children)
                {
                    storageGrid.Children.Remove(child);
                }
                cabinet.RemoveGridCell(id);
            }
            storageGrid.RowDefinitions.Clear();
            storageGrid.ColumnDefinitions.Clear();
            AddCellRow();
        }

        protected override async void ConfirmationCancelEvent(Action finishEvent)
        {
            base.ConfirmationCancelEvent(finishEvent);
            bool cancelConfirmed = await ContentManager.pageController.DisplayAlert("Confirmation", "Do you want to discard all current changes?", "Discard", "Cancel");
            if (cancelConfirmed) 
            {
                if (preSaveState != null)
                    ContentManager.cabinetInfo[nameLegacy] = preSaveState;
                else
                {
                    ContentManager.cabinetInfo.Remove(nameLegacy);
                    ContentManager.cabinetItemBase.Remove(nameLegacy);
                }
                    
                finishEvent.Invoke(); 
            }
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
            var addLeftCellButton = new ImageButton() { Source = countArrow, Rotation = -90, BackgroundColor = Color.Transparent,
                Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            var addLeftLabel = new Label() { Text = "+", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 20 };
            var deleteLeftCellButton = new ImageButton() { Source = countArrow, Rotation = 90, BackgroundColor = Color.Transparent,
                Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            var deleteLeftLabel = new Label() { Text = "-", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 20 };
            addLeftCellButton.Clicked += (obj, args) => ChangeSideCell(true, true);
            deleteLeftCellButton.Clicked += (obj, args) => ChangeSideCell(true, false);
            var addRightCellButton = new ImageButton() { Source = countArrow, Rotation = -90, BackgroundColor = Color.Transparent,
                Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            var addRightLabel = new Label() { Text = "+", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 20 };
            var deleteRightCellButton = new ImageButton() { Source = countArrow, Rotation = 90, BackgroundColor = Color.Transparent,
                Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            var deleteRightLabel = new Label() { Text = "-", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 20 };
            addRightCellButton.Clicked += (obj, args) => ChangeSideCell(false, true);
            deleteRightCellButton.Clicked += (obj, args) => ChangeSideCell(false, false);
            var subdivideButton = new ImageButton() { Source = subdivideIcon, Aspect = Aspect.Fill, BackgroundColor = Color.Transparent,
                WidthRequest = 50, HeightRequest = 50 };
            subdivideButton.Clicked += (object obj, EventArgs args) => { if (CanTransform()) { SubdivideCell(subdivideAmount); } };
            var subdivideLabel = new Label() { Text = "Div", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var mergeButton = new ImageButton() { Source = mergeIcon, Aspect = Aspect.Fill, BackgroundColor = Color.Transparent,
                WidthRequest = 50, HeightRequest = 50 };
            mergeButton.Clicked += (obj, args) => { if (CanTransform()) { MergeCell(); } };
            var mergeLabel = new Label() { Text = "Merge", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var deleteButton = new ImageButton() { Source = ContentManager.deleteCellIcon, BackgroundColor = Color.Transparent,
                Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            deleteButton.Clicked += (obj, args) => { if (CanTransform()) { DeleteCell(); } };
            var deleteLabel = new Label() { Text = "Del", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var numLabel = new Label() { Text = "2", HorizontalOptions = LayoutOptions.Center, FontSize = 20, VerticalOptions = LayoutOptions.Center, TextColor = Color.Black };
            var minusButton = new ImageButton() { Source = countArrow, BackgroundColor = Color.Transparent, Rotation = 180 };
            minusButton.Clicked += (obj, args) =>
            {
                if (subdivideAmount > 2) { subdivideAmount--; numLabel.Text = subdivideAmount.ToString(); }
            };
            var plusButton = new ImageButton() { Source = countArrow, BackgroundColor = Color.Transparent };
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
                    var button = new ImageButton() { Source = ContentManager.transIcon, BackgroundColor = Color.Transparent, Aspect = Aspect.Fill };
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
                var button = new ImageButton() { Source = ContentManager.transIcon, BackgroundColor = Color.Transparent,
                    Aspect = Aspect.Fill,BorderColor = Color.Blue, BorderWidth = 5 };
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
        protected void SubdivideCell(int amount)
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
                var button = new ImageButton() { Source = ContentManager.transIcon, BackgroundColor = Color.Transparent,
                    Aspect = Aspect.Fill, WidthRequest = parentWidth / amount };
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
        protected void MergeCell()
        {
            for (int i = ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children.Count - 1; i >= 1; i--)
            {
                ContentManager.fridgeInfo[nameLegacy][selectedCellIndex].Children.RemoveAt(i);
            }
            ContentManager.fridgeItemBase[nameLegacy].Remove(selectedCellIndex);

            var index = selectedCellIndex;
            var buttonIndex = ContentManager.fridgeInfo[nameLegacy][index].Children.Count - 1;
            var button = new ImageButton() { Source = ContentManager.transIcon, BackgroundColor = Color.Transparent, Aspect = Aspect.Fill };

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
            base.ConfirmationCancelEvent(finishEvent);
            bool cancelConfirmed = await ContentManager.pageController.DisplayAlert("Confirmation", "Do you want to discard all current changes?", "Discard", "Cancel");
            if (cancelConfirmed) 
            {
                if(ContentManager.fridgeInfo[nameLegacy].Count == 0) { ContentManager.fridgeInfo.Remove(nameLegacy); }
                if (preSaveState != null)
                {
                    ContentManager.fridgeInfo[nameLegacy] = preSaveState;
                }
                else
                {
                    ContentManager.fridgeInfo.Remove(nameLegacy);
                    ContentManager.fridgeItemBase.Remove(nameLegacy);
                }
                finishEvent.Invoke(); 
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
    }
}
