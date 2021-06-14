using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ZestyKitchenHelper
{
    public partial class App : Application
    {
        public App()
        {
            ContentManager.pageController.InitializePageSequence();
            MainPage = ContentManager.pageController;

        }

        protected override void OnStart()
        {
            ContentManager.InitializeApp();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
