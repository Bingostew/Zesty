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
            bool hasMappings;
            Task<int> getHasMappingTask = SQLDatabase.Table<UserProfile>().CountAsync();
            getHasMappingTask.Wait();
            hasMappings = getHasMappingTask.Result > 0;

            if (!isInitialized && !hasMappings)
            {
                Console.WriteLine("Local 40 init storage " + SQLDatabase.TableMappings.Count());
                ContentManager.isUserNew = true;
                await SQLDatabase.CreateTableAsync(typeof(Item), CreateFlags.None).ConfigureAwait(false);
                await SQLDatabase.CreateTableAsync(typeof(StorageCell), CreateFlags.None).ConfigureAwait(false);
                await SQLDatabase.CreateTableAsync(typeof(Cabinet), CreateFlags.None).ConfigureAwait(false);
                await SQLDatabase.CreateTableAsync(typeof(Fridge), CreateFlags.None).ConfigureAwait(false);
                await SQLDatabase.CreateTableAsync(typeof(UserProfile), CreateFlags.None).ConfigureAwait(false);
                await SQLDatabase.CreateTableAsync(typeof(MetaUserInfo), CreateFlags.None).ConfigureAwait(false);

                isInitialized = true;
            }
          
        }

        private static async void SafeFireAndForget(Task task, bool returnToContext, Action<Exception> onException = null)
        {
            await task.ConfigureAwait(returnToContext);
        }

        public static void ResetDatabase()
        {

            Console.WriteLine("LocalStorage 63 database reset started +====================+");
            SQLDatabase.DeleteAllAsync<Item>();
            SQLDatabase.DeleteAllAsync<StorageCell>();
            SQLDatabase.DeleteAllAsync<Cabinet>();
            SQLDatabase.DeleteAllAsync<Fridge>();
            SQLDatabase.DeleteAllAsync<UserProfile>();
            SQLDatabase.DeleteAllAsync<MetaUserInfo>();
            Console.WriteLine("LocalStorage 63 database reset end +====================+");
        }
        //Retrieval Methods
        public static async Task<MetaUserInfo> GetMetaUserInfo()
        {
            if(!ContentManager.isUserNew)
                return await SQLDatabase.Table<MetaUserInfo>().FirstAsync();
            return null;
        }
        public static async Task<UserProfile> GetUserAsync()
        {
            return await SQLDatabase.Table<UserProfile>().FirstAsync();
        }
        public static Task<List<T>> GetTableListAsync<T>() where T : new()
        {
            return SQLDatabase.Table<T>().ToListAsync();
        }
        public static Task<Item> GetItemAsync(int ID)
        {
            return SQLDatabase.Table<Item>().Where(i => i.ID == ID).FirstOrDefaultAsync();
        }
        public static Task<Cabinet> GetCabinetAsync(string cabinetName)
        {
            return SQLDatabase.Table<Cabinet>().Where(c => c.Name == cabinetName).FirstOrDefaultAsync();
        }
        public static Task<Fridge> GetFridgeAsync(string fridgeName)
        {
            return SQLDatabase.Table<Fridge>().Where(c => c.Name == fridgeName).FirstOrDefaultAsync();
        }

        // Insertion/Update Methods
        public static async void SetMetaUserInfo(MetaUserInfo metaUserInfo)
        {
            var currentInfo = await GetMetaUserInfo();
            if (currentInfo != null)
                await SQLDatabase.DeleteAsync(currentInfo);
            await SQLDatabase.InsertAsync(metaUserInfo);
        }
        public static async void AddUser(UserProfile user)
        {
            await SQLDatabase.InsertAsync(user);
        }
        public static async void UpdateUser(UserProfile user)
        {
            var currentUser = await GetUserAsync();
            if (currentUser == null)
                return;
            await SQLDatabase.DeleteAllAsync(SQLDatabase.TableMappings.First(m => m.MappedType == typeof(UserProfile)));
            await SQLDatabase.InsertAsync(user);
        }
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

        public static async void UpdateStorageCell(StorageCell storageCell)
        {
            await SQLDatabase.UpdateAsync(storageCell);
        }

        //Deletion Methods
        public static Task DeleteTable<T>() where T : new()
        {
             return SQLDatabase.DeleteAllAsync<T>();
        }
        public static async void DeleteCabinetSynchronous(string name)
        {
            SQLDatabase.DeleteAsync(ContentManager.CabinetMetaBase[name]);
        }
        public static async void DeleteFridgeSynchronous(string name)
        {
            SQLDatabase.DeleteAsync(ContentManager.FridgeMetaBase[name]);
        }
        public static async void DeleteItem(Item item)
        {
            if (await GetItemAsync(item.ID) != null)
                await SQLDatabase.DeleteAsync(item);
        }
    }
}