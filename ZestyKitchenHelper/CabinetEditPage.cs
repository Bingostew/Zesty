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
        protected const int side_margin = 8;
        protected const int bottom_margin = 10;
        protected const int top_margin = 5;
        protected const int name_height = 50;
        protected const double storage_height_proportional = 0.75;
        protected const int asset_grid_spacing = 10;

        protected ImageSource cabinetCellImage = ContentManager.cabinetCellIcon;
        protected ImageSource cabinetDividerLeft = "cabinet_divider_left.png";
        protected ImageSource cabinetDividerMiddle = "cabinet_divider_middle.png";
        protected ImageSource cabinetDividerRight = "cabinet_divider_right.png";
        protected ImageSource subdivideIcon = "subdivide.png";
        protected ImageSource mergeIcon = "merge.png";
        protected ImageSource countArrow = "small_arrow.png";

        protected Grid assetGrid;
        protected Grid storageGrid;

        protected StackLayout pageContent = new StackLayout() { BackgroundColor = ContentManager.ThemeColor };

        protected Action<string> storageSaveLocalEvent, storageSaveBaseEvent;
        protected int selectedCellIndex = -1;

        protected string nameLegacy;
        protected string name;

        protected abstract string cellImageSource { get; }
        public EditPage(bool newShelf, string storageName = "")
        {
            name = storageName;
            nameLegacy = storageName;
            var nameEntry = new Entry() { Text = "untitled", Margin = new Thickness(side_margin, top_margin, side_margin, 0), HeightRequest = name_height };
            nameEntry.TextChanged += (obj, args) => name = args.NewTextValue;
            nameEntry.Completed += (obj, arg) => {
                SaveStorageInfo();
            };


            SetNewStorage(newShelf);

            var saveGrid = new Grid()
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                VerticalOptions = LayoutOptions.EndAndExpand,
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition()
                }
            };

            var saveButton = new Button() { Text = "Save", BackgroundColor = Color.WhiteSmoke, TextColor = Color.Black, BorderColor = Color.Black, BorderWidth = 1,
                WidthRequest = ContentManager.screenWidth / 2, Margin = new Thickness(5)};
            var cancelButton = new Button() { Text = "Exit", BackgroundColor = Color.WhiteSmoke, TextColor = Color.Black, BorderColor = Color.Black, BorderWidth = 1,
                WidthRequest = ContentManager.screenWidth / 2, Margin = new Thickness(5) };
            saveButton.Clicked += (obj, args) => ConfirmationSaveEvent(() =>  ContentManager.pageController.ReturnToPrevious()  );
            cancelButton.Clicked +=(obj, args) => ConfirmationCancelEvent(() =>  ContentManager.pageController.ReturnToPrevious() );

            saveGrid.Children.Add(saveButton, 0, 0);
            saveGrid.Children.Add(cancelButton, 1, 0);

            pageContent.Children.Add(nameEntry);

            if (!newShelf) 
                SaveGridState(name);
            
            SetBasicView(newShelf);

            pageContent.Children.Add(saveGrid);

            Content = pageContent;
        }

        protected abstract void SetNewStorage(bool newShelf);

        protected virtual void SetBasicView(bool newShelf)
        {
            storageGrid.WidthRequest = ContentManager.screenWidth;
            storageGrid.HeightRequest = ContentManager.screenHeight * storage_height_proportional;
            storageGrid.BackgroundColor = Color.SaddleBrown;
            storageGrid.Margin = new Thickness(side_margin, top_margin, side_margin, bottom_margin);

            pageContent.Children.Add(GetAssetGrid());
            pageContent.Children.Add(storageGrid);
        }
        protected abstract void SaveGridState(string name);

        protected abstract Grid GetAssetGrid();
        protected abstract void SaveStorageInfo();


        public abstract void DeleteStorage();

        protected abstract void ConfirmationSaveEvent(Action finishEditEvent);
        protected virtual void ConfirmationCancelEvent(Action finishEditEvent)
        {
            IDGenerator.DeleteIDGroup(name);
        }

        protected abstract void ResetCell();
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
        protected override void SetNewStorage(bool newShelf)
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
                IDGenerator.InitializeIDGroup(name);
                Console.WriteLine("CabinetEdit 147 ID set " + name);

                AddCell(new Vector2D<int>(0, 0));

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
                StorageCell cellCopy = new StorageCell().SetStorageCell(cell.GetPosition(), cell.Index, cabinetName, storageGrid, "none", cell.ColumnSpan, cell.RowSpan);
                List<View> childrenCopy = new List<View>();

                // cycle through each child to copy.
                foreach (var child in cell.GetChildren())
                {
                    childrenCopy.Add(child);
                }

                // put copied children into copied grid
                cellCopy.AddItem(childrenCopy);
                GridManager.AddGridItemAtPosition(gridCopy, cellCopy.GetChildren(), cellCopy.GetPosition());
            }
           
            initialCabinetState = gridCopy;
        }

        // Set up tool bar 
        protected override Grid GetAssetGrid()
        {
            Grid assetGrid = new Grid()
            {
                Margin = new Thickness(side_margin, bottom_margin),
                RowSpacing = asset_grid_spacing,
                RowDefinitions =
                {
                    new RowDefinition() {Height = 50 },
                    new RowDefinition() {Height = 20}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition(){Width = 2 },
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition(){Width = 2 },
                    new ColumnDefinition()
                }
            };

            var dividerImage = new Image() { BackgroundColor = Color.Gray };
            var subdivideButton = new ImageButton() { Source = subdivideIcon, BackgroundColor = Color.Transparent, };
            subdivideButton.Clicked += (object obj, EventArgs args) => { if (CanTransform()) { SubdivideCellHorizontal(); } };
            var subdivideLabel = new Label() { Text = "Subdivide", HorizontalTextAlignment = TextAlignment.Center, FontSize = 15 };
            var subdivideVerticalButton = new ImageButton() { Source = subdivideIcon, Rotation = 90, BackgroundColor = Color.Transparent, };
            subdivideVerticalButton.Clicked += (object obj, EventArgs args) => { if (CanTransform()) { SubdivideCellVertical(); } };
            var subdivideDivider = new BoxView() { BackgroundColor = Color.Gray };

            var mergeColumnButton = new ImageButton() { Source = mergeIcon, Rotation = 90, BackgroundColor = Color.Transparent, };
            mergeColumnButton.Clicked += (obk, args) => { if (CanTransform()) { MergeCellVertical(); } };
            var mergeLabel = new Label() { Text = "Merge", HorizontalTextAlignment = TextAlignment.Center, FontSize = 15 };
            var mergeRowButton = new ImageButton() { Source = mergeIcon, BackgroundColor = Color.Transparent, };
            mergeRowButton.Clicked += (obj, args) => { if (CanTransform()) MergeCellHorizontal(); };
            var mergeDivider = new BoxView() { BackgroundColor = Color.Gray };

            var deleteButton = new ImageButton() { Source = ContentManager.deleteCellIcon, BackgroundColor = Color.Transparent, };
            deleteButton.Clicked += (obj, args) => { if (CanTransform()) ResetCell(); };
            var deleteLabel = new Label() { Text = "Reset", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };

            assetGrid.Children.Add(subdivideButton, 0, 0);
            assetGrid.Children.Add(subdivideLabel, 0, 1);
            Grid.SetColumnSpan(subdivideLabel, 2);
            assetGrid.Children.Add(subdivideVerticalButton, 1, 0);
            assetGrid.Children.Add(subdivideDivider, 2, 0);
            Grid.SetRowSpan(subdivideDivider, 2);

            assetGrid.Children.Add(mergeColumnButton, 3, 0);
            assetGrid.Children.Add(mergeLabel, 3, 1);
            Grid.SetColumnSpan(mergeLabel, 2);
            assetGrid.Children.Add(mergeRowButton, 4, 0);
            assetGrid.Children.Add(mergeDivider, 5, 0);
            Grid.SetRowSpan(mergeDivider, 2);

            assetGrid.Children.Add(deleteButton, 6, 0);
            assetGrid.Children.Add(deleteLabel, 6, 1);
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
                transparentButton.ToggleEffects(new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5), ImagePath = ContentManager.buttonTintImage }, null);
                selectedCellIndex = cellIndex;
            };

            // register cipher of new cell
            StorageCell cell = new StorageCell().SetStorageCell(position, cellIndex, name, storageGrid, "none", columnSpan, rowSpan);
            cabinet.AddGridCell(cellIndex, cell);
            // register children of new cell
            cabinet.AddGridCellUI(cellIndex, cabinetCellBackground, transparentButton);
            // set the column and row span of cell children
            columnSpan = columnSpan < 1 ? 1 : columnSpan;
            rowSpan = rowSpan < 1 ? 1 : rowSpan;
            cell.SetColumnSpan(columnSpan);
            cell.SetRowSpan(rowSpan);
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
            cabinet.GetGridCell(selectedCellIndex).GetButton().RemoveEffect(typeof(ImageTint));
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
                    && compareLayerPositionGetter(pos) - 1 == compareLayerPositionGetter(selectedCell.GetPosition()) && compareLayerPositionGetter(pos) < closestLayer)
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
            cabinet.GetGridCell(selectedCellIndex).GetButton().RemoveEffect(typeof(ImageTint));
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

        protected override void ResetCell()
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
            AddCell(new Vector2D<int>(0, 0));
        }

        public override void DeleteStorage()
        {
            ContentManager.CabinetMetaBase.Remove(name);
        }

        protected override async void ConfirmationCancelEvent(Action finishEvent)
        {
            ContentManager.pageController.ShowAlert("Confirmation", "Do you want to discard all changes?", "Discard", "Cancel", () =>
            {
                if (initialCabinetState != null)
                    ContentManager.CabinetMetaBase[nameLegacy].MainGrid = initialCabinetState;
                else
                {
                    ContentManager.CabinetMetaBase.Remove(nameLegacy);
                }

                finishEvent.Invoke();
            }, null);
        }

        protected async override void ConfirmationSaveEvent(Action finishEvent)
        {
            ContentManager.pageController.ShowAlert("Confirmation", "Do you want to save all changes?", "Save", "Cancel", () =>
            {
                storageGrid.Children.RemoveEffects(typeof(ImageTint));

                SaveStorageInfo();
                finishEvent.Invoke();
            }, null);
        }

        protected override void SaveStorageInfo()
        {
            if (nameLegacy != name)
            {
                ContentManager.CabinetMetaBase.Remove(nameLegacy);
                ContentManager.CabinetMetaBase.Add(name, cabinet);
            }
            nameLegacy = name;

            if(ContentManager.isLocal)
                storageSaveLocalEvent(name);
            else
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
        private const double main_fridge_width_percentage = 0.6;
        private const double fridge_grid_spacing = 12;
        protected override string cellImageSource => ContentManager.fridgeIcon;

        private StorageCell selectedStorageCell;

        private Grid leftSideStorageGrid;
        private Grid rightSideStorageGrid;

        private Fridge fridge;
        private Grid[] initialFridgeState; // the cabinet at the beginning of the edit, in the case where user discards all changes.

        public FridgeEditPage(bool newShelf, Action<string> saveFridgeLocalEvent, Action<string> saveFridgeBaseEvent, string storageName = "")
            : base(newShelf, storageName)
        {
            storageSaveLocalEvent = saveFridgeLocalEvent;
            storageSaveBaseEvent = saveFridgeBaseEvent;
        }

        private string GetGridTypeName(Grid grid)
        {
            if(grid == leftSideStorageGrid)
            {
                return "Left";
            }
            else if (grid == rightSideStorageGrid)
            {
                return "Right";
            }
            return "Main";
        }
        protected override void SetBasicView(bool newShelf)
        {
            base.SetBasicView(newShelf);
            if (newShelf)
            {
                AddCell(new Vector2D<int>(0, 0), leftSideStorageGrid);
                AddCell(new Vector2D<int>(0, 0), storageGrid);
                AddCell(new Vector2D<int>(0, 0), rightSideStorageGrid);
            }

            Grid containerGrid = new Grid()
            {
                ColumnSpacing = fridge_grid_spacing,
                RowDefinitions = new RowDefinitionCollection()
                {
                    new RowDefinition()
                },
                ColumnDefinitions = new ColumnDefinitionCollection()
                {
                    new ColumnDefinition() { Width = GridLength.Star },
                    new ColumnDefinition(){Width = main_fridge_width_percentage * ContentManager.screenWidth },
                    new ColumnDefinition(){Width = GridLength.Star}
                }
            };

            GridManager.AddGridItem(containerGrid, new List<View>() { leftSideStorageGrid, storageGrid, rightSideStorageGrid }, false);

            pageContent.Children.Add(containerGrid);
        }

        protected override void SetNewStorage(bool newFridge)
        {
            if (newFridge)
            {
                int id = IDGenerator.GetID(ContentManager.fridgeEditIdGenerator);
                storageGrid = GridManager.InitializeGrid("fridge" + id, 0, 0, GridLength.Star, GridLength.Star);
                leftSideStorageGrid = GridManager.InitializeGrid("fridge left" + id, 0, 0, GridLength.Star, GridLength.Star);
                leftSideStorageGrid.Margin = new Thickness(side_margin, top_margin, 0, bottom_margin);
                rightSideStorageGrid = GridManager.InitializeGrid("fridge right" + id, 0, 0, GridLength.Star, GridLength.Star);
                rightSideStorageGrid.Margin = new Thickness(0, top_margin, side_margin, bottom_margin);
                leftSideStorageGrid.RowSpacing = 0;
                leftSideStorageGrid.ColumnSpacing = 0;
                rightSideStorageGrid.RowSpacing = 0;
                rightSideStorageGrid.ColumnSpacing = 0;
                storageGrid.RowSpacing = 0;
                storageGrid.ColumnSpacing = 0;

                name = "untitled fridge " + id;
                fridge = new Fridge().SetFridge(name, storageGrid, leftSideStorageGrid, rightSideStorageGrid, id);
                nameLegacy = name;
                IDGenerator.InitializeIDGroup(name);

                ContentManager.FridgeMetaBase.Add(name, fridge);
            }
            else
            {
                storageGrid = ContentManager.GetFridgeView(name) as Grid;
            }
        }

        protected override void SaveGridState(string fridgeName)
        {
            Fridge fridge = ContentManager.FridgeMetaBase[fridgeName];
            Grid mainGridCopy = new Grid();
            Grid leftGridCopy = new Grid();
            Grid rightGridCopy = new Grid();

            // cycle through each cell of the grid to retrieve and copy info
            foreach (var cell in fridge.GetGridCells())
            {
                // create copies of cells and childlist.
                StorageCell cellCopy = new StorageCell().SetStorageCell(cell.GetPosition(), cell.Index, fridgeName, cell.GetParentGrid(), 
                    GetGridTypeName(cell.GetParentGrid()),  cell.ColumnSpan, cell.RowSpan);
                List<View> childrenCopy = new List<View>();

                // cycle through each child to copy.
                foreach (var child in cell.GetChildren())
                {
                    childrenCopy.Add(child);
                }

                // put copied children into copied grid
                cellCopy.AddItem(childrenCopy);
                if (cell.GetParentGrid() == storageGrid)
                {
                    GridManager.AddGridItemAtPosition(mainGridCopy, cellCopy.GetChildren(), cellCopy.GetPosition());
                }
                else if(cell.GetParentGrid() == leftSideStorageGrid)
                {
                    GridManager.AddGridItemAtPosition(leftGridCopy, cellCopy.GetChildren(), cellCopy.GetPosition());
                }
                else
                {
                    GridManager.AddGridItemAtPosition(rightGridCopy, cellCopy.GetChildren(), cellCopy.GetPosition());
                }
            }
            initialFridgeState = new Grid[3];
            initialFridgeState[0] = mainGridCopy;
            initialFridgeState[1] = leftGridCopy;
            initialFridgeState[2] = rightGridCopy;
        }

        // Set up tool bar 
        protected override Grid GetAssetGrid()
        {
            Grid assetGrid = new Grid()
            {
                Margin = new Thickness(side_margin, top_margin),
                RowSpacing = asset_grid_spacing,
                RowDefinitions =
                {
                    new RowDefinition() {Height = 50 },
                    new RowDefinition() {Height = 20}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition(){Width = 2 },
                    new ColumnDefinition(),
                    new ColumnDefinition(),
                    new ColumnDefinition(){Width = 2 },
                    new ColumnDefinition()
                }
            };

            var dividerImage = new Image() { BackgroundColor = Color.Gray };
            var subdivideButton = new ImageButton() { Source = subdivideIcon, BackgroundColor = Color.Transparent, };
            subdivideButton.Clicked += (object obj, EventArgs args) => { if (CanTransform()) { SubdivideCellHorizontal(); } };
            var subdivideLabel = new Label() { Text = "Subdivide", HorizontalTextAlignment = TextAlignment.Center, FontSize = 15 };
            var subdivideVerticalButton = new ImageButton() { Source = subdivideIcon, Rotation = 90, BackgroundColor = Color.Transparent, };
            subdivideVerticalButton.Clicked += (object obj, EventArgs args) => { if (CanTransform()) { SubdivideCellVertical(); } };
            var subdivideDivider = new BoxView() { BackgroundColor = Color.Gray };

            var mergeColumnButton = new ImageButton() { Source = mergeIcon, Rotation = 90, BackgroundColor = Color.Transparent, };
            mergeColumnButton.Clicked += (obk, args) => { if (CanTransform()) { MergeCellVertical(); } };
            var mergeLabel = new Label() { Text = "Merge", HorizontalTextAlignment = TextAlignment.Center, FontSize = 15 };
            var mergeRowButton = new ImageButton() { Source = mergeIcon, BackgroundColor = Color.Transparent, };
            mergeRowButton.Clicked += (obj, args) => { if (CanTransform()) MergeCellHorizontal(); };
            var mergeDivider = new BoxView() { BackgroundColor = Color.Gray };

            var deleteButton = new ImageButton() { Source = ContentManager.deleteCellIcon, BackgroundColor = Color.Transparent, };
            deleteButton.Clicked += (obj, args) => { if (CanTransform()) ResetCell(); };
            var deleteLabel = new Label() { Text = "Reset", HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };


            assetGrid.Children.Add(subdivideButton, 0, 0);
            assetGrid.Children.Add(subdivideLabel, 0, 1);
            Grid.SetColumnSpan(subdivideLabel, 2);
            assetGrid.Children.Add(subdivideVerticalButton, 1, 0);
            assetGrid.Children.Add(subdivideDivider, 2, 0);
            Grid.SetRowSpan(subdivideDivider, 2);

            assetGrid.Children.Add(mergeColumnButton, 3, 0);
            assetGrid.Children.Add(mergeLabel, 3, 1);
            Grid.SetColumnSpan(mergeLabel, 2);
            assetGrid.Children.Add(mergeRowButton, 4, 0);
            assetGrid.Children.Add(mergeDivider, 5, 0);
            Grid.SetRowSpan(mergeDivider, 2);

            assetGrid.Children.Add(deleteButton, 6, 0);
            assetGrid.Children.Add(deleteLabel, 6, 1);
            return assetGrid;
        }
        protected void AddCell(Vector2D<int> position, Grid grid, int columnSpan = 1, int rowSpan = 1)
        {
            // button for touch indication, image of cabinet
            Image fridgeCellBackground = new Image() { Source = cellImageSource, Aspect = Aspect.Fill };
            ImageButton transparentButton = new ImageButton() { Source = ContentManager.transIcon, BackgroundColor = Color.Transparent, Aspect = Aspect.Fill };

            // retrieve an ID for the new row
            int cellIndex = IDGenerator.GetID(name);

            // add items to the new position
            GridManager.AddGridItemAtPosition(grid, new List<View>() { fridgeCellBackground, transparentButton }, position);

            // register cipher of new cell
            StorageCell cell = new StorageCell().SetStorageCell(position, cellIndex, name, grid, GetGridTypeName(grid), columnSpan, rowSpan);
            fridge.AddGridCell(cellIndex, cell);

            // registering tint event on transparent button
            transparentButton.Clicked += (o, a) =>
            {
                if(selectedStorageCell != null)
                    selectedStorageCell.GetButton().RemoveEffect(typeof(ImageTint));
                selectedStorageCell = cell;
                selectedCellIndex = cellIndex;
                selectedStorageCell.GetButton().ToggleEffects(new ImageTint() { tint = Color.FromHsla(1, .1, .5, .5), ImagePath = ContentManager.buttonTintImage }, null);
            };

            // register children of new cell
            fridge.AddGridCellUI(cellIndex, fridgeCellBackground, transparentButton);
            // set the column and row span of cell children
            columnSpan = columnSpan < 1 ? 1 : columnSpan;
            rowSpan = rowSpan < 1 ? 1 : rowSpan;
            cell.SetColumnSpan(columnSpan);
            cell.SetRowSpan(rowSpan);
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
            if (selectedCellIndex < 0 || selectedStorageCell == null)
                return;

            StorageCell selectedCell = fridge.GetGridCell(selectedCellIndex);
            Vector2D<int> position = selectedCell.GetPosition();

            // For horizontal, cycle through all cells and retrieve the number of columns for the given row
            // For vertical, cycle through all cells and retrieve the number row for each column
            int layerInLayerCount = 0;
            foreach (StorageCell cell in fridge.GetGridCells())
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

            foreach (int id in fridge.GetGridIDs())
            {
                StorageCell cell = fridge.GetGridCell(id);
                //Check to see if the cell's parent corresponds with the selected cell's parent
                if (cell.GetParentGrid() == selectedCell.GetParentGrid())
                {
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
                        GridManager.AddGridItemAtPosition(selectedStorageCell.GetParentGrid(), children, newPosition);
                        cell.SetPosition(newPosition);
                        // If unaffected by subdivision, then double row/column span. Else, maintain row/column span.
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
            }

            // add the new cell after subdivision
            Vector2D<int> newCellPosition = getNewPosition(position.X, position.Y,
                (int)(compareLayerPositionGetter(position) / (float)oldLayerCount * newLayerCount) + defaultLayerSpan);
            addCell(newCellPosition, defaultLayerSpan, selectedCell);
            
            // reset cell index and remove tints
            selectedCellIndex = -1;
            selectedStorageCell.GetButton().RemoveEffect(typeof(ImageTint));
        }

        protected void SubdivideCellHorizontal()
        {
            SubdivideCell(() => selectedStorageCell.GetParentGrid().ColumnDefinitions.Count, v => v.Y, v => v.X, c => c.ColumnSpan,
                () => selectedStorageCell.GetParentGrid().ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star }), (c, i) => c.SetColumnSpan(i), c => c.SetRowSpan(c.RowSpan),
                (x, y, p) => new Vector2D<int>(p, y), (v, s, c) => AddCell(v, selectedStorageCell.GetParentGrid(), s, c.RowSpan));
        }

        protected void SubdivideCellVertical()
        {
            SubdivideCell(() => selectedStorageCell.GetParentGrid().RowDefinitions.Count, v => v.X, v => v.Y, c => c.RowSpan,
                () => selectedStorageCell.GetParentGrid().RowDefinitions.Add(new RowDefinition { Height = GridLength.Star }), (c, i) => c.SetRowSpan(i), c => c.SetColumnSpan(c.ColumnSpan),
                 (x, y, p) => new Vector2D<int>(x, p), (v, s, c) => AddCell(v, selectedStorageCell.GetParentGrid(), c.ColumnSpan, s));
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
            if (selectedCellIndex < 0 || selectedStorageCell == null)
                return;
            StorageCell selectedCell = fridge.GetGridCell(selectedCellIndex);
            StorageCell nextCell = null;
            int closestLayer = layerCount;

            foreach (int id in fridge.GetGridIDs())
            {
                StorageCell cell = fridge.GetGridCell(id);
                // Check to see if the cell's parent grid corresponds with the selected parent grid
                if (cell.GetParentGrid() == selectedStorageCell.GetParentGrid())
                {
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
            }

            // If no cell found, do not execute
            if (nextCell == null)
                return;

            // Remove children from cell, then remove cell from grid
            foreach (View child in nextCell.GetChildren())
            {
                selectedStorageCell.GetParentGrid().Children.Remove(child);
            }
            selectedStorageCell.GetParentGrid().Children.Remove(nextCell.GetButton());
            selectedStorageCell.GetParentGrid().Children.Remove(nextCell.GetBackground());
            fridge.RemoveGridCell(nextCell.Index);


            // add the column/row span of both cells to merge
            setLayerSpan(selectedCell, mergeLayerSpanGetter(selectedCell) + mergeLayerSpanGetter(nextCell));
            updateControlSpan(selectedCell);
            // reset cell index and remove tints
            selectedCellIndex = -1;
            selectedStorageCell.GetButton().RemoveEffect(typeof(ImageTint));
        }

        protected void MergeCellHorizontal()
        {
            MergeCell(selectedStorageCell.GetParentGrid().ColumnDefinitions.Count, v => v.Y, v => v.X, c => c.RowSpan, c => c.ColumnSpan, 
                (c, i) => c.SetColumnSpan(i), c => c.SetRowSpan(c.RowSpan));

        }

        protected void MergeCellVertical()
        {
            MergeCell(selectedStorageCell.GetParentGrid().RowDefinitions.Count, v => v.X, v => v.Y, c => c.ColumnSpan, c => c.RowSpan, 
                (c, i) => c.SetRowSpan(i), c => c.SetColumnSpan(c.ColumnSpan));
        }

        protected override void ResetCell()
        {
            foreach (int index in fridge.GetGridIDs())
            {
                var cell = fridge.GetGridCell(index);
                Grid grid = cell.GetParentGrid();

                grid.Children.Remove(cell.GetButton());
                grid.Children.Remove(cell.GetBackground());
                fridge.RemoveGridCell(index);
            }


            storageGrid.RowDefinitions.Clear();
            storageGrid.ColumnDefinitions.Clear();
            leftSideStorageGrid.RowDefinitions.Clear();
            leftSideStorageGrid.ColumnDefinitions.Clear();
            rightSideStorageGrid.RowDefinitions.Clear();
            rightSideStorageGrid.ColumnDefinitions.Clear();

            AddCell(new Vector2D<int>(0, 0), storageGrid);
            AddCell(new Vector2D<int>(0, 0), leftSideStorageGrid);
            AddCell(new Vector2D<int>(0, 0), rightSideStorageGrid);
        }

        public override void DeleteStorage()
        {
            ContentManager.FridgeMetaBase.Remove(name);
        }

        protected override async void ConfirmationCancelEvent(Action finishEvent)
        {
            ContentManager.pageController.ShowAlert("Confirmation", "Do you want to discard all changes?", "Discard", "Cancel", () =>
            {
                if (initialFridgeState != null)
                {
                    ContentManager.FridgeMetaBase[nameLegacy].MainGrid = initialFridgeState[0];
                    ContentManager.FridgeMetaBase[nameLegacy].LeftGrid = initialFridgeState[1];
                    ContentManager.FridgeMetaBase[nameLegacy].RightGrid = initialFridgeState[2];
                }
                else
                {
                    ContentManager.FridgeMetaBase.Remove(nameLegacy);
                }

                finishEvent.Invoke();
            }, null);
        }

        protected async override void ConfirmationSaveEvent(Action finishEvent)
        {
            ContentManager.pageController.ShowAlert("Confirmation", "Do you want to save all changes?", "Save", "Cancel", () =>
            {
                storageGrid.Children.RemoveEffects(typeof(ImageTint));
                leftSideStorageGrid.Children.RemoveEffects(typeof(ImageTint));
                rightSideStorageGrid.Children.RemoveEffects(typeof(ImageTint));

                SaveStorageInfo();
                finishEvent.Invoke();
            }, null);
        }

        protected override void SaveStorageInfo()
        {
            if (nameLegacy != name)
            {
                ContentManager.FridgeMetaBase.Remove(nameLegacy);
                ContentManager.FridgeMetaBase.Add(name, fridge);
            }
            nameLegacy = name;

            if (ContentManager.isLocal)
                storageSaveLocalEvent(name);
            else
                storageSaveBaseEvent(name);
        }
    }
}
