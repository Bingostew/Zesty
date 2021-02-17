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
        FirebaseClient client = new FirebaseClient("https://istorage-1f60f.firebaseio.com/");
        private const string baseChild = "Persons";
        public static FirebaseObject<UserProfile> sessionUserProfile;
        public async Task<List<UserProfile>> GetAllUsers()
        {
            return (await client.Child(baseChild).OnceAsync<UserProfile>()).
                Select(user => new UserProfile { Name = user.Object.Name, Email = user.Object.Email}).ToList();
        }
        public async Task AddUser(string user, string email)
        {
            await client.Child(baseChild).PostAsync(new UserProfile() { Name = user, Email = email});
        }
        public async Task<bool> HasUser(string email)
        {
            var user = (await client.Child(baseChild).OnceAsync<UserProfile>()).Where(a => a.Object.Email == email);
            return user.Any();
        }
        public async Task<UserProfile> GetUser(string email)
        {
            var userList = await GetAllUsers();
            await client.Child(baseChild).OnceAsync<UserProfile>();
            var user = userList.Where(a => a.Email == email);
            if (user.Any()) { return user.FirstOrDefault(); }
            else { return null; }
        }
        public async Task<FirebaseObject<UserProfile>> GetUserObject(string email)
        {  
            var userList = await client.Child(baseChild).OnceAsync<UserProfile>();
            var user = userList.Where(e => e.Object.Email == email);
            if (userList != null && userList.Count > 0) { return user.FirstOrDefault(); }
            else { return null; }
        }

        public static int i = 0;
        public async Task UpdateUser(string email, string dataType, object data)
        {
            if (sessionUserProfile != null) { await client.Child(baseChild).Child(sessionUserProfile.Key).Child(dataType).PostAsync(JsonConvert.SerializeObject(data)); }
        }
        public async Task UpdateUser(string email,UserProfile newProfile)
        {
            if (sessionUserProfile != null) { await client.Child(baseChild).Child(sessionUserProfile.Key).PutAsync(newProfile); }
        }
        public async Task UpdateItem(string email, Item item)
        {
            var toUpdate = (await client.Child(baseChild).Child(sessionUserProfile.Key).Child("ItemList").OnceAsync<Item>()).Where(i => i.Object.ID == item.ID).FirstOrDefault();
            if (toUpdate != null) { await client.Child(baseChild).Child(sessionUserProfile.Key).Child("ItemList").Child(toUpdate.Key).PutAsync(item); }
        }
        public async Task DeleteUserItem(string email, Item data)
        {
            var toDelete = (await client.Child(baseChild).Child(sessionUserProfile.Key).OnceAsync<Item>()).Where(i => i.Object.ID == data.ID);
            if (toDelete.Any()) { await client.Child(baseChild).Child(sessionUserProfile.Key).Child(toDelete.FirstOrDefault().Key).DeleteAsync(); }
        }

        public async Task DeleteUserStorage(string email, Cabinet cabinet)
        {
            var toDelete = (await client.Child(baseChild).Child(sessionUserProfile.Key).OnceAsync<Cabinet>()).Where(i => i.Object.Name == cabinet.Name);
            if (toDelete.Any()) { await client.Child(baseChild).Child(sessionUserProfile.Key).Child(toDelete.FirstOrDefault().Key).DeleteAsync(); }
        }
        public async Task DeleteUserStorage(string email, Fridge fridge)
        {
            var toDelete = (await client.Child(baseChild).Child(sessionUserProfile.Key).OnceAsync<Fridge>()).Where(i => i.Object.Name == fridge.Name);
            if (toDelete.Any()) { await client.Child(baseChild).Child(sessionUserProfile.Key).Child(toDelete.FirstOrDefault().Key).DeleteAsync(); }
        }
        public async Task DeleteUser(string email)
        {
            await client.Child(baseChild).Child(sessionUserProfile.Key).DeleteAsync();
        }
        public async Task UpdateCabinet(string email, FirebaseObject<Cabinet> cabinet)
        {
            await client.Child(baseChild).Child(sessionUserProfile.Key).Child("CabinetList").Child(cabinet.Key).PutAsync(cabinet);
        }
        public async Task UpdateFridge(string email, FirebaseObject<Fridge> fridge)
        {
            await client.Child(baseChild).Child(sessionUserProfile.Key).Child("FridgeList").Child(fridge.Key).PutAsync(fridge);
        }
        public async Task<FirebaseObject<Cabinet>> GetUserCabinet(string email, string cabinetName)
        {
            return (await client.Child(baseChild).Child(sessionUserProfile.Key).Child("CabinetList").OnceAsync<Cabinet>()).Where(c => c.Object.Name == cabinetName).FirstOrDefault();
        }
        public async Task<FirebaseObject<Fridge>> GetUserFridge(string email, string fridgeName)
        {
            return (await client.Child(baseChild).Child(sessionUserProfile.Key).Child("FridgeList").OnceAsync<Fridge>()).Where(c => c.Object.Name == fridgeName).FirstOrDefault();
        }

        public async Task<List<Cabinet>> GetUserCabinetList(string email)
        {
            var firebaseList = (await client.Child(baseChild).Child(sessionUserProfile.Key).Child("CabinetList").OnceAsync<Cabinet>()).ToList();
            List<Cabinet> list = new List<Cabinet>();
            foreach(var cab in firebaseList)
            {
                list.Add(cab.Object);
            }
            return list;
        }
        public async Task<List<Fridge>> GetUserFridgeList(string email)
        {
            var firebaseList = (await client.Child(baseChild).Child(sessionUserProfile.Key).Child("FridgeList").OnceAsync<Fridge>()).ToList();
            List<Fridge> list = new List<Fridge>();
            foreach (var cab in firebaseList)
            {
                list.Add(cab.Object);
            }
            return list;
        }

        public async Task<List<Item>> GetUserItemList(string email)
        {
            var firebaseList = (await client.Child(baseChild).Child(sessionUserProfile.Key).Child("ItemList").OnceAsync<Item>()).ToList();
            List<Item> list = new List<Item>();
            foreach (var item in firebaseList)
            {
                list.Add(item.Object);
            }
            return list;
        }
    }


    public class FireBaseMediator 
    {
        public static FireBaseController fireBaseController = new FireBaseController();
        public static async void PutItem(Utility.Item item) 
        {
            if (ContentManager.sessionUserName != null)
            {
                await fireBaseController.UpdateUser(ContentManager.sessionUserName, "ItemList", item);
            }
        }

        public static async void DeleteItem(Item item)
        {
            if (ContentManager.sessionUserName != null)
            {
                await fireBaseController.DeleteUserItem(ContentManager.sessionUserName, item);
            }
        }

        public static async void PutCabinet(Utility.Cabinet cabinet)
        {
            if (ContentManager.sessionUserName != null)
            {
                var tryCabinet = await fireBaseController.GetUserCabinet(ContentManager.sessionUserName, cabinet.Name);
                if (tryCabinet != null)
                {
                    await fireBaseController.UpdateCabinet(ContentManager.sessionUserName, tryCabinet);
                }
                else
                {
                    await fireBaseController.UpdateUser(ContentManager.sessionUserName, "CabinetList", cabinet);
                }
            }
        }

        public static async void PutFridge(Utility.Fridge fridge)
        {
            if (ContentManager.sessionUserName != null)
            {
                var tryFridge = await fireBaseController.GetUserFridge(ContentManager.sessionUserName, fridge.Name);
                if (tryFridge != null)
                {
                    await fireBaseController.UpdateFridge(ContentManager.sessionUserName, tryFridge);
                }
                else
                {
                    await fireBaseController.UpdateUser(ContentManager.sessionUserName, "FridgeList", fridge);
                }
            }
        }
        public static async void DeleteCabinet(string name)
        {
            if (ContentManager.sessionUserName != null)
            {
                await fireBaseController.DeleteUserStorage(ContentManager.sessionUserName, (await fireBaseController.GetUserCabinet(ContentManager.sessionUserName, name)).Object);
            }
        }
        public static async void DeleteFridge(string name)
        {
            if (ContentManager.sessionUserName != null)
            {
                await fireBaseController.DeleteUserStorage(ContentManager.sessionUserName, (await fireBaseController.GetUserFridge(ContentManager.sessionUserName, name)).Object);
            }
        }
        public static async void UpdateItem(Item item)
        {
            if (ContentManager.sessionUserName != null)
            {
                await fireBaseController.UpdateItem(ContentManager.sessionUserName, item);
            }
        }
        public static void SaveCabinet(string name, string cabinetRows, string rowItems)
        {
            if (ContentManager.sessionUserName != null)
            {
                PutCabinet(new Cabinet().SetCabinet(cabinetRows, rowItems, name));
            }
        }
        public static void SaveFridge(string name, string fridgeRows, string rowItems)
        {
            if (ContentManager.sessionUserName != null)
            {
                FireBaseMediator.PutFridge(new Fridge().SetFridge(fridgeRows, rowItems, name));
            }
        }
    }

}