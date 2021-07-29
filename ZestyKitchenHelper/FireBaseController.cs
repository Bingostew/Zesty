using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using Utility;

namespace ZestyKitchenHelper
{
    public class FireBaseController
    {
        static FirebaseClient client = new FirebaseClient("https://istorage-1f60f.firebaseio.com/");

        private const string base_child = "Persons";
        private const string item_list_key = "Item List";
        private const string cabinet_list_key = "Cabinet List";
        private const string fridge_list_key = "Fridge List";
        private const string storage_cell_list_key = "Storage Cell List";

        private static string EmailToPath(string email)
        {
            return email.Replace('.', ':');
        }
        public static async Task<List<UserProfile>> GetAllUsers()
        {
            return (await client.Child(base_child).OnceAsync<UserProfile>()).
                Select(user => new UserProfile { Name = user.Object.Name, Email = user.Object.Email}).ToList();
        }
        public static async Task AddUser(UserProfile userProfile)
        {
            string emailPath = EmailToPath(userProfile.Email);
            await client.Child(base_child).Child(emailPath).PutAsync(userProfile);
         //   await client.Child(base_child).Child(emailPath).PutAsync(item_list_key + ":");
          //  await client.Child(base_child).Child(emailPath).PutAsync(cabinet_list_key);
          //  await client.Child(base_child).Child(emailPath).PutAsync(fridge_list_key);
          //  await client.Child(base_child).Child(emailPath).PutAsync(storage_cell_list_key);
        }
        public static async Task UpdateUser(UserProfile userProfile)
        {
            var cabinets = await GetCabinets();
            var fridges = await GetFridges();
            var items = await GetItems();

            await client.Child(base_child).Child(EmailToPath(userProfile.Email)).DeleteAsync();
            await AddUser(userProfile);
            foreach(var c in cabinets)
            {
                SaveCabinet(c.Object.Name);
            }
            foreach (var f in fridges)
            {
                SaveFridge(f.Object.Name);
            }
            foreach (var i in items)
            {
                SaveItem(i.Object);
            }
        }
        public static async Task<bool> HasUser(string email)
        {
            var user = (await client.Child(base_child).OnceAsync<UserProfile>()).Where(a => a.Object.Email == email);
            return user.Any();
        }
        public static async Task<UserProfile> GetUser(string email)
        {
            return await client.Child(base_child).Child(EmailToPath(email)).OnceSingleAsync<UserProfile>();
        }
        public static async Task<FirebaseObject<UserProfile>> GetUserObject(string email)
        {  
            var userList = await client.Child(base_child).OnceAsync<UserProfile>();
            var user = userList.Where(e => e.Object.Email == email);
            if (userList != null && userList.Count > 0) { return user.FirstOrDefault(); }
            else { return null; }
        }

        public static int i = 0;
        public static async Task UpdateUser(string dataType, object data, string key)
        {
            if (ContentManager.sessionUserProfile != null) { await client.Child(base_child).Child(EmailToPath(ContentManager.sessionUserProfile.Email)).
                    Child(dataType).Child(key).PutAsync(data); }
        }
        public static async Task UpdateUser(string dataType, object data, string key, string key2)
        {
            if (ContentManager.sessionUserProfile != null)
            {
                await client.Child(base_child).Child(EmailToPath(ContentManager.sessionUserProfile.Email)).
                    Child(dataType).Child(key).Child(key2).PutAsync(data);
            }
        }

        public static async Task DeleteUserItem<T>(string dataType, string key, Func<FirebaseObject<T>, bool> predicate)
        {
            if (ContentManager.sessionUserProfile != null)
            {
                var toDelete = (await client.Child(base_child).Child(EmailToPath(ContentManager.sessionUserProfile.Email)).OnceAsync<T>()).Where(predicate);
                if (ContentManager.sessionUserProfile != null && toDelete != null)
                { await client.Child(base_child).Child(EmailToPath(ContentManager.sessionUserProfile.Email)).Child(dataType).Child(key).DeleteAsync(); }
            }
        }

        public async Task DeleteUser()
        {
            await client.Child(base_child).Child(EmailToPath(ContentManager.sessionUserProfile.Email)).DeleteAsync();
        }

        //-------------------------------- Methods to manipulate item, fridge, and cabinets
        // Addition Methods
        public static async void SaveItem(Item item)
        {
            await UpdateUser(item_list_key, item, item.ID.ToString());
        }
        public static async void SaveCabinet(string cabinetName)
        {
            Cabinet cabinet = ContentManager.CabinetMetaBase[cabinetName];
            await UpdateUser(cabinet_list_key, cabinetName , cabinetName, "name");
            await UpdateUser(cabinet_list_key, cabinet.ID, cabinetName, "id");

            foreach (StorageCell cell in cabinet.GetGridCells())
            {
                await UpdateUser(storage_cell_list_key, cell, cell.MetaID.ToString());
            }
        }
        public static async void SaveFridge(string fridgeName)
        {
            Fridge fridge = ContentManager.FridgeMetaBase[fridgeName];
            await UpdateUser(cabinet_list_key, fridge, fridgeName);

            foreach (StorageCell cell in fridge.GetGridCells())
            {
                await UpdateUser(storage_cell_list_key, cell, cell.MetaID.ToString());
            }
        }
        // Deletion Methods
        public static async void DeleteItem(Item item)
        {
            await DeleteUserItem<Item>(item_list_key, item.ID.ToString(), i => i.Object.ID == item.ID);
        }
        public static void DeleteCabinetSynchronous(string cabinetName)
        {
            DeleteUserItem<Cabinet>(cabinet_list_key, cabinetName, i => i.Object.Name == cabinetName);
        }
        public static void DeleteFridgeSynchronous(string fridgeName)
        {
            DeleteUserItem<Fridge>(fridge_list_key, fridgeName, i => i.Object.Name == fridgeName);
        }

        // Retrieval methods
        public static Task<IReadOnlyCollection<FirebaseObject<Cabinet>>> GetCabinets()
        {
            var query = client.Child(base_child).Child(EmailToPath(ContentManager.sessionUserProfile.Email)).Child(cabinet_list_key);
            if (query != null)
                return query.OnceAsync<Cabinet>();
            return null;
        }
        public static Task<IReadOnlyCollection<FirebaseObject<Fridge>>> GetFridges()
        {
            var query = client.Child(base_child).Child(EmailToPath(ContentManager.sessionUserProfile.Email)).Child(fridge_list_key);
            if (query != null) 
                return query.OnceAsync<Fridge>();
            return null;
        }
        public static Task<IReadOnlyCollection<FirebaseObject<Item>>> GetItems()
        {
            var query = client.Child(base_child).Child(EmailToPath(ContentManager.sessionUserProfile.Email)).Child(item_list_key);
            if (query != null)
                return query.OnceAsync<Item>();
            return null;
        }

        public static Task<List<StorageCell>> GetStorageCells()
        {
            // Since storage cell use int64 as keys, OnceAsync<T> returns an array of StorageCells instead of a Dictionary<key, StorageCell>.
            // The former is not compatible with Json deserialization, therefore, OnceSingleAsync must be used 
            var query = client.Child(base_child).Child(EmailToPath(ContentManager.sessionUserProfile.Email)).Child(storage_cell_list_key);
            if (query != null)
                return query.OnceSingleAsync<List<StorageCell>>();
            return null;
        }
    }
}