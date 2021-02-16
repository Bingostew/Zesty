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

    public interface INavigatablePage
    {
        void SetView();
    }

    public struct Vector2D
    {
        public double X, Y;
        public Vector2D(double x, double y)
        {
            X = x; Y = y;
        }
    }

    [Table("Item")]
    public class Item
    {
        public static int IDIterator = 0;
        [Column("Expiration")]
        public int expYear { get; set; }

        public int expMonth { get; set; }
        public int expDay { get; set; }
        public int daysUntilExp { get; set; }
        
        [Column("Warnings")]
        public bool weekWarning { get; set; }
        public bool threeDaysWarning { get; set; }
        public bool oneDayWarning { get; set; }
        [PrimaryKey]
        [Column("Main")]
        public int ID { get; set; }
        public int amount { get; set; }
        public string name { get; set; }
        public string icon { get; set; }
        public bool stored { get; set; }
        public Item SetItem(int expYr, int expMth, int expD, int quantity, string productName, string image)
        {
            while (ContentManager.MetaItemBase.ContainsKey(IDIterator)) { IDIterator++; }
            ID = IDIterator;
            expDay = expD; expMonth = expMth; expYear = expYr; amount = quantity; name = productName;
            daysUntilExp = 0;
            icon = image;
            weekWarning = false;
            threeDaysWarning = false;
            oneDayWarning = false;
            stored = false;
            SetDaysUntilExpiration();
            IDIterator++;
            return this;
        }
        public void SetDaysUntilExpiration()
        {
            daysUntilExp = DateCalculator.SubtractDate(expYear, expMonth, expDay);
        }
        public void SetStorage()
        {
            stored = true;
        }
    }

    [Table("Cabinet")]
    public class Cabinet
    {
        [PrimaryKey]
        [Column("name")]
        public string Name { get; set; }
        [Column("row")]
        //Format: button proportional position each row, separated in comma, ex- (.25+.5),(.5),
        public string RowInfo { get; set; }
        //Format: button index and items surrounede by parenthesis, separated in comma: ex- 1(Item1+ITem2),2(Item1)
        public string RowItems { get; set; }
        public Cabinet SetCabinet(string rowInfo, string rowItems, string name)
        {
            RowInfo = rowInfo;
            RowItems = rowItems;
            Name = name;
            return this;
        }
    }

    [Table("Fridge")]
    public class Fridge
    {
        [PrimaryKey]
        [Column("name")]
        public string Name { get; set; }
        [Column("row")]
        //Format: button proportional position each row, separated in comma, ex- (.25+.5),(.5),
        public string RowInfo { get; set; }
        //Format: button index and items surrouneded by parenthesis, separated in comma: ex- 1(Item1+ITem2),2(Item1)
        public string RowItems { get; set; }
        public Fridge SetFridge(string rowInfo, string rowItems, string name)
        {
            RowInfo = rowInfo;
            RowItems = rowItems;
            Name = name;
            return this;
        }
    }
    public static class PositionExtention
    {
        public static Vector2D GetAbsolutePosition(this VisualElement element, int parentConstraint = 100)
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
            return new Vector2D(x, y);
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

    public static class EffectToggle
    {
        public static void RemoveEffect(this VisualElement element, Type effectType)
        {
            var tryEffect = element.Effects.Where(e => e.GetType() == effectType);
            if (tryEffect.Any()) { element.Effects.Remove(tryEffect.FirstOrDefault()); }
        }
        public static void RemoveEffects(this IList<View> elements, Type effectType)
        {
            foreach(View element in elements)
            {
                var tryEffect = element.Effects.Where(e => e.GetType() == effectType);
                if (tryEffect.Any()) { element.Effects.Remove(tryEffect.FirstOrDefault()); }
            }
        }
        public static Effect GetEffect(this VisualElement element, Type effectType)
        {
            var tryEffect = element.Effects.Where(e => e.GetType() == effectType);
            if (tryEffect.Any())
            {
                return tryEffect.First();
            }
            return null;
        }

        public static void AddEffect(this VisualElement element, Effect effect)
        {
            var tryEffect = element.Effects.Where(e => e.GetType() == effect.GetType());
            if (!tryEffect.Any())
            {
                element.Effects.Add(effect);
            }
        }
        public static void ToggleEffects(this VisualElement button, Effect effect, List<VisualElement> toggleElements)
        {
            if (button != null)
            {
                var tryEffect = button.Effects.Where(e => e.GetType() == effect.GetType());
                if (tryEffect.Any())
                {
                    button.Effects.Remove(tryEffect.FirstOrDefault());
                    if (toggleElements != null)
                        foreach (var element in toggleElements)
                        {
                            element.IsVisible = false;
                        }
                }
                else
                {
                    button.Effects.Add(effect);

                    if (toggleElements != null)
                        foreach (var element in toggleElements)
                        {
                            element.IsVisible = true;
                        }
                }
            }
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
                T min = sorter[start];
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

        public static void OnSearchUnplacedGrid(Grid unplacedGrid, string name)
        {
            string input = name;
            unplacedGrid.Children.Clear();
            List<View> results = new List<View>();
            foreach (var item in ContentManager.MetaItemBase.Values)
            {
                var match = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    if (i < item.ItemData.name.Length && string.Equals(input[i].ToString(), item.ItemData.name[i].ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        match++;
                    }
                    else { match = 0; break; }
                }
                if ((match > 0 && !unplacedGrid.Children.Contains(item)) || input == "" || input == ContentManager.defaultSearchAllBarText)
                {
                    results.Add(item);
                }

                unplacedGrid.OrganizeGrid(results, GridOrganizer.OrganizeMode.HorizontalLeft);
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
        private static Vector2D gridPair;
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
        public enum SortingType
        {
            Expiration_Close,
            A_Z
        }
        /// <summary>
        /// Must input a grid with its children as ItemLayout and set using the SetGridChildrenList method.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="sortingType"></param>
        public static void SortItemGrid(Grid grid, SortingType sortingType)
        {
            var list = grid.GetGridChilrenList().Cast<ItemLayout>().ToList();
            Console.WriteLine("casty cast " + list.Count);
            switch (sortingType)
            {
                case SortingType.Expiration_Close:
                    List<int> expDates = new List<int>();
                    for (int i = 0; i < list.Count; i ++)
                    {
                        expDates.Add(list[i].ItemData.daysUntilExp);
                    }
                    ListSorter.SortToListAscending(expDates, list);
                    break;
                case SortingType.A_Z:
                    List<string> name = new List<string>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        name.Add(list[i].ItemData.name);
                        Console.WriteLine("premin " + name[i]);
                    }
                    ListSorter.SortToListAscending(name, list);
                    break;
            }
            grid.OrganizeGrid(list, OrganizeMode.HorizontalLeft);
            grid.SetGridChildrenList(list);
        }

        public static void OrganizeGrid<T>(this Grid grid, List<T> items, OrganizeMode mode)
        {
            rowCount = grid.RowDefinitions.Count;
            columnCount = grid.ColumnDefinitions.Count;


            if (!typeof(View).IsAssignableFrom(typeof(T))) { throw new ArgumentException(); }
            if(items.Count == 0) { return; }

            if (mode == OrganizeMode.HorizontalRight )
            {
                gridPair = new Vector2D() { X = columnCount - 1, Y = 0 };

                grid.Children.Add(items[0] as View, columnCount - 1, 0);
            }
            else
            {
                gridPair = new Vector2D() { X = 0, Y = 0 };

                grid.Children.Add(items[0] as View, 0, 0);
            }
            for (int i = 1; i < items.Count; i++)
            {
                SetGridPairs(mode);
                if (gridPair.Y > rowCount - 1)
                {
                    var height = grid.RowDefinitions.Count > 0 ? grid.RowDefinitions[0].Height : grid.Height;
                    grid.RowDefinitions.Add(new RowDefinition() { Height = height });
                }
                if (gridPair.X > columnCount - 1)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                }

                grid.Children.Add(items[i] as View, (int)gridPair.X, (int)gridPair.Y);
            }
        }

        public static void OrganizeGrid(this Grid grid, List<List<View>> items, OrganizeMode mode)
        {
            rowCount = grid.RowDefinitions.Count;
            columnCount = grid.ColumnDefinitions.Count;
            if (items.Count == 0) { return; }

            if (mode == OrganizeMode.HorizontalRight)
            {
                gridPair = new Vector2D() { X = columnCount - 1, Y = 0 };

                foreach (View view in items[0])
                {
                    grid.Children.Add(view, columnCount - 1, 0);
                }

            }
            else
            {
                gridPair = new Vector2D() { X = 0, Y = 0 };
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
                }
                if (gridPair.X > columnCount - 1)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = grid.ColumnDefinitions[0].Width });
                }
                foreach (View view in items[i])
                {
                    grid.Children.Add(view, (int)gridPair.X, (int)gridPair.Y);
                }
            }
        }

        private static Vector2D SetGridPairs(OrganizeMode mode)
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
