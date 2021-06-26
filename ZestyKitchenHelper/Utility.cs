using System;
using ZestyKitchenHelper;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using System.Collections;
using SQLite;

namespace Utility
{
    public interface INotificationManager
    {
        event EventHandler NotificationReceived;

        void Initialize();

        int ScheduleNotification(string title, string message);

        void ReceiveNotification(string title, string message);
    }
    public struct Vector2D<T> where T : IComparable
    {
        public T X, Y;
        /// <summary>
        /// Creates a vector of a comparable type
        /// </summary>
        /// <param name="x">First value of vector</param>
        /// <param name="y">Second value of vector</param>
        public Vector2D(T x, T y)
        {
            X = x; Y = y;
        }
    }

    public class IDGenerator
    {
        private static Dictionary<string, List<int>> idBase = new Dictionary<string, List<int>>();

        public static void InitializeIDGroup(string groupName)
        {
            idBase.Add(groupName, new List<int>());
        }

        public static void DeleteIDGroup(string groupName)
        {
            idBase.Remove(groupName);
        }
        public static int GetID(string groupName)
        {
            int newId = 0;
            while (idBase[groupName].Contains(newId))
            {
                newId++;
            }

            idBase[groupName].Add(newId);
            return newId;
        }
        /// <summary>
        /// Forces ID Generator to skip over said ID for the said group.
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="id"> ID to skip over.</param>
        /// <returns></returns>
        public static void SkipID(string groupName, int id)
        {
            if(!idBase[groupName].Contains(id))
                idBase[groupName].Add(id);
        }
    }
    
    public class BarcodeItem
    {
        public ScannedItems[] items;
        public class ScannedItems
        {
            public string title;
        }
    }

    [Table("Item")]
    public class Item
    {
        [PrimaryKey]
        public int ID { get; set; }

        [Column("Expiration Year")]
        public int expYear { get; set; }
        [Column("Expiration Month")]
        public int expMonth { get; set; }
        [Column("Expiration Day")]
        public int expDay { get; set; }
        [Column("Expiration DUE")]
        public int daysUntilExp { get; set; }

        [Column("Week Warning")]
        public bool weekWarning { get; set; }
        [Column("Three Days Warning")]
        public bool threeDaysWarning { get; set; }
        [Column("One Day Warning")]
        public bool oneDayWarning { get; set; }
        [Column("Amount")]
        public int Amount { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("Storage Type")]
        public string StorageType { get; set; }
        [Column("Icon")]
        public string Icon { get; set; }
        [Column("Stored")]
        public bool Stored { get; set; }

        [Column("Storage Name")]
        public string StorageName { get; set; }
        [Column("Storage Cell Index")]
        public int StorageCellIndex { get; set; }
        public Item SetItem(int expYr, int expMth, int expD, int quantity, string productName, string image)
        {
            ID = IDGenerator.GetID(ContentManager.itemStorageIdGenerator);
            expDay = expD; expMonth = expMth; expYear = expYr; Amount = quantity; Name = productName;
            daysUntilExp = 0;
            Icon = image;
            weekWarning = false;
            threeDaysWarning = false;
            oneDayWarning = false;
            Stored = false;
            SetDaysUntilExpiration();
            return this;
        }
        public void SetDaysUntilExpiration()
        {
            daysUntilExp = DateCalculator.SubtractDate(expYear, expMonth, expDay);
        }
        public void SetStorage(string storageName, int storageCellIndex, string storageType)
        {
            StorageName = storageName;
            StorageCellIndex = storageCellIndex;
            StorageType = storageType;
            Stored = true;
        }
    }

    [Table("Storage Cell")]
    public class StorageCell
    {
        [PrimaryKey]
        public int MetaID { get; set; }
        [Column("Index")]
        public int Index { get; set; }
        [Column("Storage Name")]
        public string StorageName { get; set; }
        [Column("X")]
        public int X { get; set; }
        [Column("Y")]
        public int Y { get; set; }
        [Column("Column Span")]
        public int ColumnSpan { get; set; }
        [Column("Row Span")]
        public int RowSpan { get; set; }
        [Column("Grid")]
        public string GridType { get; set; } // Only applies to fridge
        [Ignore]
        public Grid ParentGrid { get; set; } // Only applies to fridge
        private Vector2D<int> Position;
        private Grid Grid = new Grid();
        private Image background;
        private ImageButton button;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">Position in the parent grid (left, top).</param>
        /// <param name="index">Index in the IStorage storage cell dictionary.</param>
        /// <param name="storageName">Name of storage the cell belongs in.</param>
        /// <param name="parentGrid">The parent grid. Only applies to fridge.</param>
        /// <param name="gridType">The type of grid. Left, Main, or Right. Only applies to fridge.</param>
        /// <param name="columnSpan">Column span of cell in the parent grid.</param>
        /// <param name="rowSpan">Row span of cell in the parent grid.</param>
        /// <returns></returns>
        public StorageCell SetStorageCell(Vector2D<int> position, int index, string storageName, Grid parentGrid, string gridType = "", int columnSpan = 1, int rowSpan = 1)
        {
            Position = position;
            ColumnSpan = columnSpan;
            StorageName = storageName;
            RowSpan = rowSpan;
            Index = index;
            ParentGrid = parentGrid;
            GridType = gridType;
            Grid = GridManager.InitializeGrid(6, 4, GridLength.Star, GridLength.Star);

            MetaID = IDGenerator.GetID(ContentManager.storageCellIdGenerator);
            X = position.X; Y = position.Y;

            return this;
        }

        public void SetPosition(Vector2D<int> position)
        {
            Position = position;
            X = position.X; Y = position.Y;
        }

        public Vector2D<int> GetPosition()
        {
            return Position;
        }

        public void SetColumnSpan(int columnSpan)
        {
            ColumnSpan = columnSpan;

            Grid.SetColumnSpan(Grid, columnSpan);
            Grid.SetColumnSpan(background, columnSpan);
            Grid.SetColumnSpan(button, columnSpan);
        }

        public void SetRowSpan(int rowSpan)
        {
            RowSpan = rowSpan;

            Grid.SetRowSpan(Grid, rowSpan);
            Grid.SetRowSpan(background, rowSpan);
            Grid.SetRowSpan(button, rowSpan);
        }


        public List<View> GetChildren()
        {
            return Grid.Children.ToList();
        }

        public int GetChildrenCount()
        {
            return GetChildren().Count;
        }
        public void AddItem(List<View> items)
        {
            GridManager.AddGridItem(Grid, items, false);
        }
        public void AddUI(Image background, ImageButton button)
        {
            this.background = background;
            this.button = button;
        }
        public Image GetBackground()
        {
            return background;
        }
        public ImageButton GetButton()
        {
            return button;
        }
        public Grid GetItemGrid()
        {
            return Grid;
        }
    }

    public interface IStorage
    {
        int ID { get; set; }
        [Ignore]
        Grid MainGrid { get; set; }
        void AddGridCell(int ID, StorageCell cell);
        void RemoveGridCell(int ID);
        void AddGridCellUI(int ID, Image background, ImageButton button);
        /// <summary>
        /// Add item to the storage
        /// </summary>
        /// <param name="ID">The index of the cell</param>
        /// <param name="items">List of Views</param>
        void AddGridItems(int ID, List<View> items);
        StorageCell GetGridCell(int ID);
        List<int> GetGridIDs();
        List<StorageCell> GetGridCells();
        IEnumerable<View> GetChildren();
    }


    [Table("Cabinet")]
    public class Cabinet : IStorage
    {
        [Column("Name")]
        public string Name { get; set; }
        [Ignore]
        public Grid MainGrid { get; set; }
        [PrimaryKey, Column("ID")]
        public int ID { get; set; }

        // Matches the grid position of each cell to the cell ID.
        private Dictionary<int, StorageCell> gridCells = new Dictionary<int, StorageCell>();

        public Cabinet SetCabinet(string name, Grid grid, int id)
        {
            Name = name;
            ID = id;
            MainGrid = grid;
            return this;
        }

        public void AddGridCell(int ID, StorageCell cell)
        {
            gridCells.Add(ID, cell);
        }

        public void RemoveGridCell(int ID)
        {
            gridCells.Remove(ID);
        }

        public void AddGridCellUI(int ID, Image background, ImageButton button)
        {
            if (gridCells.ContainsKey(ID))
            {
                var cell = gridCells[ID];
                cell.AddUI(background, button);

                GridManager.AddGridItemAtPosition(MainGrid, new List<View>() { background, button }, cell.GetPosition());
            }
        }

        public void AddGridItems(int ID, List<View> items)
        {
            var cell = gridCells[ID];
            cell.AddItem(items);
        }

        public StorageCell GetGridCell(int ID)
        {
            return gridCells[ID];
        }

        public List<StorageCell> GetGridCells()
        {
            return gridCells.Values.ToList();
        }

        public List<int> GetGridIDs()
        {
            return gridCells.Keys.ToList();
        }

        public IEnumerable<View> GetChildren()
        {
            return MainGrid.Children.ToList();
        }
    }

    [Table("Fridge")]
    public class Fridge : IStorage
    {
        [Column("Name")]
        public string Name { get; set; }

        [Ignore]
        public Grid MainGrid { get; set; }
        [Ignore]
        public Grid LeftGrid { get; set; }
        [Ignore]
        public Grid RightGrid { get; set; }
        [PrimaryKey, Column("ID")]
        public int ID { get; set; }
        // Matches the grid position of each cell to the cell ID.
        private Dictionary<int, StorageCell> gridCells = new Dictionary<int, StorageCell>();

        //TEMPORARY
        public Fridge SetFridge(string name, Grid mainGrid, Grid leftGrid, Grid rightGrid, int id)
        {
            Name = name;
            MainGrid = mainGrid;
            LeftGrid = leftGrid;
            RightGrid = rightGrid;
            ID = id;
            return this;
        }
        public void AddGridCell(int ID, StorageCell cell)
        {
            gridCells.Add(ID, cell);
        }

        public void RemoveGridCell(int ID)
        {
            gridCells.Remove(ID);
        }

        public void AddGridCellUI(int ID, Image background, ImageButton button)
        {
            if (gridCells.ContainsKey(ID))
            {
                var cell = gridCells[ID];
                cell.AddUI(background, button);
                GridManager.AddGridItemAtPosition(cell.ParentGrid, new List<View>() { background, button }, cell.GetPosition());
            }
        }

        public void AddGridItems(int ID, List<View> items)
        {
            if (gridCells.ContainsKey(ID))
            {
                var cell = gridCells[ID];
                cell.AddItem(items);

                GridManager.AddGridItemAtPosition(cell.ParentGrid, items, cell.GetPosition());
            }
        }

        public StorageCell GetGridCell(int ID)
        {
            return gridCells[ID];
        }

        public List<StorageCell> GetGridCells()
        {
            return gridCells.Values.ToList();
        }

        public List<int> GetGridIDs()
        {
            return gridCells.Keys.ToList();
        }

        public IEnumerable<View> GetChildren()
        {
            return MainGrid.Children.ToList().Concat(LeftGrid.Children.ToList()).Concat(RightGrid.Children.ToList());
        }
    }
    public static class PositionExtention
    {
        public static Vector2D<double> GetAbsolutePosition(this VisualElement element, int parentConstraint = 100)
        {
            var y = element.Y + element.TranslationY;
            var x = element.X + element.TranslationX;
            var parent = (VisualElement)element.Parent;
            var iterator = 0;
            while (parent != null && parent.Parent != null && iterator < parentConstraint && typeof(VisualElement).IsAssignableFrom(parent.Parent.GetType()))
            {
                y += parent.Y + parent.TranslationY; x += parent.X + parent.TranslationX;  parent = (VisualElement)parent.Parent;
                iterator++;
            }
            return new Vector2D<double>(x, y);
        }
    }

    public class DateCalculator
    {
        private static List<int> monthDays;
        private static void SetMonthList()
        {
            if (DateTime.IsLeapYear(DateTime.Today.Year))
            {
                monthDays = new List<int>() { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            }
            else
            {
                monthDays = new List<int>() { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
            }
        }

        public static int GetDaysOfYear(int yr)
        {
            return DateTime.IsLeapYear(yr) ? 366 : 365;
        }
        public static List<int> GetMonthList()
        {
            SetMonthList();
            return monthDays;
        }
        public static string GetDateIn(int days)
        {
            var date = DateTime.Today.AddDays(days);
            return date.Month + "/" + date.Day + "/" + date.Year;
        }
        public static int SubtractDate(int yr, int m, int d)
        {
            SetMonthList();
            if (DateTime.Compare(DateTime.Today, new DateTime(yr, m, d)) <= 0)
            {
                var date = DateTime.Today.Subtract(new DateTime(yr, m, d));
                var days = Math.Abs(date.Days);
                return days;
            }
            else { return 0; }
        }
        public static string SubtractDateStringed(int yr, int m, int d)
        {
            SetMonthList();

            if (DateTime.Compare(DateTime.Today, new DateTime(yr, m, d)) <= 0)
            {
                var date = DateTime.Today.Subtract(new DateTime(yr, m, d));
                var days = Math.Abs(date.Days);
                if (days < 365) { return days + " days"; }
                else if (days < 730) { return "1+ year"; }
                else { return Math.Round((double)days / 365) + "+ years"; }
            }
            else { return "Exp"; }
        }
    }

   
    public static class ListSorter
    {
        public static void Swap<T>(this List<T> list, int index1, int index2)
        {
            T a = list[index1];
            list[index1] = list[index2];
            list[index2] = a;
        }
        public static void SortToListAscending<T, V>(List<T> sorter, List<V> outValues) where T : IComparable
        { 
            int FindMinimum(int start)
            {
                int minIndex = start;
                for(int x = start + 1; x < sorter.Count; x++)
                {
                    if(sorter[minIndex].CompareTo(sorter[x]) >= 0)
                    {
                        minIndex = x;
                    }
                }
                return minIndex;
            }
            
            for(int i = 0; i < sorter.Count; i++)
            {
                var min = FindMinimum(i);
                if (min != i)
                {
                    sorter.Swap(i, min);
                    outValues.Swap(i, min);
                }
            }
        }
    }

    public static class ElementBinder
    {
        public static Dictionary<Grid, List<ItemLayout>> itemGridDictItemLayout = new Dictionary<Grid, List<ItemLayout>>();
        public static Dictionary<Grid, List<View>> itemGridDictView = new Dictionary<Grid, List<View>>();
        public static void SetGridChildrenList(this Grid grid, List<ItemLayout> list)
        {
            if (itemGridDictItemLayout.ContainsKey(grid))
            {
                itemGridDictItemLayout[grid] = list;
            }
            else { itemGridDictItemLayout.Add(grid, list); }
        }

        public static void SetGridChildrenList(this Grid grid, List<View> list)
        {
            if (itemGridDictView.ContainsKey(grid))
            {
                itemGridDictView[grid] = list;
            }
            else { itemGridDictView.Add(grid, list); }
        }
        public static IList GetGridChilrenList(this Grid grid)
        {
            if (itemGridDictItemLayout.ContainsKey(grid)) { return itemGridDictItemLayout[grid]; }
            else if (itemGridDictView.ContainsKey(grid)) { return itemGridDictView[grid]; }
            return null;
        }

        public static Dictionary<View, string> viewDictTags = new Dictionary<View, string>();
        public static void SetElementTag<T>(this T view, string tag) where T : View
        {
            if(!viewDictTags.ContainsKey(view))
            viewDictTags.Add(view, tag);
        }
        public static string GetElementTag<T>(this T view) where T : View
        {
            if (viewDictTags.ContainsKey(view))
                return viewDictTags[view];
            return string.Empty;
        }
    }
    public static class GridOrganizer
    {
        private static Vector2D<int> gridPair;
        private static int rowCount;
        private static int columnCount;
        private static List<View> singularList = new List<View>();
        public enum OrganizeMode
        {
            HorizontalLeft,
            HorizontalRight,
            VerticalLeft,
            TwoRowSpanLeft,
            HorizontalZigZag
        }
        public enum ItemSortingMode
        {
            Expiration_Close,
            A_Z
        }
        /// <summary>
        /// Must input a grid with its children as ItemLayout and set using the SetGridChildrenList method.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="sortingType"></param>
        public static void SortItemGrid(Grid grid, ItemSortingMode sortingType)
        {
            var children = grid.Children.ToList();
            switch (sortingType)
            {
                case ItemSortingMode.Expiration_Close:
                    List<int> expDates = new List<int>();
                    for (int i = 0; i < children.Count; i ++)
                    {
                        expDates.Add(((ItemLayout)children[i]).ItemData.daysUntilExp);
                    }
                    ListSorter.SortToListAscending(expDates, children);
                    break;
                case ItemSortingMode.A_Z:
                    List<string> name = new List<string>();
                    for (int i = 0; i < children.Count; i++)
                    {
                        name.Add(((ItemLayout)children[i]).ItemData.Name);
                    }
                    ListSorter.SortToListAscending(name, children);
                    break;
            }
            grid.OrganizeGrid(children, OrganizeMode.HorizontalLeft);
        }


        public static void OrganizeGrid<T>(this Grid grid, IEnumerable<T> items, OrganizeMode mode)
        {
            rowCount = grid.RowDefinitions.Count;
            columnCount = grid.ColumnDefinitions.Count;

            if (!typeof(View).IsAssignableFrom(typeof(T))) { throw new ArgumentException(); }
            if(items.Count() == 0) { return; }
           
            if (mode == OrganizeMode.HorizontalRight )
            {
                gridPair = new Vector2D<int>() { X = columnCount - 1, Y = 0 };

                grid.Children.Add(items.ElementAt(0) as View, columnCount - 1, 0);
            }
            else
            {
                gridPair = new Vector2D<int>() { X = 0, Y = 0 };

                grid.Children.Add(items.ElementAt(0) as View, 0, 0);
            }


            if (grid.RowDefinitions.Count == 0)
                grid.RowDefinitions.Add(new RowDefinition());
            if (grid.ColumnDefinitions.Count == 0)
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 1; i < items.Count(); i++)
            {
                SetGridPairs(mode);
                if (gridPair.Y > rowCount - 1)
                {
                    //var height = grid.RowDefinitions.Count > 0 ? grid.RowDefinitions[0].Height : grid.Height;
                    grid.RowDefinitions.Add(new RowDefinition() { Height = grid.RowDefinitions.First().Height });
                    rowCount++;
                }
                if (gridPair.X > columnCount - 1)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = grid.ColumnDefinitions.First().Width });
                    columnCount++;
                }

                grid.Children.Add(items.ElementAt(i) as View, gridPair.X, gridPair.Y);
            }
        }

        public static void OrganizeGrid(this Grid grid, List<List<View>> items, OrganizeMode mode)
        {
            rowCount = grid.RowDefinitions.Count;
            columnCount = grid.ColumnDefinitions.Count;
            if (items.Count == 0) { return; }

            if (mode == OrganizeMode.HorizontalRight)
            {
                gridPair = new Vector2D<int>() { X = columnCount - 1, Y = 0 };

                foreach (View view in items[0])
                {
                    grid.Children.Add(view, columnCount - 1, 0);
                }

            }
            else
            {
                gridPair = new Vector2D<int>() { X = 0, Y = 0 };
                foreach (View view in items[0])
                {
                    grid.Children.Add(view, 0, 0);
                }
            }
            for (int i = 1; i < items.Count; i++)
            {
                SetGridPairs(mode);
                if (gridPair.Y > rowCount - 1)
                {
                    grid.RowDefinitions.Add(new RowDefinition() { Height = grid.RowDefinitions[0].Height });
                    rowCount++;
                }
                if (gridPair.X > columnCount - 1)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = grid.ColumnDefinitions[0].Width });
                    columnCount++;
                }
                foreach (View view in items[i])
                {
                    grid.Children.Add(view, (int)gridPair.X, (int)gridPair.Y);
                }
            }
        }

        private static Vector2D<int> SetGridPairs(OrganizeMode mode)
        {
            if (mode == OrganizeMode.HorizontalLeft)
            {
                if (gridPair.X < columnCount - 1) { gridPair.X++; }
                else { gridPair.X = 0; gridPair.Y++; }
            }
            else if (mode == OrganizeMode.HorizontalRight)
            {
                if (gridPair.X > 0) { gridPair.X--; }
                else { gridPair.X = columnCount - 1; gridPair.Y++; }
            }
            else if (mode == OrganizeMode.VerticalLeft)
            {
                if (gridPair.Y < rowCount - 1) { gridPair.Y++; }
                else { gridPair.Y = 0; gridPair.X++; }
            }
            else if (mode == OrganizeMode.TwoRowSpanLeft)
            {
                if(gridPair.Y % 2 == 0) { gridPair.Y++; }
                else if(gridPair.X < columnCount - 1){ gridPair.Y -= 1; gridPair.X++; }
                else { gridPair.Y++; gridPair.X = 0; }
            }
            else if (mode == OrganizeMode.HorizontalZigZag)
            {
                if (gridPair.Y % 2 == 0)
                {
                    if (gridPair.X < columnCount - 1) { gridPair.X++; }
                    else { gridPair.Y++; }
                }
                else {
                    if(gridPair.X > 0) { gridPair.X--; }
                    else { gridPair.Y++; }
                }
            }
            return gridPair;
        }

    }

}
