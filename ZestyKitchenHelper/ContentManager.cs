using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using System.Linq;
using Xamarin.Forms.PlatformConfiguration;
using Utility;
using System.Globalization;
using Xamarin.Essentials;
using Firebase.Database;

namespace ZestyKitchenHelper
{
    public class ContentManager
    {
        public static UserProfile sessionUserProfile;
        public static bool isLocal;
        public static bool isUserNew;
        private static Color _themeColor = Color.Wheat;
        public static Color ThemeColor
        {
            get { return _themeColor; }
            set
            {
                _themeColor = value;
                foreach (var listener in OnColorChangeEvents)
                {
                    listener?.Invoke(_themeColor);
                }
            }
        }

        public static double screenWidth;
        public static double screenHeight;
        public static double item_layout_size;
        private const int storage_margin = 10;
        public const int exp_warning_size = 50;

        public const string buttonTintImage = "button_tint.png";
        public const string cabinetCellIcon = "cabinet_cell.png";
        public const string cabinetLeftIcon = "cabinet_divider_left.png";
        public const string cabinetMiddleIcon = "cabinet_divider_middle.png";
        public const string cabinetRightIcon = "cabinet_divider_right.png";
        public const string safeIcon = "swan.JPG";
        public const string addIcon = "add_new_content.png";
        public const string addItemIcon = "add_new_item.png";
        public const string subdivideIcon = "swan.JPG";
        public const string transIcon = "transparent.png";
        public const string fridgeIcon = "fridge_cell.png";
        public const string fridgeSideIcon = "fridge_side_cell.png";
        public const string fridgeDividerIcon = "fridge_cell_divider.png";
        public const string deleteCellIcon = "delete_cell_button.png";
        public const string backButton = "back_arrow.png";
        public const string pantryIcon = "pantry.png";
        public const string refridgeIcon = "fridge.png";
        public const string countIcon = "small_arrow.png";
        public const string expWarningIcon = "apple.png";

        public const string defaultSearchAllBarText = "Search item...";
        public const string exp_notification_title = "Zesty's Expiration Warning";

        public const string cabinetStorageType = "Cabinet";
        public const string fridgeStorageType = "Fridge";
        public const string metaGridName = "Meta Grid";
        public const string unplacedGridName = "Unplaced Grid";
        public const string pUnplacedGridName = "Partial Unplaced Grid";

        public const string itemStorageIdGenerator = "ItemId";
        public const string storageCellIdGenerator = "StorageCell";
        public const string cabinetEditIdGenerator = "Cabinet";
        public const string fridgeEditIdGenerator = "Fridge";
        public const string IOSNotificationIdGenerator = "IOSNotification";

        public static PageController pageController = new PageController();

        public static Dictionary<string, Cabinet> CabinetMetaBase = new Dictionary<string, Cabinet>();
        public static Dictionary<string, Fridge> FridgeMetaBase = new Dictionary<string, Fridge>();
        private static List<Action<Color>> OnColorChangeEvents = new List<Action<Color>>();
        public static async void InitializeApp()
        {
            // Initialize screen width and height
            screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density; 
            item_layout_size = screenWidth / 4;

            // Initialize ID Groups
            IDGenerator.InitializeIDGroup(itemStorageIdGenerator);
            IDGenerator.InitializeIDGroup(cabinetEditIdGenerator);
            IDGenerator.InitializeIDGroup(fridgeEditIdGenerator);
            IDGenerator.InitializeIDGroup(storageCellIdGenerator);
            IDGenerator.InitializeIDGroup(IOSNotificationIdGenerator);
            //LocalStorageController.ResetDatabase(); // WARNING: FOR TESTING PURPOSES ONLY
            LocalStorageController.InitializeLocalDataBase();

            // Initialize Important Grids
            GridManager.InitializeGrid(metaGridName, 9, 4, GridLength.Auto, GridLength.Star);
            GridManager.InitializeGrid(unplacedGridName, 0, 4, GridLength.Star, GridLength.Star);

            Console.WriteLine("ContentManger 75 is user new -=================================================================== " + ContentManager.isUserNew + ContentManager.isLocal);
            // Load saved data
            if (!isUserNew)
            {
                List<Cabinet> localCabinets = await LocalStorageController.GetTableListAsync<Cabinet>();
                List<Fridge> localFridges = await LocalStorageController.GetTableListAsync<Fridge>();
                List<StorageCell> localStorageCells = await LocalStorageController.GetTableListAsync<StorageCell>();
                List<Item> localItems = await LocalStorageController.GetTableListAsync<Item>();
                List<Cabinet> baseCabinets = new List<Cabinet>();
                List<Fridge> baseFridges = new List<Fridge>();
                List<StorageCell> baseStorageCells = new List<StorageCell>();
                List<Item> baseItems = new List<Item>();

                if (!isLocal)
                {
                    // Populating list with firebase 
                    baseCabinets = (await FireBaseController.GetCabinets()).ToList().ConvertAll(o => o.Object);
                    baseFridges = (await FireBaseController.GetFridges()).ToList().ConvertAll(o => o.Object);
                    baseItems = (await FireBaseController.GetItems()).ToList().ConvertAll(o => o.Object);
                    baseStorageCells = (await FireBaseController.GetStorageCells());


                    // Load with cloud data
                    ContentLoader.LoadItems(baseItems);
                    ContentLoader.LoadCabinets(baseCabinets, baseStorageCells, baseItems);
                    ContentLoader.LoadFridges(baseFridges, baseStorageCells, baseItems);
                }
                else
                {
                    // Load with local data
                    ContentLoader.LoadItems(localItems);
                    ContentLoader.LoadCabinets(localCabinets, localStorageCells, localItems);
                    ContentLoader.LoadFridges(localFridges, localStorageCells, localItems);
                }
            }

            if (Xamarin.Essentials.Connectivity.NetworkAccess == Xamarin.Essentials.NetworkAccess.Internet)
                Console.WriteLine("Contentmanager 135 User Has Internet :)");
            else
            {
                Console.WriteLine("Contentmanager 138 User Has No Internet :<(");
            }

                // start UI sequence
                pageController.InitializePageSequence();
        }
        /// <summary>
        /// Returns the string that represents the storage the current user is in.
        /// </summary>
        /// <returns></returns>
        public static string GetStorageType()
        {
            return storageSelection == StorageSelection.cabinet ? cabinetStorageType : fridgeStorageType;
        }
        /// <summary>
        /// Returns the StorageType enum associated with the given storageType string constant.
        /// </summary>
        /// <param name="storageType"></param>
        /// <returns></returns>
        public static StorageSelection FromStorageType(string storageType)
        {
            return storageType == cabinetStorageType ? StorageSelection.cabinet : StorageSelection.fridge;
        }
        /// <summary>
        /// Calls GetFridgeView or GetCabinetView depending on whether the user entered the cabinet or fridge selection page.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Layout<View> GetStorageView(string name)
        {
            if (storageSelection == StorageSelection.cabinet)
                return GetCabinetView(name);
            else
                return GetFridgeView(name);
        }
        /// <summary>
        /// Calls GetFridgeView or GetCabinetView depending on the given type.
        /// </summary>
        /// <param name="storageType">Type of storage represented by constants in ContentManager.</param>
        /// <returns></returns>
        public static Layout<View> GetStorageView(StorageSelection storageType, string name)
        {
            if (storageType == StorageSelection.cabinet)
                return GetCabinetView(name);
            else
                return GetFridgeView(name);
        }
        /// <summary>
        /// Retrieves the grids that make up the fridge's layout
        /// </summary>
        /// <param name="name">name of fridge.</param>
        /// <returns></returns>
        public static Layout<View> GetFridgeView(string name)
        {
            Grid gridContainer = new Grid()
            {
                Margin = new Thickness(storage_margin),
                ColumnDefinitions =
                {
                    new ColumnDefinition(){Width = GridLength.Star },
                    new ColumnDefinition(){Width = new GridLength(2, GridUnitType.Star) },
                    new ColumnDefinition(){Width = GridLength.Star }
                }
            };
            Fridge fridge = FridgeMetaBase[name];

            gridContainer.Children.Add(fridge.LeftGrid, 0, 0);
            gridContainer.Children.Add(fridge.MainGrid, 1, 0);
            gridContainer.Children.Add(fridge.RightGrid, 2, 0);

            return gridContainer;
        }
        /// <summary>
        /// Retrieves the grid that makes up the cabinet layout.
        /// </summary>
        /// <param name="name">name of cabinet.</param>
        /// <returns></returns>
        public static Layout<View> GetCabinetView(string name)
        {
            var mainGrid = CabinetMetaBase[name].MainGrid;
            mainGrid.Margin = new Thickness(storage_margin);
            return mainGrid;
        }
        /// <summary>
        /// Retreives the IStorage storage by name, depending on whether user selected to enter the cabinet or fridge selection pages.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IStorage GetSelectedStorage(string name)
        {
            if(storageSelection == StorageSelection.cabinet && CabinetMetaBase.ContainsKey(name))
            {
                return CabinetMetaBase[name];
            }
            else if (storageSelection == StorageSelection.fridge && FridgeMetaBase.ContainsKey(name))
            {
                return FridgeMetaBase[name];
            }

            return null;
        }
        /// <summary>
        /// Retreives the IStorage storage by name and type string.
        /// </summary>
        /// <param name="storageType">Type of storage represented by constants in ContentManager</param>
        /// <returns></returns>
        public static IStorage GetSelectedStorage(StorageSelection storageType, string name)
        {
            if (storageType == StorageSelection.cabinet && CabinetMetaBase.ContainsKey(name))
            {
                return CabinetMetaBase[name];
            }
            else if (storageType == StorageSelection.fridge && FridgeMetaBase.ContainsKey(name))
            {
                return FridgeMetaBase[name];
            }

            return null;
        }

        public static void AddSelectedStorage(string name, IStorage storage)
        {
            if (storageSelection == StorageSelection.cabinet)
            {
                CabinetMetaBase.Add(name, (Cabinet)storage);
            }
            else
            {
                FridgeMetaBase.Add(name, (Fridge)storage);
            }
        }
        public static void RemoveSelectedStorage(string name)
        {
            if (storageSelection == StorageSelection.cabinet)
            {
                CabinetMetaBase.Remove(name);
            }
            else
            {
                FridgeMetaBase.Remove(name);
            }
        }

        public static void AddOnBackgroundChangeListener(Action<Color> listener)
        {
            OnColorChangeEvents.Add(listener);
        }

        /// <summary>
        /// Retrieves information of expired items.
        /// </summary>
        /// <param name="expiredCabinetsName">Will be populated with a list of cabinet names with expired items.</param>
        /// <param name="expiredFridgesName">Will be populated with a list of fridge names with expired items.</param>
        /// <param name="expiredItemsId">Will be populated with a list of all expired item's id.</param>
        public static void GetItemExpirationInfo(List<string> expiredCabinetsName = null, List<string> expiredFridgesName = null, List<int> expiredItemsId = null)
        {
            foreach(var itemLayout in MetaItemBase.Values)
            {
                var item = itemLayout.ItemData;
                if (item.daysUntilExp == 0)
                {
                    expiredItemsId?.Add(item.ID);
                    if (item.StorageType == cabinetStorageType)
                        expiredCabinetsName?.Add(item.StorageName);
                    else if (item.StorageType == fridgeStorageType)
                    {
                        expiredFridgesName?.Add(item.StorageName);
                    }
                }
            }
        }

        public static readonly Dictionary<string, IconLayout> PresetIcons = new Dictionary<string, IconLayout>()
        { 
            {"carrot", new IconLayout("carrot.png", "carrot") },
            {"apple",  new IconLayout("apple.png", "apple") },
            {"blueberry", new IconLayout("blueberry.png", "blueberry") },
            {"cookie",  new IconLayout("cookie.png", "cookie") },
            {"chocolate",  new IconLayout("chocolate.png", "chocolate") },
            {"cucumber",  new IconLayout("cucumber.png", "cucumber") },
            {"egg",  new IconLayout("egg.png", "egg") },
            {"grape",  new IconLayout("grape.png", "grape") },
            {"lemon",  new IconLayout("lemon.png", "lemon") },
            {"meat",  new IconLayout("meat.png", "meat") },
           // {"meat",  new IconLayout("meat.png", "meat") },
            {"orange",  new IconLayout("orange.png", "orange") },
            {"watermelon",  new IconLayout("watermelon.png", "watermelon") },
           // {"meat",  new IconLayout("meat.png", "meat") },
            {"potato", new IconLayout("potato.png", "potato") }
        };
        public static readonly Dictionary<string, IconLayout> DefaultIcons = new Dictionary<string, IconLayout>()
        {
            {"product",  new IconLayout("carrot.png", "carrot") }
        };

        public static readonly Dictionary<string, string> PresetExpirationBase = new Dictionary<string, string>()
        {
            {"carrot", $"Whole carrots last 4 weeks (until {DateCalculator.GetDateIn(28)}). Baby carrots last 3 weeks (until {DateCalculator.GetDateIn(21)})."},
            {"apple", $"Apples last 1 week in pantry (until {DateCalculator.GetDateIn(7)}) and 5 weeks in fridge (until {DateCalculator.GetDateIn(35)})." },
            {"blueberry", $"Blueberries last 3 days in pantry (until {DateCalculator.GetDateIn(3)}) and 10 days in fridge (until {DateCalculator.GetDateIn(10)})." },
            {"blueberrie", $"Blueberries last 3 days in pantry (until {DateCalculator.GetDateIn(3)}) and 10 days in fridge (until {DateCalculator.GetDateIn(10)})." },
            {"cucumber", $"Cucumbers last 1 week in fridge (until {DateCalculator.GetDateIn(7)})." },
            {"grape", $"Grapes last 4 days in pantry (until {DateCalculator.GetDateIn(4)}) and 8 days in fridge (until {DateCalculator.GetDateIn(8)})." },
            {"lemon", $"Lemons last 3 weeks in pantry (until {DateCalculator.GetDateIn(21)}) and 8 weeks in fridge (until {DateCalculator.GetDateIn(56)})." },
            {"orange", $"Oranges last 2 weeks in pantry (until {DateCalculator.GetDateIn(14)}) and a month in fridge (until {DateCalculator.GetDateIn(30)})." },
            {"watermelon", $"Watermelons last 1 week in pantry (until {DateCalculator.GetDateIn(7)}) and 2 weeks in fridge (until {DateCalculator.GetDateIn(14)})." },
            {"potato", $"Potatoes last 3 weeks in pantry (until {DateCalculator.GetDateIn(21)}) and 2 months in fridge (until {DateCalculator.GetDateIn(60)})." },
            {"potatoe", $"Potatoes last 3 weeks in pantry (until {DateCalculator.GetDateIn(21)}) and 2 months in fridge (until {DateCalculator.GetDateIn(60)})." },
        };

        public static readonly List<string> ProfileIcons = new List<string>()
        {
            addIcon,
            "apple.png",
            "potato.png"
        };
        public static Dictionary<int, ItemLayout> UnplacedItemBase = new Dictionary<int, ItemLayout>();
        public static Dictionary<int, ItemLayout> MetaItemBase = new Dictionary<int, ItemLayout>();

        public enum StorageSelection
        {
            cabinet,
            fridge
        }
        public static StorageSelection storageSelection;
    }
}
