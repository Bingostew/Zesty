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
        public static Grid InitializeGrid(string name, int rows, int columns)
        {
            // if exist, then get instead of create
            if (gridDataBase.ContainsKey(name))
                return gridDataBase[name];

            // initialize grid with rows and columns parameter
            Grid grid = new Grid();
            for(int i = 0; i < rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < columns; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            gridDataBase.Add(name, grid);
            return grid;
        }

        public static void AddGridItem(Grid grid, IEnumerable<View> item, bool replaceExisting) 
        {
            // If replacing existing children, then does not retrive child list
            List<View> gridChildren = replaceExisting ? item.ToList() : grid.Children.ToList();


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
            if (gridChildren.Contains(item))
                gridChildren.Remove(item);
           
            grid.OrganizeGrid(gridChildren, GridOrganizer.OrganizeMode.HorizontalLeft);
        }

        public static void RemoveGridItem<T>(string name, T item) where T : View
        {
            RemoveGridItem(gridDataBase[name], item);
        }

        /// <summary>
        /// Returns a grid with constraint in children from another grid
        /// </summary>
        /// <param name="baseGrid">Grid to extract from</param>
        /// <param name="startChildIndex">Starting child index to extract from</param>
        /// <param name="endChildIndex">End child index of the extraction</param>
        /// <param name="newGrid">If the extraction creates a new grid or constrain the original grid</param>
        /// <param name="newgridName">New name of the new grid</param>
        /// <returns></returns>
        /// 
        public static Grid ConstrainGrid(Grid baseGrid, int startChildIndex, int endChildIndex,  bool newGrid, Converter<View, View> converter = null,
            int rows = 0, int columns = 0, string newgridName = "") 
        {
            // if initializing a new grid, create one, otherwise, use provided grid 
            Grid grid = newGrid ? InitializeGrid(newgridName, rows, columns) : baseGrid;

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

            // If creating a new grid, creates a deep copy of the base grid children
            if (newGrid)
                extractedChildren = baseGrid.Children.ToList().GetRange(startIndex, endIndex - startIndex).ConvertAll(converter);
            else
                extractedChildren = baseGrid.Children.ToList().GetRange(startIndex, endIndex - startIndex);

            AddGridItem(grid, extractedChildren, true);

            return grid;
        }
        public static Grid ConstrainGrid(string name, int startChildIndex, int endChildIndex,  bool newGrid, Converter<View, View> converter = null,
            int rows = 0, int columns = 0, string newgridName = "")
        {
            return ConstrainGrid(gridDataBase[name], startChildIndex, endChildIndex, newGrid, converter, rows, columns, newgridName);
        }

        public static Grid GetGrid(string name)
        {
            return gridDataBase[name];
        }
    }
}
