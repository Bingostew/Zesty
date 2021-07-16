using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Work;
using Android.Support.V4.App;
using Xamarin.Forms;
using AndroidApp = Android.App.Application;
using ZestyKitchenHelper.Droid;
using Android.Graphics;
using Utility;

[assembly: Dependency(typeof(NotificationWorker))]
namespace ZestyKitchenHelper.Droid
{  
    public class NotificationSender : INotificationManager
    {
        const string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "main notification";
        int notificationInt = 0;
        bool channelInitialized;

        private NotificationManager manager;
        public void ReceiveNotification(string title, string message) { }
        public void Initialize() { }
        public event EventHandler NotificationReceived;


        void CreateNotificationChannel()
        {
            manager = (NotificationManager)AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = channelDescription
                };
                manager.CreateNotificationChannel(channel);
            }

            channelInitialized = true;
        }
        public int ScheduleNotification(string title, string message)
        {
            CreateNotificationChannel();
            NotificationCompat.Builder builder = new NotificationCompat.Builder(AndroidApp.Context, channelId)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.palm)
                .SetDefaults((int)NotificationDefaults.Sound);

            var notification = builder.Build();
            manager.Notify(notificationInt, notification);
            notificationInt++;
            manager.Notify(notificationInt, notification);
            notificationInt++;
            return notificationInt;
        }
    }
    public class NotificationWorker : Worker
    {
        NotificationSender sender = new NotificationSender();
        private static List<Item> itemList = new List<Item>();
        public NotificationWorker(Context context, WorkerParameters workParams) : base(context, workParams)
        {
        }
        private async void SetItemList()
        {
            itemList = await LocalStorageController.GetTableListAsync<Item>();
        }
        public override Result DoWork()
        {
            SetItemList();
            foreach (var item in itemList)
            {
                item.SetDaysUntilExpiration();
                if (item.daysUntilExp < 1 && !item.oneDayWarning)
                { sender.ScheduleNotification(ContentManager.exp_notification_title, "Your " + item.Name + " expires in 1 day!"); item.oneDayWarning = true; }
                else if (item.daysUntilExp < 3 && !item.threeDaysWarning)
                { sender.ScheduleNotification(ContentManager.exp_notification_title, "Your " + item.Name + " expires in 3 days!"); item.threeDaysWarning = true; }
                else if (item.daysUntilExp < 7 && !item.weekWarning)
                { sender.ScheduleNotification(ContentManager.exp_notification_title, "Your " + item.Name + " expires in one week!"); item.weekWarning = true; }
                LocalStorageController.UpdateItem(item);
            }
            Console.WriteLine("item item weeeee " + itemList.Count);
            return Result.InvokeSuccess();
        }
    }
}