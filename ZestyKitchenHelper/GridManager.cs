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
        /// Creates a new grid if name not exist, otherwise, retrieve grid.
        /// </summary>
        /// <param name="name">Name of grid</param>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static Grid InitializeGrid(string name, int rows, int columns, GridLength gridHeight, GridLength gridWidth)
        {
            // if exist, then get instead of create
            if (gridDataBase.ContainsKey(name))
                return gridDataBase[name];

            // initialize grid with rows and columns parameter
            Grid grid = new Grid();
            for(int i = 0; i < rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = gridHeight });
            }
            for (int i = 0; i < columns; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = gridWidth });
            }

            gridDataBase.Add(name, grid);
            return grid;
        }

        public static void AddGridItem(Grid grid, IEnumerable<View> item, bool replaceExisting) 
        {
            // If replacing existing children, then does not retrive child list
            List<View> gridChildren = replaceExisting ? item.ToList() : grid.Children.ToList();
            Console.WriteLine("GridManager 48: grid children length = " + grid.Children.Count);

            //Append new child to the grid if not replacing
            if (!replaceExisting)
            {
                foreach (var child in item)
                {
                    gridChildren.Add(child);
                }
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
        /// Returns a grid with constraint in children from another grid
        /// </summary>
        /// <param name="baseGrid">grid to extract from</param>
        /// <param name="startChildIndex">index of basegrid to start extraction</param>
        /// <param name="endChildIndex">index of basegrid to end extractionparam>
        /// <param name="converter">method to convert item from basegrid to something else in constrained grid</param>
        /// <param name="newGrid">a new grid, if applicable</param>
        /// <returns></returns>
        public static Grid ConstrainGrid(Grid baseGrid, int startChildIndex, int endChildIndex, Converter<View, View> converter = null, Grid newGrid = null) 
        {
            // if initializing a new grid, create one, otherwise, use provided grid 
            Grid grid = newGrid != null ? newGrid : baseGrid;

            // note the maximum number of children the grid is allowed to have
            if(!constraintBase.ContainsKey(grid))
                constraintBase.Add(grid, endChildIndex - startChildIndex);

            // If no child, then no constraint
            if (baseGrid.Children.Count == 0)
                return grid;

            // clamp the child indeces
            int endIndex = baseGrid.Children.Count >= endChildIndex ? endChildIndex: baseGrid.Children.Count;
            int startIndex = baseGrid.Children.Count > startChildIndex ? startChildIndex : 0;

            // get the child list from beginning index to end index
            List<View> extractedChildren = new List<View>(); 

            // If creating a new grid and has a converter, creates a deep copy of the base grid children
            if (newGrid != null && converter != null)
                extractedChildren = baseGrid.Children.ToList().GetRange(startIndex, endIndex - startIndex).ConvertAll(converter);
            else
                extractedChildren = baseGrid.Children.ToList().GetRange(startIndex, endIndex - startIndex);

            AddGridItem(grid, extractedChildren, true);

            return grid;
        }
        public static Grid ConstrainGrid(string name, int startChildIndex, int endChildIndex, Converter<View, View> converter = null, Grid newGrid = null)
        {
            return ConstrainGrid(gridDataBase[name], startChildIndex, endChildIndex, converter, newGrid);
        }

        public static Grid GetGrid(string name)
        {
            return gridDataBase[name];
        }
    }
}
