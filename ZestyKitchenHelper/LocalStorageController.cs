using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SQLite;
using SQLitePCL;
using System.Threading.Tasks;
using Utility;

namespace ZestyKitchenHelper
{
    public class LocalStorageController
    {
        public static string DatabasePath
        {
            get
            {
                var basePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);

                return Path.Combine(basePath, "zesty.db3");
            }
        }
        public const SQLiteOpenFlags SQLFlags = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
        static readonly Lazy<SQLiteAsyncConnection> sqlStorageInitializer = new Lazy<SQLiteAsyncConnection>(
            () => { return new SQLiteAsyncConnection(DatabasePath, SQLFlags); } 
            );
        static SQLiteAsyncConnection SQLDatabase => sqlStorageInitializer.Value;
        static bool isInitialized = false;

        public static void InitializeLocalDataBase()
        {
            SafeFireAndForget(InitializeAsync(), false);
        }

        static async Task InitializeAsync()
        {
            bool hasMappings = SQLDatabase.TableMappings.Any(m => m.MappedType == typeof(Item) || m.MappedType == typeof(Cabinet)
                                || m.MappedType == typeof(StorageCell) || m.MappedType == typeof(Fridge));
            if (!isInitialized && !hasMappings)
            {
                Console.WriteLine("LocalStorage 44 : new table created !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                ContentManager.isUserNew = true;
                await SQLDatabase.CreateTableAsync(typeof(Item), CreateFlags.None).ConfigureAwait(false);
                await SQLDatabase.CreateTableAsync(typeof(StorageCell), CreateFlags.None).ConfigureAwait(false);
                await SQLDatabase.CreateTableAsync(typeof(Cabinet), CreateFlags.None).ConfigureAwait(false);
                await SQLDatabase.CreateTableAsync(typeof(Fridge), CreateFlags.None).ConfigureAwait(false);

                isInitialized = true;
            }
        }

        private static async void SafeFireAndForget(Task task, bool returnToContext, Action<Exception> onException = null)
        {
            await task.ConfigureAwait(returnToContext);
        }

        public static void ResetDatabase()
        {
            DeleteTable<Fridge>();
            DeleteTable<StorageCell>();
            DeleteTable<Cabinet>();
            DeleteTable<Item>();
        }
        //Retrieval Methods
        public static Task<List<T>> GetTableListAsync<T>() where T : new()
        {
            return SQLDatabase.Table<T>().ToListAsync();
        }
        public static Task<Item> GetItemAsync(int ID)
        {
            return SQLDatabase.Table<Item>().Where(i => i.ID == ID).FirstAsync();
        }
        public static Task<Cabinet> GetCabinetAsync(string cabinetName)
        {
            return SQLDatabase.Table<Cabinet>().Where(c => c.Name == cabinetName).FirstAsync();
        }
        public static Task<Fridge> GetFridgeAsync(string fridgeName)
        {
            return SQLDatabase.Table<Fridge>().Where(c => c.Name == fridgeName).FirstAsync();
        }

        // Insertion/Update Methods
        public static async void AddItem(Item item)
        {
            await SQLDatabase.InsertAsync(item);
        }
        public static async void UpdateItem(Item item)
        {
            if (await GetItemAsync(item.ID) != null)
                await SQLDatabase.UpdateAsync(item);
        }
        public static async void AddCabinet(string name)
        {
            Cabinet cabinet = ContentManager.CabinetMetaBase[name];
            await SQLDatabase.InsertAsync(cabinet);

            foreach (StorageCell cell in cabinet.GetGridCells())
            {
                Console.WriteLine("LocalStorageController 100 cell " + cell.RowSpan);
                await SQLDatabase.InsertAsync(cell);
            }
        }
        public static async void AddFridge(string name)
        {
            Fridge fridge = ContentManager.FridgeMetaBase[name];
            await SQLDatabase.InsertAsync(fridge);

            foreach (StorageCell cell in fridge.GetGridCells())
            {
                await SQLDatabase.InsertAsync(cell);
            }
        }

        /// <summary>
        /// Updates cabinets that is already stored
        /// </summary>
        /// <param name="cabinet"></param>
        /// <param name="updateItems">If true, then the cells and items are updated.</param>
        /// <param name="updateCabinet"> If true, then the cabinet meta data is updated</param>
        public static async void UpdateCabinetLocal(Cabinet cabinet, bool updateCabinet, bool updateItems)
        {
            if (await GetCabinetAsync(cabinet.Name) != null)
            {
                await SQLDatabase.UpdateAsync(cabinet);
            }

            if (updateItems)
            {
                await SQLDatabase.UpdateAllAsync(cabinet.GetGridCells());

                foreach (StorageCell cell in cabinet.GetGridCells())
                {
                    foreach (ItemLayout child in cell.GetItemGrid().Children)
                    {
                        if (await GetItemAsync(child.ItemData.ID) != null)
                        {
                            await SQLDatabase.UpdateAsync(child.ItemData);
                        }
                        else
                        {
                            await SQLDatabase.InsertAsync(child.ItemData);
                        }
                    }
                }
            }
        }

        //Deletion Methods
        public static void DeleteTable<T>() where T : new()
        {
            SQLDatabase.DeleteAllAsync<T>();
        }
        public static async void DeleteCabinet(string name)
        {
            if (await GetCabinetAsync(name) != null)
                await SQLDatabase.DeleteAsync(ContentManager.CabinetMetaBase[name]);
        }
        public static async void DeleteFridge(string name)
        {
            if (await GetFridgeAsync(name) != null)
                await SQLDatabase.DeleteAsync(ContentManager.FridgeMetaBase[name]);
        }
        public static async void DeleteItem(Item item)
        {
            if (await GetItemAsync(item.ID) != null)
                await SQLDatabase.DeleteAsync(item);
        }
    }
}