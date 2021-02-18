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
        bool isInitialized = false;

        public LocalStorageController()
        {
            SafeFireAndForget(InitializeAsync(), false);
        }

        async Task InitializeAsync()
        {
            if (!isInitialized && !SQLDatabase.TableMappings.Any(m => m.MappedType.Name == typeof(Item).Name))
            {
                await SQLDatabase.CreateTableAsync(typeof(Item), CreateFlags.None).ConfigureAwait(false);
                await SQLDatabase.CreateTableAsync(typeof(Cabinet), CreateFlags.None).ConfigureAwait(false);
                await SQLDatabase.CreateTableAsync(typeof(Fridge), CreateFlags.None).ConfigureAwait(false);

                isInitialized = true;
            }
        }

        public static async void DeleteTableAsync<T>()
        {
            await SQLDatabase.DeleteAllAsync<T>();
        }
        public static async void SaveItemAsync<T>(T item)
        {
            await SQLDatabase.InsertAsync(item);

        }
        public static async void UpdateItemsAsync<T>(T item)
        {
            await SQLDatabase.UpdateAsync(item);
        }
        public static async void DeleteItemAsync<T>(T item)
        {
            await SQLDatabase.DeleteAsync(item);
        }
        public static async void DeleteFridgeAsync(string name)
        {
            await SQLDatabase.Table<Fridge>().DeleteAsync(f => f.Name == name);
        }
        public static async void DeleteCabinetAsync(string name)
        {
            await SQLDatabase.Table<Cabinet>().DeleteAsync(f => f.Name == name);
        }
        public static async void SaveFridgeLocal(string name, string fridgeRows, string rowItems)
        {
            var tryFridge = await GetFridgeAsync(name);
            if (tryFridge != null)
            {
                UpdateItemsAsync(new Fridge().SetFridge(fridgeRows, rowItems, name));
            }
            else
            {
                Fridge fridge = new Fridge().SetFridge(fridgeRows, rowItems, name);
                SaveItemAsync(fridge);
            }
        }
        public static async void SaveCabinetLocal(string name, string cabinetRows, string rowItems)
        {
            var tryCabinet = await GetCabinetAsync(name);
            if (tryCabinet != null)
            {
                UpdateItemsAsync(new Cabinet().SetCabinet(cabinetRows, rowItems, name));
            }
            else
            {
                Cabinet cabinet = new Cabinet().SetCabinet(cabinetRows, rowItems, name);
                SaveItemAsync(cabinet);
            }
        }
        public static async void UpdateItemAsync(Item item)
        {
            var toUpdate = GetItemAsync(item);
            await SQLDatabase.Table<Item>().DeleteAsync((i) => i.ID == item.ID);
            SaveItemAsync(item);
        }
        public static Task<Item> GetItemAsync(Item item)
        {
            return SQLDatabase.Table<Item>().Where(i => i.name == item.name).FirstOrDefaultAsync();
        }
        public static Task<Cabinet> GetCabinetAsync(string name)
        {
            return SQLDatabase.Table<Cabinet>().Where(c => c.Name == name).FirstOrDefaultAsync();
        }
        public static Task<Fridge> GetFridgeAsync(string name)
        {
            return SQLDatabase.Table<Fridge>().Where(f => f.Name == name).FirstOrDefaultAsync();
        }
        public static Task<List<Cabinet>> GetCabinetListAsync()
        {
            return SQLDatabase.Table<Cabinet>().ToListAsync();
        }
        public static Task<List<Fridge>> GetFridgeListAsync()
        {
            return SQLDatabase.Table<Fridge>().ToListAsync();
        }
        public static Task<List<Item>> GetItemListAsync()
        {
            return SQLDatabase.Table<Item>().ToListAsync();
        }
        private async void SafeFireAndForget(Task task, bool returnToContext, Action<Exception> onException = null)
        {
            await task.ConfigureAwait(returnToContext);
        }
    }
}