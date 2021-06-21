using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;
using Utility;
using System.Linq;

namespace ZestyKitchenHelper
{
    public class GridManager
    {
        private static Dictionary<string, Grid> gridDataBase = new Dictionary<string, Grid>();
        private static Dictionary<Grid, int> constraintBase = new Dictionary<Grid, int>();

        /// <summary>
        /// Creates a new grid with a name does if name not exist, otherwise, retrieve grid.
        /// </summary>
        /// <param name="name">Name of grid</param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static Grid InitializeGrid(string name, int rows, int columns, GridLength gridHeight, GridLength gridWidth)
        {
            return InitializeGrid(rows, columns, gridHeight, gridWidth, name);
        }

        public static Grid InitializeGrid(int rows, int columns, GridLength gridHeight, GridLength gridWidth, string name = null)
        {
            // if exist, then get instead of create
            if (name != null && gridDataBase.ContainsKey(name))
                return gridDataBase[name];

            // initialize grid with rows and columns parameter
            Grid grid = new Grid();
            for (int i = 0; i < rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = gridHeight });
            }
            for (int i = 0; i < columns; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = gridWidth });
            }

            if(name != null)
                gridDataBase.Add(name, grid);

            return grid;
        }

        /// <summary>
        /// Add items to one position in the grid.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="item">List of children</param>
        /// <param name="position">position in (x, y) notation</param>
        public static void AddGridItemAtPosition(Grid grid, IEnumerable<View> item, Vector2D<int> position)
        {
            while(grid.RowDefinitions.Count < position.Y + 1)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }

            while(grid.ColumnDefinitions.Count < position.X + 1)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            foreach(View view in item)
            {
                grid.Children.Add(view, position.X, position.Y);
            }
        }

        public static void AddGridItemAtPosition(string name, IEnumerable<View> item, Vector2D<int> position)
        {
            AddGridItemAtPosition(GetGrid(name), item, position);
        }

        /// <summary>
        /// Populate the grid sequentially with a list of items, where each grid cell contains one item.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="item">List of children</param>
        /// <param name="replaceExisting">Whether to clear the grid children first</param>
        public static void AddGridItem(Grid grid, IEnumerable<View> item, bool replaceExisting) 
        {
            // If replacing existing children, then does not retrive child list
            List<View> gridChildren; 

            //Append new child to the grid if not replacing
            if (!replaceExisting)
            {
                gridChildren = grid.Children.ToList();
                foreach (var child in item)
                {
                    gridChildren.Add(child);
                }
            }
            else
            {
                grid.Children.Clear();
                gridChildren = item.ToList();
            }

            // Check if the grid is constrained, then constrain children output
            if (constraintBase.ContainsKey(grid) && constraintBase[grid] < gridChildren.Count)
            {
                gridChildren = gridChildren.GetRange(0, constraintBase[grid]);
                Console.WriteLine("GridManager 57: " + constraintBase[grid] + " " + grid.Children.Count + " "+ gridChildren.Count);
            }
            grid.OrganizeGrid(gridChildren, GridOrganizer.OrganizeMode.HorizontalLeft);
        }

        public static void AddGridItem(string name, IEnumerable<View> item, bool replaceExisting)
        {
            AddGridItem(gridDataBase[name], item, replaceExisting);
        }

        public static void RemoveGridItem<T>(Grid grid, T item) where T : View
        {
            List<View> gridChildren =  grid.Children.ToList();

            // check if child exist, if so, remove
            if (grid.Children.Contains(item))
                gridChildren.Remove(item);

            Console.WriteLine("GridManager 81: removed grid children length: " + gridChildren.Count);
            grid.OrganizeGrid(gridChildren, GridOrganizer.OrganizeMode.HorizontalLeft);
            grid.Children.Remove(item);
        }

        public static void RemoveGridItem<T>(string name, T item) where T : View
        {
            RemoveGridItem(gridDataBase[name], item);
        }


        /// <summary>
        /// Constraining a grid given a list of views to extract from
        /// </summary>
        /// <param name="baseGrid">grid to extract from</param>
        /// <param name="startChildIndex">index of basegrid to start extraction</param>
        /// <param name="endChildIndex">index of basegrid to end extractionparam>
        /// <param name="converter">method to convert item from basegrid to something else in constrained grid</param>
        /// <param name="grid">grid to be populated</param>
        /// <param name="constrainSize">if true, then no child can added if the grid has the size over endChildIndex-startChildIndex</param>
        /// <returns></returns>
        public static Grid ConstrainGrid<T>(List<T> children, int startChildIndex, int endChildIndex, Grid grid, Converter<View, View> converter = null, bool constrainSize = false) where T : View
        {
            // note the maximum number of children the grid is allowed to have
            if (!constraintBase.ContainsKey(grid) && constrainSize)
                constraintBase.Add(grid, endChildIndex - startChildIndex + 1);

            // If no child, then no constraint
            if (children.Count == 0)
                return grid;

            // clamp the child indeces
            int endIndex = children.Count >= endChildIndex ? endChildIndex: children.Count - 1;
            int startIndex = 0 <= startChildIndex ? startChildIndex : 0;

            // If invalid range, then no constraint
            if (endIndex - startIndex < 0)
                return grid;

            // get the child list from beginning index to end index
            List<View> extractedChildren = new List<View>();

            // If has a converter, creates a deep copy of the base grid children
            if (converter != null)
                extractedChildren = children.GetRange(startIndex, endIndex - startIndex + 1).ConvertAll(converter);
            else
                extractedChildren = new List<View>(children.GetRange(startIndex, endIndex - startIndex + 1));

            AddGridItem(grid, extractedChildren, true);

            return grid;
        }

        /// <summary>
        /// Constraining a grid given the name of the base grid.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="startChildIndex"></param>
        /// <param name="endChildIndex"></param>
        /// <param name="grid"></param>
        /// <param name="converter"></param>
        /// <param name="constrainSize"></param>
        /// <returns></returns>
        public static Grid ConstrainGrid(string name, int startChildIndex, int endChildIndex, Grid grid, Converter<View, View> converter = null, bool constrainSize = false)
        {
            return ConstrainGrid(gridDataBase[name].Children.ToList(), startChildIndex, endChildIndex, grid, converter,constrainSize);
        }

        /// <summary>
        /// Constraining a grid given a base grid.
        /// </summary>
        /// <param name="baseGrid"></param>
        /// <param name="startChildIndex"></param>
        /// <param name="endChildIndex"></param>
        /// <param name="grid"></param>
        /// <param name="converter"></param>
        /// <param name="constrainSize"></param>
        /// <returns></returns>
        public static Grid ConstrainGrid(Grid baseGrid, int startChildIndex, int endChildIndex, Grid grid, Converter<View, View> converter = null, bool constrainSize = false)
        {
            return ConstrainGrid(baseGrid.Children.ToList(), startChildIndex, endChildIndex, grid, converter, constrainSize);
        }

        /// <summary>
        /// Filters an ItemLayout grid by the name
        /// </summary>
        /// <param name="grid">Grid with all its children with type ItemLayout.</param>
        /// <param name="input">The name to search for</param>
        public static void FilterItemGrid(Grid grid, string input)
        {
            List<View> results = new List<View>();
            foreach (var item in ContentManager.MetaItemBase.Values)
            {
                var match = 0;
                for (int i = 0; i < input.Length; i++)
                {
                    if (i < item.ItemData.Name.Length && string.Equals(input[i].ToString(), item.ItemData.Name[i].ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        match++;
                    }
                    else { match = 0; break; }
                }
                if ((match > 0 && !grid.Children.Contains(item)) || input == "" || input == ContentManager.defaultSearchAllBarText)
                {
                    results.Add(item);
                }

                AddGridItem(grid, results, true);
            }
        }

        public static Grid GetGrid(string name)
        {
            return gridDataBase[name];
        }

        public static void RemoveGrid(string name)
        {
            gridDataBase.Remove(name);
        }
    }
}
