using System;
using Utility;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public class ContentLoader
    {
        public static void LoadItems(List<Item> items)
        {
            List<View> metaGridChildren = new List<View>();
            List<View> unplacedGridChildren = new List<View>();
            foreach (Item item in items)
            {
                // Create Itemlayout from item
                ItemLayout itemLayout = new ItemLayout(ContentManager.item_layout_size, ContentManager.item_layout_size, item).AddMainImage()
                                        .AddExpirationMark()
                                        .AddTitle()
                                        .AddInfoIcon();
                ItemLayout itemLayoutCopy = new ItemLayout(ContentManager.item_layout_size, ContentManager.item_layout_size, item).AddMainImage()
                                        .AddExpirationMark()
                                        .AddTitle()
                                        .AddInfoIcon();

                itemLayout.RecalculateDate();
                itemLayoutCopy.RecalculateDate();

                ContentManager.MetaItemBase.Add(item.ID, itemLayout);

                metaGridChildren.Add(itemLayout);

                // Record existing ID to the generator
                IDGenerator.SkipID(ContentManager.itemStorageIdGenerator, item.ID);

                // Add to unplaced dictionary if item is not stored
                if (!item.Stored) {
                    unplacedGridChildren.Add(itemLayoutCopy);
                    ContentManager.UnplacedItemBase.Add(item.ID, itemLayoutCopy);
                }
            }

            // populates both grids with corresponding children
            GridManager.AddGridItem(ContentManager.metaGridName, metaGridChildren, true);
            GridManager.AddGridItem(ContentManager.unplacedGridName, unplacedGridChildren, true);
        }
        /// <summary>
        /// Set up cabinets when starting application. Must be called after LoadItems().
        /// </summary>
        /// <param name="cabinets">All cabinets.</param>
        /// <param name="storageCells">All storage cells.</param>
        /// <param name="items">All items.</param>
        public static void LoadCabinets(List<Cabinet> cabinets, List<StorageCell> storageCells, List<Item> items)
        {    
            foreach (Cabinet cabinet in cabinets)
            {
                Console.WriteLine("Content Loader 58 cabinet ID = " + "[]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]" + cabinet.ID);
                ContentManager.CabinetMetaBase.Add(cabinet.Name, cabinet);
                // Set the cabinet's grid
                Grid cabinetGrid = new Grid() { RowSpacing = 0, ColumnSpacing = 0 };
                cabinet.SetCabinet(cabinet.Name, cabinetGrid, cabinet.ID);

                // Record cabinet ID 
                IDGenerator.SkipID(ContentManager.cabinetEditIdGenerator, cabinet.ID);
                IDGenerator.InitializeIDGroup(cabinet.Name);

                foreach (StorageCell cell in storageCells)
                {
                    // Record cell ID.
                    IDGenerator.SkipID(ContentManager.storageCellIdGenerator, cell.MetaID);
                    // Check if cell belongs to the given cabinet
                    if (cell.StorageName == cabinet.Name)
                    {
                        IDGenerator.SkipID(cabinet.Name, cell.Index);
                        cell.SetStorageCell(new Vector2D<int>(cell.X, cell.Y), cell.Index, cell.StorageName, cabinetGrid, "", cell.ColumnSpan, cell.RowSpan);

                        Console.WriteLine("ContentLoader 78 cabinet name: " + cabinet.Name + " cell storage: " + cell.StorageName + " cell index: " + cell.Index);
                        // Add cell to storage children dictionary
                        cabinet.AddGridCell(cell.Index, cell);

                        List<View> cellChildren = new List<View>();
                        // Add items to gridcells
                        foreach (Item item in items)
                        {
                          //  Console.WriteLine("ContentLoader 78 item storage: " + item.StorageName);
                            // Check if item belongs to both the storage and the cell
                            if(item.StorageName == cabinet.Name && item.StorageCellIndex == cell.Index)
                            {
                                // Adds to the list of children, will be used to populate grid later.
                                ItemLayout itemLayout = new ItemLayout(ContentManager.item_layout_size, ContentManager.item_layout_size, item).AddMainImage()
                                        .AddExpirationMark()
                                        .AddTitle()
                                        .AddInfoIcon();
                                cellChildren.Add(itemLayout);
                            }
                        }
                        cabinet.AddGridItems(cell.Index, cellChildren);

                        // set UI for each cell
                        Image background = new Image() { Source = ContentManager.cabinetCellIcon, Aspect = Aspect.Fill };
                        ImageButton transparentButton = new ImageButton() { Source = ContentManager.transIcon, BackgroundColor = Color.Transparent, Aspect = Aspect.Fill };
                        cabinet.AddGridCellUI(cell.Index, background, transparentButton);

                        // Set row and column span of cell
                        cell.SetRowSpan(cell.RowSpan);
                        cell.SetColumnSpan(cell.ColumnSpan);
                    }
                }
            }
        }

        /// <summary>
        /// Set up fridges when starting application. Must call after LoadItems().
        /// </summary>
        /// <param name="fridges"> List of all stored fridges</param>
        /// <param name="storageCells">List of all stored storage cells.</param>
        /// <param name="items">List of all stored items.</param>
        public static void LoadFridges(List<Fridge> fridges, List<StorageCell> storageCells, List<Item> items)
        {
            foreach (Fridge fridge in fridges)
            {
                ContentManager.FridgeMetaBase.Add(fridge.Name, fridge);
                // Set the cabinet's grid
                Grid fridgeMainGrid = new Grid() { RowSpacing = 0, ColumnSpacing = 0 };
                Grid fridgeLeftGrid = new Grid() { RowSpacing = 0, ColumnSpacing = 0 };
                Grid fridgeRightGrid = new Grid() { RowSpacing = 0, ColumnSpacing = 0 };
                fridge.SetFridge(fridge.Name, fridgeMainGrid, fridgeLeftGrid, fridgeRightGrid, fridge.ID);

                // Record cabinet ID 
                IDGenerator.SkipID(ContentManager.fridgeEditIdGenerator, fridge.ID);
                IDGenerator.InitializeIDGroup(fridge.Name);

                foreach (StorageCell cell in storageCells)
                {
                    // Record cell ID.
                    IDGenerator.SkipID(ContentManager.storageCellIdGenerator, cell.MetaID);
                    // Check if cell belongs to the given cabinet
                    if (cell.StorageName == fridge.Name)
                    {
                        IDGenerator.SkipID(fridge.Name, cell.MetaID);
                        Grid cellParentGrid = cell.GridType == "Left" ? fridgeLeftGrid : cell.GridType == "Right" ? fridgeRightGrid : fridgeMainGrid;
                        cell.SetStorageCell(new Vector2D<int>(cell.X, cell.Y), cell.Index, cell.StorageName, cellParentGrid, cell.GridType, cell.ColumnSpan, cell.RowSpan);

                        // Add cell to storage children dictionary
                        fridge.AddGridCell(cell.Index, cell);

                        List<View> cellChildren = new List<View>();
                        // Add items to gridcells
                        foreach (Item item in items)
                        {
                            //  Console.WriteLine("ContentLoader 78 item storage: " + item.StorageName);
                            // Check if item belongs to both the storage and the cell
                            if (item.StorageName == fridge.Name && item.StorageCellIndex == cell.Index)
                            {
                                // Adds to the list of children, will be used to populate grid later.
                                ItemLayout itemLayout = new ItemLayout(100, ContentManager.item_layout_size, item).AddMainImage()
                                        .AddExpirationMark()
                                        .AddTitle()
                                        .AddInfoIcon();
                                cellChildren.Add(itemLayout);
                            }
                        }
                        fridge.AddGridItems(cell.Index, cellChildren);

                        // set UI for each cell
                        Image background = new Image() { Source = ContentManager.fridgeIcon, Aspect = Aspect.Fill };
                        ImageButton transparentButton = new ImageButton() { Source = ContentManager.transIcon, BackgroundColor = Color.Transparent, Aspect = Aspect.Fill };
                        fridge.AddGridCellUI(cell.Index, background, transparentButton);

                        // Set row and column span of cell
                        cell.SetRowSpan(cell.RowSpan);
                        cell.SetColumnSpan(cell.ColumnSpan);
                    }
                }
            }
        }
    }
}

