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

        protected ImageSource cabinetCellImage = ContentManager.cabinetCellIcon;
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

        protected Action<string> storageSaveLocalEvent, storageSaveBaseEvent;
        protected int selectedCellIndex = -1;

        protected string nameLegacy;
        protected string name;
        protected double screenWidth = Application.Current.MainPage.Width;
        protected double screenHeight = Application.Current.MainPage.Width;

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
                SaveGridState(name);
            
            SetBasicView(newShelf);

            pageContent.Children.Add(saveGrid);

            Content = pageContent;
        }

        protected abstract void SetNewShelf(bool newShelf);

        protected virtual void SetBasicView(bool newShelf)
        {
            storageGrid.WidthRequest = screenWidth;
            storageGrid.HeightRequest = 7 * screenWidth / 8;
            storageGrid.BackgroundColor = Color.SaddleBrown;
            if(newShelf) AddCellRow();

            addRowButton.Clicked += (object obj, EventArgs args) => AddCellRow();

            pageContent.Children.Add(GetAssetGrid());
            pageContent.Children.Add(storageGrid);
            pageContent.Children.Add(addRowButton);
        }
        protected abstract void SaveGridState(string name);

        protected abstract Grid GetAssetGrid();
        protected abstract void StoreCabinetInfo();


        public abstract void DeleteStorage();
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
        protected override string cellImageSource => ContentManager.cabinetCellIcon;

        private Cabinet cabinet;
        private Grid initialCabinetState; // the cabinet at the beginning of the edit, in the case where user discards all changes.

        public CabinetEditPage(bool newShelf, Action<string> saveCabinetLocalEvent, Action<string> saveCabinetBaseEvent, string storageName = "")
            : base(newShelf, storageName)
        {
            storageSaveLocalEvent = saveCabinetLocalEvent;
            storageSaveBaseEvent = saveCabinetBaseEvent;
        }
        protected override void SetNewShelf(bool newShelf)
        {
            if (newShelf)
            {

                int id = IDGenerator.GetID(ContentManager.cabinetEditIdGenerator);
                storageGrid = GridManager.InitializeGrid("cabinet" + id, 0, 0, GridLength.Star, GridLength.Star);
                storageGrid.RowSpacing = 0;
                storageGrid.ColumnSpacing = 0;

                name = "untitled shelf " + id;
                cabinet = new Cabinet().SetCabinet(name, storageGrid, id);
                nameLegacy = name;

                ContentManager.CabinetMetaBase.Add(name, cabinet);
            }
            else
            {
                storageGrid = ContentManager.GetCabinetView(name) as Grid;
            }
        }

        protected override void SaveGridState(string cabinetName)
        {
            Cabinet cabinet = ContentManager.CabinetMetaBase[cabinetName];
            Grid gridCopy = new Grid();

            // cycle through each cell of the grid to retrieve and copy info
            foreach(var cell in cabinet.GetGridCells())
            {
                // create copies of cells and childlist.
                StorageCell cellCopy = new StorageCell().SetStorageCell(cell.GetPosition(), cell.Index, cabinetName, cell.ColumnSpan, cell.RowSpan);
                List<View> childrenCopy = new List<View>();

                // cycle through each child to copy.
                foreach (var child in cell.GetChildren())
                {
                    childrenCopy.Add(child);
                }

                // put copied children into copied grid
                cellCopy.SetChildren(childrenCopy);
                GridManager.AddGridItemAtPosition(gridCopy, cellCopy.GetChildren(), cellCopy.GetPosition());
            }
           
            initialCabinetState = gridCopy;
        }

        // Set up tool bar 
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
            StorageCell cell = new StorageCell().SetStorageCell(position, cellIndex, name, columnSpan, rowSpan);
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
            Vector2D<int> position = selectedCell.GetPosition();

            // For horizontal, cycle through all cells and retrieve the number of columns for the given row
            // For vertical, cycle through all cells and retrieve the number row for each column
            int layerInLayerCount = 0;
            foreach (StorageCell cell in cabinet.GetGridCells())
            {
                Vector2D<int> pos = cell.GetPosition();
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
                Vector2D<int> pos = cell.GetPosition();
                bool isSelectedCell = pos.X == position.X && pos.Y == position.Y;

                // retrieve the children of a cell
                List<View> children = cell.GetChildren();
                children.Add(cell.GetBackground());
                children.Add(cell.GetButton());

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
                    cell.SetPosition(newPosition);
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
            SubdivideCell(() => storageGrid.ColumnDefinitions.Count, v => v.Y, v => v.X, c => c.ColumnSpan,
                () => storageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }), (c, i) => c.SetColumnSpan(i), c => c.SetRowSpan(c.RowSpan),
                (x, y, p) => new Vector2D<int>(p, y),(v, s, c) => AddCell(v, s, c.RowSpan));
        }

        protected void SubdivideCellVertical()
        {
            SubdivideCell(() => storageGrid.RowDefinitions.Count, v => v.X, v => v.Y, c => c.RowSpan,
                () => storageGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }), (c, i) => c.SetRowSpan(i), c => c.SetColumnSpan(c.ColumnSpan),
                 (x, y, p) => new Vector2D<int>(x, p), (v, s, c) => AddCell(v, c.ColumnSpan, s));
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
                Vector2D<int> pos = cell.GetPosition();

                // constrain: (If for horizontal) same row, column index must be greater than selected cell column index, row span of both cells are equal,
                // column index must be smaller than previous candidate's column index.
                // (Same logic for vertical)
                if (sameLayerPositionGetter(pos) == sameLayerPositionGetter(selectedCell.GetPosition()) && sameLayerSpanGetter(cell) == sameLayerSpanGetter(selectedCell) 
                    && compareLayerPositionGetter(pos) > compareLayerPositionGetter(selectedCell.GetPosition()) && compareLayerPositionGetter(pos) < closestLayer)
                {
                    closestLayer = compareLayerPositionGetter(pos);
                    nextCell = cell;
                }
            }

            // If no cell found, do not execute
            if (nextCell == null)
                return;

            // Remove children from cell, then remove cell from grid
            foreach (View child in nextCell.GetChildren())
            {
                storageGrid.Children.Remove(child);
            }
            storageGrid.Children.Remove(nextCell.GetButton());
            storageGrid.Children.Remove(nextCell.GetBackground());
            cabinet.RemoveGridCell(nextCell.Index);
            

            // add the column/row span of both cells to merge
            setLayerSpan(selectedCell, mergeLayerSpanGetter(selectedCell) + mergeLayerSpanGetter(nextCell));
            updateControlSpan(selectedCell);
            // reset cell index
            selectedCellIndex = -1;
        }

        protected void MergeCellHorizontal()
        {
            MergeCell(storageGrid.ColumnDefinitions.Count, v => v.Y, v => v.X, c => c.RowSpan, c => c.ColumnSpan, (c, i) => c.SetColumnSpan(i), c => c.SetRowSpan(c.RowSpan));
         
        }

        protected void MergeCellVertical()
        {
            MergeCell(storageGrid.RowDefinitions.Count, v => v.X, v => v.Y, c => c.ColumnSpan, c => c.RowSpan, (c, i) => c.SetRowSpan(i), c => c.SetColumnSpan(c.ColumnSpan));
        }

        protected override void DeleteCell()
        {
            foreach (int index in cabinet.GetGridIDs())
            {
                var cell = cabinet.GetGridCell(index);
                foreach (View child in cell.GetChildren())
                {
                    storageGrid.Children.Remove(child);
                }
                storageGrid.Children.Remove(cell.GetButton());
                storageGrid.Children.Remove(cell.GetBackground());
                cabinet.RemoveGridCell(index);
            }
            storageGrid.RowDefinitions.Clear();
            storageGrid.ColumnDefinitions.Clear();
            AddCellRow();
        }

        public override void DeleteStorage()
        {
            ContentManager.CabinetMetaBase.Remove(name);
        }

        protected override async void ConfirmationCancelEvent(Action finishEvent)
        {
            base.ConfirmationCancelEvent(finishEvent);
            bool cancelConfirmed = await ContentManager.pageController.DisplayAlert("Confirmation", "Do you want to discard all current changes?", "Discard", "Cancel");
            if (cancelConfirmed) 
            {
                if (initialCabinetState != null)
                    ContentManager.CabinetMetaBase[nameLegacy].Grid = initialCabinetState;
                else
                {
                    ContentManager.CabinetMetaBase.Remove(nameLegacy);
                }
                    
                finishEvent.Invoke(); 
            }
        }

        protected async override void ConfirmationSaveEvent(Action finishEvent)
        {
            bool saveConfirmed = await ContentManager.pageController.DisplayAlert("Confirmation", "Do you want to save all changes?", "Save", "Cancel");
            if (saveConfirmed)
            {
                storageGrid.Children.RemoveEffects(typeof(ImageTint));

                StoreCabinetInfo();
                finishEvent.Invoke();
            }

        }

        protected override void StoreCabinetInfo()
        {
            if (nameLegacy != name)
            {
                ContentManager.CabinetMetaBase.Remove(nameLegacy);
                ContentManager.CabinetMetaBase.Add(name, cabinet);
            }
            nameLegacy = name;

            storageSaveLocalEvent(name);
            storageSaveBaseEvent(name);
        }
    }

    //______________________________________________________________________________________________________________________________________________FRIDGE MARKER
    //______________________________________________________________________________________________________________________________________________FRIDGE MARKER
    //______________________________________________________________________________________________________________________________________________FRIDGE MARKER
    //______________________________________________________________________________________________________________________________________________FRIDGE MARKER
    //______________________________________________________________________________________________________________________________________________FRIDGE MARKER
    //______________________________________________________________________________________________________________________________________________FRIDGE MARKER
    //______________________________________________________________________________________________________________________________________________FRIDGE MARKER
    //______________________________________________________________________________________________________________________________________________FRIDGE MARKER
    public class FridgeEditPage : EditPage
    {
        const string untitledName = "untitled fridge ";
        public const string Left_Cell_Tag = "sideLeft";
        public const string Right_Cell_Tag = "sideRight";
        public const double Main_Cell_Width_Div = 2;
        public const double Side_Cell_Width_Div = 5;
        public const double fridge_height = 50;
        protected override string cellImageSource => ContentManager.fridgeIcon;
        public FridgeEditPage(bool newShelf, Action<string> _saveFridgeLocalEvent, Action<string> _saveFridgeBaseEvent, string storageName = "")
            : base(newShelf, storageName)
        {
            storageSaveBaseEvent = _saveFridgeBaseEvent;
            storageSaveLocalEvent = _saveFridgeLocalEvent;
        }

        protected override void SetNewShelf(bool newShelf)
        {/*
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
            }*/
        }

        protected override void SaveGridState(string name)
        {
            
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
            subdivideButton.Clicked += (object obj, EventArgs args) => { if (CanTransform()) { SubdivideCell(); } };
            var subdivideLabel = new Label() { Text = "Div", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var mergeButton = new ImageButton() { Source = mergeIcon, Aspect = Aspect.Fill, BackgroundColor = Color.Transparent,
                WidthRequest = 50, HeightRequest = 50 };
            mergeButton.Clicked += (obj, args) => { if (CanTransform()) { MergeCell(); } };
            var mergeLabel = new Label() { Text = "Merge", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var deleteButton = new ImageButton() { Source = ContentManager.deleteCellIcon, BackgroundColor = Color.Transparent,
                Aspect = Aspect.Fill, WidthRequest = 50, HeightRequest = 50 };
            deleteButton.Clicked += (obj, args) => { if (CanTransform()) { DeleteCell(); } };
            var deleteLabel = new Label() { Text = "Del", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };

            assetGrid.Children.Add(addLeftCellButton, 0, 1);
            assetGrid.Children.Add(deleteLeftCellButton, 1, 1);
            assetGrid.Children.Add(deleteButton, 2, 1);
            assetGrid.Children.Add(mergeButton, 3, 1);
            assetGrid.Children.Add(subdivideButton, 4, 1);
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


        AbsoluteLayout leftCellContainer = new AbsoluteLayout();
        AbsoluteLayout rightCellContainer = new AbsoluteLayout();

        protected async override void AddCellRow() { }
        /*
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

        */
        /*
        private void ConfigureSideCells()
        {
            var height = cellContainer.Children.Count * fridge_height;
            var width = cellContainer.WidthRequest;
            AbsoluteLayout.SetLayoutBounds(leftCellContainer, new Rectangle(0, 0, width / Side_Cell_Width_Div / cellContainer.WidthRequest, fridge_height * (cellContainer.Children.Count - 2) / cellContainer.HeightRequest));
            AbsoluteLayout.SetLayoutBounds(rightCellContainer, new Rectangle(1, 0, width / Side_Cell_Width_Div / cellContainer.WidthRequest, fridge_height * (cellContainer.Children.Count - 2) / cellContainer.HeightRequest));
        }*/
        
        protected void ChangeSideCell(bool left, bool add)
        {/*
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
                    Aspect = Aspect.Fill, BorderColor = Color.Blue, BorderWidth = 5 };
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
            indexer++;*/
        }
        protected void SubdivideCell() { }
        /*
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
        }*/

        protected void MergeCell() { }
        /*
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
        }*/

        protected override void DeleteCell() { }
        /*
        protected override void DeleteCell()
        {
            var index = selectedCellIndex;
            ContentManager.fridgeItemBase[nameLegacy].Remove(selectedCellIndex);
            cellContainer.Children.Remove(ContentManager.fridgeInfo[nameLegacy][selectedCellIndex]);
            ContentManager.fridgeInfo[nameLegacy].Remove(index);
            ConfigureSideCells();
            ReCalculateCellBounds();
        }
        */

        public override void DeleteStorage()
        {

        }

        protected override async void ConfirmationCancelEvent(Action finishEvent)
        {
            base.ConfirmationCancelEvent(finishEvent);
            bool cancelConfirmed = await ContentManager.pageController.DisplayAlert("Confirmation", "Do you want to discard all current changes?", "Discard", "Cancel");
            /*
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
            }*/
        }

        protected override async void ConfirmationSaveEvent(Action finishEvent)
        {
            /*
            bool saveConfirmed = await ContentManager.pageController.DisplayAlert("Confirmation", "Do you want to save all changes?", "Save", "Cancel");
            if (saveConfirmed)
            {
                foreach (var layout in ContentManager.fridgeInfo[nameLegacy].Values)
                {
                    layout.Children.RemoveEffects(typeof(ImageTint));
                }
                StoreCabinetInfo();
                finishEvent?.Invoke();
            }*/
        }
        
        protected override void StoreCabinetInfo()
        {
            /*
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
            storageSaveBaseEvent(name, rowInfo, itemInfo);*/
        }
    }
}
